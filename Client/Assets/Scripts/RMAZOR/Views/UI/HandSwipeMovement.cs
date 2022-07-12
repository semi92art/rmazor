using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using Common.CameraProviders;
using Common.Exceptions;
using Common.Extensions;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Coordinate_Converters;
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
        
        protected override Dictionary<EMazeMoveDirection, float> HandAngles => 
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
            ITicker                  _Ticker,
            ICameraProvider          _CameraProvider,
            ICoordinateConverter _CoordinateConverter,
            IColorProvider           _ColorProvider,
            Vector4                  _Offsets)
        {
            base.Init(_Ticker, _CameraProvider, _CoordinateConverter, _ColorProvider, _Offsets);
            var uiCol = _ColorProvider.GetColor(ColorIds.UI);
            hand.color = uiCol;
            trace.startColor = trace.endColor = uiCol.SetA(0f);
            trace.widthMultiplier = 1f;
        }

        public void ShowMoveLeftPrompt()
        {
            Direction = EMazeMoveDirection.Left;
            ReadyToAnimate = true;
        }

        public void ShowMoveRightPrompt()
        {
            Direction = EMazeMoveDirection.Right;
            ReadyToAnimate = true;
        }

        public void ShowMoveUpPrompt()
        {
            Direction = EMazeMoveDirection.Up;
            ReadyToAnimate = true;
        }

        public void ShowMoveDownPrompt()
        {
            Direction = EMazeMoveDirection.Down;
            ReadyToAnimate = true;
        }

        public override void HidePrompt()
        {
            base.HidePrompt();
            trace.enabled = false;
        }
        
        public override void UpdateTick()
        {
            if (Direction.HasValue && ReadyToAnimate)
                AnimateHandAndTrace(Direction.Value, moveParams.aTimeEnd);
        }
        
        #endregion

        #region nonpublic methods
        
        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            base.OnColorChanged(_ColorId, _Color);
            if (_ColorId != ColorIds.UI)
                return;
            var oldCol = trace.startColor;
            var newCol = oldCol.SetR(_Color.r).SetG(_Color.g).SetB(_Color.b);
            trace.startColor = trace.endColor = newCol;
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
            if (TutorialFinished)
                yield break;
            yield return Cor.Lerp(
                Ticker,
                moveParams.aTimeStart,
                _OnProgress: _P =>
                {
                    hand.color = hand.color.SetA(_P);
                    trace.startColor = trace.endColor = trace.startColor.SetA(_P * 0.5f);
                },
                _BreakPredicate: () => ReadyToAnimate || TutorialFinished);
            if (TutorialFinished)
                yield break;
            trace.enabled = true;
            yield return Cor.Lerp(
                Ticker,
                moveParams.aTimeMiddle - moveParams.aTimeStart, 
                _OnProgress: _P =>
                {
                    var vec = Vector2.Lerp(a.posStart, a.posMiddle, _P);
                    trace.SetPosition(1, vec);
                },
                _BreakPredicate: () => ReadyToAnimate || TutorialFinished);
            if (TutorialFinished)
                yield break;
            var traceCol = trace.startColor;
            yield return Cor.Lerp(
                Ticker,
                moveParams.aTimeEnd - moveParams.aTimeMiddle, 
                _OnProgress: _P =>
                {
                    trace.SetPosition(1, Vector2.Lerp(a.posMiddle, a.posEnd, _P));
                    trace.startColor = trace.endColor = Color.Lerp(traceCol.SetA(0.5f), traceCol.SetA(0f), _P);
                },
                _BreakPredicate: () => ReadyToAnimate || TutorialFinished);
        }

        protected override IEnumerator AnimateHandPositionCoroutine(EMazeMoveDirection _Direction)
        {
            yield return AnimateHandPositionCoroutine(_Direction, moveParams);
        }

        #endregion
    }
}