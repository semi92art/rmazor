using RMAZOR;

namespace Mono_Installers
{
    public class PreloadMonoInstaller : MonoInstallerImplBase
    {
        public override void InstallBindings()
        {
            base.InstallBindings();
            Container.Bind<ApplicationInitializerRmazor>().FromComponentInHierarchy().AsSingle();
        }
    }
}