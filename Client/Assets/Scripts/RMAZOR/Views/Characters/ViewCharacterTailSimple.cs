using System.Collections;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Extensions;
using Common.Helpers;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Utils;
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
        private Triangle m_TailBorder;
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
            ModelSettings            _ModelSettings,
            IMazeCoordinateConverter _CoordinateConverter,
            IContainersGetter        _ContainersGetter,
            IViewGameTicker          _GameTicker,
            IColorProvider           _ColorProvider)
        {
            ModelSettings       = _ModelSettings;
            CoordinateConverter = _CoordinateConverter;
            ContainersGetter    = _ContainersGetter;
            GameTicker          = _GameTicker;
            ColorProvider       = _ColorProvider;
        }
        
        #endregion
        
        #region api
        
        public bool Activated
        {
            get => m_Activated;
            set
            {
                if (value && !m_Initialized)
                {
                    ColorProvider.ColorChanged += OnColorChanged;
                    InitShape();
                    m_Initialized = true;
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
            m_ShowTail = _Args.LevelStage == ELevelStage.StartedOrContinued;
            switch (_Args.LevelStage)
            {
                case ELevelStage.Loaded:
                    HideTail();
                    break;
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
            m_Tail.Color = ColorProvider.GetColor(ColorIds.CharacterTail);
            m_TailBorder.Color = Color.black;
            var dir = (Vector2)(_Args.To - _Args.From).NormalizedOrth;
            var orth = new Vector2(dir.y, dir.x); //-V3066
            var currPos = Vector2.Lerp(_Args.From, _Args.To, _Args.Progress);
            var b = currPos - dir * 0.2f + orth * 0.3f;
            var c = currPos - dir * 0.2f - orth * 0.3f;
            var d = (b + c) * 0.5f;
            var a = Vector2.Distance(_Args.From, d) < MaxTailLength ? _Args.From : d - dir * MaxTailLength;
            m_Tail.A = m_TailBorder.A = CoordinateConverter.ToLocalCharacterPosition(a);
            m_Tail.B = m_TailBorder.B = CoordinateConverter.ToLocalCharacterPosition(b);
            m_Tail.C = m_TailBorder.C = CoordinateConverter.ToLocalCharacterPosition(c);
            var mazeCenter = CoordinateConverter.GetMazeCenter();
            m_Tail.gameObject.transform.SetPosXY(mazeCenter);
            m_TailBorder.gameObject.transform.SetPosXY(mazeCenter);
        }

        public void HideTail(CharacterMovingFinishedEventArgs _Args = null)
        {
            if (!m_Initialized)
                return;
            if (_Args == null)
            {
                m_Tail.SetColor(ColorProvider.GetColor(ColorIds.CharacterTail).SetA(0f));
                m_TailBorder.SetColor(Color.black.SetA(0f));
            }
            else
                Cor.Run(HideTailCoroutine(_Args));
        }

        #endregion

        #region nonpublic methods

        private void InitShape()
        {
            var cont = ContainersGetter.GetContainer(ContainerNames.Character);
            m_Tail = cont
                .AddComponentOnNewChild<Triangle>("Character Tail", out _)
                .SetColor(ColorProvider.GetColor(ColorIds.CharacterTail))
                .SetRoundness(0.4f)
                .SetSortingOrder(SortingOrders.Character - 2);
            m_TailBorder = cont
                .AddComponentOnNewChild<Triangle>("Character Tail Border", out _)
                .SetColor(ColorProvider.GetColor(ColorIds.Character2))
                .SetRoundness(0.4f)
                .SetBorder(true)
                .SetThickness(0.1f)
                .SetSortingOrder(SortingOrders.Character - 1);
        }

        private IEnumerator HideTailCoroutine(CharacterMovingFinishedEventArgs _Args)
        {
            var tailCol = ColorProvider.GetColor(ColorIds.CharacterTail);
            var tailBorderCol = ColorProvider.GetColor(ColorIds.Character2);
            m_Hiding = true;
            Vector2 startA = m_Tail.A;
            var dir = (_Args.To - _Args.From).Normalized;
            var finishA = CoordinateConverter.ToLocalCharacterPosition(_Args.To - dir * 0.4f);
            float distance = V2Int.Distance(_Args.From, _Args.To);
            yield return Cor.Lerp(
                GameTicker,
                distance / ModelSettings.characterSpeed,
                _OnProgress: _P =>
                {
                    var a = Vector2.Lerp(startA, finishA, _P);
                    m_Tail.A = a;
                    m_TailBorder.A = a;
                },
                _OnFinish: () =>
                {
                    m_Tail.Color = tailCol.SetA(0f);
                    m_TailBorder.Color = tailBorderCol.SetA(0f);
                },
                _BreakPredicate: () => !m_Hiding || !m_ShowTail);
        }

        #endregion
    }
}