using Common.CameraProviders;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using RMAZOR.Views.Common.ViewMazeBackgroundPropertySets;
using RMAZOR.Views.Utils;
using UnityEngine;

namespace RMAZOR.Views.Common.ViewMazeBackgroundTextureProviders
{
    public interface IViewMazeBackgroundLinesTextureProvider
        : IViewMazeBackgroundTextureProvider
    {
        void SetProperties(LinesTextureSetItem _Item);
    }
    
    public class ViewMazeBackgroundLinesTextureProvider : 
        ViewMazeBackgroundTextureProviderBase,
        IViewMazeBackgroundLinesTextureProvider
    {
        #region nonpublic members
        
        protected override int    SortingOrder      => SortingOrders.BackgroundTexture;
        protected override string TexturePrefabName => "lines_texture";

        #endregion

        #region inject

        public ViewMazeBackgroundLinesTextureProvider(
            IPrefabSetManager _PrefabSetManager,
            IContainersGetter _ContainersGetter,
            ICameraProvider   _CameraProvider,
            IViewGameTicker   _Ticker,
            IColorProvider    _ColorProvider) 
            : base(
                _PrefabSetManager,
                _ContainersGetter,
                _CameraProvider,
                _ColorProvider,
                _Ticker) { }

        #endregion

        #region api
        
        public void SetProperties(LinesTextureSetItem _Item)
        {
            Material.SetInteger(TilingId, _Item.tiling);
            Material.SetFloat(DirectionId, _Item.direction);
            Material.SetFloat(WrapScaleId, _Item.wrapScale);
            Material.SetFloat(WrapTilingId, _Item.wrapTiling);
        }

        #endregion
    }
}