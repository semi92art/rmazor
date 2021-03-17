using Zenject;

namespace Mono_Installers
{
    public class PreloadMonoInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<ApplicationInitializer>().FromComponentInHierarchy().AsSingle();
        }
    }
}