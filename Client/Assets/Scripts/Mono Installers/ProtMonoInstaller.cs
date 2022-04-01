using RMAZOR;
using Zenject;

namespace Mono_Installers
{
    public class ProtMonoInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            #if UNITY_EDITOR
                Container.Bind<LevelDesigner>().FromComponentInHierarchy().AsSingle();
            #endif
        }
    }
}