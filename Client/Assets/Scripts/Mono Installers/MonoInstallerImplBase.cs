using Controllers;
using Entities;
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
            Container.Bind<IGameObservable>().To<GameObservable>().AsSingle();

            Container.Bind<ISoundGameObserver>().To<SoundGameObserver>().AsSingle();
        }
    }
}