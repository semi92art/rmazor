using Common.CameraProviders;
using Common.Helpers;
using Common.Managers;
using Common.Ticker;
using RMAZOR.Views.Common.ViewMazeBackgroundPropertySets;
using RMAZOR.Views.Utils;
using UnityEngine;
using Common.Providers;

namespace RMAZOR.Views.Common.ViewMazeBackgroundTextureProviders
{
    
    public interface IViewMazeBackgroundCircles2TextureProvider 
        : IViewMazeBackgroundTextureProvider
    {
        void SetProperties(Circles2TextureSetItem _Item);
    }
    
    public class ViewMazeBackgroundCircles2TextureProvider : 
        ViewMazeBackgroundTextureProviderBase,
        IViewMazeBackgroundCircles2TextureProvider
    {
        #region nonpublic members
        
        private static readonly int
            RadiusId     = Shader.PropertyToID("_Radius"),
            StepXId    = Shader.PropertyToID("_StepX"),
            StepYId    = Shader.PropertyToID("_StepY"),
            AlternateX = Shader.PropertyToID("_AlternateX");

        #endregion

        #region inject

        public ViewMazeBackgroundCircles2TextureProvider(
            IPrefabSetManager _PrefabSetManager,
            IContainersGetter _ContainersGetter,
            ICameraProvider   _CameraProvider,
            IViewGameTicker   _Ticker,
            IColorProvider    _ColorProvider) 
            : base(
                _PrefabSetManager,
                _ContainersGetter,
                _CameraProvider,
                _Ticker,
                _ColorProvider) { }

        #endregion

        #region api

        protected override int    SortingOrder      => SortingOrders.BackgroundTexture;
        protected override string TexturePrefabName => "circles_texture";

        public void SetProperties(Circles2TextureSetItem _Item)
        {
            Material.SetFloat(RadiusId, _Item.radius);
            Material.SetFloat(StepXId, _Item.stepX);
            Material.SetFloat(StepYId, _Item.stepY);
            Material.SetFloat(AlternateX, _Item.alternateX ? 1f : 0f);
        }

        #endregion
    }
}