using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Helpers;
using Common.Managers;
using Common.Managers.Advertising;
using Common.Managers.Notifications;
using Common.Managers.PlatformGameServices;
using mazing.common.Runtime;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Managers.IAP;
using mazing.common.Runtime.Network;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Controllers;
using RMAZOR.Helpers;
using RMAZOR.Managers;
using RMAZOR.Views.UI.Game_Logo;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using UnityEngine.SceneManagement;
using Zenject;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR
{
    public class ApplicationInitializer : MonoBehaviour
    {
        #region inject

        private GlobalGameSettings         GlobalGameSettings        { get; set; }
        private IRemotePropertiesCommon    RemoteProperties          { get; set; }
        private IGameClient                GameClient                { get; set; }
        private IAdsManager                AdsManager                { get; set; }
        private ILocalizationManager       LocalizationManager       { get; set; }
        private ILevelsLoader              LevelsLoader              { get; set; }
        private IScoreManager              ScoreManager              { get; set; }
        private IHapticsManager            HapticsManager            { get; set; }
        private IShopManager               ShopManager               { get; set; }
        private IRemoteConfigManager       RemoteConfigManager       { get; set; }
        private IPermissionsRequester      PermissionsRequester      { get; set; }
        private IAssetBundleManager        AssetBundleManager        { get; set; }
        private IAchievementsSet           AchievementsSet           { get; set; }
        private ILeaderboardsSet           LeaderboardsSet           { get; set; }
        private IAnalyticsManager          AnalyticsManager          { get; set; }
        private IApplicationVersionUpdater ApplicationVersionUpdater { get; set; }
        private IPushNotificationsProvider PushNotificationsProvider { get; set; }
        private IViewUIGameLogo            GameLogo                  { get; set; }
        private IFirebaseInitializer       FirebaseInitializer       { get; set; }

        [Inject] 
        private void Inject(
            GlobalGameSettings         _GameSettings,
            IRemotePropertiesCommon    _RemoteProperties,
            IGameClient                _GameClient,
            IAdsManager                _AdsManager,
            ILocalizationManager       _LocalizationManager,
            ILevelsLoader              _LevelsLoader,
            IScoreManager              _ScoreManager,
            IHapticsManager            _HapticsManager,
            IAssetBundleManager        _AssetBundleManager,
            IShopManager               _ShopManager,
            IRemoteConfigManager       _RemoteConfigManager,
            IPermissionsRequester      _PermissionsRequester,
            ICommonTicker              _CommonTicker,
            IAchievementsSet           _AchievementsSet,
            ILeaderboardsSet           _LeaderboardsSet,
            IApplicationVersionUpdater _ApplicationVersionUpdater,
            IAnalyticsManager          _AnalyticsManager,
            IPushNotificationsProvider _PushNotificationsProvider,
            IViewUIGameLogo            _GameLogo,
            IFirebaseInitializer       _FirebaseInitializer)
        {
            GlobalGameSettings        = _GameSettings;
            RemoteProperties          = _RemoteProperties;
            GameClient                = _GameClient;
            AdsManager                = _AdsManager;
            LocalizationManager       = _LocalizationManager;
            LevelsLoader              = _LevelsLoader;
            ScoreManager              = _ScoreManager;
            HapticsManager            = _HapticsManager;
            AssetBundleManager        = _AssetBundleManager;
            ShopManager               = _ShopManager;
            RemoteConfigManager       = _RemoteConfigManager;
            PermissionsRequester      = _PermissionsRequester;
            AchievementsSet           = _AchievementsSet;
            LeaderboardsSet           = _LeaderboardsSet;
            ApplicationVersionUpdater = _ApplicationVersionUpdater;
            AnalyticsManager          = _AnalyticsManager;
            PushNotificationsProvider = _PushNotificationsProvider;
            GameLogo                  = _GameLogo;
            FirebaseInitializer       = _FirebaseInitializer;
        }
        
        #endregion
    
        #region engine methods

        private IEnumerator Start()
        {
            FirebaseInitializer.Init();
            yield return UpdateTodaySessionsCountCoroutine();
            ApplicationVersionUpdater.UpdateToCurrentVersion();
            yield return SetGameId();
            yield return LogAppInfoCoroutine();
            yield return PermissionsRequestCoroutine();
            yield return InitStartDataCoroutine();
            yield return InitGameManagersCoroutine();
            yield return LoadGameSceneCoroutine();
        }

        private IEnumerator PermissionsRequestCoroutine()
        {
            var permissionsEntity = PermissionsRequester.RequestPermissions();
            while (permissionsEntity.Result == EEntityResult.Pending)
                yield return new WaitForEndOfFrame();
        }

        private static IEnumerator SetGameId()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.name == SceneNames.Preload)
                CommonData.GameId = GameIds.RMAZOR;
            yield return null;
        }

        private static IEnumerator LogAppInfoCoroutine()
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
            yield return null;
        }

        private IEnumerator LoadGameSceneCoroutine()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            yield return WaitWhile(
                () => !FirebaseInitializer.Initialized,
                () =>
                {
                    // SceneManager.LoadScene(SceneNames.Level);
                    var @params = new LoadSceneParameters(LoadSceneMode.Single);
                    SceneManager.LoadSceneAsync(SceneNames.Level, @params);
                }, 3f);
        }

        #endregion
    
        #region nonpublic methods

        private IEnumerator UpdateTodaySessionsCountCoroutine()
        {
            var today = DateTime.Now.Date;
            var dict = SaveUtils.GetValue(SaveKeysRmazor.SessionCountByDays) 
                       ?? new Dictionary<DateTime, int>();
            int sessionsCount = dict.GetSafe(today, out _);
            sessionsCount++;
            dict.SetSafe(today, sessionsCount);
            SaveUtils.PutValue(SaveKeysRmazor.SessionCountByDays, dict);
            yield return null;
        }
        
        private void OnSceneLoaded(Scene _Scene, LoadSceneMode _Mode)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            GameLogo.Init();
            Cor.Run(InitGameControllerCoroutine());
        }
        
        private IEnumerator InitGameControllerCoroutine()
        {
            const float waitingTime = 3f;
            yield return Cor.WaitWhile(
                () => !RemoteConfigManager.Initialized || !AssetBundleManager.Initialized,
                () =>
                {
                    LevelsLoader.Initialize += GameControllerMVC.CreateInstance().Init;
                    LevelsLoader.Init();
                }, _Seconds: waitingTime);
        }

        private static IEnumerator WaitWhile(
            Func<bool>  _Predicate,
            UnityAction _Action,
            float       _Seconds)
        {
            float time = Time.time;
            bool FinalPredicate() => _Predicate() && (time + _Seconds > Time.time);
            while (FinalPredicate())
                yield return null;
            _Action();
        }
    
        private IEnumerator InitGameManagersCoroutine()
        {
            yield return null;
            AnalyticsManager.Initialize += () => AnalyticsManager.SendAnalytic(AnalyticIds.SessionStart);
            InitAdsManager();
            InitRemoteConfigManager();
            InitAssetBundleManager();
            InitShopManager();
            InitScoreManager();
            InitGameClient();
            InitLocalizationManager();
            InitPushNotificationsProvider();
        }

        private void InitAdsManager()
        {
            TryExecute( AdsManager.Init);
        }

        private void InitRemoteConfigManager()
        {
            TryExecute(() =>
            {
                RemoteConfigManager.Initialize += () => RemoteProperties.DebugEnabled |= GlobalGameSettings.debugAnyway;
                RemoteConfigManager.Initialize += AdsManager.Init;
                RemoteConfigManager.Initialize += HapticsManager.Init;
                RemoteConfigManager.Init();
            });
        }

        private void InitAssetBundleManager()
        {
            TryExecute(AssetBundleManager.Init);
        }

        private void InitShopManager()
        {
            TryExecute(() =>
            {
                
                ShopManager.RegisterProductInfos(GetProductInfos());
                ShopManager.Init();
            });
        }

        private void InitScoreManager()
        {
            TryExecute(() =>
            {
                ScoreManager.RegisterLeaderboardsSet(LeaderboardsSet.GetSet());
                ScoreManager.RegisterAchievementsSet(AchievementsSet.GetSet());
                ScoreManager.Init();
            });
        }

        private void InitGameClient()
        {
            TryExecute(GameClient.Init);
        }

        private void InitLocalizationManager()
        {
            TryExecute(LocalizationManager.Init);
        }

        private void InitPushNotificationsProvider()
        {
            TryExecute(PushNotificationsProvider.Init);
        }

        private static void TryExecute(UnityAction _Action)
        {
            try
            {
                _Action?.Invoke();
            }
            catch (Exception ex)
            {
                Dbg.LogError(ex);
            }
        }

        private void OnScoreManagerInitialize()
        {
            var savedGame = ScoreManager.GetSavedGame(MazorCommonData.SavedGameFileName);
            if (savedGame != null)
                return;
            savedGame = new SavedGameV2
            {
                Arguments = new Dictionary<string, object>
                {
                    {KeyLevelIndexMainLevels,   0},
                    {KeyLevelIndexPuzzleLevels, 0},
                    {KeyLevelIndexBigLevels,    0},
                    {KeyMoneyCount,             0}
                }
            };
            ScoreManager.SaveGame(savedGame);
        }
        
        private IEnumerator InitStartDataCoroutine()
        {
            ScoreManager.Initialize += OnScoreManagerInitialize;
            MazorCommonData.Release = true;
            SaveUtils.PutValue(SaveKeysMazor.AppVersion, Application.version);
            Application.targetFrameRate = GraphicUtils.GetTargetFps();
            Dbg.LogLevel = GlobalGameSettings.logLevel;
            if (SaveUtils.GetValue(SaveKeysMazor.NotFirstLaunch))
                yield break;
            SaveUtils.PutValue(SaveKeysCommon.SettingSoundOn,         true);
            SaveUtils.PutValue(SaveKeysCommon.SettingMusicOn,         true);
            SaveUtils.PutValue(SaveKeysCommon.SettingNotificationsOn, true);
            SaveUtils.PutValue(SaveKeysCommon.SettingHapticsOn,       true);
            SaveUtils.PutValue(SaveKeysMazor .NotFirstLaunch,         true);
            CommonUtils.DoOnInitializedEx(LocalizationManager, SetDefaultLanguage);
        }

        private void SetDefaultLanguage()
        {
            if (Application.isEditor)
            {
                LocalizationManager.SetLanguage(ELanguage.English);
                return;
            }
            ELanguage lang = Application.systemLanguage switch
            {
                SystemLanguage.Russian    => ELanguage.Russian,
                SystemLanguage.Belarusian => ELanguage.Russian,
                SystemLanguage.Ukrainian  => ELanguage.Russian,
                SystemLanguage.German     => ELanguage.German,
                SystemLanguage.Spanish    => ELanguage.Spanish,
                SystemLanguage.Portuguese => ELanguage.Portugal,
                SystemLanguage.Japanese   => ELanguage.Japanese,
                SystemLanguage.Korean     => ELanguage.Korean,
                _                         => ELanguage.English
            };
            LocalizationManager.SetLanguage(lang);
        }
        
        private static List<ProductInfo> GetProductInfos()
        {
            string suffix = CommonUtils.Platform == RuntimePlatform.Android ? string.Empty : "_2";
            const ProductType cons    = ProductType.Consumable;
            const ProductType nonCons = ProductType.NonConsumable;
            return new List<ProductInfo>
            {
                new ProductInfo(PurchaseKeys.Money1,       $"small_pack_of_coins{suffix}",           cons),
                new ProductInfo(PurchaseKeys.Money2,       $"medium_pack_of_coins{suffix}",          cons),
                new ProductInfo(PurchaseKeys.Money3,       $"big_pack_of_coins{suffix}",             cons),
                new ProductInfo(PurchaseKeys.SpecialOffer, "special_offer",                          nonCons),
                new ProductInfo(PurchaseKeys.X2NewCoins,   "x2_new_coins",                           nonCons),
                new ProductInfo(PurchaseKeys.NoAds,        $"disable_mandatory_advertising{suffix}", nonCons),
            };
        }

        #endregion
    }
}