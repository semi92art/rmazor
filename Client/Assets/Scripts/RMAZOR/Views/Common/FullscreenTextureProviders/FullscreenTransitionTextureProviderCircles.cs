using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using RMAZOR.Views.Utils;
using UnityEngine;

namespace RMAZOR.Views.Common.FullscreenTextureProviders
{
    public interface IFullscreenTransitionTextureProviderCircles
        : IFullscreenTransitionTextureProvider { }
    
    public class FullscreenTransitionTextureProviderCircles 
        : FullscreenTransitionTextureProviderSimpleBase, IFullscreenTransitionTextureProviderCircles
    {
        #region nonpublic members

        protected override int    SortingOrder      => SortingOrders.GameLogoBackground - 1;
        protected override string MaterialAssetName => "transition_texture_material_circles";
        
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

        public override void Activate(bool _Active)
        {
            Material.SetColor(Color1Id, Color.white);
            base.Activate(_Active);
        }

        #endregion
    }
}