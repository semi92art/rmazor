using UI;

namespace Mono_Installers
{
    public class MainMenuMonoInstaller : MonoInstallerImplBase
    {
        public override void InstallBindings()
        {
            base.InstallBindings();
            
            Container.Bind<IntroSceneViewer>().FromComponentInHierarchy().AsSingle();
        }
    }
}