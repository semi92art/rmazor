using System.Linq;
using Common;
using Common.Helpers;
using Common.Managers;
using Common.Managers.Advertising;
using Common.Managers.Advertising.AdBlocks;
using Common.Managers.Advertising.AdsProviders;
using Common.Managers.Analytics;
using Common.Managers.Notifications;
using Common.Managers.PlatformGameServices;
using Common.Managers.PlatformGameServices.Achievements;
using Common.Managers.PlatformGameServices.GameServiceAuth;
using Common.Managers.PlatformGameServices.Leaderboards;
using Common.Managers.PlatformGameServices.SavedGames;
using mazing.common.Runtime;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Debugging;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Managers.IAP;
using mazing.common.Runtime.Managers.Notifications;
using mazing.common.Runtime.Network;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Settings;
using mazing.common.Runtime.Ticker;
using RMAZOR;
using RMAZOR.Camera_Providers;
using RMAZOR.Helpers;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.InputSchedulers;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Settings;
using RMAZOR.Views;
using RMAZOR.Views.Common.FullscreenTextureProviders;
using RMAZOR.Views.ContainerGetters;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.UI.Game_Logo;
using UnityEngine;
using Zenject;
using ZMAZOR.Views.Camera_Providers;
using ZMAZOR.Views.Coordinate_Converters;
#if YANDEX_GAMES
using Common.Managers.IAP;
using YG;
#endif

namespace Mono_Installers
{
    public class GlobalMonoInstaller : MonoInstaller
    {
        public GlobalGameSettings globalGameSettings;
        public ModelSettings      modelSettings;
        public ViewSettings       viewSettings;

        public GameObject colorProvider;
        public GameObject companyLogo;
        public GameObject debugConsoleView;
        public GameObject debugConsoleViewFake;

        public override void InstallBindings()
        {
            BindCamera();
            BindModel();
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
            Container.Bind<IRetroModeSetting>()   .To<RetroModeSetting>()                   .AsSingle();
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
            Container.Bind<IMyOwnAnalyticsProvider>().To<MyOwnAnalyticsProvider>().AsSingle();
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
#if YANDEX_GAMES
            Container.Bind<IYandexGamesAdsProvider>()    .To<YandexGamesAdsProvider>()       .AsSingle();
            Container.Bind<IYandexGamesInterstitialAd>() .To<YandexGamesInterstitialAd>()    .AsSingle();
            Container.Bind<IYandexGamesRewardedAd>()     .To<YandexGamesRewardedAd>()        .AsSingle();
#endif
            Container.Bind<IAdsProvidersSet>()          .To<AdsProvidersSet>()              .AsSingle();
        }
        
        private void BindStoreAndGameServices()
        {
#if UNITY_EDITOR
            Container.Bind<IShopManager>()         .To<ShopManagerFake>()               .AsSingle();
            Container.Bind<ILeaderboardProvider>() .To<LeaderboardProviderFake>()       .AsSingle();
            Container.Bind<IPlatformGameServiceAuthenticator>()
                .To<PlatformGameServiceAuthenticatorFake>()
                .AsSingle();
#elif UNITY_ANDROID
            Container.Bind<IShopManager>()         .To<AndroidUnityIAPShopManager>()    .AsSingle();
            Container.Bind<ILeaderboardProvider>().To<LeaderboardProviderGooglePlayGames>().AsSingle();
            Container.Bind<IPlatformGameServiceAuthenticator>()
                .To<PlatformGameServiceAuthenticatorGooglePlayGames>()
                .AsSingle();
#elif UNITY_IOS || UNITY_IPHONE
            Container.Bind<IShopManager>()         .To<AppleUnityIAPShopManager>()        .AsSingle();
            Container.Bind<ILeaderboardProvider>() .To<LeaderboardProviderIos>()         .AsSingle();
            Container.Bind<IPlatformGameServiceAuthenticator>()
                .To<PlatformGameServiceAuthenticatorIos>()
                .AsSingle();
#elif UNITY_WEBGL && YANDEX_GAMES
            Container.Bind<IShopManager>()         .To<ShopManagerFake>()               .AsSingle();
            //Container.Bind<IShopManager>()         .To<IAP_ShopManagerYandexGames>()    .AsSingle();
            Container.Bind<ILeaderboardProvider>() .To<LeaderboardProviderFake>()       .AsSingle();
            Container.Bind<IPlatformGameServiceAuthenticator>()
                .To<PlatformGameServiceAuthenticatorFake>()
                .AsSingle();
#endif
            Container.Bind<IScoreManager>()             .To<ScoreManager>()                 .AsSingle();
            Container.Bind<IAchievementsProvider>()     .To<AchievementsProvider>()         .AsSingle();
            Container.Bind<ILeaderboardsSet>()          .To<LeaderboardsSetRmazor>()        .AsSingle();
            Container.Bind<IAchievementsSet>()          .To<AchievementsSetRmazor>()        .AsSingle();
        }

        private void BindHaptics()
        {
#if NICE_VIBRATIONS_3_9
            Container.Bind<IHapticsManager>()           .To<HapticsManagerFake>().AsSingle();
#elif NICE_VIBRATIONS_4_1
            Container.Bind<IHapticsManager>()           .To<HapticsManagerNiceVibrations_4_1>().AsSingle();
#else
            Container.Bind<IHapticsManager>()           .To<HapticsManagerFake>().AsSingle();
#endif
        }

        private void BindPermissionsRequester()
        {
#if UNITY_EDITOR || UNITY_WEBGL
            Container.Bind<IPermissionsRequester>() .To<PermissionsRequesterFake>().AsSingle();
#elif UNITY_ANDROID
            Container.Bind<IPermissionsRequester>().To<PermissionsRequesterFake>() .AsSingle();
#elif UNITY_IOS || UNITY_IPHONE
            Container.Bind<IPermissionsRequester>().To<IosPermissionsRequester>()  .AsSingle();
#endif
        }
        
        private void BindModel()
        {
            Container.Bind<IInputScheduler>()             .To<InputScheduler>()             .AsSingle();
            Container.Bind<IInputSchedulerGameProceeder>().To<InputSchedulerGameProceeder>().AsSingle();
            Container.Bind<IInputSchedulerUiProceeder>()  .To<InputSchedulerUiProceeder>()  .AsSingle();
            Container.Bind<IModelData>()                  .To<ModelData>()                  .AsSingle();
            Container.Bind<IModelMazeRotation>()          .To<ModelMazeRotation>()          .AsSingle();
            Container.Bind<IModelCharacter>()             .To<ModelCharacter>()             .AsSingle();
            Container.Bind<IModelGame>()                  .To<ModelGame>()                  .AsSingle();
            Container.Bind<IModelLevelStaging>()          .To<ModelLevelStaging>()          .AsSingle();
            Container.Bind<IPathItemsProceeder>()         .To<PathItemsProceeder>()         .AsSingle();
            Container.Bind<ITrapsMovingProceeder>()       .To<TrapsMovingProceeder>()       .AsSingle();
            Container.Bind<IGravityItemsProceeder>()      .To<GravityItemsProceeder>()      .AsSingle();
            Container.Bind<ITrapsReactProceeder>()        .To<TrapsReactProceeder>()        .AsSingle();
            Container.Bind<ITurretsProceeder>()           .To<TurretsProceeder>()           .AsSingle();
            Container.Bind<ITrapsIncreasingProceeder>()   .To<TrapsIncreasingProceeder>()   .AsSingle();
            Container.Bind<IPortalsProceeder>()           .To<PortalsProceeder>()           .AsSingle();
            Container.Bind<IShredingerBlocksProceeder>()  .To<ShredingerBlocksProceeder>()  .AsSingle();
            Container.Bind<ISpringboardProceeder>()       .To<SpringboardProceeder>()       .AsSingle();
            Container.Bind<IHammersProceeder>()           .To<HammersProceeder>()           .AsSingle();
            Container.Bind<ISpearsProceeder>()            .To<SpearsProceeder>()            .AsSingle();
            Container.Bind<IDiodesProceeder>()            .To<DiodesProceeder>()            .AsSingle();
            Container.Bind<IKeyLockMazeItemsProceeder>()  .To<KeyLockMazeItemsProceeder>()  .AsSingle();
            Container.Bind<IModelItemsProceedersSet>()    .To<ModelItemsProceedersSet>()    .AsSingle();
            
            Container.Bind<IModelMazeInfoCorrector>()         
                .To<ModelMazeInfoCorrectorWithWallSurrounding>()
                .AsSingle();
            Container.Bind<IModelCharacterPositionValidator>()
                .To<ModelCharacterPositionValidator>()
                .AsSingle();
            Container.Bind<IRevivePositionProvider>()
                .To<RevivePositionProvider>()
                .AsSingle();
        }

        private void BindOther()
        {
#if YANDEX_GAMES
            Container.Bind<IYandexGameFacade>().To<YandexGameFacade>().AsSingle();
            Container.Bind<IYandexGameActionsProvider>().To<YandexGameActionsProvider>().AsSingle();
#endif
            
            Container.Bind<CompanyLogoMonoBeh>()             
                .FromComponentInNewPrefab(companyLogo)
                .AsSingle();
            
            if (!CommonDataMazor.Release)
                Container.Bind<IViewUIGameLogo>() .To<ViewUIGameLogoFake>()     .AsSingle();
            else
                Container.Bind<IViewUIGameLogo>().To<ViewUIGameLogoBladyMaze2>().AsSingle();
            Container.Bind<IGameClient>()                  .To<GameClient>()                       .AsSingle();
            Container.Bind<ISavedGameProvider>()           .To<SavedGamesProvider>()               .AsSingle();
            Container.Bind<ILocalizationManager>()         .To<LeanLocalizationManager>()          .AsSingle();
            Container.Bind<IPrefabSetManager>()            .To<PrefabSetManager>()                 .AsSingle();
            Container.Bind<ILevelsLoader>()                .To<LevelsLoaderRmazor>()               .AsSingle();
            Container.Bind<ILevelAnalyzerRmazor>()         .To<LevelAnalyzerRmazor>()              .AsSingle();
            Container.Bind<ILevelGeneratorRmazor>()        .To<LevelGeneratorRmazor>()             .AsSingle();
            Container.Bind<IMazeInfoValidator>()           .To<MazeInfoValidator>()                .AsSingle();
            Container.Bind<IFontProvider>()                .To<FontProviderMazor>()                .AsSingle();
            Container.Bind<IRemotePropertiesInfoProvider>().To<RemotePropertiesInfoProvider>()     .AsSingle();
            
#if UNITY_EDITOR || UNITY_WEBGL
                Container.Bind<INotificationsManager>().To<NotificationsManagerFake>()      .AsSingle();
#elif UNITY_ANDROID || UNITY_IOS || UNITY_IPHONE
                Container.Bind<INotificationsManager>()
                    .To<NotificationsManagerUnity>()  
                    .AsSingle();
#endif
            
#if FIREBASE && !UNITY_EDITOR && !YANDEX_GAMES
                Container.Bind<IRemoteConfigManager>() .To<RemoteConfigManager>()           .AsSingle();
                // Container.Bind<IPushNotificationsProvider>()
                //     .To<PushNotificationsProviderFirebase>()
                //     .AsSingle();
                Container.Bind<IRemoteConfigProvider>().To<FirebaseRemoteConfigProvider>()  .AsSingle();
#else
            Container.Bind<IRemoteConfigManager>() .To<RemoteConfigManagerFake>()       .AsSingle();
#endif
            Container.Bind<IPushNotificationsProvider>() 
                .To<PushNotificationsProviderFake>()
                .AsSingle();

            Container.Bind<IFpsCounter>()
                .To<FpsCounterRmazor>()
                // .To<FpsCounterFake>()
                .AsSingle();
            
#if UNITY_EDITOR
            Container.Bind<IDebugConsoleView>().FromComponentInNewPrefab(debugConsoleView).AsSingle();
#else
            Container.Bind<IDebugConsoleView>().FromComponentInNewPrefab(debugConsoleViewFake).AsSingle();
#endif

            
            Container.Bind<IAssetBundleManager>()
                // .To<AssetBundleManager>()
                .To<AssetBundleManagerFake>()
                .AsSingle();

            Container.Bind<IApplicationVersionUpdater>().To<ApplicationVersionUpdater>().AsSingle();
            
#if FIREBASE
            Container.Bind<IFirebaseInitializer>().To<FirebaseInitializer>().AsSingle();
#else
            Container.Bind<IFirebaseInitializer>().To<FirebaseInitializerFake>().AsSingle();
#endif
            
            Container.Bind<IColorProvider>()             
                .FromComponentInNewPrefab(colorProvider)
                .AsSingle();
            Container.Bind<IViewMazeGameLogoTextureProvider>()   
                .To<ViewMazeGameLogoTextureProviderCirclesToSquares>()
                .AsSingle();
            Container.Bind(typeof(IContainersGetter), typeof(IContainersGetterRmazor))
                .To<ContainersGetterRmazor>()
                .AsSingle();
            Container.Bind<ICoordinateConverterForSmallLevels>().To<CoordinateConverterForSmallLevels>().AsSingle();
            Container.Bind<ICoordinateConverterForBigLevels>()  .To<CoordinateConverterForBigLevels>()  .AsSingle();
            switch (CommonData.GameId)
            {
                case GameIds.RMAZOR:
                    Container.Bind<ICoordinateConverter>().To<CoordinateConverterRmazor>().AsSingle();
                    break;
                case GameIds.ZMAZOR:
                    Container.Bind<ICoordinateConverter>().To<CoordinateConverterZmazor>().AsSingle();
                    break;
            }
            
#if UNITY_EDITOR
            Container.Bind<IViewInputCommandsProceeder>().To<ViewCommandsProceederInEditor>().AsSingle();
#elif UNITY_WEBGL
            Container.Bind<IViewInputCommandsProceeder>().To<ViewCommandsProceederWebGl>().AsSingle();
#else
            Container.Bind<IViewInputCommandsProceeder>().To<ViewInputCommandsProceeder>().AsSingle();
#endif
            
        }
        
        private void BindCamera()
        {
            Container.Bind<IStaticCameraProvider>() .To<StaticCameraProvider>() .AsSingle();
            Container.Bind<IDynamicCameraProvider>().To<DynamicCameraProvider>().AsSingle();
            Container.Bind<ICameraProviderFake>()   .To<CameraProviderFake>()   .AsSingle();
            switch (CommonData.GameId)
            {
                case GameIds.RMAZOR:
                    Container.Bind<ICameraProvider>().To<CameraProviderRmazor>().AsSingle();
                    break;
                case GameIds.ZMAZOR:
                    Container.Bind<ICameraProvider>().To<CameraProviderZmazor>().AsSingle();
                    break;
            }
        }
    }
}