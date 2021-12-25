using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DI.Extensions;
using Ticker;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Utils
{
    public static partial class Coroutines
    {
        public static IEnumerator Action(UnityAction _Action)
        {
            _Action?.Invoke();
            yield break;
        }
    
        public static IEnumerator WaitEndOfFrame(UnityAction _Action, uint _NumOfFrames = 1)
        {
            for (uint i = 0; i < _NumOfFrames; i++)
                yield return new WaitForEndOfFrame();
            _Action?.Invoke();
        }

        public static IEnumerator Delay(
            UnityAction _OnDelay,
            float  _Delay,
            Func<bool> _OnBreak = null
        )
        {
            if (_Delay > float.Epsilon)
                yield return new WaitForSeconds(_Delay);
            if (_OnBreak != null && _OnBreak())
                yield break;
            _OnDelay?.Invoke();
        }

        public static IEnumerator WaitWhile(
            Func<bool> _Predicate,
            UnityAction _Action,
            Func<bool> _OnBreak = null)
        {
            if (_Action == null || _Predicate == null)
                yield break;
            while (_Predicate())
                yield return new WaitForEndOfFrame();
            if (_OnBreak != null && _OnBreak())
                yield break;
            _Action();
        }
        
        public static IEnumerator DoWhile(
            Func<bool> _Predicate,
            UnityAction _Action,
            UnityAction _FinishAction,
            ITicker _Ticker,
            Func<bool> _Pause = null,
            bool _WaitEndOfFrame = true,
            bool _FixedUpdate = false)
        {
            if (_Action == null || _Predicate == null)
                yield break;

            while (_Predicate.Invoke())
            {
                if (_Pause != null && _Pause())
                    continue;
                _Action();
                if (!_WaitEndOfFrame) 
                    continue;
                float dt = _FixedUpdate ? _Ticker.FixedDeltaTime : _Ticker.DeltaTime;
                yield return new WaitForSecondsRealtime(dt);
            }
        
            _FinishAction?.Invoke();
        }

        public static IEnumerator Repeat(
            UnityAction _Action,
            float _RepeatDelta,
            float _RepeatTime,
            ITicker _Ticker,
            Func<bool> _DoStop = null,
            UnityAction _OnFinish = null)
        {
            if (_Action == null)
                yield break;
            
            float startTime = _Ticker.Time;
            
            while (_Ticker.Time - startTime < _RepeatTime 
                   && (_DoStop == null || !_DoStop()))
            {
                _Action();
                yield return new WaitForSeconds(_RepeatDelta);
            }
            
            _OnFinish?.Invoke();
        }
        
        public static IEnumerator Repeat(
            UnityAction _Action,
            float _RepeatDelta,
            long _RepeatCount,
            ITicker _Ticker,
            Func<bool> _DoStop = null,
            UnityAction _OnFinish = null)
        {
            if (_Action == null)
                yield break;
            float repeatTime = _RepeatDelta * _RepeatCount;
            float startTime = _Ticker.Time;
            while (_Ticker.Time - startTime < repeatTime 
                   && (_DoStop == null || !_DoStop()))
            {
                _Action();
                yield return new WaitForSeconds(_RepeatDelta);
            }
            
            _OnFinish?.Invoke();
        }
    }
}