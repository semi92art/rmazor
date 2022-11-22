using Common;
using Common.CameraProviders;
using Common.Debugging;
using Common.Helpers;
using Common.Managers;
using Common.Managers.Advertising;
using Common.Managers.Advertising.AdBlocks;
using Common.Managers.Advertising.AdsProviders;
using Common.Managers.Analytics;
using Common.Managers.IAP;
using Common.Managers.Notifications;
using Common.Managers.PlatformGameServices;
using Common.Managers.PlatformGameServices.Achievements;
using Common.Managers.PlatformGameServices.GameServiceAuth;
using Common.Managers.PlatformGameServices.Leaderboards;
using Common.Managers.PlatformGameServices.SavedGames;
using Common.Managers.PlatformGameServices.SavedGames.RemoteSavedGameProviders;
using Common.Network;
using Common.Settings;
using Common.Ticker;
using RMAZOR;
using RMAZOR.Helpers;
using RMAZOR.Managers;
using RMAZOR.Settings;
using UnityEngine;
using Zenject;

namespace Mono_Installers
{
    public class GlobalMonoInstaller : MonoInstaller
    {
        public GameObject         companyLogo;
        public GlobalGameSettings globalGameSettings;
        public ModelSettings      modelSettings;
        public ViewSettings       viewSettings;
        public GameObject         debugConsoleView;
        public GameObject         debugConsoleViewFake;
        
        public override void InstallBindings()
        {
            BindSettings();
            BindTickers();
            BindAnalytics();
            BindAds();
            BindStoreAndGameServices();
            BindHaptics();
            BindPermissionsRequester();
            BindOther();
        }
        
        private void BindSettings()
        {
            Container.Bind(typeof(IRemotePropertiesRmazor), typeof(IRemotePropertiesCommon))
                .To<RemotePropertiesRmazor>()
                .AsSingle();
            Container.Bind<GlobalGameSettings>()  .FromScriptableObject(globalGameSettings) .AsSingle();
            Container.Bind<ModelSettings>()       .FromScriptableObject(modelSettings)      .AsSingle();
            Container.Bind<ViewSettings>()        .FromScriptableObject(viewSettings)       .AsSingle();
            
            Container.Bind<ISettingsGetter>()     .To<SettingsGetter>()                     .AsSingle();
            Container.Bind<ISoundSetting>()       .To<SoundSetting>()                       .AsSingle();
            Container.Bind<IMusicSetting>()       .To<MusicSetting>()                       .AsSingle();
            Container.Bind<INotificationSetting>().To<NotificationsSetting>()               .AsSingle();
            Container.Bind<IHapticsSetting>()     .To<HapticsSetting>()                     .AsSingle();
            Container.Bind<ILanguageSetting>()    .To<LanguageSetting>()                    .AsSingle();
            Container.Bind<IDebugSetting>()       .To<DebugSetting>()                       .AsSingle();
        }
        
        private void BindTickers()
        {
            Container.Bind<ICommonTicker>()             .To<CommonTicker>()                 .AsSingle();
            Container.Bind<IViewGameTicker>()           .To<ViewGameTicker>()               .AsSingle();
            Container.Bind<IModelGameTicker>()          .To<ModelGameTicker>()              .AsSingle();
            Container.Bind<IUITicker>()                 .To<UITicker>()                     .AsSingle();
            Container.Bind<ISystemTicker>()             .To<SystemTicker>()                 .AsSingle();
        }

        private void BindAnalytics()
        {
            var managerType = Application.isEditor ? typeof(AnalyticsManagerFake) : typeof(AnalyticsManager);
            Container.Bind<IAnalyticsManager>().To(managerType).AsSingle();
            Container.Bind<IAnalyticsProvidersSet>().To<AnalyticsProvidersSet>().AsSingle();
            Container.Bind<IUnityAnalyticsProvider>().To<UnityAnalyticsProvider>().AsSingle();
#if FIREBASE
            Container.Bind<IFirebaseAnalyticsProvider>().To<FirebaseAnalyticsProvider>().AsSingle();
#endif
#if APPODEAL_3
            Container.Bind<IAppodealAnalyticsProvider>().To<AppodealAnalyticsProvider>().AsSingle();
#endif
        }

        private void BindAds()
        {
            Container.Bind<IAdsManager>()               .To<AdsManager>()                   .AsSingle();
#if ADMOB_API
            Container.Bind<IAdMobAdsProvider>()         .To<AdMobAdsProvider>()             .AsSingle();
            Container.Bind<IAdMobInterstitialAd>()      .To<AdMobInterstitialAd>()          .AsSingle();
            Container.Bind<IAdMobRewardedAd>()          .To<AdMobRewardedAd>()              .AsSingle();
#endif
#if UNITY_ADS_API
            Container.Bind<IUnityAdsProvider>()         .To<UnityAdsProvider>()             .AsSingle();
            Container.Bind<IUnityAdsInterstitialAd>()   .To<UnityAdsInterstitialAd>()       .AsSingle();
            Container.Bind<IUnityAdsRewardedAd>()       .To<UnityAdsRewardedAd>()           .AsSingle();
#endif
#if APPODEAL_3
            Container.Bind<IAppodealAdsProvider>()      .To<AppodealAdsProvider>()          .AsSingle();
            Container.Bind<IAppodealInterstitialAd>()   .To<AppodealInterstitialAd>()       .AsSingle();
            Container.Bind<IAppodealRewardedAd>()       .To<AppodealRewardedAd>()           .AsSingle();
#endif
            Container.Bind<IAdsProvidersSet>()          .To<AdsProvidersSet>()              .AsSingle();
        }
        
        private void BindStoreAndGameServices()
        {
            if (Application.isEditor)
            {
                Container.Bind<IShopManager>()         .To<ShopManagerFake>()               .AsSingle();
                Container.Bind<ILeaderboardProvider>() .To<LeaderboardProviderFake>()       .AsSingle();
                Container.Bind<IPlatformGameServiceAuthenticator>().To<PlatformGameServiceAuthenticatorFake>().AsSingle();
            }
            else
            {
#if UNITY_ANDROID
                Container.Bind<IShopManager>()         .To<AndroidUnityIAPShopManager>()    .AsSingle();
                Container.Bind<ILeaderboardProvider>().To<LeaderboardProviderGooglePlayGames>().AsSingle();
                Container.Bind<IPlatformGameServiceAuthenticator>().To<PlatformGameServiceAuthenticatorGooglePlayGames>().AsSingle();
#elif UNITY_IOS || UNITY_IPHONE
            Container.Bind<IShopManager>()        .To<AppleUnityIAPShopManager>()        .AsSingle();
            Container.Bind<ILeaderboardProvider>() .To<LeaderboardProviderIos>()         .AsSingle();
            Container.Bind<IPlatformGameServiceAuthenticator>().To<PlatformGameServiceAuthenticatorIos>().AsSingle();
#endif
            }
            
            Container.Bind<IScoreManager>()             .To<ScoreManager>()                 .AsSingle();
            Container.Bind<IAchievementsProvider>()     .To<AchievementsProvider>()         .AsSingle();
            Container.Bind<IRemoteSavedGameProvider>()  .To<FakeRemoteSavedGameProvider>()  .AsSingle();
            Container.Bind<ILeaderboardsSet>()          .To<LeaderboardsSetRmazor>()        .AsSingle();
            Container.Bind<IAchievementsSet>()          .To<AchievementsSetRmazor>()        .AsSingle();
        }

        private void BindHaptics()
        {
#if NICE_VIBRATIONS_3_9
            Container.Bind<IHapticsManager>()           .To<HapticsManagerNiceVibrations_3_9>().AsSingle();
#else
            Container.Bind<IHapticsManager>()           .To<HapticsManagerNiceVibrations_4_1>().AsSingle();
#endif
        }

        private void BindPermissionsRequester()
        {
            if (Application.isEditor)
            {
                Container.Bind<IPermissionsRequester>() .To<FakePermissionsRequester>().AsSingle();
            }
            else
            {
#if UNITY_ANDROID
                Container.Bind<IPermissionsRequester>().To<FakePermissionsRequester>() .AsSingle();
#elif UNITY_IOS || UNITY_IPHONE
                Container.Bind<IPermissionsRequester>().To<IosPermissionsRequester>()  .AsSingle();
#endif
            }
        }

        private void BindOther()
        {
            Container.Bind<CompanyLogo>()                  .FromComponentInNewPrefab(companyLogo) .AsSingle();
            Container.Bind<IGameClient>()                  .To<GameClient>()                      .AsSingle();
            Container.Bind<ISavedGameProvider>()           .To<SavedGamesProvider>()              .AsSingle();
            Container.Bind<ILocalizationManager>()         .To<LeanLocalizationManager>()         .AsSingle();
            Container.Bind<IPrefabSetManager>()            .To<PrefabSetManager>()                .AsSingle();
            Container.Bind<ILevelsLoader>()                .To<LevelsLoaderRmazor>()              .AsSingle();
            Container.Bind<IMazeInfoValidator>()           .To<MazeInfoValidator>()               .AsSingle();
            Container.Bind<IFontProvider>()                .To<DefaultFontProvider>()             .AsSingle();
            Container.Bind<ISRDebuggerInitializer>()       .To<SRDebuggerInitializer>()           .AsSingle();
            Container.Bind<IRemotePropertiesInfoProvider>().To<RemotePropertiesInfoProvider>()    .AsSingle();
            
            if (Application.isEditor)
            {
                Container.Bind<IRemoteConfigManager>() .To<RemoteConfigManagerFake>()       .AsSingle();
                Container.Bind<INotificationsManager>().To<NotificationsManagerFake>()      .AsSingle();
            }
            else
            {
                Container.Bind<INotificationsManager>().To<NotificationsManagerUnity>()     .AsSingle();
                Container.Bind<IRemoteConfigManager>() .To<RemoteConfigManager>()           .AsSingle();
                Container.Bind<IRemoteConfigProvider>().To<FirebaseRemoteConfigProvider>()  .AsSingle();
            }
            
            Container.Bind<IFpsCounter>()
                .To<FpsCounter>()
                // .To<FpsCounterFake>()
                .AsSingle();
            
            Container.Bind<IDebugConsoleView>()
                .FromComponentInNewPrefab(debugConsoleView)
                // .FromComponentInNewPrefab(debugConsoleViewFake)
                .AsSingle();
            
            Container.Bind<IAssetBundleManager>()
                // .To<AssetBundleManager>()
                .To<AssetBundleManagerFake>()
                .AsSingle();
            
#if UNITY_ANDROID
            Container.Bind<IAndroidPerformanceTunerClient>().To<AndroidPerformanceTunerClient>().AsSingle();
#endif

            Container.Bind<IApplicationVersionUpdater>().To<ApplicationVersionUpdater>().AsSingle();
        }
    }
}