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
        
        protected ITicker                  m_Ticker;
        protected ICameraProvider          m_CameraProvider;
        protected IMazeCoordinateConverter m_CoordinateConverter;
        protected IColorProvider           m_ColorProvider;
        protected Vector4                  m_Offsets;
        
        protected bool                m_ReadyToAnimate;
        protected EMazeMoveDirection? m_Direction;
        protected IEnumerator         m_LastTraceAnimCoroutine;
        protected IEnumerator         m_LastHandAnimCoroutine;
        protected IEnumerator         m_LastWaitCoroutine;
        
        protected abstract Dictionary<EMazeMoveDirection, float> m_HandAngles { get; }

        #endregion

        #region api
        
        public virtual void Init(
            ITicker _Ticker,
            ICameraProvider _CameraProvider,
            IMazeCoordinateConverter _CoordinateConverter,
            IColorProvider _ColorProvider,
            Vector4 _Offsets)
        {
            _Ticker.Register(this);
            m_Ticker = _Ticker;
            m_CameraProvider = _CameraProvider;
            m_CoordinateConverter = _CoordinateConverter;
            m_ColorProvider = _ColorProvider;
            m_Offsets = _Offsets;
        }

        public virtual void HidePrompt()
        {
            m_ReadyToAnimate = false;
            hand.enabled = false;
            m_Direction = null;
        }

        #endregion

        #region nonpublic methods
        
        public abstract void UpdateTick();

        protected void AnimateHandAndTrace(EMazeMoveDirection _Direction, float _Time)
        {            
            m_ReadyToAnimate = false;
            Cor.Stop(m_LastTraceAnimCoroutine);
            Cor.Stop(m_LastHandAnimCoroutine);
            Cor.Stop(m_LastWaitCoroutine);
            m_LastTraceAnimCoroutine = AnimateTraceCoroutine(_Direction);
            m_LastHandAnimCoroutine = AnimateHandPositionCoroutine(_Direction);
            m_LastWaitCoroutine = Cor.Delay(
                _Time,
                () => m_ReadyToAnimate = true);
            Cor.Run(m_LastTraceAnimCoroutine);
            Cor.Run(m_LastHandAnimCoroutine);
            Cor.Run(m_LastWaitCoroutine);
            
        }
        
        protected abstract IEnumerator AnimateTraceCoroutine(EMazeMoveDirection _Direction);
        protected abstract IEnumerator AnimateHandPositionCoroutine(EMazeMoveDirection _Direction);
        
        
        protected Dictionary<EMazeMoveDirection, Func<Vector2>> m_HandPositions =>
            new Dictionary<EMazeMoveDirection,Func<Vector2>>
            {
                {
                    EMazeMoveDirection.Left, 
                    () => new Vector2(
                        m_CoordinateConverter.GetMazeCenter().x, 
                        GetScreenBounds().min.y + m_Offsets.z + 10f)
                },
                {
                    EMazeMoveDirection.Right, 
                    () => new Vector2(
                        m_CoordinateConverter.GetMazeCenter().x, 
                        GetScreenBounds().min.y + m_Offsets.z + 10f)
                },
                {
                    EMazeMoveDirection.Down, 
                    () => new Vector2(
                        GetScreenBounds().max.x - m_Offsets.y - 5f,
                        m_CoordinateConverter.GetMazeCenter().y)
                },
                {
                    EMazeMoveDirection.Up, 
                    () => new Vector2(
                        GetScreenBounds().max.x - m_Offsets.y - 5f,
                        m_CoordinateConverter.GetMazeCenter().y)
                },
            };
        
        private Bounds GetScreenBounds()
        {
            return GraphicUtils.GetVisibleBounds(m_CameraProvider.MainCamera);
        }
        
        protected IEnumerator AnimateHandPositionCoroutine(
            EMazeMoveDirection _Direction,
            HandSpriteTraceParamsBase _Params)
        {
            transform.SetPosXY(m_HandPositions[_Direction].Invoke());
            hand.transform.rotation = Quaternion.Euler(0, 0, m_HandAngles[_Direction]);
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
                0f,
                1f,
                _Params.aTimeMiddle,
                _Progress =>
                {
                    var pos = Vector2.Lerp(posStart, posEnd, _Progress);
                    hand.transform.SetLocalPosXY(pos);
                },
                m_Ticker,
                _BreakPredicate: () => m_ReadyToAnimate,
                _ProgressFormula: _P => _P);
            var handCol = hand.color;
            yield return Cor.Lerp(
                0f,
                1f,
                _Params.aTimeEnd - _Params.aTimeMiddle,
                _Progress =>
                {
                    hand.color = Color.Lerp(handCol.SetA(1f), handCol.SetA(0f), _Progress);
                },
                m_Ticker,
                _BreakPredicate: () => m_ReadyToAnimate);
        }

        #endregion
    }
}