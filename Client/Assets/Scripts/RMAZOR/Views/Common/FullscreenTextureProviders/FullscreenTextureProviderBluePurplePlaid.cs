using Common.CameraProviders;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using RMAZOR.Views.Utils;
using UnityEngine;

namespace RMAZOR.Views.Common.FullscreenTextureProviders
{
    public interface IFullscreenTextureProviderBluePurplePlaid
        : IFullscreenTextureProvider { }
    
    public class FullscreenTextureProviderBluePurplePlaid
        : FullscreenTextureProviderBase,
          IFullscreenTextureProviderBluePurplePlaid, IUpdateTick
    {
        #region nonpublic members
        
        protected override int    SortingOrder      => SortingOrders.BackgroundTexture;
        protected override string MaterialAssetName => "background_blue_purple_plaid";

        private readonly int m_FrameNumId = Shader.PropertyToID("_FrameNum");
        
        #endregion

        #region inject
        
        public FullscreenTextureProviderBluePurplePlaid(
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

        public override void Init()
        {
            Ticker.Register(this);
            base.Init();
        }

        public void UpdateTick()
        {
            Material.SetInt(m_FrameNumId, Time.frameCount);
        }
    }
}