using Common.CameraProviders;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using RMAZOR.Views.Utils;
using UnityEngine;

namespace RMAZOR.Views.Common.FullscreenTextureProviders
{
    public class ViewMazeGameLogoTextureProviderCirclesToSquares 
        : FullscreenTransitionTextureProviderSimpleBase,
          IViewMazeGameLogoTextureProvider
    {
        #region nonpublic members

        protected override int    SortingOrder      => SortingOrders.GameLogoBackground;
        protected override string MaterialAssetName => "transition_texture_material_circles_to_squares";
        
        #endregion

        #region inject

        private ViewMazeGameLogoTextureProviderCirclesToSquares(
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

        public override void SetTransitionValue(float _Value, bool _Appear)
        {
            Material.SetFloat(TransitionValueId, _Value);
        }

        public void SetColor(Color _Color)
        {
            Material.SetColor(Color1Id, _Color);
        }
        
        public override void Activate(bool _Active)
        {
            Renderer.enabled = _Active;
        }

        #endregion
    }
}