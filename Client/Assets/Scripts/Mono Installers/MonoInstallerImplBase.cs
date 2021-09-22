using GameHelpers;
using Ticker;
using Zenject;

namespace Mono_Installers
{
    public class MonoInstallerImplBase : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IGameTicker>().To<GameTicker>().AsSingle();
            Container.Bind<IUITicker>().To<UITicker>().AsSingle();
            Container.Bind<ILevelsLoader>().To<LevelsLoader>().AsSingle();
        }
    }
}