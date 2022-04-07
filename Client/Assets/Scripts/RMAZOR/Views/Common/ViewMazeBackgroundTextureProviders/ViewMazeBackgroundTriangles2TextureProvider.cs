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
    public interface IViewMazeBackgroundTriangles2TextureProvider
        : IViewMazeBackgroundTextureProvider
    {
        void SetProperties(Triangles2TextureSetItem _Item);
    }
    
    public class ViewMazeBackgroundTriangles2TextureProvider :
        ViewMazeBackgroundTextureProviderBase,
        IViewMazeBackgroundTriangles2TextureProvider
    {
        #region nonpublic members

        protected override int    SortingOrder      => SortingOrders.BackgroundTexture;
        protected override string MaterialAssetName => "triangles_background";

        private static readonly int
            SizeId  = Shader.PropertyToID("_Size"),
            RatioId = Shader.PropertyToID("_Ratio"),
            AId     = Shader.PropertyToID("_A"),
            BId     = Shader.PropertyToID("_B");

        #endregion

        #region inject

        public ViewMazeBackgroundTriangles2TextureProvider(
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

        public void SetProperties(Triangles2TextureSetItem _Item)
        {
            Material.SetFloat(SizeId, _Item.size);
            Material.SetFloat(RatioId, _Item.ratio);
            Material.SetFloat(AId, _Item.a);
            Material.SetFloat(BId, _Item.b);
        }

        #endregion
    }
}