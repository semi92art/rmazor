using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using RMAZOR.Views.Utils;

namespace RMAZOR.Views.Common.FullscreenTextureProviders
{

    public interface IFullscreenTextureProviderSynthwave
        : IFullscreenTextureProvider { }
    
    public class FullscreenTextureProviderSynthwave
        : FullscreenTextureProviderBase,
          IFullscreenTextureProviderSynthwave
    {
        #region nonpublic members
        
        protected override int SortingOrder => SortingOrders.BackgroundTexture;

        #endregion

        #region inject
        
        private FullscreenTextureProviderSynthwave(
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
            new FullscreenTextureProviderSynthwave(
                PrefabSetManager,
                ContainersGetter, 
                CameraProvider, 
                ColorProvider, 
                Ticker);

        #endregion
    }
}