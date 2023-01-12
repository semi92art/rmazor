using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;

namespace RMAZOR.Camera_Providers
{
    public interface IStaticCameraProvider : ICameraProvider { }

    public class StaticCameraProvider : CameraProviderBase, IStaticCameraProvider
    {
        private StaticCameraProvider(
            IPrefabSetManager _PrefabSetManager,
            IViewGameTicker   _ViewGameTicker) 
            : base(
                _PrefabSetManager,
                _ViewGameTicker) { }
        protected override string CameraName => "Static Camera";
    }
}