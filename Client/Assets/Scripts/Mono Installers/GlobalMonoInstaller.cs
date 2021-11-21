using Controllers;
using GameHelpers;
using Managers;
using Settings;
using Ticker;
using Zenject;

namespace Mono_Installers
{
    public class GlobalMonoInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            #region settings

            Container.Bind<ISettingsGetter>()     .To<SettingsGetter>()      .AsSingle();
            Container.Bind<ISoundSetting>()       .To<SoundSetting>()        .AsSingle();
            Container.Bind<IMusicSetting>()       .To<MusicSetting>()        .AsSingle();
            Container.Bind<INotificationSetting>().To<NotificationsSetting>().AsSingle();
            Container.Bind<IHapticsSetting>()     .To<HapticsSetting>()      .AsSingle();
            Container.Bind<ILanguageSetting>()    .To<LanguageSetting>()     .AsSingle();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Container.Bind<IDebugSetting>()       .To<DebugSetting>()        .AsSingle();
#endif
            #endregion

            #region managers

            Container.Bind<IAdsManager>()         .To<UnityMediationAdsManager>()               .AsSingle();
            Container.Bind<IAnalyticsManager>()   .To<AnalyticsManager>()         .AsSingle();
            Container.Bind<IShopManager>()        .To<UnityIAPShopManagerFacade>().AsSingle();
            Container.Bind<ILocalizationManager>().To<LeanLocalizationManager>()  .AsSingle();
            Container.Bind<IScoreManager>()       .To<ScoreManager>()             .AsSingle();
            Container.Bind<IHapticsManager>()     .To<HapticsManager>()           .AsSingle();

            #endregion
            
            Container.Bind<ICommonTicker>()       .To<CommonTicker>()             .AsSingle();
            Container.Bind<IViewGameTicker>()     .To<ViewGameTicker>()           .AsSingle();
            Container.Bind<IModelGameTicker>()    .To<ModelGameTicker>()          .AsSingle();
            Container.Bind<IUITicker>()           .To<UITicker>()                 .AsSingle();
            Container.Bind<ILevelsLoader>()       .To<LevelsLoader>()             .AsSingle();
        }
    }
}