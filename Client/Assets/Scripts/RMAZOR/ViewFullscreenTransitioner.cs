using System.Collections;
using mazing.common.Runtime;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using RMAZOR.Views;
using RMAZOR.Views.Common.FullscreenTextureProviders;
using UnityEngine;
using UnityEngine.Events;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR
{
    public interface IViewFullscreenTransitioner : IInit, IOnLevelStageChanged, ISendMessage
    {
        event UnityAction<bool> TransitionFinished;
        void                    DoTextureTransition(bool _Appear, float _Duration);
        bool                    Enabled { get; set; }
    }
    
    public class ViewFullscreenTransitioner : InitBase, IViewFullscreenTransitioner
    {
        #region nonpublic members

        private IEnumerator  m_TextureCoroutine;
        private int          m_TransitionsCount;

        private IFullscreenTransitionTextureProvider CurrentProvider { get; set; }

        private string m_TextureProviderDefaultName;

        #endregion

        #region inject

        private ViewSettings                             ViewSettings        { get; }
        private ICameraProvider                          CameraProvider      { get; }
        private IViewGameTicker                          GameTicker          { get; }
        private IFullscreenTransitionTextureProvidersSet TextureProvidersSet { get; }

        private ViewFullscreenTransitioner(
            ViewSettings                             _ViewSettings,
            ICameraProvider                          _CameraProvider,
            IViewGameTicker                          _GameTicker,
            IFullscreenTransitionTextureProvidersSet _TextureProvidersSet)
        {
            ViewSettings        = _ViewSettings;
            CameraProvider      = _CameraProvider;
            GameTicker          = _GameTicker;
            TextureProvidersSet = _TextureProvidersSet;
        }

        #endregion

        #region api

        public event UnityAction<bool> TransitionFinished;

        public override void Init()
        {
            CameraProvider.ActiveCameraChanged += OnActiveCameraChanged;
            TextureProvidersSet.Init();
            m_TextureProviderDefaultName = ViewSettings.betweenLevelsTransitionTextureName;
            CurrentProvider = TextureProvidersSet.GetTextureProvider(m_TextureProviderDefaultName);
            Enabled = false;
            base.Init();
        }

        public void DoTextureTransition(bool _Appear, float _Duration)
        {
            m_TransitionsCount++;
            Cor.Stop(m_TextureCoroutine);
            m_TextureCoroutine = DoTextureTransitionCoroutine(_Appear, _Duration);
            Cor.Run(m_TextureCoroutine);
        }

        public bool Enabled
        {
            get => CurrentProvider.Renderer.enabled;
            set => CurrentProvider.Renderer.enabled = value;
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            string gameMode = (string)_Args.Arguments.GetSafe(KeyGameMode, out _);
            string providerName = _Args.LevelStage switch
            {
                ELevelStage.None when _Args.PreviousStage == ELevelStage.None       => m_TextureProviderDefaultName,
                ELevelStage.Finished when _Args.PreviousStage != ELevelStage.Paused => m_TextureProviderDefaultName,
                ELevelStage.ReadyToUnloadLevel when _Args.PreviousStage != ELevelStage.Paused =>
                    gameMode != ParameterGameModePuzzles || !_Args.Arguments.ContainsKey(KeyShowPuzzleLevelHint)
                        ? m_TextureProviderDefaultName
                        : "circles",
                _ => null
            };
            if (string.IsNullOrEmpty(providerName))
                return;
            CurrentProvider.Activate(false);
            CurrentProvider = TextureProvidersSet.GetTextureProvider(providerName);
        }
        
        public void SendMessage(string _Message)
        {
            CurrentProvider.Activate(false);
            if (_Message == "puzzle_hint")
                CurrentProvider = TextureProvidersSet.GetTextureProvider("circles");
        }

        #endregion

        #region nonpublic members
        
        private void OnActiveCameraChanged(Camera _Camera)
        {
            CurrentProvider.Renderer.transform.SetParent(_Camera.transform);
            CurrentProvider.Renderer.transform.SetLocalPosXY(Vector2.zero);
        }

        private IEnumerator DoTextureTransitionCoroutine(bool _Appear, float _Duration)
        {
            int transitionsCount = m_TransitionsCount;
            if (_Appear)
                CurrentProvider.Activate(true);
            yield return Cor.Lerp(
                GameTicker,
                _Duration,
                _Appear ? 0f : 1f,
                _Appear ? 1f : 0f,
                _P =>
                {
                    CurrentProvider.SetTransitionValue(_P, _Appear);
                },
                () =>
                {
                    TransitionFinished?.Invoke(_Appear);
                    if (!_Appear)
                        CurrentProvider.Activate(false);
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