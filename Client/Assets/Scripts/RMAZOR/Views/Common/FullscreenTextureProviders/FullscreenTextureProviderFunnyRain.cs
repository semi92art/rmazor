using Common.CameraProviders;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using RMAZOR.Views.Utils;

namespace RMAZOR.Views.Common.FullscreenTextureProviders
{

    public interface IFullscreenTextureProviderFunnyRain
        : IFullscreenTextureProvider { }
    
    public class FullscreenTextureProviderFunnyRain
        : FullscreenTextureProviderBase,
          IFullscreenTextureProviderFunnyRain
    {
        #region nonpublic members
        
        protected override int    SortingOrder      => SortingOrders.BackgroundTexture;
        protected override string MaterialAssetName => "background_funny_rain";
        
        #endregion

        #region inject
        
        public FullscreenTextureProviderFunnyRain(
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