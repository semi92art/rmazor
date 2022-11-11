using Common.CameraProviders;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using RMAZOR.Views.Utils;

namespace RMAZOR.Views.Common.FullscreenTextureProviders
{
    public interface IFullscreenTextureProviderEmpty
        : IFullscreenTextureProvider { }
    
    public class FullscreenTextureProviderEmpty :
        FullscreenTextureProviderBase,
        IFullscreenTextureProviderEmpty
    {
        #region nonpublic members

        protected override int   SortingOrder => SortingOrders.BackgroundTexture;
        public override    float Quality      => 0.1f;

        #endregion

        #region inject

        private FullscreenTextureProviderEmpty(
            IPrefabSetManager _PrefabSetManager, 
            IContainersGetter _ContainersGetter, 
            ICameraProvider   _CameraProvider, 
            IViewGameTicker   _Ticker,
            IColorProvider    _ColorProvider)
            : base(
                _PrefabSetManager,
                _ContainersGetter,
                _CameraProvider,
                _ColorProvider,
                _Ticker) { }

        #endregion
    }
}