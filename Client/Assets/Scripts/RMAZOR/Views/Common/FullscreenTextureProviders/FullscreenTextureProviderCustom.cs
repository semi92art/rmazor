using Common.CameraProviders;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using RMAZOR.Views.Utils;

namespace RMAZOR.Views.Common.FullscreenTextureProviders
{

    public interface IFullscreenTextureProviderCustom
        : IFullscreenTextureProvider { }
    
    public class FullscreenTextureProviderCustom
        : FullscreenTextureProviderBase,
          IFullscreenTextureProviderCustom
    {
        #region nonpublic members
        
        protected override int    SortingOrder       => SortingOrders.BackgroundTexture;

        #endregion

        #region inject
        
        private FullscreenTextureProviderCustom(
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

        #region api

        public override float Quality => 0.5f;

        public override object Clone() =>
            new FullscreenTextureProviderCustom(
                PrefabSetManager,
                ContainersGetter, 
                CameraProvider, 
                ColorProvider, 
                Ticker);

        #endregion
    }
}