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
        void Activate(bool  _Active);
        void SetColor(Color _Color);
    }
    
    public class ViewMazeGameLogoTextureProvider 
        : ViewMazeFullscreenTextureProviderBase,
          IViewMazeGameLogoTextureProvider
    {
        #region nonpublic members

        protected override int    SortingOrder      => SortingOrders.GameLogoBackground;
        protected override string TexturePrefabName => "solid_texture";

        private static readonly int ColorId = Shader.PropertyToID("_Color");
        
        #endregion

        #region inject

        public ViewMazeGameLogoTextureProvider(
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

        public void SetColor(Color _Color)
        {
            Material.SetColor(ColorId, _Color);
        }
        
        public void Activate(bool _Active)
        {
            Renderer.enabled = _Active;
        }

        #endregion
    }
}