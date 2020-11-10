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
        
        public static void Run(IEnumerator _Coroutine)
        {
            CoroutineRunner.StartCoroutine(_Coroutine);
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
            var animators = _Item.GetComponentsInChildrenEnabled<Animator>();
            var buttons = _Item.GetComponentsInChildrenEnabled<Button>();
            foreach (var animator in animators)
                animator.Key.enabled = false;
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

            float currTime = Time.time;
            float shadowBackgrTime = 0.05f;

            if (backgroundShadows != null)
            {
                // if (_Disappear)
                //     while (Time.time < currTime + shadowBackgrTime)
                //     {
                //         yield return new WaitForEndOfFrame();
                //         float timeCoeff = (currTime + shadowBackgrTime - Time.time) / shadowBackgrTime;
                //         float alphaCoeff = timeCoeff;
                //     
                //         foreach (var ga in backgroundShadows)
                //         {
                //             var graphic = ga.Key;
                //             if (graphic.IsAlive())
                //                 ga.Key.color = ga.Key.color.SetAlpha(ga.Value * alphaCoeff);
                //         }
                //
                //         yield return new WaitForEndOfFrame();
                //     }
                // else
                    foreach (var ga in backgroundShadows)
                    {
                        var graphic = ga.Key;
                        if (graphic.IsAlive())
                            ga.Key.color = ga.Key.color.SetAlpha(0);
                    }
            }
            
            currTime = Time.time;
            if (other != null)
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

            shadowBackgrTime = 0.2f;    
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

            foreach (var animator in animators)
                if (animator.Key.IsAlive())
                    animator.Key.enabled = animator.Value;
            foreach (var button in buttons)
                if (button.Key.IsAlive())
                    button.Key.enabled = button.Value; 
            if (_Disappear)
                _Item.gameObject.SetActive(false);
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
        
        public static IEnumerator LerpPosition(
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
            Action _OnFinish = null)
        {
            if (_Action == null)
                yield break;
            
            float currTime = Time.time;
            float waitingTime = 0f;
            while (Time.time < currTime + _RepeatTime)
            {
                if (Time.time - currTime > waitingTime)
                {
                    _Action();
                    waitingTime += _RepeatDelta;
                }
                
                yield return new WaitForEndOfFrame();
            }
            
            _OnFinish?.Invoke();
        }
    }
}
