using RMAZOR;
using UnityEngine;
using Zenject;

namespace Mono_Installers
{
    public class ProtMonoInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<PromotionGraphics>().FromComponentInHierarchy().AsSingle();
        }
    }
}