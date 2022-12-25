using System.Collections;
using Common;
using Common.Extensions;
using Common.Helpers;
using Common.Utils;
using mazing.common.Runtime;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Views.Common.FullscreenTextureProviders;
using UnityEngine;
using UnityEngine.Events;

namespace RMAZOR
{
    public interface IViewFullscreenTransitioner : IInit
    {
        event UnityAction<bool> TransitionFinished;
        void                    DoTextureTransition(bool _Appear, float _Duration);
    }
    
    public class ViewFullscreenTransitioner : InitBase, IViewFullscreenTransitioner
    {
        #region nonpublic members

        private IEnumerator  m_TextureCoroutine;
        private int          m_TransitionsCount;

        #endregion

        #region inject

        private ICameraProvider                      CameraProvider  { get; }
        private IViewGameTicker                      GameTicker      { get; }
        private IFullscreenTransitionTextureProvider TextureProvider { get; }

        private ViewFullscreenTransitioner(
            ICameraProvider                      _CameraProvider,
            IViewGameTicker                      _GameTicker,
            IFullscreenTransitionTextureProvider _TextureProvider)
        {
            CameraProvider  = _CameraProvider;
            GameTicker      = _GameTicker;
            TextureProvider = _TextureProvider;
        }

        #endregion

        #region api

        public event UnityAction<bool> TransitionFinished;

        public override void Init()
        {
            CameraProvider.ActiveCameraChanged += OnActiveCameraChanged;
            TextureProvider.Init();
            base.Init();
        }

        public void DoTextureTransition(bool _Appear, float _Duration)
        {
            m_TransitionsCount++;
            Cor.Stop(m_TextureCoroutine);
            m_TextureCoroutine = DoTextureTransitionCoroutine(_Appear, _Duration);
            Cor.Run(m_TextureCoroutine);
        }

        #endregion

        #region nonpublic members
        
        private void OnActiveCameraChanged(Camera _Camera)
        {
            TextureProvider.Renderer.transform.SetParent(_Camera.transform);
            TextureProvider.Renderer.transform.SetLocalPosXY(Vector2.zero);
        }

        private IEnumerator DoTextureTransitionCoroutine(bool _Appear, float _Duration)
        {
            int transitionsCount = m_TransitionsCount;
            if (_Appear)
                TextureProvider.Activate(true);
            yield return Cor.Lerp(
                GameTicker,
                _Duration,
                _Appear ? 0f : 1f,
                _Appear ? 1f : 0f,
                _P =>
                {
                    TextureProvider.SetTransitionValue(_P, _Appear);
                },
                () =>
                {
                    TransitionFinished?.Invoke(_Appear);
                    if (!_Appear)
                        TextureProvider.Activate(false);
                },
                () => transitionsCount != m_TransitionsCount,
                _P =>
                {
                    float p = _Appear ? (_P - 0.2f) * 1.25f : _P * 1.25f;
                    p = MathUtils.Clamp(p, 0f, 1f);
                    return p;
                });
        }

        #endregion
        
    }
}