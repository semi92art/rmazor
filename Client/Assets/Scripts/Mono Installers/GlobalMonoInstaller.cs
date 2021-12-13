using Controllers;
using GameHelpers;
using Managers;
using Managers.Advertising;
using Settings;
using Ticker;
using Zenject;

namespace Mono_Installers
{
    public class GlobalMonoInstaller : MonoInstaller
    {
        public CommonGameSettings commonGameSettings;
        
        public override void InstallBindings()
        {
            Container.Bind<CommonGameSettings>().FromScriptableObject(commonGameSettings).AsSingle();
            
            #region settings

            Container.Bind<ISettingsGetter>()     .To<SettingsGetter>()               .AsSingle();
            Container.Bind<ISoundSetting>()       .To<SoundSetting>()                 .AsSingle();
            Container.Bind<IMusicSetting>()       .To<MusicSetting>()                 .AsSingle();
            Container.Bind<INotificationSetting>().To<NotificationsSetting>()         .AsSingle();
            Container.Bind<IHapticsSetting>()     .To<HapticsSetting>()               .AsSingle();
            Container.Bind<ILanguageSetting>()    .To<LanguageSetting>()              .AsSingle();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Container.Bind<IDebugSetting>()       .To<DebugSetting>()                 .AsSingle();
#endif
            #endregion

            #region managers

            Container.Bind<IAnalyticsManager>()   .To<AnalyticsManager>()             .AsSingle();
#if UNITY_EDITOR
            Container.Bind<IShopManager>()        .To<ShopManagerFake>()              .AsSingle();
#else
            Container.Bind<IShopManager>()        .To<UnityIAPShopManagerFacade>()    .AsSingle();
#endif
            Container.Bind<ILocalizationManager>().To<LeanLocalizationManager>()      .AsSingle();
            Container.Bind<IScoreManager>()       .To<ScoreManager>()                 .AsSingle();
            Container.Bind<IHapticsManager>()     .To<HapticsManager>()               .AsSingle();
            Container.Bind<IAdsManager>()         .To<CustomMediationAdsManager>()    .AsSingle();
            Container.Bind<IPrefabSetManager>()   .To<PrefabSetManager>()             .AsSingle();
            Container.Bind<IAssetBundleManager>() .To<AssetBundleManager>()           .AsSingle();

            #endregion
            
            Container.Bind<ICommonTicker>()       .To<CommonTicker>()                 .AsSingle();
            Container.Bind<IViewGameTicker>()     .To<ViewGameTicker>()               .AsSingle();
            Container.Bind<IModelGameTicker>()    .To<ModelGameTicker>()              .AsSingle();
            Container.Bind<IUITicker>()           .To<UITicker>()                     .AsSingle();
            Container.Bind<ILevelsLoader>()       .To<LevelsLoader>()                 .AsSingle();
        }
    }
}