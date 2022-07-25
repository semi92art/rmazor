using Common;
using Common.CameraProviders;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using RMAZOR.Views.Utils;
using UnityEngine;

namespace RMAZOR.Views.Common.FullscreenTextureProviders
{
    public interface IViewMazeGameLogoTextureProvider : IInit
    {
        void Activate(bool            _Active);
        void SetTransitionValue(float _Value);
        void SetColor(Color           _Color);
    }
    
    public class ViewMazeGameLogoSingleCircleTextureProvider 
        : FullscreenTransitionTextureProviderSimpleBase,
          IViewMazeGameLogoTextureProvider
    {
        #region nonpublic members

        protected override int    SortingOrder      => SortingOrders.GameLogoBackground;
        protected override string MaterialAssetName => "transition_texture_material_circles";

        
        #endregion

        #region inject

        private ViewMazeGameLogoSingleCircleTextureProvider(
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

        public override void SetTransitionValue(float _Value)
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