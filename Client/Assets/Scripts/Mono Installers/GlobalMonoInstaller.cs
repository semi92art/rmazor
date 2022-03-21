using Common;
using Common.CameraProviders;
using Common.Helpers;
using Common.Managers;
using Common.Managers.Advertising;
using Common.Managers.IAP;
using Common.Managers.Scores;
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
        public GameObject         cameraProvider;
        public GameObject         companyLogo;
        public CommonGameSettings commonGameSettings;
        public ModelSettings      modelSettings;
        public ViewSettings       viewSettings;
        
        public override void InstallBindings()
        {
            Container.BindInstance(new RemoteProperties());
            Container.Bind<CommonGameSettings>()  .FromScriptableObject(commonGameSettings) .AsSingle();
            Container.Bind<ModelSettings>()       .FromScriptableObject(modelSettings)      .AsSingle();
            Container.Bind<ViewSettings>()        .FromScriptableObject(viewSettings)       .AsSingle();
            Container.Bind<ICameraProvider>()     .FromComponentInNewPrefab(cameraProvider) .AsSingle();
            Container.Bind<CompanyLogo>()         .FromComponentInNewPrefab(companyLogo)    .AsSingle();

            #region settings

            Container.Bind<ISettingsGetter>()     .To<SettingsGetter>()                     .AsSingle();
            Container.Bind<ISoundSetting>()       .To<SoundSetting>()                       .AsSingle();
            Container.Bind<IMusicSetting>()       .To<MusicSetting>()                       .AsSingle();
            Container.Bind<INotificationSetting>().To<NotificationsSetting>()               .AsSingle();
            Container.Bind<IHapticsSetting>()     .To<HapticsSetting>()                     .AsSingle();
            Container.Bind<ILanguageSetting>()    .To<LanguageSetting>()                    .AsSingle();
            Container.Bind<IDebugSetting>()       .To<DebugSetting>()                       .AsSingle();
            Container.Bind<IDarkThemeSetting>()   .To<DarkThemeSetting>()                   .AsSingle();
            
            #endregion

            #region managers

            Container.Bind<IRemoteConfigManager>().To<RemoteConfigManager>()                .AsSingle();
            Container.Bind<IAnalyticsManager>()   .To<AnalyticsManager>()                   .AsSingle();
            Container.Bind<IGameClient>()         .To<GameClient>()                         .AsSingle();
            
#if UNITY_EDITOR
            Container.Bind<IScoreManager>()       .To<ScoreManagerFake>()                   .AsSingle();
            Container.Bind<IShopManager>()        .To<ShopManagerFake>()                    .AsSingle();
            Container.Bind<IRemoteSavedGameProvider>().To<FakeRemoteSavedGameProvider>()    .AsSingle();
            Container.Bind<IPermissionsRequester>().To<FakePermissionsRequester>().AsSingle();
#elif UNITY_ANDROID
            Container.Bind<IPermissionsRequester>().To<FakePermissionsRequester>().AsSingle();
            Container.Bind<IScoreManager>()       .To<AndroidScoreManager>()                .AsSingle();
            Container.Bind<IShopManager>()        .To<AndroidUnityIAPShopManager>()         .AsSingle();
            Container.Bind<IRemoteSavedGameProvider>().To<FakeRemoteSavedGameProvider>()    .AsSingle();
#elif UNITY_IOS || UNITY_IPHONE
            Container.Bind<IScoreManager>()       .To<IosScoreManager>()                    .AsSingle();
            Container.Bind<IShopManager>()        .To<AppleUnityIAPShopManager>()           .AsSingle();
            Container.Bind<IRemoteSavedGameProvider>().To<FakeRemoteSavedGameProvider>()    .AsSingle();
            Container.Bind<IPermissionsRequester>().To<IosPermissonsRequester>().AsSingle();
#endif
            
            Container.Bind<ILocalizationManager>().To<LeanLocalizationManager>()            .AsSingle();
            Container.Bind<IHapticsManager>()     .To<HapticsManager>()                     .AsSingle();
            Container.Bind<IAdsManager>()         .To<CustomMediationAdsManager>()          .AsSingle();
            Container.Bind<IPrefabSetManager>()   .To<PrefabSetManager>()                   .AsSingle();
            Container.Bind<IAssetBundleManager>() .To<AssetBundleManager>()                 .AsSingle();

            #endregion
            
            Container.Bind<ICommonTicker>()       .To<CommonTicker>()                       .AsSingle();
            Container.Bind<IViewGameTicker>()     .To<ViewGameTicker>()                     .AsSingle();
            Container.Bind<IModelGameTicker>()    .To<ModelGameTicker>()                    .AsSingle();
            Container.Bind<IUITicker>()           .To<UITicker>()                           .AsSingle();
            Container.Bind<ILevelsLoader>()       .To<LevelsLoader>()                       .AsSingle();
        }
    }
}