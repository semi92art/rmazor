using System;
using System.Collections;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Extensions;
using Common.Helpers;
using Common.Utils;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.Characters
{
    public class ViewCharacterTailTriangleReversed 
        : InitBase, IViewCharacterTail, IFixedUpdateTick
    {
        #region constants

        private const float MaxTailLength = 2f;
        
        #endregion
        
        #region nonpublic members

        private Triangle m_Tail;
        private Triangle m_TailBorder;
        private bool     m_DoShowTail;
        private bool     m_DoUpdateTailOnFixedUpdate;
        private int      m_MoveCount, m_MoveCountCheck;
        private Vector2  m_CharacterPositionOnMoveStart;
        
        private CharacterMovingStartedEventArgs m_CurrentMovingArgs;
        
        #endregion
        
        #region inject

        private ModelSettings        ModelSettings       { get; }
        private IModelGame           Model               { get; }
        private ICoordinateConverter CoordinateConverter { get; }
        private IContainersGetter    ContainersGetter    { get; }
        private IViewGameTicker      ViewGameTicker      { get; }
        private IColorProvider       ColorProvider       { get; }

        public ViewCharacterTailTriangleReversed(
            ModelSettings        _ModelSettings,
            IModelGame           _Model,
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter    _ContainersGetter,
            IViewGameTicker      _ViewGameTicker,
            IColorProvider       _ColorProvider)
        {
            ModelSettings       = _ModelSettings;
            Model               = _Model;
            CoordinateConverter = _CoordinateConverter;
            ContainersGetter    = _ContainersGetter;
            ViewGameTicker      = _ViewGameTicker;
            ColorProvider       = _ColorProvider;
        }
        
        #endregion
        
        #region api
        
        public bool Activated { get; set; }

        public override void Init()
        {
            ViewGameTicker.Register(this);
            InitShape();
            base.Init();
        }

        public Func<ViewCharacterInfo> GetCharacterObjects { get; set; }
        
        public void OnPathCompleted(V2Int _LastPath)
        {
            HideTail();
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            m_DoShowTail = _Args.LevelStage == ELevelStage.StartedOrContinued;
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
            m_MoveCount++;
            (m_Tail.B, m_Tail.C) = GetStartTailBCPositions();
            (m_TailBorder.B, m_TailBorder.C) = GetStartTailBCPositions();
            m_CharacterPositionOnMoveStart = GetCharacterObjects().Transform.position;
            m_Tail.SetColor(ColorProvider.GetColor(ColorIds.PathFill));
            m_TailBorder.SetColor(ColorProvider.GetColor(ColorIds.Character2));
            m_CurrentMovingArgs = _Args;
            ShowTail(_Args);
        }
        
        public void OnCharacterMoveContinued(CharacterMovingContinuedEventArgs _Args)
        {
            ShowTail(_Args);
        }
        
        public void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            ShowTail(_Args);
            if (m_DoShowTail)
                HideTail(Model.PathItemsProceeder.AllPathsProceeded ? null : _Args);
        }

        public void FixedUpdateTick()
        {
            if (!m_DoUpdateTailOnFixedUpdate)
                return;
            ShowTailCore();
            m_DoUpdateTailOnFixedUpdate = false;
        }

        #endregion

        #region nonpublic methods
        
        private void ShowTail(CharacterMoveEventArgsBase _Args)
        {
            if (_Args.From == _Args.To)
                return;
            if (!m_DoShowTail)
                return;
            m_DoUpdateTailOnFixedUpdate = true;
        }

        private void HideTail(CharacterMovingFinishedEventArgs _Args = null)
        {
            if (!Initialized)
                return;
            if (_Args == null)
            {
                m_Tail.SetColor(ColorProvider.GetColor(ColorIds.PathFill).SetA(0f));
                m_TailBorder.SetColor(ColorProvider.GetColor(ColorIds.Character2).SetA(0f));
                (m_Tail.B, m_Tail.C) = GetStartTailBCPositions();
                (m_TailBorder.B, m_TailBorder.C) = GetStartTailBCPositions();
            }
            else
            {
                if (_Args.From == _Args.To)
                    return;
                Cor.Run(HideTailCoroutine(_Args));
            }
        }
        
        private void InitShape()
        {
            var cont = ContainersGetter.GetContainer(ContainerNamesMazor.Character);
            m_Tail = cont
                .AddComponentOnNewChild<Triangle>("Character Tail", out _)
                .SetColor(ColorProvider.GetColor(ColorIds.PathFill))
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
            int moveCount = m_MoveCount;
            var tailCol = ColorProvider.GetColor(ColorIds.PathFill);
            var tailBorderCol = ColorProvider.GetColor(ColorIds.Character2);
            Vector2 startB = m_Tail.A;
            Vector2 startC = m_Tail.A;
            Vector2 finishB, finishC;
            (finishB, finishC) = GetStartTailBCPositions();
            yield return Cor.Lerp(
                ViewGameTicker,
                10f / ModelSettings.characterSpeed,
                _OnProgress: _P =>
                {
                    var b = Vector2.Lerp(startB, finishB, _P);
                    var c = Vector2.Lerp(startC, finishC, _P);
                    m_Tail.B = m_TailBorder.B = b;
                    m_Tail.C = m_TailBorder.C = c;
                },
                _OnFinishEx: (_Broken, _Progress) =>
                {
                    if (_Broken)
                        return;
                    m_Tail.Color = tailCol.SetA(0f);
                    m_TailBorder.Color = tailBorderCol.SetA(0f);
                },
                _BreakPredicate: () => moveCount != m_MoveCount || !m_DoShowTail);
        }

        private void ShowTailCore()
        {
            float scale = CoordinateConverter.Scale;
            var dir = (Vector2)RmazorUtils.GetDirectionVector(
                m_CurrentMovingArgs.Direction, Model.MazeRotation.Orientation);
            var cP = (Vector2)GetCharacterObjects().Transform.position;
            bool isMaxTailLength = Vector2.Distance(m_CharacterPositionOnMoveStart, cP) > MaxTailLength * scale;
            var addict = -dir * (isMaxTailLength
                ? MaxTailLength * scale
                : Vector2.Distance(m_CharacterPositionOnMoveStart, cP) + 0.1f);
            Vector2 b, c;
            (b, c) = GetStartTailBCPositions();
            b += addict;
            c += addict;
            m_Tail.A = m_TailBorder.A = default;
            m_Tail.B = m_TailBorder.B = b;
            m_Tail.C = m_TailBorder.C = c;
        }

        private Tuple<Vector2, Vector2> GetStartTailBCPositions()
        {
            if (m_CurrentMovingArgs == null)
                return new Tuple<Vector2, Vector2>(Vector2.one * 0.1f, Vector2.one * -0.1f);
            float scale = CoordinateConverter.Scale;
            var dir = (Vector2)RmazorUtils.GetDirectionVector(
                m_CurrentMovingArgs.Direction, Model.MazeRotation.Orientation);
            var orth = new Vector2(dir.y, dir.x); //-V3066
            var b = orth * 0.3f * scale;
            var c = -orth * 0.3f * scale;
            return new Tuple<Vector2, Vector2>(b,c);
            // if (m_CurrentMovingArgs == null)
            //     return Vector2.one * 0.1f;
            // var dir = (Vector2)RmazorUtils.GetDirectionVector(
            //     m_CurrentMovingArgs.Direction, Model.MazeRotation.Orientation);
            // return -dir * 0.1f;
        }
        
        #endregion
    }
}