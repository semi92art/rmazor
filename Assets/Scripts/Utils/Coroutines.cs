using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Network;
using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
    public static class Coroutines
    {
        public static Coroutine StartCoroutine(IEnumerator _Coroutine)
        {
            return GameClient.Instance.StartCoroutine(_Coroutine);
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
    
        public static IEnumerator WaitWhile(Action _Action, Func<bool> _Predicate)
        {
            if (_Action == null || _Predicate == null)
                yield break;
        
            yield return new WaitWhile(_Predicate);
        
            _Action();
        }

        public static IEnumerator DoWhile(Action _Action, Action _FinishAction, Func<bool> _Predicate, Func<bool> _WaitEndOfFrame)
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
            _Item.gameObject.SetActive(true);
            
            float currTime = Time.time;
            while (Time.time < currTime + _Time)
            {
                float timeCoeff = (currTime + _Time - Time.time) / _Time;
                float alphaCoeff = _Disappear ? timeCoeff : 1 - timeCoeff;

                foreach (var ga in _GraphicsAndAlphas.ToList())
                {
                    var graphic = ga.Key;
                    if (graphic.IsAlive())
                        ga.Key.color = ga.Key.color.SetAlpha(ga.Value * alphaCoeff);
                }
            
                yield return new WaitForEndOfFrame();
            }
            
            foreach (var ga in _GraphicsAndAlphas.ToList())
            {
                var graphic = ga.Key;
                if (graphic.IsAlive())
                    graphic.color = graphic.color.SetAlpha(_Disappear ? 0 : ga.Value);
            }

            _OnFinish?.Invoke();
            yield return null;
        }
    }
}
