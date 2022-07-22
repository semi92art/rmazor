using System.Collections;
using Common;
using Common.Helpers;
using Common.Ticker;
using Common.Utils;
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
        private MeshRenderer m_BetweenLevelTextureRend;
        private int          m_TransitionsCount;

        #endregion

        #region inject

        private IViewGameTicker                      GameTicker      { get; }
        private IFullscreenTransitionTextureProvider TextureProvider { get; }

        private ViewFullscreenTransitioner(
            IViewGameTicker                      _GameTicker,
            IFullscreenTransitionTextureProvider _TextureProvider)
        {
            GameTicker      = _GameTicker;
            TextureProvider = _TextureProvider;
        }

        #endregion

        #region api

        public event UnityAction<bool> TransitionFinished;

        public override void Init()
        {
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
                    TextureProvider.SetTransitionValue(_P);
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