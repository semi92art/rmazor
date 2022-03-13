using System;
using System.Collections;
using Common.Ticker;
using UnityEngine;
using UnityEngine.Events;

namespace Common.Utils
{
    public static partial class Cor
    {
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