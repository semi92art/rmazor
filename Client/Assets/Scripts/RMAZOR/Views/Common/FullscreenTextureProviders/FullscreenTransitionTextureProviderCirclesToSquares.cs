using Common.CameraProviders;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using RMAZOR.Views.Utils;

namespace RMAZOR.Views.Common.FullscreenTextureProviders
{
    public interface IFullscreenTransitionTextureProviderCirclesToSquares
        : IFullscreenTransitionTextureProvider { }
    
    public class FullscreenTransitionTextureProviderCirclesToSquares 
        : FullscreenTransitionTextureProviderSimpleBase
          , IFullscreenTransitionTextureProviderCirclesToSquares
    {
        private FullscreenTransitionTextureProviderCirclesToSquares(
            IPrefabSetManager _PrefabSetManager,
            IContainersGetter _ContainersGetter, 
            ICameraProvider _CameraProvider,
            IColorProvider _ColorProvider)
            : base(
                _PrefabSetManager, 
                _ContainersGetter,
                _CameraProvider,
                _ColorProvider) { }

        protected override int    SortingOrder      => SortingOrders.GameLogoBackground - 1;
        protected override string MaterialAssetName => "transition_texture_material_circles_to_squares";
    }
}