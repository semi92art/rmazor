using System.Collections;
using System.Collections.Generic;
using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Entities;
using Common.Enums;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Managers.Advertising;
using Common.Managers.IAP;
using Common.Managers.Scores;
using Common.Network;
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
    public class ApplicationInitializer : MonoBehaviour
    {
        #region inject
    
        private CommonGameSettings    Settings             { get; set; }
        private IGameClient           GameClient           { get; set; }
        private IAdsManager           AdsManager           { get; set; }
        private IAnalyticsManager     AnalyticsManager     { get; set; }
        private ILocalizationManager  LocalizationManager  { get; set; }
        private ILevelsLoader         LevelsLoader         { get; set; }
        private IScoreManager         ScoreManager         { get; set; }
        private IHapticsManager       HapticsManager       { get; set; }
        private IShopManager          ShopManager          { get; set; }
        private IRemoteConfigManager  RemoteConfigManager  { get; set; }
        private IPermissionsRequester PermissionsRequester { get; set; }

        [Inject] 
        public void Inject(
            CommonGameSettings    _Settings,
            IGameClient           _GameClient,
            IAdsManager           _AdsManager,
            IAnalyticsManager     _AnalyticsManager,
            ILocalizationManager  _LocalizationManager,
            ILevelsLoader         _LevelsLoader,
            IScoreManager         _ScoreManager,
            IHapticsManager       _HapticsManager,
            IAssetBundleManager   _AssetBundleManager,
            IShopManager          _ShopManager,
            IRemoteConfigManager  _RemoteConfigManager,
            ICameraProvider       _CameraProvider,
            IPermissionsRequester _PermissionsRequester,
            CompanyLogo           _CompanyLogo)
        {
            Settings             = _Settings;
            GameClient           = _GameClient;
            AdsManager           = _AdsManager;
            AnalyticsManager     = _AnalyticsManager;
            LocalizationManager  = _LocalizationManager;
            LevelsLoader         = _LevelsLoader;
            ScoreManager         = _ScoreManager;
            HapticsManager       = _HapticsManager;
            ShopManager          = _ShopManager;
            RemoteConfigManager  = _RemoteConfigManager;
            PermissionsRequester = _PermissionsRequester;
        }


        #endregion
    
        #region engine methods
    
        private IEnumerator Start()
        {
            Dbg.Log("Application started, platform: " + Application.platform);
            yield return Cor.Delay(0.5f, null); // для более плавной загрузки логотипа компании
            var permissionsEntity = PermissionsRequester.RequestPermissions();
            while (permissionsEntity.Result == EEntityResult.Pending)
                yield return null;
            InitStartData();
            InitLogging();
            InitGameManagers();
            InitDefaultData();
            SceneManager.sceneLoaded += OnSceneLoaded;
            yield return LoadSceneLevel();
        }

        private static IEnumerator LoadSceneLevel()
        {
            var @params = new LoadSceneParameters(LoadSceneMode.Single);
            var op =  SceneManager.LoadSceneAsync(SceneNames.Level, @params);
            while (!op.isDone)
                yield return null;
        }
    
        private void OnSceneLoaded(Scene _Scene, LoadSceneMode _Mode)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (!_Scene.name.EqualsIgnoreCase(SceneNames.Level)) 
                return;
            Cor.Run(Cor.WaitWhile(
                () => !RemoteConfigManager.Initialized,
                () =>
                {
                    GameClientUtils.GameId = 1; // если игра будет только одна, то и париться с GameId нет смысла
                    InitGameController();
                }));
        }

        #endregion
    
        #region nonpublic methods
    
        private void InitGameManagers()
        {
            RemoteConfigManager.Initialize += AdsManager.Init;
            RemoteConfigManager.Init();
            ShopManager.RegisterProductInfos(GetProductInfos());
            ShopManager        .Init();
            ScoreManager.RegisterLeaderboards(GetLeaderBoardIdKeyPairs());
            ScoreManager       .Initialize += OnScoreManagerInitialize;
            ScoreManager       .Init();
            GameClient         .Init();
            AnalyticsManager   .Init();
            LocalizationManager.Init();
            HapticsManager     .Init();
        }

        private void InitGameController()
        {
            var controller = GameController.CreateInstance();
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
            var info = LevelsLoader.LoadLevel(1, _LevelIndex);
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
            SaveUtils.PutValue(SaveKeysCommon.SettingSoundOn, true);
            SaveUtils.PutValue(SaveKeysCommon.SettingMusicOn, true);
            SaveUtils.PutValue(SaveKeysCommon.SettingHapticsOn, true);
            SaveUtils.PutValue(SaveKeysCommon.NotFirstLaunch, true);
            SetDefaultLanguage();
        }

        private static void InitStartData()
        {
            SaveUtils.PutValue(SaveKeysCommon.AppVersion, Application.version);
            Application.targetFrameRate = GraphicUtils.GetTargetFps();
            CommonData.Release = true;
        }

        private void InitLogging()
        {
            Dbg.LogLevel = Settings.logLevel;
        }

        private void SetDefaultLanguage()
        {
            var language = Language.English;
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Russian:
                case SystemLanguage.Belarusian:
                case SystemLanguage.Ukrainian:
                    language = Language.Russian;
                    LocalizationManager.SetLanguage(Language.Russian);
                    break;
                case SystemLanguage.Spanish:
                    language = Language.Spanish;
                    LocalizationManager.SetLanguage(Language.Spanish);
                    break;
                case SystemLanguage.Portuguese:
                    language = Language.Portugal;
                    LocalizationManager.SetLanguage(Language.Portugal);
                    break;
            }
            LocalizationManager.SetLanguage(language);
        }
        
        private static List<ProductInfo> GetProductInfos()
        {
            string suffix = Application.platform == RuntimePlatform.Android ? string.Empty : "_2";
            const ProductType ptCons = ProductType.Consumable;
            const ProductType ptNonCons = ProductType.NonConsumable;
            return new List<ProductInfo>
            {
                new ProductInfo(PurchaseKeys.Money1,    $"small_pack_of_coins{suffix}",           ptCons),
                new ProductInfo(PurchaseKeys.Money2,    $"medium_pack_of_coins{suffix}",          ptCons),
                new ProductInfo(PurchaseKeys.Money3,    $"big_pack_of_coins{suffix}",             ptCons),
                new ProductInfo(PurchaseKeys.NoAds,     $"disable_mandatory_advertising{suffix}", ptNonCons),
                // new ProductInfo(PurchaseKeys.DarkTheme, $"dark_theme{suffix}",                    ptNonCons)
            };
        }

        private static List<LeaderBoardIdKeyPair> GetLeaderBoardIdKeyPairs()
        {
            string levelLbKey = Application.platform == RuntimePlatform.Android ?
                "CgkI1IvonNkDEAIQBg" : "level";
            return new List<LeaderBoardIdKeyPair>
            {
                new LeaderBoardIdKeyPair(DataFieldIds.Level, levelLbKey)
            };
        }

        #endregion
    }
}