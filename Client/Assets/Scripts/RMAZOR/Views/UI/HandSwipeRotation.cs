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
        
        protected override Dictionary<EMazeMoveDirection, float> HandAngles => 
            new Dictionary<EMazeMoveDirection, float>
            {
                {EMazeMoveDirection.Left, -45f},
                {EMazeMoveDirection.Right, 45f}
            };

        #endregion
        
        #region api

        public override void Init(
            ITicker                  _Ticker,
            ICameraProvider          _CameraProvider,
            IMazeCoordinateConverter _CoordinateConverter,
            IColorProvider           _ColorProvider,
            Vector4                  _Offsets)
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
            Direction = EMazeMoveDirection.Left;
            ReadyToAnimate = true;
        }

        public void ShowRotateCounterClockwisePrompt()
        {
            Direction = EMazeMoveDirection.Right;
            ReadyToAnimate = true;
        }
        
        public override void HidePrompt()
        {
            base.HidePrompt();
            Direction = null;
            trace1.enabled = false;
            trace2.enabled = false;
        }
        
        public override void UpdateTick()
        {
            if (Direction.HasValue && ReadyToAnimate)
                AnimateHandAndTrace(Direction.Value, @params.aTimeEnd);
        }

        #endregion

        #region nonpublic methods

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            base.OnColorChanged(_ColorId, _Color);
            if (_ColorId != ColorIds.UI)
                return;
            var oldCol = trace1.startColor;
            var newCol = oldCol.SetR(_Color.r).SetG(_Color.g).SetB(_Color.b);
            trace1.startColor = trace1.endColor = newCol;
            trace2.startColor = trace2.endColor = newCol;
        }

        protected override IEnumerator AnimateTraceCoroutine(EMazeMoveDirection _Direction)
        {
#pragma warning disable 8509
            var a1 = _Direction switch
            {
                EMazeMoveDirection.Left  => @params.a1MoveLeftPositions,
                EMazeMoveDirection.Right => @params.a1MoveRightPositions,
            };
#pragma warning restore 8509
            trace1.enabled = false;
            trace1.SetPosition(0, a1.bPos);
            trace1.SetPosition(1, a1.posStart);
#pragma warning disable 8509
            var a2 = _Direction switch
            {
                EMazeMoveDirection.Left  => @params.a2MoveLeftPositions,
                EMazeMoveDirection.Right => @params.a2MoveRightPositions,
            };
#pragma warning restore 8509
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
                Ticker,
                _BreakPredicate: () => ReadyToAnimate);
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
                Ticker,
                _BreakPredicate: () => ReadyToAnimate);
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
                    Ticker,
                    _BreakPredicate: () => ReadyToAnimate);
        }

        protected override IEnumerator AnimateHandPositionCoroutine(EMazeMoveDirection _Direction)
        {
            yield return AnimateHandPositionCoroutine(_Direction, @params);
        }

        #endregion
    }
}