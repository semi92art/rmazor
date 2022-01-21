using System;
using System.Collections;
using System.Collections.Generic;
using Common.CameraProviders;
using Common.Extensions;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Common;
using UnityEngine;

namespace RMAZOR.Views.UI
{
    public class HandSwipeRotation : HandSwipeBase
    {
        #region types
        
        [Serializable]
        public class HandSpriteRotationTraceParams : HandSpriteTraceParamsBase
        {
            public PointPositions a1MoveLeftPositions;
            public PointPositions a1MoveRightPositions;
            public PointPositions a2MoveLeftPositions;
            public PointPositions a2MoveRightPositions;
        }

        #endregion

        #region serialized fields

        [SerializeField] private LineRenderer                  trace1;
        [SerializeField] private LineRenderer                  trace2;
        [SerializeField] private HandSpriteRotationTraceParams @params;

        #endregion

        #region nonpublic members
        
        protected override Dictionary<EMazeMoveDirection, float> m_HandAngles => 
            new Dictionary<EMazeMoveDirection, float>
            {
                {EMazeMoveDirection.Left, -45f},
                {EMazeMoveDirection.Right, 45f}
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
            trace1.startColor = trace1.endColor = uiCol.SetA(0f);
            trace2.startColor = trace2.endColor = uiCol.SetA(0f);
            trace1.widthMultiplier = trace2.widthMultiplier = 1f;
        }
        
        public void ShowRotateClockwisePrompt()
        {
            m_Direction = EMazeMoveDirection.Left;
            m_ReadyToAnimate = true;
        }

        public void ShowRotateCounterClockwisePrompt()
        {
            m_Direction = EMazeMoveDirection.Right;
            m_ReadyToAnimate = true;
        }
        
        public override void HidePrompt()
        {
            base.HidePrompt();
            m_Direction = null;
            trace1.enabled = false;
            trace2.enabled = false;
        }
        
        public override void UpdateTick()
        {
            if (m_Direction.HasValue && m_ReadyToAnimate)
                AnimateHandAndTrace(m_Direction.Value, @params.aTimeEnd);
        }

        #endregion

        #region nonpublic methods
        
        protected override IEnumerator AnimateTraceCoroutine(EMazeMoveDirection _Direction)
        {
            var a1 = _Direction switch
            {
                EMazeMoveDirection.Left  => @params.a1MoveLeftPositions,
                EMazeMoveDirection.Right => @params.a1MoveRightPositions,
            };
            trace1.enabled = false;
            trace1.SetPosition(0, a1.bPos);
            trace1.SetPosition(1, a1.posStart);
            
            var a2 = _Direction switch
            {
                EMazeMoveDirection.Left  => @params.a2MoveLeftPositions,
                EMazeMoveDirection.Right => @params.a2MoveRightPositions,
            };
            trace2.enabled = false;
            trace2.SetPosition(0, a2.bPos);
            trace2.SetPosition(1, a2.posStart);
            yield return Cor.Lerp(
                0f,
                1f,
                @params.aTimeStart,
                _Progress =>
                {
                    hand.color = hand.color.SetA(_Progress);
                    trace1.startColor = trace1.endColor = trace1.startColor.SetA(_Progress * 0.5f);
                    trace2.startColor = trace2.endColor = trace2.startColor.SetA(_Progress * 0.5f);
                },
                m_Ticker,
                _BreakPredicate: () => m_ReadyToAnimate);
            trace1.enabled = trace2.enabled = true;
            yield return Cor.Lerp(
                0f,
                1f,
                @params.aTimeMiddle - @params.aTimeStart, 
                _Progress =>
                {
                    var trace1Pos = Vector2.Lerp(a1.posStart, a1.posMiddle, _Progress);
                    trace1.SetPosition(1, trace1Pos);
                    var trace2Pos = Vector2.Lerp(a2.posStart, a2.posMiddle, _Progress);
                    trace2.SetPosition(1, trace2Pos);
                },
                m_Ticker,
                _BreakPredicate: () => m_ReadyToAnimate);
                var traceCol = trace1.startColor;
                yield return Cor.Lerp(
                    0f,
                    1f,
                    @params.aTimeEnd - @params.aTimeMiddle, 
                    _Progress =>
                    {
                        trace1.SetPosition(1, Vector2.Lerp(a1.posMiddle, a1.posEnd, _Progress));
                        trace1.startColor = trace1.endColor = Color.Lerp(traceCol.SetA(0.5f), traceCol.SetA(0f), _Progress);
                        trace2.SetPosition(1, Vector2.Lerp(a2.posMiddle, a2.posEnd, _Progress));
                        trace2.startColor = trace2.endColor = Color.Lerp(traceCol.SetA(0.5f), traceCol.SetA(0f), _Progress);
                    },
                    m_Ticker,
                    _BreakPredicate: () => m_ReadyToAnimate);
        }

        protected override IEnumerator AnimateHandPositionCoroutine(EMazeMoveDirection _Direction)
        {
            yield return AnimateHandPositionCoroutine(_Direction, @params);
        }

        #endregion
    }
}