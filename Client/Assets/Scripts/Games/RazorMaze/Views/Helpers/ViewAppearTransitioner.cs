using System;
using System.Collections.Generic;
using System.Linq;
using DI.Extensions;
using Entities;
using Exceptions;
using Shapes;
using Ticker;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Games.RazorMaze.Views.Helpers
{
    public enum EAppearTransitionType
    {
        Circled,
        TopToBottom,
        Random
    }
    
    public interface IViewAppearTransitioner
    {
        EAppearTransitionType AppearTransitionType { get; }
        void DoAppearTransitionSimple(
            bool _Appear,
            IGameTicker _GameTicker,
            Dictionary<object[], Func<Color>> _Sets,
            V2Int _ItemPosition,
            UnityAction _OnFinish = null);
    }

    public class ViewAppearTransitioner : IViewAppearTransitioner
    {
        public EAppearTransitionType AppearTransitionType { get; } = EAppearTransitionType.Circled;

        public void DoAppearTransitionSimple(
            bool _Appear,
            IGameTicker _GameTicker,
            Dictionary<object[], Func<Color>> _Sets,
            V2Int _ItemPosition,
            UnityAction _OnFinish = null)
        {
            const float transitionTime = 0.3f;
            float delay = GetDelay(_Appear, _ItemPosition);
            
            foreach (var set in _Sets)
            {
                Func<Color> toAlpha0 = () => set.Value().SetA(0f); 
                
                var startCol = _Appear ? toAlpha0 : set.Value;
                var endCol = !_Appear ? toAlpha0 : set.Value;
                var shapes = set.Key.Where(_Shape => _Shape != null).ToList();
                
                foreach (var shape in shapes)
                {
                    if (shape is ShapeRenderer shapeRenderer)
                        shapeRenderer.Color = startCol();
                    else if (shape is SpriteRenderer spriteRenderer)
                        spriteRenderer.color = startCol();
                }
                
                if (_Appear)
                    foreach (var shape in shapes)
                    {
                        if (shape is ShapeRenderer shapeRenderer)
                            shapeRenderer.enabled = true;
                        else if (shape is SpriteRenderer spriteRenderer)
                            spriteRenderer.enabled = true;
                    }

                Coroutines.Run(Coroutines.Delay(() =>
                    {
                        Coroutines.Run(Coroutines.Lerp(
                            0f,
                            1f,
                            transitionTime,
                            _Progress =>
                            {
                                var col = Color.Lerp(startCol(), endCol(), _Progress);
                                foreach (var shape in shapes)
                                {
                                    if (shape is ShapeRenderer shapeRenderer)
                                        shapeRenderer.Color = col;
                                    else if (shape is SpriteRenderer spriteRenderer)
                                        spriteRenderer.color = col;
                                }
                            },
                            _GameTicker,
                            (_Finished, _Progress) =>
                            {
                                foreach (var shape in shapes)
                                {
                                    if (shape is ShapeRenderer shapeRenderer)
                                        shapeRenderer.Color = endCol();
                                    else if (shape is SpriteRenderer spriteRenderer)
                                        spriteRenderer.color = endCol();
                                }
                                
                                if (!_Appear)
                                    foreach (var shape in shapes)
                                    {
                                        if (shape is ShapeRenderer shapeRenderer)
                                            shapeRenderer.enabled = false;
                                        else if (shape is SpriteRenderer spriteRenderer)
                                            spriteRenderer.enabled = false;
                                    }
                                _OnFinish?.Invoke();
                            }));
                    },
                    delay));
            }
        }

        private float GetDelay(bool _Appear, V2Int _Position)
        {
            switch (AppearTransitionType)
            {
                case EAppearTransitionType.Circled:
                    return Mathf.Sqrt(
                        Mathf.Pow(Mathf.Abs(5f - _Position.X), 2)
                        + Mathf.Pow(Mathf.Abs(5f - _Position.Y), 2)) * 0.05f;
                case EAppearTransitionType.TopToBottom:
                    return (_Appear ? _Position.Y : 10 -_Position.Y) * 0.05f;
                case EAppearTransitionType.Random:
                    return UnityEngine.Random.value * 10f * 0.05f;
                default:
                    throw new SwitchCaseNotImplementedException(AppearTransitionType);
            }
        }
    }
}