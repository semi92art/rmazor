using UI;
using Zenject;

namespace Mono_Installers
{
    public class MainMenuMonoInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<MainMenuStarter>().FromComponentInHierarchy().AsSingle();
            Container.Bind<IMainMenuUiLoader>().To<MainMenuUiLoader>().AsSingle();
            Container.Bind<IMainMenuUI>().To<MainMenuUi>().AsSingle();
        }
    }
}