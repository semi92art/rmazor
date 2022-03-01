using Common.CameraProviders;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using RMAZOR.Views.Utils;
using UnityEngine;

namespace RMAZOR.Views.Common.ViewMazeBackgroundTextureProviders
{
    public interface IViewMazeBackgroundSolidTextureProvider 
        : IViewMazeBackgroundTextureProvider { }
    
    public class ViewMazeBackgroundSolidTextureProvider :
        ViewMazeBackgroundTextureProviderBase,
        IViewMazeBackgroundSolidTextureProvider
    {
        #region nonpbulc members

        private static readonly int ColorId = Shader.PropertyToID("_Color");

        #endregion
        
        #region inject

        public ViewMazeBackgroundSolidTextureProvider(
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

        protected override int    SortingOrder      => SortingOrders.BackgroundTransitionTexture;
        protected override string TexturePrefabName => "solid_texture";

        public override void SetColors(Color _Color1, Color _)
        {
            Material.SetColor(ColorId, _Color1);
        }
        
        #endregion
    }
}