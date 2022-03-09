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
    public interface IViewMazeBackgroundTrianglesTextureProvider
        : IViewMazeBackgroundTextureProvider
    {
        void SetProperties(TrianglesTextureSetItem _Item);
    }
    
    public class ViewMazeBackgroundTrianglesTextureProvider :
        ViewMazeBackgroundTextureProviderBase,
        IViewMazeBackgroundTrianglesTextureProvider
    {
        #region nonpublic members

        protected override int    SortingOrder      => SortingOrders.BackgroundTexture;
        protected override string TexturePrefabName => "triangles_texture";

        private static readonly int
            SizeId  = Shader.PropertyToID("_Size"),
            RatioId = Shader.PropertyToID("_Ratio"),
            AId     = Shader.PropertyToID("_A"),
            BId     = Shader.PropertyToID("_B");

        #endregion

        #region inject

        public ViewMazeBackgroundTrianglesTextureProvider(
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

        public void SetProperties(TrianglesTextureSetItem _Item)
        {
            Material.SetFloat(SizeId, _Item.size);
            Material.SetFloat(RatioId, _Item.ratio);
            Material.SetFloat(AId, _Item.a);
            Material.SetFloat(BId, _Item.b);
        }

        #endregion
    }
}