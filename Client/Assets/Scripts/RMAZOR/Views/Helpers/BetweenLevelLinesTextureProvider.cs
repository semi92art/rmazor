using Common;
using Common.CameraProviders;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using RMAZOR.Views.Common.ViewMazeBackgroundTextureProviders;
using RMAZOR.Views.Utils;
using UnityEngine;

namespace RMAZOR.Views.Helpers
{
    public interface IBetweenLevelTextureProvider : IInit
    {
        void Activate(bool            _Active);
        void SetTransitionValue(float _Value);
        void SetColors(Color          _Color1, Color _Color2);
    }
    
    public class BetweenLevelLinesTextureProvider 
        : ViewMazeFullscreenTextureProviderBase,
          IBetweenLevelTextureProvider
    {
        #region nonpublic members

        protected override int    SortingOrder      => SortingOrders.GameLogoBackground;
        protected override string MaterialAssetName => "between_level_texture_material";

        private static readonly int Color1Id = Shader.PropertyToID("_Color1");
        private static readonly int Color2Id = Shader.PropertyToID("_Color2");
        private static readonly int IndentId = Shader.PropertyToID("_Indent");
        
        #endregion

        #region inject

        public BetweenLevelLinesTextureProvider(
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

        public void SetTransitionValue(float _Value)
        {
            Material.SetFloat(IndentId, 1f - _Value);
        }

        public void SetColors(Color _Color1, Color _Color2)
        {
            Material.SetColor(Color1Id, _Color1);
            Material.SetColor(Color2Id, _Color2);
        }
        
        public void Activate(bool _Active)
        {
            Renderer.enabled = _Active;
        }

        #endregion
    }
}