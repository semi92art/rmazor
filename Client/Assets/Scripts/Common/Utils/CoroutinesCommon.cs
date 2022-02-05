using System;
using System.Collections;
using Common.Ticker;
using UnityEngine;
using UnityEngine.Events;

namespace Common.Utils
{
    public static partial class Cor
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
            float  _Delay,
            UnityAction _OnDelay,
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
            Func<bool> _OnBreak = null,
            float? _Seconds = null,
            ITicker _Ticker = null)
        {
            if (_Action == null || _Predicate == null)
                yield break;
            if (_Seconds.HasValue && _Ticker == null || !_Seconds.HasValue && _Ticker != null)
            {
                Dbg.LogError($"Arguments {nameof(_Seconds)} and {_Ticker} must be not null.");
                yield break;
            }
            float time = _Seconds.HasValue ? _Ticker.Time : default;
            bool IsTimeValid()
            {
                // ReSharper disable once PossibleNullReferenceException
                return _Seconds.HasValue && time + _Seconds.Value > _Ticker.Time;
            }
            while (_Predicate() || IsTimeValid())
            {
                yield return new WaitForEndOfFrame();
                if (_OnBreak != null && _OnBreak())
                    yield break;
            }
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
    }
}