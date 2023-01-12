using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using RMAZOR;
using RMAZOR.Camera_Providers;
using RMAZOR.Models;

namespace ZMAZOR.Views.Camera_Providers
{
    public class CameraProviderZmazor : DynamicCameraProvider
    {
        protected CameraProviderZmazor(
            ViewSettings      _ViewSettings,
            IModelGame        _Model,
            IPrefabSetManager _PrefabSetManager,
            IViewGameTicker   _ViewGameTicker) 
            : base(
                _ViewSettings,
                _Model,
                _PrefabSetManager,
                _ViewGameTicker) { }
    }
}