using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using RMAZOR.Views.Utils;
using UnityEngine;

namespace RMAZOR.Views.Common.FullscreenTextureProviders
{
    public interface IFullscreenTransitionTextureProviderCircles2
        : IFullscreenTransitionTextureProvider { }
    
    public class FullscreenTransitionTextureProviderCircles2
        : FullscreenTransitionTextureProviderSimpleBase, 
          IFullscreenTransitionTextureProviderCircles2
    {
        #region nonpublic members

        protected override int    SortingOrder      => SortingOrders.GameLogoBackground - 1;
        protected override string MaterialAssetName => "transition_texture_material_circles_2";

        #endregion

        #region inject

        public FullscreenTransitionTextureProviderCircles2(
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
            Material.SetColor(Color1Id, Color.white);
            base.Activate(_Active);
        }

        #endregion
    }
}