using System.Collections;
using Common.Constants;
using Common.Entities;
using Common.Extensions;
using Common.Helpers;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Common;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.Characters
{
    public class ViewCharacterTailSimple : IViewCharacterTail
    {
        #region constants

        private const float MaxTailLength = 4f;
        
        #endregion
        
        #region nonpublic members

        private Triangle m_Tail;
        private bool     m_Hiding;
        private bool     m_Activated;
        private bool     m_Initialized;
        private bool     m_ShowTail;
        
        #endregion
        
        #region inject

        private ModelSettings            ModelSettings       { get; }
        private IMazeCoordinateConverter CoordinateConverter { get; }
        private IContainersGetter        ContainersGetter    { get; }
        private IViewGameTicker          GameTicker          { get; }
        private IColorProvider           ColorProvider       { get; }

        public ViewCharacterTailSimple(
            ModelSettings _ModelSettings,
            IMazeCoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IViewGameTicker _GameTicker,
            IColorProvider _ColorProvider)
        {
            ModelSettings = _ModelSettings;
            CoordinateConverter = _CoordinateConverter;
            ContainersGetter = _ContainersGetter;
            GameTicker = _GameTicker;
            ColorProvider = _ColorProvider;
        }
        
        #endregion
        
        #region api
        
        public bool Activated
        {
            get => m_Activated;
            set
            {
                if (value)
                {
                    if (!m_Initialized)
                    {
                        ColorProvider.ColorChanged += OnColorChanged;
                        InitShape();
                        m_Initialized = true;
                    }
                }
                m_Activated = value;
            }
        }

        private void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId == ColorIds.CharacterTail)
                m_Tail.Color = _Color;
        }

        public void OnAllPathProceed(V2Int _LastPath)
        {
            HideTail();
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            m_ShowTail = _Args.Stage == ELevelStage.StartedOrContinued;
            switch (_Args.Stage)
            {
                case ELevelStage.ReadyToStart:
                case ELevelStage.StartedOrContinued:
                    Activated = true;
                    HideTail();
                    break;
                case ELevelStage.CharacterKilled:
                    HideTail();
                    break;
            }
        }
        
        public void OnCharacterMoveStarted(CharacterMovingStartedEventArgs _Args)
        {
            ShowTail(_Args);
        }
        
        public void OnCharacterMoveContinued(CharacterMovingContinuedEventArgs _Args)
        {
            ShowTail(_Args);
        }
        
        public void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            if (m_ShowTail)
                HideTail(_Args);
        }

        public void ShowTail(CharacterMoveEventArgsBase _Args)
        {
            if (!m_ShowTail)
                return;
            m_Hiding = false;
            m_Tail.enabled = true;
            var dir = (Vector2)(_Args.To - _Args.From).NormalizedOrth;
            var orth = new Vector2(dir.y, dir.x); //-V3066
            var currPos = Vector2.Lerp(_Args.From, _Args.To, _Args.Progress);
            var b = currPos - dir * 0.2f + orth * 0.3f;
            var c = currPos - dir * 0.2f - orth * 0.3f;
            var d = (b + c) * 0.5f;
            var a = Vector2.Distance(_Args.From, d) < MaxTailLength ? _Args.From : d - dir * MaxTailLength;
            m_Tail.A = CoordinateConverter.ToLocalCharacterPosition(a);
            m_Tail.B = CoordinateConverter.ToLocalCharacterPosition(b);
            m_Tail.C = CoordinateConverter.ToLocalCharacterPosition(c);
            m_Tail.gameObject.transform.SetPosXY(CoordinateConverter.GetMazeCenter());
        }

        public void HideTail(CharacterMovingFinishedEventArgs _Args = null)
        {
            Cor.Run(Cor.WaitWhile(
                        () => !m_Initialized,
                        () =>
                        {
                            m_Tail.enabled = _Args != null;
                            if (_Args != null)                
                                Cor.Run(HideTailCoroutine(_Args));
                        }));
        }

        #endregion

        #region nonpublic methods

        private void InitShape()
        {
            var go = new GameObject("Character Tail");
            go.SetParent(ContainersGetter.GetContainer(ContainerNames.Character));
            m_Tail = go.AddComponent<Triangle>();
            m_Tail.Color = ColorProvider.GetColor(ColorIds.CharacterTail);
            m_Tail.Roundness = 0.4f;
            m_Tail.enabled = false;
        }

        private IEnumerator HideTailCoroutine(CharacterMovingFinishedEventArgs _Args)
        {
            m_Hiding = true;
            Vector2 startA = m_Tail.A;
            var dir = (_Args.To - _Args.From).Normalized;
            var finishA = CoordinateConverter.ToLocalCharacterPosition(_Args.To - dir * 0.4f);
            var distance = V2Int.Distance(_Args.From, _Args.To);
            yield return Cor.Lerp(
                0f,
                1f,
                distance / ModelSettings.CharacterSpeed,
                _Progress =>
                {
                    m_Tail.A = Vector2.Lerp(startA, finishA, _Progress);
                },
                GameTicker,
                (_Breaked, _Progress) => m_Tail.enabled = false,
                () => !m_Hiding || !m_ShowTail);
        }

        #endregion
    }
}