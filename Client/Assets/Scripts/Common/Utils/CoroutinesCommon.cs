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
            float time = _Ticker?.Time ?? Time.time;
            bool IsTimeValid()
            {
                return time + _Seconds.Value > (_Ticker?.Time ?? Time.time);
            }
            bool FinalPredicate()
            {
                return _Seconds.HasValue ? _Predicate() && IsTimeValid() : _Predicate();
            }
            while (FinalPredicate())
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
        
         public static IEnumerator Lerp(
            float                    _From,
            float                    _To,
            float                    _Time,
            UnityAction<float>       _OnProgress,
            ITicker                  _Ticker,
            UnityAction<bool, float> _OnFinish        = null,
            Func<bool>               _BreakPredicate  = null,
            Func<float, float>       _ProgressFormula = null,
            bool                     _FixedUpdate     = false)
        {
            if (_OnProgress == null)
                yield break;
            float GetTime() => _FixedUpdate ? _Ticker.FixedTime : _Ticker.Time;
            float currTime = GetTime();
            float progress = _From;
            bool breaked = false;
            while (GetTime() < currTime + _Time)
            {
                if (_BreakPredicate != null && _BreakPredicate())
                {
                    breaked = true;
                    break;
                }
                if (_Ticker.Pause)
                {
                    yield return PauseCoroutine(_FixedUpdate);
                    continue;
                }
                float timeCoeff = 1 - (currTime + _Time - GetTime()) / _Time;
                progress = Mathf.Lerp(_From, _To, timeCoeff);
                if (_ProgressFormula != null)
                    progress = _ProgressFormula(progress);
                _OnProgress(progress);
                yield return new WaitForEndOfFrame();
            }
            if (_BreakPredicate != null && _BreakPredicate())
                breaked = true;
            if (_Ticker.Pause)
                yield return PauseCoroutine(_FixedUpdate);
            if (!breaked)
            {
                progress = _ProgressFormula?.Invoke(_To) ?? _To;
                _OnProgress(progress);
            }
            if (_Ticker.Pause)
                yield return PauseCoroutine(_FixedUpdate);
            _OnFinish?.Invoke(breaked, breaked ? progress : _To);
        }

        private static IEnumerator PauseCoroutine(bool _FixedUpdate)
        {
            if (_FixedUpdate)
                yield return new WaitForFixedUpdate();
            else 
                yield return new WaitForEndOfFrame();
        }
    }
}