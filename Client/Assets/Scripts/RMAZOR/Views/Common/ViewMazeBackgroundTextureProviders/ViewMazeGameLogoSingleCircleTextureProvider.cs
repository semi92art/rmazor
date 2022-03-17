using Common;
using Common.CameraProviders;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using RMAZOR.Views.Utils;
using UnityEngine;

namespace RMAZOR.Views.Common.ViewMazeBackgroundTextureProviders
{
    public interface IViewMazeGameLogoTextureProvider : IInit
    {
        void Activate(bool            _Active);
        void SetTransitionValue(float _Value);
        void SetColor(Color           _Color);
    }
    
    public class ViewMazeGameLogoSingleCircleTextureProvider 
        : ViewMazeFullscreenTextureProviderBase,
          IViewMazeGameLogoTextureProvider
    {
        #region nonpublic members

        protected override int    SortingOrder      => SortingOrders.GameLogoBackground;
        protected override string TexturePrefabName => "single_circle_texture";

        private static readonly int Color1Id = Shader.PropertyToID("_Color1");
        private static readonly int Color2Id = Shader.PropertyToID("_Color2");
        private static readonly int RadiusId = Shader.PropertyToID("_Radius");
        
        #endregion

        #region inject

        public ViewMazeGameLogoSingleCircleTextureProvider(
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

        public override void Init()
        {
            base.Init();
            Material.SetColor(Color2Id, new Color(0f, 0f, 0f, 0f));
        }

        public void SetTransitionValue(float _Value)
        {
            Material.SetFloat(RadiusId, _Value);
        }

        public void SetColor(Color _Color)
        {
            Material.SetColor(Color1Id, _Color);
        }
        
        public void Activate(bool _Active)
        {
            Renderer.enabled = _Active;
        }

        #endregion
    }
}