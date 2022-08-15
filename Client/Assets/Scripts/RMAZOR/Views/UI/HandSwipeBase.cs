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
    public abstract class HandSwipeBase : MonoBehaviour, IUpdateTick
    {
        #region types
        
        [Serializable]
        public class PointPositions
        {
            public Vector2 posStart;
            public Vector2 posMiddle;
            public Vector2 posEnd;
            public Vector2 bPos;
        }
        
        [Serializable]
        public abstract class HandSpriteTraceParamsBase
        {
            public float aTimeStart;
            public float aTimeMiddle;
            public float aTimeEnd;
        }

        #endregion

        #region serialized fields

        [SerializeField] protected SpriteRenderer hand;

        #endregion

        #region nonpublic members

        protected IUnityTicker         Ticker;
        private   ICameraProvider      m_CameraProvider;
        private   ICoordinateConverter m_CoordinateConverter;
        private   Vector4              m_Offsets;
        protected bool                 ReadyToAnimate;
        protected EMazeMoveDirection?  Direction;
        private   IEnumerator          m_LastTraceAnimCoroutine;
        private   IEnumerator          m_LastHandAnimCoroutine;
        private   IEnumerator          m_LastWaitCoroutine;
        protected bool                 TutorialFinished;
        
        protected abstract Dictionary<EMazeMoveDirection, float> HandAngles { get; }

        #endregion

        #region api

        public virtual void Init(
            IUnityTicker         _Ticker,
            ICameraProvider      _CameraProvider,
            ICoordinateConverter _CoordinateConverter,
            IColorProvider       _ColorProvider,
            Vector4              _Offsets)
        {
            _Ticker.Register(this);
            Ticker = _Ticker;
            m_CameraProvider = _CameraProvider;
            m_CoordinateConverter = _CoordinateConverter;
            m_Offsets = _Offsets;
            _ColorProvider.ColorChanged += OnColorChanged;
        }

        public virtual void HidePrompt()
        {
            ReadyToAnimate = false;
            hand.enabled = false;
            Direction = null;
            TutorialFinished = true;
        }

        public abstract void UpdateTick();
        
        #endregion

        #region nonpublic methods

        protected virtual void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId != ColorIds.UI)
                return;
            hand.color = _Color;
        }

        protected void AnimateHandAndTrace(EMazeMoveDirection _Direction, float _Time)
        {            
            ReadyToAnimate = false;
            Cor.Stop(m_LastTraceAnimCoroutine);
            Cor.Stop(m_LastHandAnimCoroutine);
            Cor.Stop(m_LastWaitCoroutine);
            m_LastTraceAnimCoroutine = AnimateTraceCoroutine(_Direction);
            m_LastHandAnimCoroutine = AnimateHandPositionCoroutine(_Direction);
            m_LastWaitCoroutine = Cor.Delay(_Time, Ticker, () => ReadyToAnimate = true);
            Cor.Run(m_LastTraceAnimCoroutine);
            Cor.Run(m_LastHandAnimCoroutine);
            Cor.Run(m_LastWaitCoroutine);
            
        }
        
        protected abstract IEnumerator AnimateTraceCoroutine(EMazeMoveDirection _Direction);
        protected abstract IEnumerator AnimateHandPositionCoroutine(EMazeMoveDirection _Direction);


        private Dictionary<EMazeMoveDirection, Func<Vector2>> HandPositions
        {
            get
            {
                var mazeCenter = m_CoordinateConverter.GetMazeBounds().center;
                var screeenBds = GetScreenBounds();
                return new Dictionary<EMazeMoveDirection, Func<Vector2>>
                {
                    {
                        EMazeMoveDirection.Left,
                        () => new Vector2(mazeCenter.x, screeenBds.min.y + m_Offsets.z + 10f)
                    },
                    {
                        EMazeMoveDirection.Right,
                        () => new Vector2(mazeCenter.x, screeenBds.min.y + m_Offsets.z + 10f)
                    },
                    {
                        EMazeMoveDirection.Down,
                        () => new Vector2(screeenBds.max.x - m_Offsets.y - 5f, mazeCenter.y)
                    },
                    {
                        EMazeMoveDirection.Up,
                        () => new Vector2(screeenBds.max.x - m_Offsets.y - 5f, mazeCenter.y)
                    },
                };
            }
        }

        private Bounds GetScreenBounds()
        {
            return GraphicUtils.GetVisibleBounds(m_CameraProvider.Camera);
        }
        
        protected IEnumerator AnimateHandPositionCoroutine(
            EMazeMoveDirection _Direction,
            HandSpriteTraceParamsBase _Params)
        {
            transform.SetPosXY(HandPositions[_Direction].Invoke());
            hand.transform.rotation = Quaternion.Euler(0, 0, HandAngles[_Direction]);
            Vector2 posStart, posEnd;
            switch (_Direction)
            {
                case EMazeMoveDirection.Left:
                    posStart = new Vector2(2f, 0f);
                    posEnd = new Vector2(-2f, 0f);
                    break;
                case EMazeMoveDirection.Right:
                    posStart = new Vector2(-2f, 0f);
                    posEnd = new Vector2(2f, 0f);
                    break;
                case EMazeMoveDirection.Down:
                    posStart = new Vector2(0f, 2f);
                    posEnd = new Vector2(0f, -2f);
                    break;
                case EMazeMoveDirection.Up:
                    posStart = new Vector2(0f, -2f);
                    posEnd = new Vector2(0f, 2f);
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Direction);
            }
            yield return Cor.Lerp(
                Ticker,
                _Params.aTimeMiddle,
                _OnProgress: _P =>
                {
                    var pos = Vector2.Lerp(posStart, posEnd, _P);
                    hand.transform.SetLocalPosXY(pos);
                },
                _BreakPredicate: () => ReadyToAnimate,
                _ProgressFormula: _P => _P);
            var handCol = hand.color;
            yield return Cor.Lerp(
                Ticker,
                _Params.aTimeEnd - _Params.aTimeMiddle,
                _OnProgress: _P =>
                {
                    hand.color = Color.Lerp(handCol.SetA(1f), handCol.SetA(0f), _P);
                },
                _BreakPredicate: () => ReadyToAnimate);
        }

        #endregion
    }
}