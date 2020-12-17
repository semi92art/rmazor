using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Helpers;
using LeTai;
using LeTai.TrueShadow;
using Network;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Utils
{
    public static class Coroutines
    {
        private static DontDestroyOnLoad CoroutineRunner;

        static Coroutines()
        {
            CoroutineRunner = GameObject.Find("CoroutinesRunner").GetComponent<DontDestroyOnLoad>();
        }
        
        public static Coroutine Run(IEnumerator _Coroutine)
        {
#if UNITY_EDITOR
            if (GameClient.Instance.IsModuleTestsMode && CoroutineRunner == null)
                CoroutineRunner = GameObject.Find("CoroutinesRunner").GetComponent<DontDestroyOnLoad>();
#endif
            return CoroutineRunner.StartCoroutine(_Coroutine);
        }

        public static void Stop(IEnumerator _Coroutine)
        {
            if (_Coroutine != null)
                CoroutineRunner.StopCoroutine(_Coroutine);
        }

        public static void Stop(Coroutine _Coroutine)
        {
            if (_Coroutine != null)
                CoroutineRunner.StopCoroutine(_Coroutine);
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
            Action _Action,
            Func<bool> _Predicate,
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
            Action _Action,
            Action _FinishAction,
            Func<bool> _Predicate,
            Func<bool> _Pause = null,
            bool _WaitEndOfFrame = true)
        {
            if (_Action == null || _Predicate == null)
                yield break;

            while (_Predicate.Invoke())
            {
                if (_Pause != null && _Pause())
                    continue;
                _Action();
                if (_WaitEndOfFrame)
                    yield return new WaitForEndOfFrame();
            }
        
            _FinishAction?.Invoke();
        }

        public static IEnumerator DoTransparentTransition(
            RectTransform _Item,
            Dictionary<Graphic, float> _GraphicsAndAlphas,
            float _Time,
            bool _Disappear = false,
            Action _OnFinish = null,
            bool _ShadowsAfterOther = false)
        {
            if (_Item == null)
                yield break;

            var graphicsAndAlphas = _GraphicsAndAlphas.CloneAlt();
            
            _Item.gameObject.SetActive(true);
            var selectables = _Item.GetComponentsInChildrenEnabled<Selectable>();
            foreach (var button in selectables)
                button.Key.enabled = false;

            //group graphic elements by background and foreground criteria 
            var groups = 
                graphicsAndAlphas.GroupBy(_Ga =>
                    _Ga.Key != null && _Ga.Key.GetComponent<TrueShadow>() != null && _Ga.Key.GetComponent<TrueShadow>().isBackground);
            var enumerable = groups as IGrouping<bool, KeyValuePair<Graphic, float>>[] ?? groups.ToArray();
            
            //get background shadows group
            var backgroundShadows = enumerable
                .FirstOrDefault(_G => _G.Key)
                ?.ToList();

            if (!GraphicUtils.IsGoodQuality() && backgroundShadows != null)
            {
                foreach (var shadow in backgroundShadows)
                    shadow.Key.GetComponent<TrueShadow>().enabled = false;
                backgroundShadows.Clear();
                backgroundShadows = null;
            }

            if (GraphicUtils.IsGoodQuality())
            {
                //get foreground group
                var foreground = enumerable
                    .FirstOrDefault(_G => !_G.Key)
                    ?.ToList();
            
                //before start transition, make background shadows transparent
                if (backgroundShadows != null)
                    foreach (var ga in from ga 
                        in backgroundShadows let graphic = ga.Key where graphic.IsAlive() select ga)
                        ga.Key.color = ga.Key.color.SetAlpha(0);
                
                //do transition for foreground graphic elements
                float currTime = UiTimeProvider.Instance.Time;
                if (!_Disappear && foreground != null && GraphicUtils.IsGoodQuality())
                    while (UiTimeProvider.Instance.Time < currTime + _Time)
                    {
                        float timeCoeff = (currTime + _Time - UiTimeProvider.Instance.Time) / _Time;
                        float alphaCoeff = 1 - timeCoeff;
                        var collection = foreground;
                        if (backgroundShadows != null && !_ShadowsAfterOther)
                            collection = collection.Concat(backgroundShadows).ToList();
                        foreach (var ga in from ga 
                            in collection let graphic = ga.Key where graphic.IsAlive() select ga)
                            ga.Key.color = ga.Key.color.SetAlpha(ga.Value * alphaCoeff);
                        yield return new WaitForEndOfFrame();
                    } 
            }

            //enable selectable elements (buttons, toggles, etc.)
            foreach (var button in selectables
                .Where(_Button => _Button.Key.IsAlive()))
                button.Key.enabled = button.Value;


            if (GraphicUtils.IsGoodQuality())
            {
                //do transition for background shadows after other transitions only if _ShadowsAfterOther == true
                float shadowBackgrTime = 0.3f;    
                float currTime = UiTimeProvider.Instance.Time;
                if (!_Disappear && backgroundShadows != null && _ShadowsAfterOther)
                    while (UiTimeProvider.Instance.Time < currTime + shadowBackgrTime)
                    {
                        float timeCoeff = (currTime + shadowBackgrTime - UiTimeProvider.Instance.Time) / shadowBackgrTime;
                        float alphaCoeff = 1 - timeCoeff;
                        foreach (var ga in from ga
                            in backgroundShadows let graphic = ga.Key where graphic.IsAlive() select ga)
                            ga.Key.color = ga.Key.color.SetAlpha(ga.Value * alphaCoeff);
                        yield return new WaitForEndOfFrame();
                    }
            }

            
            
            //set color alphas to finish values for all graphic elements
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
            ITimeProvider _TimeProvider,
            Action _OnFinish = null,
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
            Action<float> _Result,
            ITimeProvider _TimeProvider,
            Action _OnFinish = null,
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
            Action<int> _Result,
            ITimeProvider _TimeProvider,
            Action _OnFinish = null)
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
            Action<Color> _Result,
            ITimeProvider _TimeProvider,
            Action _OnFinish = null)
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
            RectTransform _Item,
            Vector3 _From,
            Vector3 _To,
            float _Time,
            ITimeProvider _TimeProvider,
            Action _OnFinish = null)
        {
            if (_Item == null)
                yield break;
            
            _Item.position = _From;
            
            float currTime = _TimeProvider.Time;
            while (_TimeProvider.Time < currTime + _Time)
            {
                float timeCoeff = 1 - (currTime + _Time - _TimeProvider.Time) / _Time;
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
            ITimeProvider _TimeProvider,
            Func<bool> _DoStop = null,
            Action _OnFinish = null)
        {
            if (_Action == null)
                yield break;
            
            float startTime = _TimeProvider.Time;
            
            while (_TimeProvider.Time - startTime < _RepeatTime 
                   && (_DoStop == null || !_DoStop()))
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
            ITimeProvider _TimeProvider,
            Func<bool> _DoStop = null,
            Action _OnFinish = null)
        {
            if (_Action == null)
                yield break;
            float repeatTime = _RepeatDelta * _RepeatCount;
            float startTime = _TimeProvider.Time;
            while (_TimeProvider.Time - startTime < repeatTime 
                   && (_DoStop == null || !_DoStop()))
            {
                _Action();
                yield return new WaitForSeconds(_RepeatDelta);
            }
            
            _OnFinish?.Invoke();
        }
    }
}
