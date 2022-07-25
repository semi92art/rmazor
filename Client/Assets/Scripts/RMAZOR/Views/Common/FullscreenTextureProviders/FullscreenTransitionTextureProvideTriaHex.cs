using Common;
using Common.CameraProviders;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using RMAZOR.Views.Utils;
using UnityEngine;

namespace RMAZOR.Views.Common.FullscreenTextureProviders
{
    public interface IFullscreenTransitionTextureProviderTriaHex 
        : IFullscreenTransitionTextureProvider { }
    
    public class FullscreenTransitionTextureProvideTriaHex
        : FullscreenTransitionTextureProviderBase, 
          IFullscreenTransitionTextureProviderTriaHex
    {
        #region nonpublic members

        protected override int    SortingOrder      => SortingOrders.GameLogoBackground;
        protected override string MaterialAssetName => "transition_texture_material_tria_hex";

        private static readonly int   BackgroundColorId  = Shader.PropertyToID("_BackgroundColor");
        private static readonly int   EdgesColorId       = Shader.PropertyToID("_EdgesColor");
        private static readonly int   TransitionValue1Id = Shader.PropertyToID("_TransitionValue1");
        private static readonly int   TransitionValue2Id = Shader.PropertyToID("_TransitionValue2");
        private static readonly Color EdgesColor         = new Color(0.72f, 0.72f, 0.72f);
        
        #endregion

        #region inject

        public FullscreenTransitionTextureProvideTriaHex(
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
            float edgesAlpha, transition1, transition2;
            if (_Value < 0.4f)
            {
                transition1 = _Value / 0.4f;
                edgesAlpha = 0f;
                transition2 = 0f;
            }
            else if (_Value < 0.6f)
            {
                transition1 = 1f;
                edgesAlpha = (_Value - 0.4f) / 0.2f;
                transition2 = 0f;
            }
            else
            {
                transition1 = 1f;
                edgesAlpha = 1f;
                transition2 = (_Value - 0.6f) / 0.4f;
            }
            Material.SetColor(EdgesColorId, EdgesColor.SetA(edgesAlpha));
            Material.SetFloat(TransitionValue1Id, transition1);
            Material.SetFloat(TransitionValue2Id, transition2);
        }

        public override void Activate(bool _Active)
        {
            Renderer.enabled = _Active;
            Material.SetColor(BackgroundColorId, CommonData.CompanyLogoBackgroundColor);
            Material.SetColor(EdgesColorId, EdgesColor.SetA(0f));
            Material.SetFloat(TransitionValue1Id, 0f);
            Material.SetFloat(TransitionValue2Id, 0f);
        }
        
        #endregion
    }
}