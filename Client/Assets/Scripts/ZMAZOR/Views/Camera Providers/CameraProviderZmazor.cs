using Common.Managers;
using Common.Ticker;
using RMAZOR;
using RMAZOR.Camera_Providers;
using RMAZOR.Models;

namespace ZMAZOR.Views.Camera_Providers
{
    public class CameraProviderZmazor : DynamicCameraProvider
    {
        protected CameraProviderZmazor(
            IModelGame        _Model,
            IPrefabSetManager _PrefabSetManager,
            ViewSettings      _ViewSettings,
            IViewGameTicker   _ViewGameTicker) 
            : base(
                _Model,
                _PrefabSetManager,
                _ViewSettings,
                _ViewGameTicker) { }
    }
}