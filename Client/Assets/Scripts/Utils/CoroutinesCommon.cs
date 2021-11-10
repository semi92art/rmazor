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
                    yield return new WaitForSecondsRealtime(Time.deltaTime);
                // yield return new WaitForEndOfFrame();
            }
        
            _FinishAction?.Invoke();
        }

        public static IEnumerator DoTransparentTransition(
            RectTransform _Item,
            Dictionary<Graphic, float> _GraphicsAndAlphas,
            float _Time,
            ITicker _Ticker,
            bool _Disappear = false,
            UnityAction _OnFinish = null)
        {
            if (_Item == null)
                yield break;

            var graphicsAndAlphas = _GraphicsAndAlphas.CloneAlt();
            
            _Item.gameObject.SetActive(true);
            var selectables = _Item.GetComponentsInChildrenEnabled<Selectable>();
            foreach (var button in selectables)
                button.Key.enabled = false;

            //do transition for graphic elements
            float currTime = _Ticker.Time;
            if (!_Disappear)
                while (_Ticker.Time < currTime + _Time)
                {
                    float timeCoeff = (currTime + _Time - _Ticker.Time) / _Time;
                    float alphaCoeff = 1 - timeCoeff;
                    var collection = graphicsAndAlphas.ToList();
                    foreach (var ga in from ga 
                        in collection let graphic = ga.Key where !graphic.IsNull() select ga)
                        ga.Key.color = ga.Key.color.SetA(ga.Value * alphaCoeff);
                    yield return new WaitForEndOfFrame();
                } 
            
            //enable selectable elements (buttons, toggles, etc.)
            foreach (var button in selectables
                .Where(_Button => !_Button.Key.IsNull()))
                button.Key.enabled = button.Value;
            
            //set color alphas to finish values for all graphic elements
            foreach (var ga in graphicsAndAlphas.ToList())
            {
                var graphic = ga.Key;
                if (!graphic.IsNull())
                    graphic.color = graphic.color.SetA(_Disappear ? 0 : ga.Value);
            }
            
            if (_Disappear)
                _Item.gameObject.SetActive(false);
            
            _OnFinish?.Invoke();
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