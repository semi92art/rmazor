using Common.Managers;
using Common.Ticker;
using RMAZOR;
using RMAZOR.Camera_Providers;

namespace ZMAZOR.Views.Camera_Providers
{
    public class CameraProviderZmazor : DynamicCameraProvider
    {
        protected CameraProviderZmazor(
            IPrefabSetManager _PrefabSetManager,
            ViewSettings      _ViewSettings,
            IViewGameTicker   _ViewGameTicker) 
            : base(
                _PrefabSetManager,
                _ViewSettings,
                _ViewGameTicker) { }
    }
}