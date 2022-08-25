using Common.CameraProviders;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using RMAZOR.Views.Utils;
using UnityEngine;

namespace RMAZOR.Views.Common.FullscreenTextureProviders
{
    public readonly struct TextureProviderNeonStreamAdditionalParams
    {
        public TextureProviderNeonStreamAdditionalParams(float _ColorMultiplier)
        {
            ColorMultiplier = _ColorMultiplier;
        }

        public float ColorMultiplier { get; }
    }
    
    public interface IFullscreenTextureProviderNeonStream
        : IFullscreenTextureProvider
    {
        void SetAdditionalParams(TextureProviderNeonStreamAdditionalParams _Params);
    }
    
    public class FullscreenTextureProviderNeonStream
        : FullscreenTextureProviderBase,
          IFullscreenTextureProviderNeonStream
    {
        #region nonpublic members
        
        protected override int    SortingOrder      => SortingOrders.BackgroundTexture;
        protected override string MaterialAssetName => "background_neon_stream";

        private static int ColorMultiplier1Id => Shader.PropertyToID("_Mc1");
        
        #endregion

        #region inject
        
        public FullscreenTextureProviderNeonStream(
            IPrefabSetManager _PrefabSetManager,
            IContainersGetter _ContainersGetter,
            ICameraProvider   _CameraProvider,
            IColorProvider    _ColorProvider,
            IViewGameTicker   _Ticker) 
            : base(
                _PrefabSetManager, 
                _ContainersGetter,
                _CameraProvider, 
                _ColorProvider,
                _Ticker) { }
        
        #endregion

        #region api

        public void SetAdditionalParams(TextureProviderNeonStreamAdditionalParams _Params)
        {
            Material.SetFloat(ColorMultiplier1Id, _Params.ColorMultiplier);
        }
        
        #endregion


    }
}