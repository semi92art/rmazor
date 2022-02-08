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
using RMAZOR.Controllers;
using RMAZOR.GameHelpers;
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
    
        private void Start()
        {
            Dbg.Log("Application started, platform: " + Application.platform);
            InitStartData();
            InitLogging();
            // yield return Cor.Delay(1f, null);
            InitGameManagers();
            InitDefaultData();
            SceneManager.sceneLoaded += OnSceneLoaded;
            Cor.Run(LoadSceneLevel());
            // yield return LoadSceneLevel();
        }

        private static IEnumerator LoadSceneLevel()
        {
            var @params = new LoadSceneParameters(LoadSceneMode.Single, LocalPhysicsMode.Physics2D);
            var op = SceneManager.LoadSceneAsync(SceneNames.Level, @params);
            while (!op.isDone)
                yield return null;
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
                var levelEntity = ScoreManager.GetSavedGameProgress(CommonData.SavedGameFileName, false);
                Cor.Run(Cor.WaitWhile(
               () => levelEntity.Result == EEntityResult.Pending,
               () =>
               {
                   if (levelEntity.Result == EEntityResult.Fail || levelEntity.Value == null)
                   {
                       levelEntity = ScoreManager.GetSavedGameProgress(CommonData.SavedGameFileName, true);
                       Cor.Run(Cor.WaitWhile(
                           () => levelEntity.Result == EEntityResult.Pending,
                           () =>
                           {
                               if (levelEntity.Result == EEntityResult.Fail || levelEntity.Value == null)
                               {
                                   var savedGame = new SavedGame
                                   {
                                       FileName = nameof(CommonData.SavedGameFileName),
                                       Level = 0,
                                       Money = 100
                                   };
                                   ScoreManager.SaveGameProgress(savedGame, true);
                                   LoadLevelByIndex(controller, 0);
                                   return;
                               }
                               long idx = levelEntity.Value.CastTo<SavedGame>().Level;
                               LoadLevelByIndex(controller, idx);
                           }));
                       return;
                   }
                   long levelIndex = levelEntity.Value.CastTo<SavedGame>().Level;
                   LoadLevelByIndex(controller, levelIndex);
               }));
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
            if (!SaveUtils.GetValue(SaveKeysRmazor.SavedGameFromServerLoadedAtLeastOnce))
                OnScoreManagerInitializeFirstLaunch();
        }

        private void OnScoreManagerInitializeFirstLaunch()
        {
            var savedGameCachedEntity = ScoreManager.GetSavedGameProgress(
                CommonData.SavedGameFileName, 
                true);
            var savedGameServerEntity = ScoreManager.GetSavedGameProgress(
                CommonData.SavedGameFileName, 
                false);
            Cor.Run(Cor.WaitWhile(
                () =>
                    savedGameCachedEntity.Result == EEntityResult.Pending,
                () =>
                {
                    Cor.Run(Cor.WaitWhile(() => savedGameServerEntity.Result == EEntityResult.Pending,
                        () =>
                        {
       
                            if (savedGameServerEntity.Result == EEntityResult.Fail)
                            {
                                Dbg.LogWarning("Failed to load money from server");
                                return;
                            }
                            var savedGameServer = savedGameServerEntity.Value.CastTo<SavedGame>();
                            long moneyServer = savedGameServer.Money;
                            long levelServer = savedGameServer.Level;
                            if (savedGameCachedEntity.Result == EEntityResult.Fail)
                            {
                                Dbg.LogWarning("Failed to load money from cache");
                                return;
                            }
                            var savedGameCached = savedGameCachedEntity.Value.CastTo<SavedGame>();
                            long moneyCached = savedGameCached.Money;
                            long levelCached = savedGameCached.Money;
                            if (moneyServer > moneyCached)
                            {
                                Dbg.Log("getting money from server");
                                var newSavedGame = new SavedGame
                                {
                                    FileName = CommonData.SavedGameFileName,
                                    Money = moneyServer,
                                    Level = levelServer
                                };
                                ScoreManager.SaveGameProgress(newSavedGame, true);
                            }
                            else
                            {
                                Dbg.Log("getting money from cache");
                                var newSavedGame = new SavedGame
                                {
                                    FileName = CommonData.SavedGameFileName,
                                    Money = moneyCached,
                                    Level = levelCached
                                };
                                ScoreManager.SaveGameProgress(newSavedGame, false);
                            }
                            SaveUtils.PutValue(SaveKeysRmazor.SavedGameFromServerLoadedAtLeastOnce, true);
                        }));
                }));
        }
        
        // private void OnScoreManagerInitializeNotFirstLaunch()
        // {
        //     var savedGameCache = ScoreManager.GetSavedGameProgress(
        //         CommonData.SavedGameFileName, 
        //         true);
        //     Cor.Run(Cor.WaitWhile(
        //         () => savedGameCache.Result == EEntityResult.Pending,
        //         () =>
        //         {
        //             long moneyCache = savedGameCache.Value.CastTo<SavedGame>().Money;
        //             if (savedGameCache.Result == EEntityResult.Fail)
        //             {
        //                 Dbg.LogWarning("Failed to load money from cache");
        //                 return;
        //             }
        //             var newSavedGame = new SavedGame
        //             {
        //                 FileName = CommonData.SavedGameFileName,
        //                 Money = moneyCache
        //             };
        //             ScoreManager.SaveGameProgress(newSavedGame, false);
        //         }));
        // }

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

        private void InitStartData()
        {
            // FIXME костыль: если на iOS стоит светлая тема, задник камеры автоматом ставится белым
            CameraProvider.MainCamera.backgroundColor = Color.black; 
            SaveUtils.PutValue(SaveKeysCommon.AppVersion, Application.version);
            Application.targetFrameRate = GraphicUtils.GetTargetFps();
            CommonData.Release = true;
        }

        private void InitLogging()
        {
            Dbg.LogLevel = Settings.LogLevel;
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
                "CgkI1IvonNkDEAIQBg" : "level";
            return new List<LeaderBoardIdKeyPair>
            {
                new LeaderBoardIdKeyPair(DataFieldIds.Level, levelLbKey)
            };
        }
        
        // private void InitDefaultDataFieldValues()
        // {
        //     var savedGame = new SavedGame
        //     {
        //         FileName = CommonData.SavedGameFileName,
        //         Money = 100
        //     };
        //     const int accId = GameClientUtils.DefaultAccountId;
        //     var df = new GameDataField(
        //         GameClient,
        //         savedGame,
        //         accId,
        //         1,
        //         (ushort) CommonUtils.StringToHash(CommonData.SavedGameFileName));
        //     df.Save(true);
        // }

        #endregion
    }
}