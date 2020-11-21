using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Helpers;
using LeTai.TrueShadow;
using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
    public static class Coroutines
    {
        private static readonly DontDestroyOnLoad CoroutineRunner;

        static Coroutines()
        {
            CoroutineRunner = GameObject.Find("CoroutinesRunner").GetComponent<DontDestroyOnLoad>();
        }
        
        public static Coroutine Run(IEnumerator _Coroutine)
        {
            return CoroutineRunner.StartCoroutine(_Coroutine);
        }
    
        public static IEnumerator Action(Action _Action)
        {
            _Action?.Invoke();
            yield break;
        }
    
        public static IEnumerator WaitEndOfFrame(Action _Action)
        {
            yield return new WaitForEndOfFrame();

            _Action?.Invoke();
        }

        public static IEnumerator Delay(
            Action _OnDelay,
            float  _Delay
        )
        {
            if (_Delay > float.Epsilon)
                yield return new WaitForSeconds(_Delay);

            _OnDelay?.Invoke();
        }
    
        public static IEnumerator WaitWhile(
            Action _Action,
            Func<bool> _Predicate)
        {
            if (_Action == null || _Predicate == null)
                yield break;
        
            while (_Predicate())
                yield return new WaitForEndOfFrame();
            
            _Action();
        }

        public static IEnumerator DoWhile(
            Action _Action,
            Action _FinishAction,
            Func<bool> _Predicate,
            Func<bool> _WaitEndOfFrame)
        {
            if (_Action == null || _Predicate == null)
                yield break;

            while (_Predicate.Invoke())
            {
                _Action();
                if (_WaitEndOfFrame.Invoke())
                    yield return new WaitForEndOfFrame();
            }
        
            _FinishAction?.Invoke();
        }

        public static IEnumerator DoTransparentTransition(
            RectTransform _Item,
            Dictionary<Graphic, float> _GraphicsAndAlphas,
            float _Time,
            bool _Disappear = false,
            Action _OnFinish = null)
        {
            if (_Item == null)
                yield break;

            var graphicsAndAlphas = _GraphicsAndAlphas.CloneAlt();
            
            _Item.gameObject.SetActive(true);
            var buttons = _Item.GetComponentsInChildrenEnabled<Button>();
            foreach (var button in buttons)
                button.Key.enabled = false;

            var groups = 
                graphicsAndAlphas.GroupBy(_Ga =>
                    _Ga.Key != null && _Ga.Key.GetComponent<TrueShadow>() != null && _Ga.Key.GetComponent<TrueShadow>().isBackground);
            var enumerable = groups as IGrouping<bool, KeyValuePair<Graphic, float>>[] ?? groups.ToArray();
            var backgroundShadows = enumerable
                .FirstOrDefault(_G => _G.Key)
                ?.ToList();
            var other = enumerable
                .FirstOrDefault(_G => !_G.Key)
                ?.ToList();
            
            if (backgroundShadows != null)
            {
                foreach (var ga in backgroundShadows)
                {
                    var graphic = ga.Key;
                    if (graphic.IsAlive())
                        ga.Key.color = ga.Key.color.SetAlpha(0);
                }
            }
            
            float currTime = Time.time;
            if (!_Disappear && other != null)
            {
                while (Time.time < currTime + _Time)
                {
                    float timeCoeff = (currTime + _Time - Time.time) / _Time;
                    float alphaCoeff = _Disappear ? timeCoeff : 1 - timeCoeff;
                    
                    foreach (var ga in other)
                    {
                        var graphic = ga.Key;
                        if (graphic.IsAlive())
                            ga.Key.color = ga.Key.color.SetAlpha(ga.Value * alphaCoeff);
                    }
                    
                    yield return new WaitForEndOfFrame();
                } 
            }
            
            foreach (var button in buttons
                .Where(_Button => _Button.Key.IsAlive()))
                button.Key.enabled = button.Value;

            float shadowBackgrTime = 0.3f;    
            currTime = Time.time;
            if (!_Disappear && backgroundShadows != null)
                while (Time.time < currTime + shadowBackgrTime)
                {
                    yield return new WaitForEndOfFrame();
                    float timeCoeff = (currTime + shadowBackgrTime - Time.time) / shadowBackgrTime;
                    float alphaCoeff = 1 - timeCoeff;
                    
                    foreach (var ga in backgroundShadows)
                    {
                        var graphic = ga.Key;
                        if (graphic.IsAlive())
                            ga.Key.color = ga.Key.color.SetAlpha(ga.Value * alphaCoeff);
                    }
                
                    yield return new WaitForEndOfFrame();
                }
            
            foreach (var ga in graphicsAndAlphas.ToList())
            {
                var graphic = ga.Key;
                if (graphic.IsAlive())
                    graphic.color = graphic.color.SetAlpha(_Disappear ? 0 : ga.Value);
            }
            
            if (_Disappear)
                _Item.gameObject.SetActive(false);
            
            _OnFinish?.Invoke();
        }
        
        public static IEnumerator Lerp(
            long _From,
            long _To,
            float _Time,
            Action<long> _Result,
            Action _OnFinish = null,
            Func<bool> _OnBreak = null)
        {
            if (_Result == null)
                yield break;
            _Result(_From);

            float currTime = Time.time;
            while (Time.time < currTime + _Time)
            {
                float timeCoeff = 1 - (currTime + _Time - Time.time) / _Time;
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
            Action<float> _Result,
            Action _OnFinish = null,
            Func<bool> _OnBreak = null)
        {
            if (_Result == null)
                yield break;
            _Result(_From);

            float currTime = Time.time;
            while (Time.time < currTime + _Time)
            {
                float timeCoeff = 1 - (currTime + _Time - Time.time) / _Time;
                float newVal = Mathf.Lerp(_From, _To, timeCoeff);
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
            int _From,
            int _To,
            float _Time,
            Action<int> _Result,
            Action _OnFinish = null)
        {
            if (_Result == null)
                yield break;
            _Result(_From);

            float currTime = Time.time;
            while (Time.time < currTime + _Time)
            {
                float timeCoeff = 1 - (currTime + _Time - Time.time) / _Time;
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
            Action<Color> _Result,
            Action _OnFinish = null)
        {
            if (_Result == null)
                yield break;
            _Result(_From);

            float currTime = Time.time;
            while (Time.time < currTime + _Time)
            {
                float timeCoeff = 1 - (currTime + _Time - Time.time) / _Time;
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
            RectTransform _Item,
            Vector3 _From,
            Vector3 _To,
            float _Time,
            Action _OnFinish = null)
        {
            if (_Item == null)
                yield break;
            
            _Item.position = _From;
            
            float currTime = Time.time;
            while (Time.time < currTime + _Time)
            {
                float timeCoeff = 1 - (currTime + _Time - Time.time) / _Time;
                Vector3 newPos = Vector3.Lerp(_From, _To, timeCoeff);
                if (_Item != null)
                    _Item.position = newPos;
                yield return new WaitForEndOfFrame();
            }

            if (_Item != null)
                _Item.position = _To;
            _OnFinish?.Invoke();
        }

        public static IEnumerator Repeat(
            Action _Action,
            float _RepeatDelta,
            float _RepeatTime,
            Func<bool> _DoStop = null,
            Action _OnFinish = null)
        {
            if (_Action == null)
                yield break;
            
            float startTime = Time.time;
            while (Time.time - startTime < _RepeatTime && !_DoStop())
            {
                _Action();
                yield return new WaitForSeconds(_RepeatDelta);
            }
            
            _OnFinish?.Invoke();
        }
        
        public static IEnumerator Repeat(
            Action _Action,
            float _RepeatDelta,
            long _RepeatCount,
            Func<bool> _DoStop = null,
            Action _OnFinish = null)
        {
            if (_Action == null)
                yield break;
            float repeatTime = _RepeatDelta * _RepeatCount;
            float startTime = Time.time;
            while (Time.time - startTime < repeatTime && !_DoStop())
            {
                _Action();
                yield return new WaitForSeconds(_RepeatDelta);
            }
            
            _OnFinish?.Invoke();
        }
    }
}
