using Managers;
using Zenject;

namespace Mono_Installers
{
    public class GlobalMonoInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IAdsManager>()         .To<AdsManager>()               .AsSingle();
            Container.Bind<IAnalyticsManager>()   .To<AnalyticsManager>()         .AsSingle();
            Container.Bind<IShopManager>()        .To<UnityIAPShopManagerFacade>().AsSingle();
            Container.Bind<ILocalizationManager>().To<LeanLocalizationManager>()  .AsSingle();
            Container.Bind<IScoreManager>()       .To<ScoreManager>()             .AsSingle();
        }
    }
}