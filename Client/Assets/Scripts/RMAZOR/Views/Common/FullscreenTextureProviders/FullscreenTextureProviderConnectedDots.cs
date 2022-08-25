using Common.CameraProviders;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using RMAZOR.Views.Utils;

namespace RMAZOR.Views.Common.FullscreenTextureProviders
{

    public interface IFullscreenTextureProviderConnectedDots
        : IFullscreenTextureProvider { }
    
    public class FullscreenTextureProviderConnectedDots
        : FullscreenTextureProviderBase,
          IFullscreenTextureProviderConnectedDots
    {
        #region nonpublic members
        
        protected override int    SortingOrder      => SortingOrders.BackgroundTexture;
        protected override string MaterialAssetName => "background_connected_dots";
        
        #endregion

        #region inject
        
        public FullscreenTextureProviderConnectedDots(
            IPrefabSetManager _PrefabSetManager,
            IContainersGetter _ContainersGetter,
            ICameraProvider   _CameraProvider,
            IColorProvider    _ColorProvider,
            IViewGameTicker   _Ticker) 
            : base(
                _PrefabSetManager, 
                _ContainersGetter,
                _CameraProvider, 
                _ColorProvider,
                _Ticker) { }
        
        #endregion
    }
}