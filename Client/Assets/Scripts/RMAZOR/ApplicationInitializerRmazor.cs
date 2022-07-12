using System.Collections;
using System.Collections.Generic;
using System.Text;
using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Entities;
using Common.Enums;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Managers.Achievements;
using Common.Managers.Advertising;
using Common.Managers.IAP;
using Common.Managers.PlatformGameServices;
using Common.Network;
using Common.Ticker;
using Common.Utils;
using Newtonsoft.Json;
using RMAZOR.Controllers;
using RMAZOR.Helpers;
using RMAZOR.Managers;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.SceneManagement;
using Zenject;

namespace RMAZOR
{
    public class ApplicationInitializerRmazor : MonoBehaviour
    {
        #region inject
    
        private IRemotePropertiesCommon RemoteProperties     { get; set; }
        private CommonGameSettings      Settings             { get; set; }
        private IGameClient             GameClient           { get; set; }
        private IAdsManager             AdsManager           { get; set; }
        private ILocalizationManager    LocalizationManager  { get; set; }
        private ILevelsLoader           LevelsLoader         { get; set; }
        private IScoreManager           ScoreManager         { get; set; }
        private IHapticsManager         HapticsManager       { get; set; }
        private IShopManager            ShopManager          { get; set; }
        private IRemoteConfigManager    RemoteConfigManager  { get; set; }
        private IPermissionsRequester   PermissionsRequester { get; set; }
        private IAssetBundleManager     AssetBundleManager   { get; set; }
        private ICommonTicker           CommonTicker         { get; set; }
        
        [Inject] 
        private void Inject(
            IRemotePropertiesCommon _RemoteProperties,
            CommonGameSettings      _Settings,
            IGameClient             _GameClient,
            IAdsManager             _AdsManager,
            ILocalizationManager    _LocalizationManager,
            ILevelsLoader           _LevelsLoader,
            IScoreManager           _ScoreManager,
            IHapticsManager         _HapticsManager,
            IAssetBundleManager     _AssetBundleManager,
            IShopManager            _ShopManager,
            IRemoteConfigManager    _RemoteConfigManager,
            ICameraProvider         _CameraProvider,
            IPermissionsRequester   _PermissionsRequester,
            ICommonTicker           _CommonTicker,
            CompanyLogo             _CompanyLogo)
        {
            RemoteProperties     = _RemoteProperties;
            Settings             = _Settings;
            GameClient           = _GameClient;
            AdsManager           = _AdsManager;
            LocalizationManager  = _LocalizationManager;
            LevelsLoader         = _LevelsLoader;
            ScoreManager         = _ScoreManager;
            HapticsManager       = _HapticsManager;
            AssetBundleManager   = _AssetBundleManager;
            ShopManager          = _ShopManager;
            RemoteConfigManager  = _RemoteConfigManager;
            PermissionsRequester = _PermissionsRequester;
            CommonTicker         = _CommonTicker;
        }
        
        #endregion
    
        #region engine methods

        private IEnumerator Start()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.name == SceneNames.Preload)
                CommonData.GameId = GameIds.RMAZOR;
            RemoteConfigManager.Initialize += () => RemoteProperties.DebugEnabled |= Settings.debugAnyway;
            LogAppInfo();
            yield return Cor.Delay(0.5f, CommonTicker); // для более плавной загрузки логотипа компании
            var permissionsEntity = PermissionsRequester.RequestPermissions();
            while (permissionsEntity.Result == EEntityResult.Pending)
                yield return new WaitForEndOfFrame();
            InitStartData();
            InitGameManagers();
            InitDefaultData();
            yield return LoadSceneLevel();
        }

        private static void LogAppInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Application started");
            sb.AppendLine("Platform: "              + Application.platform);
            sb.AppendLine("Installer name: "        + Application.installerName);
            sb.AppendLine("Identifier: "            + Application.identifier);
            sb.AppendLine("Version: "               + Application.version);
            sb.AppendLine("Data path: "             + Application.dataPath);
            sb.AppendLine("Install mode: "          + Application.installMode);
            sb.AppendLine("Sandbox type: "          + Application.sandboxType);
            sb.AppendLine("Unity version: "         + Application.unityVersion);
            sb.AppendLine("Console log path: "      + Application.consoleLogPath);
            sb.AppendLine("Streaming assets path: " + Application.streamingAssetsPath);
            sb.AppendLine("Temporary cache path: "  + Application.temporaryCachePath);
            sb.AppendLine("Absolute url: "          + Application.absoluteURL);
            Dbg.Log(sb.ToString());
        }

        private IEnumerator LoadSceneLevel()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            var @params = new LoadSceneParameters(LoadSceneMode.Single);
            var op = SceneManager.LoadSceneAsync(SceneNames.Level, @params);
            while (!op.isDone)
                yield return null;
        }

        #endregion
    
        #region nonpublic methods
        
        private void OnSceneLoaded(Scene _Scene, LoadSceneMode _Mode)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (!_Scene.name.EqualsIgnoreCase(SceneNames.Level)) 
                return;
            Cor.Run(InitGameControllerCoroutine());
        }

        private IEnumerator InitGameControllerCoroutine()
        {
            yield return Cor.WaitWhile(
                () => !RemoteConfigManager.Initialized || !AssetBundleManager.Initialized,
                () =>
                {
                    LevelsLoader.Initialize += InitGameController;
                    LevelsLoader.Init();
                });
        }
    
        private void InitGameManagers()
        {
            if (CommonData.DevelopmentBuild)
                RemoteConfigManager.Initialize += SRDebug.Init;
            RemoteConfigManager.Initialize += AdsManager.Init;
            RemoteConfigManager.Initialize += HapticsManager.Init;
            RemoteConfigManager.Init();
            AssetBundleManager .Init();
            ShopManager.RegisterProductInfos(GetProductInfos());
            ShopManager        .Init();
            ScoreManager.RegisterLeaderboardsMap(GetLeaderboardsMap());
            ScoreManager.RegisterAchievementsMap(GetAchievementsMap());
            ScoreManager       .Initialize += OnScoreManagerInitialize;
            ScoreManager       .Init();
            GameClient         .Init();
            LocalizationManager.Init();
            
        }

        private void InitGameController()
        {
            var controller = GameControllerMVC.CreateInstance();
            controller.Initialize += () =>
            {
                const string fName = CommonData.SavedGameFileName;
                var sgEntityRemote = ScoreManager.GetSavedGameProgress(fName, false);
                var sgEntityCache = ScoreManager.GetSavedGameProgress(fName, true);
                Cor.Run(Cor.WaitWhile(
               () => sgEntityRemote.Result == EEntityResult.Pending,
               () =>
               {
                   bool castSgRemoteSuccess = sgEntityRemote.Value.CastTo(out SavedGame sgRemote);
                   if (sgEntityRemote.Result != EEntityResult.Success || !castSgRemoteSuccess)
                   {
                       Cor.Run(Cor.WaitWhile(
                           () => sgEntityCache.Result == EEntityResult.Pending,
                           () =>
                           {
                               bool castSgCacheSuccess = sgEntityCache.Value.CastTo(out SavedGame sgCache);
                               if (sgEntityCache.Result != EEntityResult.Success || !castSgCacheSuccess)
                               {
                                   Dbg.LogWarning("Failed to load saved game entity: " +
                                                  $"_Result: {sgEntityCache.Result}," +
                                                  $" castSuccess: {castSgCacheSuccess}," +
                                                  $" _Value: {JsonConvert.SerializeObject(sgEntityCache.Value)}");
                                   var savedGame = new SavedGame
                                   {
                                       FileName = fName,
                                       Level = 0,
                                       Money = 100
                                   };
                                   ScoreManager.SaveGameProgress(savedGame, true);
                                   LoadLevelByIndex(controller, 0);
                                   return;
                               }
                               LoadLevelByIndex(controller, sgCache.Level);
                           },
                           _Seconds: 1f));
                       return;
                   }
                   LoadLevelByIndex(controller, sgRemote.Level);
               }, _Seconds: 1f));
            };
            controller.Init();
        }

        private void LoadLevelByIndex(IGameController _Controller, long _LevelIndex)
        {
            var info = LevelsLoader.GetLevelInfo(1, _LevelIndex);
            _Controller.Model.LevelStaging.LoadLevel(info, _LevelIndex);
        }
        
        private void OnScoreManagerInitialize()
        {
            var savedGameServerEntity = ScoreManager.GetSavedGameProgress(
                CommonData.SavedGameFileName, 
                false);
            Cor.Run(Cor.WaitWhile(
                () => savedGameServerEntity.Result == EEntityResult.Pending,
                () =>
                {
                    bool castSuccess = savedGameServerEntity.Value.CastTo(out SavedGame savedGameServer);
                    if (savedGameServerEntity.Result == EEntityResult.Fail || !castSuccess)
                    {
                        Dbg.LogWarning("Saved game from server is null");
                        return;
                    }
                    Dbg.Log("getting saved game from server");
                    var newSavedGame = new SavedGame
                    {
                        FileName = CommonData.SavedGameFileName,
                        Money = savedGameServer.Money,
                        Level = savedGameServer.Level
                    };
                    ScoreManager.SaveGameProgress(newSavedGame, true);
                }));
            
        }

        private void InitDefaultData()
        {
            if (SaveUtils.GetValue(SaveKeysCommon.NotFirstLaunch))
                return;
            Dbg.Log(nameof(InitDefaultData));
            SaveUtils.PutValue(SaveKeysCommon.SettingSoundOn,         true);
            SaveUtils.PutValue(SaveKeysCommon.SettingMusicOn,         true);
            SaveUtils.PutValue(SaveKeysCommon.SettingNotificationsOn, true);
            SaveUtils.PutValue(SaveKeysCommon.SettingHapticsOn,       true);
            SaveUtils.PutValue(SaveKeysCommon.NotFirstLaunch,         true);
            SetDefaultLanguage();
        }

        private void InitStartData()
        {
            CommonData.LoadNextLevelAutomatically = true;
            CommonData.Release = true;
            SaveUtils.PutValue(SaveKeysCommon.AppVersion, Application.version);
            Application.targetFrameRate = GraphicUtils.GetTargetFps();
            Dbg.LogLevel = Settings.logLevel;
        }

        private void SetDefaultLanguage()
        {
            ELanguage lang = Application.systemLanguage switch
            {
                SystemLanguage.Russian    => ELanguage.Russian,
                SystemLanguage.Belarusian => ELanguage.Russian,
                SystemLanguage.Ukrainian  => ELanguage.Russian,
                SystemLanguage.German     => ELanguage.German,
                SystemLanguage.Spanish    => ELanguage.Spanish,
                SystemLanguage.Portuguese => ELanguage.Portugal,
                SystemLanguage.Japanese   => ELanguage.Japaneese,
                SystemLanguage.Korean     => ELanguage.Korean,
                _                         => ELanguage.English
            };
            LocalizationManager.SetLanguage(lang);
        }
        
        private static List<ProductInfo> GetProductInfos()
        {
            string suffix = CommonUtils.Platform == RuntimePlatform.Android ? string.Empty : "_2";
            const ProductType ptCons = ProductType.Consumable;
            const ProductType ptNonCons = ProductType.NonConsumable;
            return new List<ProductInfo>
            {
                new ProductInfo(PurchaseKeys.Money1,    $"small_pack_of_coins{suffix}",           ptCons),
                new ProductInfo(PurchaseKeys.Money2,    $"medium_pack_of_coins{suffix}",          ptCons),
                new ProductInfo(PurchaseKeys.Money3,    $"big_pack_of_coins{suffix}",             ptCons),
                new ProductInfo(PurchaseKeys.NoAds,     $"disable_mandatory_advertising{suffix}", ptNonCons),
            };
        }

        private static Dictionary<ushort, string> GetLeaderboardsMap()
        {
            bool ios = CommonUtils.Platform == RuntimePlatform.IPhonePlayer;
            return new Dictionary<ushort, string>
            {
                { DataFieldIds.Level, ios ? "level" : "CgkI1IvonNkDEAIQBg"}
            };
        }

        private static Dictionary<ushort, string> GetAchievementsMap()
        {
            bool ios = CommonUtils.Platform == RuntimePlatform.IPhonePlayer;
            return new Dictionary<ushort, string>
            {
                {AchievementKeys.Level10Finished, ios ? "level_0010_finished" : "CgkI1IvonNkDEAIQBQ"},
                {AchievementKeys.Level50Finished, ios ? "level_0050_finished" : "CgkI1IvonNkDEAIQCA"},
                {AchievementKeys.Level100Finished, ios ? "level_0100_finished" : "CgkI1IvonNkDEAIQBw"},
                {AchievementKeys.Level200Finished, ios ? "level_0200_finished" : "CgkI1IvonNkDEAIQCQ"},
                {AchievementKeys.Level300Finished, ios ? "level_0300_finished" : "CgkI1IvonNkDEAIQCg"},
                {AchievementKeys.Level400Finished, ios ? "level_0400_finished" : "CgkI1IvonNkDEAIQCw"},
                {AchievementKeys.Level500Finished, ios ? "level_0500_finished" : "CgkI1IvonNkDEAIQDA"},
                {AchievementKeys.Level600Finished, ios ? "level_0600_finished" : "CgkI1IvonNkDEAIQDQ"},
                {AchievementKeys.Level700Finished, ios ? "level_0700_finished" : "CgkI1IvonNkDEAIQDg"},
                {AchievementKeys.Level800Finished, ios ? "level_0800_finished" : "CgkI1IvonNkDEAIQDw"},
                {AchievementKeys.Level900Finished, ios ? "level_0900_finished" : "CgkI1IvonNkDEAIQEA"},
                {AchievementKeys.Level1000Finished, ios ? "level_1000_finished" : "CgkI1IvonNkDEAIQEQ"},
            };
        }

        #endregion
    }
}