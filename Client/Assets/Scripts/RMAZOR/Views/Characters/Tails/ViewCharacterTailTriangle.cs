using System;
using System.Collections;
using Common;
using Common.Constants;
using Common.Extensions;
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

namespace RMAZOR.Views.Characters.Tails
{
    public interface IViewCharacterTail01 : IViewCharacterTail { }
    
    public class ViewCharacterTail01 : InitBase, IViewCharacterTail01, IFixedUpdateTick
    {
        #region constants

        private const float MaxTailLength = 4f;
        
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

        public ViewCharacterTail01(
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
            m_Tail.A = GetStartTailAPosition();
            m_TailBorder.A = GetStartTailAPosition();
            m_CharacterPositionOnMoveStart = GetCharacterObjects().Transform.position;
            m_Tail.SetColor(ColorProvider.GetColor(ColorIds.CharacterTail));
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
                m_Tail.SetColor(ColorProvider.GetColor(ColorIds.CharacterTail).SetA(0f));
                m_TailBorder.SetColor(ColorProvider.GetColor(ColorIds.Character2).SetA(0f));
                m_Tail.A = GetStartTailAPosition();
                m_TailBorder.A = GetStartTailAPosition();
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
            int moveCount = m_MoveCount;
            var tailCol = ColorProvider.GetColor(ColorIds.CharacterTail);
            var tailBorderCol = ColorProvider.GetColor(ColorIds.Character2);
            Vector2 startA = m_Tail.A;
            var finishA = GetStartTailAPosition();
            yield return Cor.Lerp(
                ViewGameTicker,
                10f / ModelSettings.characterSpeed,
                _OnProgress: _P =>
                {
                    var a = Vector2.Lerp(startA, finishA, _P);
                    m_Tail.A = a;
                    m_TailBorder.A = a;
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
            var orth = new Vector2(dir.y, dir.x); //-V3066
            var cP = (Vector2)GetCharacterObjects().Transform.position;
            var b = orth * 0.3f * scale;
            var c = -orth * 0.3f * scale;
            bool isMaxTailLength = Vector2.Distance(m_CharacterPositionOnMoveStart, cP) > MaxTailLength * scale;
            var a = -dir * (isMaxTailLength
                ? MaxTailLength * scale
                : Vector2.Distance(m_CharacterPositionOnMoveStart, cP) + 0.1f);
            m_Tail.A = m_TailBorder.A = a;
            m_Tail.B = m_TailBorder.B = b;
            m_Tail.C = m_TailBorder.C = c;
        }

        private Vector2 GetStartTailAPosition()
        {
            if (m_CurrentMovingArgs == null)
                return Vector2.one * 0.1f;
            var dir = (Vector2)RmazorUtils.GetDirectionVector(
                m_CurrentMovingArgs.Direction, Model.MazeRotation.Orientation);
            return -dir * 0.1f;
        }

        #endregion
    }
}