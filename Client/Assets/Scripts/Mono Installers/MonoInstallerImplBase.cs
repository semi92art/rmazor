using GameHelpers;
using Games.RazorMaze;
using Ticker;
using Zenject;

namespace Mono_Installers
{
    public class MonoInstallerImplBase : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<ITicker>().To<Ticker.Ticker>().AsSingle();
            Container.Bind<ILevelsLoader>().To<LevelsLoader>().AsSingle();
        }
    }
}