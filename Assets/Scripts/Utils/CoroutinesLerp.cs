using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Utils
{
    public static partial class Coroutines
    {
        public static IEnumerator Lerp(
            long _From,
            long _To,
            float _Time,
            UnityAction<long> _Result,
            ITimeProvider _TimeProvider,
            UnityAction _OnFinish = null,
            Func<bool> _OnBreak = null)
        {
            if (_Result == null)
                yield break;
            _Result(_From);

            float currTime = _TimeProvider.Time;
            while (_TimeProvider.Time < currTime + _Time)
            {
                float timeCoeff = 1 - (currTime + _Time - _TimeProvider.Time) / _Time;
                long newVal = MathUtils.Lerp(_From, _To, timeCoeff);
                _Result(newVal);
                bool? isBreak = _OnBreak?.Invoke();
                if (isBreak.HasValue && isBreak.Value)
                    yield break;
                yield return new WaitForEndOfFrame();
            }
            
            _Result(_To);
            _OnFinish?.Invoke();
        }
        
        public static IEnumerator Lerp(
            float _From,
            float _To,
            float _Time,
            UnityAction<float> _Result,
            ITimeProvider _TimeProvider,
            UnityAction _OnFinish = null,
            Func<bool> _OnBreak = null)
        {
            if (_Result == null)
                yield break;
            _Result(_From);

            float currTime = _TimeProvider.Time;
            while (_TimeProvider.Time < currTime + _Time)
            {
                if (_OnBreak != null && _OnBreak())
                    yield break;
                if (_TimeProvider.Pause)
                {
                    yield return new WaitForEndOfFrame();
                    continue;
                }
                
                float timeCoeff = 1 - (currTime + _Time - _TimeProvider.Time) / _Time;
                float newVal = Mathf.Lerp(_From, _To, timeCoeff);
                _Result(newVal);
                yield return new WaitForEndOfFrame();
            }
            
            _Result(_To);
            _OnFinish?.Invoke();
        }

        public static IEnumerator Lerp(
            int _From,
            int _To,
            float _Time,
            UnityAction<int> _Result,
            ITimeProvider _TimeProvider,
            UnityAction _OnFinish = null)
        {
            if (_Result == null)
                yield break;
            _Result(_From);

            float currTime = _TimeProvider.Time;
            while (_TimeProvider.Time < currTime + _Time)
            {
                float timeCoeff = 1 - (currTime + _Time - _TimeProvider.Time) / _Time;
                int newVal = Mathf.RoundToInt(Mathf.Lerp(_From, _To, timeCoeff));
                _Result(newVal);
                yield return new WaitForEndOfFrame();
            }
            
            _Result(_To);
            _OnFinish?.Invoke();
        }
        
        public static IEnumerator Lerp(
            Color _From,
            Color _To,
            float _Time,
            UnityAction<Color> _Result,
            ITimeProvider _TimeProvider,
            UnityAction _OnFinish = null)
        {
            if (_Result == null)
                yield break;
            _Result(_From);

            float currTime = _TimeProvider.Time;
            while (_TimeProvider.Time < currTime + _Time)
            {
                float timeCoeff = 1 - (currTime + _Time - _TimeProvider.Time) / _Time;
                float r = Mathf.Lerp(_From.r, _To.r, timeCoeff);
                float g = Mathf.Lerp(_From.g, _To.g, timeCoeff);
                float b = Mathf.Lerp(_From.b, _To.b, timeCoeff);
                float a = Mathf.Lerp(_From.a, _To.a, timeCoeff);
                var newColor = new Color(r, g, b, a);
                _Result(newColor);
                yield return new WaitForEndOfFrame();
            }
            
            _Result(_To);
            _OnFinish?.Invoke();
        }

        public static IEnumerator Lerp(
            Vector2 _From,
            Vector2 _To,
            float _Time,
            UnityAction<Vector2> _Result,
            ITimeProvider _TimeProvider,
            UnityAction _OnFinish)
        {
            if (_Result == null)
                yield break;
            _Result(_From);

            float currTime = _TimeProvider.Time;
            while (_TimeProvider.Time < currTime + _Time)
            {
                float timeCoeff = 1 - (currTime + _Time - _TimeProvider.Time) / _Time;
                Vector2 val = Vector2.Lerp(_From, _To, timeCoeff);
                _Result(val);
                yield return new WaitForEndOfFrame();
            }
            
            _Result(_To);
            _OnFinish?.Invoke();
        }
        
        public static IEnumerator Lerp(
            Vector3 _From,
            Vector3 _To,
            float _Time,
            UnityAction<Vector3> _Result,
            ITimeProvider _TimeProvider,
            UnityAction _OnFinish)
        {
            if (_Result == null)
                yield break;
            _Result(_From);

            float currTime = _TimeProvider.Time;
            while (_TimeProvider.Time < currTime + _Time)
            {
                float timeCoeff = 1 - (currTime + _Time - _TimeProvider.Time) / _Time;
                Vector3 val = Vector3.Lerp(_From, _To, timeCoeff);
                _Result(val);
                yield return new WaitForEndOfFrame();
            }
            
            _Result(_To);
            _OnFinish?.Invoke();
        }
    }
}