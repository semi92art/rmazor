using System;
using System.Collections;
using System.Collections.Generic;
using Common.CameraProviders;
using Common.Exceptions;
using Common.Extensions;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Common;
using UnityEngine;

namespace RMAZOR.Views.UI
{
    public class HandSwipeMovement : HandSwipeBase
    {
        #region types

        [Serializable]
        public class HandSpriteMovementTraceParams : HandSpriteTraceParamsBase
        {
            public PointPositions aMoveLeftPositions;
            public PointPositions aMoveRightPositions;
            public PointPositions aMoveDownPositions;
            public PointPositions aMoveUpPositions;
        }
        
        #endregion

        #region serialized fields

        [SerializeField] private LineRenderer                  trace;
        [SerializeField] private HandSpriteMovementTraceParams moveParams;

        #endregion

        #region nonpublic members
        
        protected override Dictionary<EMazeMoveDirection, float> m_HandAngles => 
            new Dictionary<EMazeMoveDirection, float>
            {
                {EMazeMoveDirection.Left, 0f},
                {EMazeMoveDirection.Right, 0f},
                {EMazeMoveDirection.Down, 90f},
                {EMazeMoveDirection.Up, 90f},
            };

        #endregion

        #region api
        
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
        
        public override void UpdateTick()
        {
            if (m_Direction.HasValue && m_ReadyToAnimate)
                AnimateHandAndTrace(m_Direction.Value, moveParams.aTimeEnd);
        }
        
        #endregion

        #region nonpublic methods
        
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
            yield return Cor.Lerp(
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
            yield return Cor.Lerp(
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
            yield return Cor.Lerp(
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

        #endregion
    }
}