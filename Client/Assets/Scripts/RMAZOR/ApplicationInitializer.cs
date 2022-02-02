using System.Collections;
using System.Collections.Generic;
using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Entities;
using Common.Enums;
using Common.Extensions;
using Common.Network;
using Common.Utils;
using GameHelpers;
using Managers;
using Managers.Advertising;
using Managers.IAP;
using Managers.Scores;
using Mono_Installers;
using RMAZOR.Controllers;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.SceneManagement;
using Zenject;

namespace RMAZOR
{
    public class ApplicationInitializer : MonoBehaviour
    {
        #region inject
    
        private CommonGameSettings   Settings            { get; set; }
        private IGameClient          GameClient          { get; set; }
        private IAdsManager          AdsManager          { get; set; }
        private IAnalyticsManager    AnalyticsManager    { get; set; }
        private ILocalizationManager LocalizationManager { get; set; }
        private ILevelsLoader        LevelsLoader        { get; set; }
        private IScoreManager        ScoreManager        { get; set; }
        private IHapticsManager      HapticsManager      { get; set; }
        private IShopManager         ShopManager         { get; set; }
        private IRemoteConfigManager RemoteConfigManager { get; set; }
        private ICameraProvider      CameraProvider      { get; set; }

        [Inject] 
        public void Inject(
            CommonGameSettings   _Settings,
            IGameClient          _GameClient,
            IAdsManager          _AdsManager,
            IAnalyticsManager    _AnalyticsManager,
            ILocalizationManager _LocalizationManager,
            ILevelsLoader        _LevelsLoader,
            IScoreManager        _ScoreManager,
            IHapticsManager      _HapticsManager,
            IAssetBundleManager  _AssetBundleManager,
            IShopManager         _ShopManager,
            IRemoteConfigManager _RemoteConfigManager,
            ICameraProvider      _CameraProvider)
        {
            Settings            = _Settings;
            GameClient          = _GameClient;
            AdsManager          = _AdsManager;
            AnalyticsManager    = _AnalyticsManager;
            LocalizationManager = _LocalizationManager;
            LevelsLoader        = _LevelsLoader;
            ScoreManager        = _ScoreManager;
            HapticsManager      = _HapticsManager;
            ShopManager         = _ShopManager;
            RemoteConfigManager = _RemoteConfigManager;
            CameraProvider      = _CameraProvider;
        }


        #endregion
    
        #region engine methods
    
        private IEnumerator Start()
        {
            Dbg.Log("Application started, platform: " + Application.platform);
            // костыль: если на iOS стоит светлая тема, задник камеры автоматом ставится белым
            CameraProvider.MainCamera.backgroundColor = Color.black; 
            yield return Cor.Delay(1f, null);
            SaveUtils.PutValue(SaveKeys.EnableRotation, true);
            InitLogging();
            Application.targetFrameRate = GraphicUtils.GetTargetFps();
            InitGameManagers();
            InitDefaultData();
            LevelMonoInstaller.Release = true;
            SceneManager.sceneLoaded += OnSceneLoaded;
            yield return LoadSceneLevel();
        }

        private static IEnumerator LoadSceneLevel()
        {
            var @params = new LoadSceneParameters(LoadSceneMode.Single, LocalPhysicsMode.Physics2D);
            var op = SceneManager.LoadSceneAsync(SceneNames.Level, @params);
            while (!op.isDone)
                yield return null;
            Dbg.Log("Level loaded");
        }
    
        private void OnSceneLoaded(Scene _Scene, LoadSceneMode _Mode)
        {
            if (_Scene.name.EqualsIgnoreCase(SceneNames.Level))
            {
                GameClientUtils.GameId = 1; // если игра будет только одна, то и париться с GameId нет смысла
                InitGameController();
            }
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        #endregion
    
        #region nonpublic methods
    
        private void InitGameManagers()
        {
            RemoteConfigManager.Initialize += AdsManager.Init;
            RemoteConfigManager.Initialize += InitDebugging;
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
                var levelEntity = ScoreManager.GetSavedGameProgress(nameof(DataFieldIds.Level), true);
                Cor.Run(Cor.WaitWhile(
               () => levelEntity.Result == EEntityResult.Pending,
               () =>
               {
                   if (levelEntity.Result == EEntityResult.Fail || levelEntity.Value == null)
                   {
                       var levelArgs = new LevelArgs
                       {
                           FileName = nameof(DataFieldIds.Level),
                           Level = 0
                       };
                       ScoreManager.SaveGameProgress(levelArgs, true);
                       LoadLevelByIndex(controller, 0);
                       return;
                   }
                   int levelIndex = levelEntity.Value.CastTo<LevelArgs>().Level;
                   LoadLevelByIndex(controller, levelIndex);
               }));
            };
            controller.Init();
        }

        private void LoadLevelByIndex(IGameController _Controller, int _LevelIndex)
        {
            var info = LevelsLoader.LoadLevel(1, _LevelIndex);
            _Controller.Model.LevelStaging.LoadLevel(info, _LevelIndex);
        }
        
        private void OnScoreManagerInitialize()
        {
            if (SaveUtils.GetValue(SaveKeys.MoneyFromServerLoadedFirstTime))
                OnScoreManagerInitializeNotFirstLaunch();
            else 
                OnScoreManagerInitializeFirstLaunch();
        }

        private void OnScoreManagerInitializeFirstLaunch()
        {
            Dbg.Log(nameof(OnScoreManagerInitializeFirstLaunch) + " " + 1);
            var savedGameCache = ScoreManager.GetSavedGameProgress(
                CommonData.SavedGameFileName, 
                true);
            var savedGameServer = ScoreManager.GetSavedGameProgress(
                CommonData.SavedGameFileName, 
                false);
            Dbg.Log(nameof(OnScoreManagerInitializeFirstLaunch) + " " + 2);
            Cor.Run(Cor.WaitWhile(
                () =>
                    savedGameCache.Result == EEntityResult.Pending,
                () =>
                {
                    Dbg.Log(nameof(OnScoreManagerInitializeFirstLaunch) + " " + 3);
                    Cor.Run(Cor.WaitWhile(() => savedGameServer.Result == EEntityResult.Pending,
                        () =>
                        {
                            Dbg.Log(nameof(OnScoreManagerInitializeFirstLaunch) + " " + 4);
                            long moneyServer = savedGameServer.Value.CastTo<MoneyArgs>().Money;
                            if (savedGameServer.Result == EEntityResult.Fail)
                            {
                                Dbg.LogWarning("Failed to load money from server");
                                return;
                            }
                            long moneyCache = savedGameCache.Value.CastTo<MoneyArgs>().Money;
                            if (savedGameCache.Result == EEntityResult.Fail)
                            {
                                Dbg.LogWarning("Failed to load money from cache");
                                return;
                            }
                            if (moneyServer > moneyCache)
                            {
                                Dbg.Log("getting money from server");
                                var newSavedGame = new MoneyArgs
                                {
                                    FileName = CommonData.SavedGameFileName,
                                    Money = moneyServer
                                };
                                ScoreManager.SaveGameProgress(newSavedGame, true);
                            }
                            else
                            {
                                Dbg.Log("getting money from cache");
                                var newSavedGame = new MoneyArgs
                                {
                                    FileName = CommonData.SavedGameFileName,
                                    Money = moneyCache
                                };
                                ScoreManager.SaveGameProgress(newSavedGame, false);
                            }
                            SaveUtils.PutValue(SaveKeys.MoneyFromServerLoadedFirstTime, true);
                        }));
                }));
        }
        
        private void OnScoreManagerInitializeNotFirstLaunch()
        {
            var savedGameCache = ScoreManager.GetSavedGameProgress(
                CommonData.SavedGameFileName, 
                true);
            Cor.Run(Cor.WaitWhile(
                () => savedGameCache.Result == EEntityResult.Pending,
                () =>
                {
                    long moneyCache = savedGameCache.Value.CastTo<MoneyArgs>().Money;
                    if (savedGameCache.Result == EEntityResult.Fail)
                    {
                        Dbg.LogWarning("Failed to load money from cache");
                        return;
                    }
                    var newSavedGame = new MoneyArgs
                    {
                        FileName = CommonData.SavedGameFileName,
                        Money = moneyCache
                    };
                    ScoreManager.SaveGameProgress(newSavedGame, false);
                }));
        }

        private void InitDefaultData()
        {
            if (SaveUtils.GetValue(SaveKeysCommon.NotFirstLaunch))
                return;
            Dbg.Log(nameof(InitDefaultData));
            DataFieldsMigrator.InitDefaultDataFieldValues(GameClient);
            SaveUtils.PutValue(SaveKeys.SettingSoundOn, true);
            SaveUtils.PutValue(SaveKeys.SettingMusicOn, true);
            SaveUtils.PutValue(SaveKeys.SettingHapticsOn, true);
            SaveUtils.PutValue(SaveKeysCommon.NotFirstLaunch, true);
            SetDefaultLanguage();
        }

        private void InitLogging()
        {
            Dbg.LogLevel = Settings.LogLevel;
        }

        private void InitDebugging()
        {
            if (!Settings.DebugEnabled)
                return;
            SRDebug.Init();
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
                new ProductInfo(PurchaseKeys.DarkTheme, $"dark_theme{suffix}",                    ptNonCons)
            };
        }

        private static List<LeaderBoardIdKeyPair> GetLeaderBoardIdKeyPairs()
        {
            string levelLbKey = Application.platform == RuntimePlatform.Android ?
                "CgkI1IvonNkDEAIQBg" : "levels";
            return new List<LeaderBoardIdKeyPair>
            {
                new LeaderBoardIdKeyPair(DataFieldIds.Level, levelLbKey)
            };
        }

        #endregion
    }
}