using System.Collections;
using Common;
using Common.CameraProviders;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using UnityEngine;

namespace RMAZOR.Views.Common.FullscreenTextureProviders
{
    public interface IFullscreenTextureProvider : IInit
    {
        MeshRenderer Renderer { get; }
        void         Activate(bool _Active);
        void         Show(float    _Time, Color _ColorFrom1, Color _ColorFrom2, Color _ColorTo1, Color _ColorTo2);
    }

    public abstract class FullscreenTextureProviderBase :
        FullscreenTransitionTextureProviderBase, IFullscreenTextureProvider
    {
        #region nonpublic members

        private static readonly int
            Color1Id = Shader.PropertyToID("_Color1"),
            Color2Id = Shader.PropertyToID("_Color2");
        protected static readonly int DirectionId = Shader.PropertyToID("_Direction");

        private IEnumerator m_LastCoroutine;

        #endregion

        #region inject

        protected IViewGameTicker Ticker { get; }

        protected FullscreenTextureProviderBase(
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

        public override void Activate(bool _Active)
        {
            Renderer.enabled = _Active;
        }

        public void Show(float _Time, Color _ColorFrom1, Color _ColorFrom2, Color _ColorTo1, Color _ColorTo2)
        {
            Cor.Stop(m_LastCoroutine);
            m_LastCoroutine = ShowTexture(_Time, _ColorFrom1, _ColorFrom2, _ColorTo1, _ColorTo2);
            Cor.Run(m_LastCoroutine);
        }

        public override void SetTransitionValue(float _Value)
        {
            throw new System.NotSupportedException();
        }

        #endregion

        #region nonpublic methods
        
        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            switch (_ColorId)
            {
                case ColorIds.Background1: Material.SetColor(Color1Id, _Color); break;
                case ColorIds.Background2: Material.SetColor(Color2Id, _Color); break;
            }
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
                Ticker,
                _Time,
                _OnProgress: _P =>
                {
                    var newCol1 = Color.Lerp(_ColorFrom1, _ColorTo1, _P);
                    var newCol2 = Color.Lerp(_ColorFrom2, _ColorTo2, _P);
                    Material.SetColor(Color1Id, newCol1);
                    Material.SetColor(Color2Id, newCol2);
                });
        }

        #endregion
    }
}