using Common.CameraProviders;
using Common.Network;
using Common.Ticker;
using GameHelpers;
using Managers;
using Managers.Advertising;
using Managers.IAP;
using Network;
using RMAZOR;
using Settings;
using UnityEngine;
using Zenject;

namespace Mono_Installers
{
    public class GlobalMonoInstaller : MonoInstaller
    {
        public GameObject         cameraProvider;
        public CommonGameSettings commonGameSettings;
        public ModelSettings      modelSettings;
        public ViewSettings       viewSettings;
        
        public override void InstallBindings()
        {
            Container.Bind<CommonGameSettings>()  .FromScriptableObject(commonGameSettings).AsSingle();
            Container.Bind<ModelSettings>()       .FromScriptableObject(modelSettings)     .AsSingle();
            Container.Bind<ViewSettings>()        .FromScriptableObject(viewSettings)      .AsSingle();
            Container.Bind<IRemoteConfigManager>().To<RemoteConfigManager>()               .AsSingle();
            Container.Bind<ICameraProvider>()     .FromComponentInNewPrefab(cameraProvider).AsSingle();

            #region settings

            Container.Bind<ISettingsGetter>()     .To<SettingsGetter>()                     .AsSingle();
            Container.Bind<ISoundSetting>()       .To<SoundSetting>()                       .AsSingle();
            Container.Bind<IMusicSetting>()       .To<MusicSetting>()                       .AsSingle();
            Container.Bind<INotificationSetting>().To<NotificationsSetting>()               .AsSingle();
            Container.Bind<IHapticsSetting>()     .To<HapticsSetting>()                     .AsSingle();
            Container.Bind<ILanguageSetting>()    .To<LanguageSetting>()                    .AsSingle();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Container.Bind<IDebugSetting>()       .To<DebugSetting>()                       .AsSingle();
#endif
            #endregion

            #region managers

            Container.Bind<IGameClient>()         .To<GameClient>()                         .AsSingle();
            Container.Bind<IAnalyticsManager>()   .To<AnalyticsManager>()                   .AsSingle();
#if UNITY_EDITOR
            Container.Bind<IShopManager>()        .To<ShopManagerFake>()                    .AsSingle();
#elif UNITY_ANDROID
            Container.Bind<IShopManager>()        .To<AndroidUnityIAPShopManager>()         .AsSingle();
#elif UNITY_IOS || UNITY_ANDROID
            Container.Bind<IShopManager>()        .To<AppleUnityIAPShopManager>()           .AsSingle();
            // Container.Bind<IShopManager>()        .To<AppleSAShopManager>()              .AsSingle();
#endif
            Container.Bind<ILocalizationManager>().To<LeanLocalizationManager>()            .AsSingle();
            Container.Bind<IScoreManager>()       .To<ScoreManager>()                       .AsSingle();
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