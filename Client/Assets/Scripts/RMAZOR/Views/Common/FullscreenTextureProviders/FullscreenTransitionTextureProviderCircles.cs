using Common.CameraProviders;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using RMAZOR.Views.Utils;
using UnityEngine;

namespace RMAZOR.Views.Common.FullscreenTextureProviders
{
    public class FullscreenTransitionTextureProviderCircles 
        : FullscreenTransitionTextureProviderSimpleBase
    {
        #region nonpublic members

        protected override int    SortingOrder      => SortingOrders.GameLogoBackground;
        protected override string MaterialAssetName => "between_level_texture_material_circles";
        
        private static readonly int   EdgesColorId = Shader.PropertyToID("_EdgesColor");
        private static readonly Color EdgesColor   = new Color(0.72f, 0.72f, 0.72f);
        
        #endregion

        #region inject

        public FullscreenTransitionTextureProviderCircles(
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
            Material.SetColor(EdgesColorId, EdgesColor);
            base.Activate(_Active);
        }

        #endregion
    }
}