using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using RMAZOR.Views.Utils;
using UnityEngine;

namespace RMAZOR.Views.Common.FullscreenTextureProviders
{
    public interface IFullscreenTransitionTextureProviderCirclesToSquares
        : IFullscreenTransitionTextureProvider { }
    
    public class FullscreenTransitionTextureProviderCirclesToSquares
        : FullscreenTransitionTextureProviderSimpleBase, 
          IFullscreenTransitionTextureProviderCirclesToSquares
    {
        #region nonpublic members

        protected override int    SortingOrder      => SortingOrders.GameLogoBackground - 1;
        protected override string MaterialAssetName => "transition_texture_material_circles_to_squares";

        #endregion

        #region inject

        public FullscreenTransitionTextureProviderCirclesToSquares(
            IPrefabSetManager _PrefabSetManager, 
            IContainersGetter _ContainersGetter,
            ICameraProvider   _CameraProvider,
            IColorProvider    _ColorProvider) 
            : base(
                _PrefabSetManager,
                _ContainersGetter,
                _CameraProvider,
                _ColorProvider) { }

        #endregion

        #region api

        public override void Activate(bool _Active)
        {
            Material.SetColor(Color1Id, Color.black);
            base.Activate(_Active);
        }

        #endregion
    }
}