using System;
using System.Collections;
using Common;
using Common.CameraProviders;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using UnityEngine;

namespace RMAZOR.Views.Common.ViewMazeBackgroundTextureProviders
{
    public interface IViewMazeBackgroundTextureProvider : IInit
    {
        MeshRenderer Renderer { get; }
        void         Activate(bool _Active);
        void         Show(float    _Time, Color _ColorFrom1, Color _ColorFrom2, Color _ColorTo1, Color _ColorTo2);
    }

    public abstract class ViewMazeBackgroundTextureProviderBase :
        ViewMazeFullscreenTextureProviderBase, IViewMazeBackgroundTextureProvider
    {
        #region nonpublic members

        private static readonly int
            Color1Id = Shader.PropertyToID("_Color1"),
            Color2Id = Shader.PropertyToID("_Color2");
        protected static readonly int          
            TilingId     = Shader.PropertyToID("_Tiling"),
            DirectionId  = Shader.PropertyToID("_Direction"),
            WrapScaleId  = Shader.PropertyToID("_WrapScale"),
            WrapTilingId = Shader.PropertyToID("_WarpTiling");

        private IEnumerator m_LastCoroutine;

        #endregion

        #region inject

        protected IViewGameTicker Ticker { get; }

        protected ViewMazeBackgroundTextureProviderBase(
            IPrefabSetManager _PrefabSetManager,
            IContainersGetter _ContainersGetter,
            ICameraProvider   _CameraProvider,
            IColorProvider    _ColorProvider,
            IViewGameTicker   _Ticker) 
            : base(
                _PrefabSetManager,
                _ContainersGetter, 
                _CameraProvider,
                _ColorProvider)
        {
            Ticker = _Ticker;
        }

        #endregion

        #region api

        public void Activate(bool _Active)
        {
            Renderer.enabled = _Active;
        }

        public void Show(float _Time, Color _ColorFrom1, Color _ColorFrom2, Color _ColorTo1, Color _ColorTo2)
        {
            Cor.Stop(m_LastCoroutine);
            m_LastCoroutine = ShowTexture(_Time, _ColorFrom1, _ColorFrom2, _ColorTo1, _ColorTo2);
            Cor.Run(m_LastCoroutine);
        }

        #endregion

        #region nonpublic methods
        
        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId == ColorIds.Background1)
                Material.SetColor(Color1Id, _Color);
            else if (_ColorId == ColorIds.Background2)
                Material.SetColor(Color2Id, _Color);
        }
        
        private IEnumerator ShowTexture(
            float _Time,
            Color _ColorFrom1, 
            Color _ColorFrom2,
            Color _ColorTo1,
            Color _ColorTo2)
        {
            if (_Time < MathUtils.Epsilon)
            {
                Material.SetColor(Color1Id, _ColorTo1);
                Material.SetColor(Color2Id, _ColorTo2);
                yield break;
            }
            yield return Cor.Lerp(
                0f,
                1f,
                _Time,
                _P =>
                {
                    var newCol1 = Color.Lerp(_ColorFrom1, _ColorTo1, _P);
                    var newCol2 = Color.Lerp(_ColorFrom2, _ColorTo2, _P);
                    Material.SetColor(Color1Id, newCol1);
                    Material.SetColor(Color2Id, newCol2);
                },
                Ticker);
        }

        #endregion
    }
}