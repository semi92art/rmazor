using Controllers;
using Entities;
using GameHelpers;
using Managers;
using Ticker;
using Zenject;

namespace Mono_Installers
{
    public class MonoInstallerImplBase : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<ICommonTicker>()       .To<CommonTicker>()           .AsSingle();
            Container.Bind<IGameTicker>()         .To<GameTicker>()             .AsSingle();
            Container.Bind<IUITicker>()           .To<UITicker>()               .AsSingle();

            Container.Bind<ILevelsLoader>()       .To<LevelsLoader>()           .AsSingle();
            Container.Bind<IManagersGetter>()     .To<ManagersGetter>()         .AsSingle();

            Container.Bind<ISoundManager>()       .To<SoundManager>()           .AsSingle();
            Container.Bind<IDebugManager>()       .To<DebugManager>()           .AsSingle();
        }
    }
}