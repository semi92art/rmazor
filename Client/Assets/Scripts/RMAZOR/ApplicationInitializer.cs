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
        #region nonpublic members

        private bool m_ScoreManInited;

        #endregion
        
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
            Dbg.Log("Application started");
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
                var levelEntity = ScoreManager.GetScore(DataFieldIds.Level, true);
                Cor.Run(Cor.WaitWhile(
               () => levelEntity.Result == EEntityResult.Pending,
               () =>
               {
                   var levelIndex = levelEntity.GetFirstScore();
                   if (levelEntity.Result == EEntityResult.Fail
                       || !levelIndex.HasValue)
                   {
                       ScoreManager.SetScore(DataFieldIds.Level, 0, true);
                       LoadLevelByIndex(controller, 0);
                       return;
                   }
                   LoadLevelByIndex(controller, (int)levelIndex.Value);
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
            if (m_ScoreManInited)
                return;
            m_ScoreManInited = true;
            const ushort id = DataFieldIds.Money;
            var moneyEntityServer = ScoreManager.GetScore(id, false);
            var moneyEntityCache = ScoreManager.GetScore(id, true);
            Cor.Run(Cor.WaitWhile(
           () =>
               moneyEntityServer.Result == EEntityResult.Pending
               || moneyEntityCache.Result == EEntityResult.Pending,
           () =>
           {
               var moneyServer = moneyEntityServer.GetFirstScore();
               if (moneyEntityServer.Result == EEntityResult.Fail
                   || !moneyServer.HasValue)
               {
                   Dbg.LogWarning("Failed to load money from server");
                   return;
               }
               var moneyCache = moneyEntityCache.GetFirstScore();
               if (moneyEntityCache.Result == EEntityResult.Fail
                   || !moneyCache.HasValue)
               {
                   Dbg.LogWarning("Failed to load money from cache");
                   return;
               }

               if (moneyServer.Value > moneyCache.Value)
                   ScoreManager.SetScore(id, moneyServer.Value, true);
               else if (moneyServer.Value < moneyCache.Value)
                   ScoreManager.SetScore(id, moneyCache.Value, false);
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
            Dbg.Log(nameof(InitDebugging));
            if (!Settings.DebugEnabled)
                return;
            CommonUtils.InitSRDebugger();
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
                new ProductInfo(PurchaseKeys.Money1, $"small_pack_of_coins{suffix}",           ptCons),
                new ProductInfo(PurchaseKeys.Money2, $"medium_pack_of_coins{suffix}",          ptCons),
                new ProductInfo(PurchaseKeys.Money3, $"big_pack_of_coins{suffix}",             ptCons),
                new ProductInfo(PurchaseKeys.NoAds,  $"disable_mandatory_advertising{suffix}", ptNonCons)
            };
        }

        #endregion
    }
}