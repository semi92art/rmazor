using Common.CameraProviders;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using RMAZOR.Views.Utils;
using UnityEngine;

namespace RMAZOR.Views.Common.FullscreenTextureProviders
{
    public readonly struct TextureProviderSwirlForPlanetAdditionalParams
    {
        public TextureProviderSwirlForPlanetAdditionalParams(float _ColorMultiplier)
        {
            ColorMultiplier = _ColorMultiplier;
        }

        public float ColorMultiplier { get; }
    }

    public interface IFullscreenTextureProviderSwirlForPlanet
        : IFullscreenTextureProvider
    {
        void SetAdditionalParams(TextureProviderSwirlForPlanetAdditionalParams _Params);
    }
    
    public class FullscreenTextureProviderSwirlForPlanet
        : FullscreenTextureProviderBase,
          IFullscreenTextureProviderSwirlForPlanet
    {
        #region nonpublic members
        
        protected override int    SortingOrder      => SortingOrders.BackgroundTexture;
        protected override string MaterialAssetName => "background_swirl_for_planet";
        
        private static int ColorMultiplier1Id => Shader.PropertyToID("_Mc1");
        
        #endregion

        #region inject
        
        public FullscreenTextureProviderSwirlForPlanet(
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

        public void SetAdditionalParams(TextureProviderSwirlForPlanetAdditionalParams _Params)
        {
            Material.SetFloat(ColorMultiplier1Id, _Params.ColorMultiplier);
        }

        #endregion
    }
}