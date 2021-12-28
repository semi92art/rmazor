using System;
using System.Collections;
using System.Collections.Generic;
using DI.Extensions;
using Exceptions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.Common;
using Ticker;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.UI
{
    public class HandSwipeMovement : HandSwipeBase
    {
        [Serializable]
        public class HandSpriteMovementTraceParams : HandSpriteTraceParamsBase
        {
            public PointPositions aMoveLeftPositions;
            public PointPositions aMoveRightPositions;
            public PointPositions aMoveDownPositions;
            public PointPositions aMoveUpPositions;
        }
        
        [SerializeField] private LineRenderer                  trace;
        [SerializeField] private HandSpriteMovementTraceParams moveParams;
        
        protected override Dictionary<EMazeMoveDirection, float> m_HandAngles => 
            new Dictionary<EMazeMoveDirection, float>
            {
                {EMazeMoveDirection.Left, 0f},
                {EMazeMoveDirection.Right, 0f},
                {EMazeMoveDirection.Down, 90f},
                {EMazeMoveDirection.Up, 90f},
            };
        
        public override void Init(
            ITicker _Ticker,
            ICameraProvider _CameraProvider,
            IMazeCoordinateConverter _CoordinateConverter,
            IColorProvider _ColorProvider,
            Vector4 _Offsets)
        {
            base.Init(_Ticker, _CameraProvider, _CoordinateConverter, _ColorProvider, _Offsets);
            var uiCol = _ColorProvider.GetColor(ColorIds.UI);
            hand.color = uiCol;
            trace.startColor = trace.endColor = uiCol.SetA(0f);
            trace.widthMultiplier = 1f;
        }

        public void ShowMoveLeftPrompt()
        {
            m_Direction = EMazeMoveDirection.Left;
            m_ReadyToAnimate = true;
        }

        public void ShowMoveRightPrompt()
        {
            m_Direction = EMazeMoveDirection.Right;
            m_ReadyToAnimate = true;
        }

        public void ShowMoveUpPrompt()
        {
            m_Direction = EMazeMoveDirection.Up;
            m_ReadyToAnimate = true;
        }

        public void ShowMoveDownPrompt()
        {
            m_Direction = EMazeMoveDirection.Down;
            m_ReadyToAnimate = true;
        }

        public override void HidePrompt()
        {
            base.HidePrompt();
            trace.enabled = false;
        }

        protected override void Update()
        {
            if (m_Direction.HasValue && m_ReadyToAnimate)
                AnimateHandAndTrace(m_Direction.Value, moveParams.aTimeEnd);
        }

        protected override IEnumerator AnimateTraceCoroutine(EMazeMoveDirection _Direction)
        {
            var a = _Direction switch
            {
                EMazeMoveDirection.Left  => moveParams.aMoveLeftPositions,
                EMazeMoveDirection.Right => moveParams.aMoveRightPositions,
                EMazeMoveDirection.Down  => moveParams.aMoveDownPositions,
                EMazeMoveDirection.Up    => moveParams.aMoveUpPositions,
                _                        => throw new SwitchCaseNotImplementedException(_Direction)
            };
            trace.enabled = false;
            trace.SetPosition(0, a.bPos);
            trace.SetPosition(1, a.posStart);
            yield return Coroutines.Lerp(
                0f,
                1f,
                moveParams.aTimeStart,
                _Progress =>
                {
                    hand.color = hand.color.SetA(_Progress);
                    trace.startColor = trace.endColor = trace.startColor.SetA(_Progress * 0.5f);
                },
                m_Ticker,
                _BreakPredicate: () => m_ReadyToAnimate);
            trace.enabled = true;
            yield return Coroutines.Lerp(
                a.posStart,
                a.posMiddle,
                moveParams.aTimeMiddle - moveParams.aTimeStart, 
                _Value =>
                {
                    trace.SetPosition(1, _Value);
                },
                m_Ticker,
                _BreakPredicate: () => m_ReadyToAnimate);
            var traceCol = trace.startColor;
            yield return Coroutines.Lerp(
                0f,
                1f,
                moveParams.aTimeEnd - moveParams.aTimeMiddle, 
                _Progress =>
                {
                    trace.SetPosition(1, Vector2.Lerp(a.posMiddle, a.posEnd, _Progress));
                    trace.startColor = trace.endColor = Color.Lerp(traceCol.SetA(0.5f), traceCol.SetA(0f), _Progress);
                },
                m_Ticker,
                _BreakPredicate: () => m_ReadyToAnimate);
        }

        protected override IEnumerator AnimateHandPositionCoroutine(EMazeMoveDirection _Direction)
        {
            yield return AnimateHandPositionCoroutine(_Direction, moveParams);
        }
    }
}