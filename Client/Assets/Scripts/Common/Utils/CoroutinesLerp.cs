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
        
        public static IEnumerator Lerp(
            Color                    _From,
            Color                    _To,
            float                    _Time,
            UnityAction<Color>       _OnProgress,
            ITicker                  _Ticker,
            UnityAction<bool, Color> _OnFinish        = null,
            Func<bool>               _BreakPredicate  = null,
            Func<Color, Color>       _ProgressFormula = null,
            bool                     _FixedUpdate     = false)
        {
            if (_OnProgress == null)
                yield break;
            float GetTime() => _FixedUpdate ? _Ticker.FixedTime : _Ticker.Time;
            float currTime = GetTime();
            Color progress = _From;
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
                float r = Mathf.Lerp(_From.r, _To.r, timeCoeff);
                float g = Mathf.Lerp(_From.g, _To.g, timeCoeff);
                float b = Mathf.Lerp(_From.b, _To.b, timeCoeff);
                float a = Mathf.Lerp(_From.a, _To.a, timeCoeff);
                progress = new Color(r, g, b, a);
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
        
        public static IEnumerator Lerp(
            Vector2                    _From,
            Vector2                    _To,
            float                      _Time,
            UnityAction<Vector2>       _OnProgress,
            ITicker                    _Ticker,
            UnityAction<bool, Vector2> _OnFinish        = null,
            Func<bool>                 _BreakPredicate  = null,
            Func<Vector2, Vector2>     _ProgressFormula = null,
            bool                       _FixedUpdate     = false)
        {
            if (_OnProgress == null)
                yield break;
            float GetTime() => _FixedUpdate ? _Ticker.FixedTime : _Ticker.Time;
            float currTime = GetTime();
            bool breaked = false;
            Vector2 progress = _From;
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
                progress = Vector2.Lerp(_From, _To, timeCoeff);
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