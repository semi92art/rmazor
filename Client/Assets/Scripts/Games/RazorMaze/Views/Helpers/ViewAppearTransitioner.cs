using System;
using System.Collections.Generic;
using System.Linq;
using DI.Extensions;
using Entities;
using Exceptions;
using Games.RazorMaze.Models;
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
        Random,
        WithoutDelay
    }
    
    public interface IViewAppearTransitioner
    {
        void DoAppearTransitionSimple(
            bool _Appear,
            IGameTicker _GameTicker,
            Dictionary<object[], Func<Color>> _Sets,
            V2Int? _ItemPosition = null,
            UnityAction _OnFinish = null,
            EAppearTransitionType _Type = EAppearTransitionType.Circled);
    }

    public class ViewAppearTransitioner : IViewAppearTransitioner
    {
        #region inject
        
        private IModelGame Model { get; }

        public ViewAppearTransitioner(IModelGame _Model)
        {
            Model = _Model;
        }

        #endregion

        #region api
        
        public void DoAppearTransitionSimple(
            bool _Appear,
            IGameTicker _GameTicker,
            Dictionary<object[], Func<Color>> _Sets,
            V2Int? _ItemPosition = null,
            UnityAction _OnFinish = null,
            EAppearTransitionType _Type = EAppearTransitionType.Circled)
        {
            const float transitionTime = 0.3f;
            float delay = GetDelay(_Appear, _Type, _ItemPosition);
            
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


        #endregion
        

        private float GetDelay(bool _Appear, EAppearTransitionType _Type, V2Int? _Position = null)
        {
            const float coeff = 0.05f;
            var mazeSize = Model.Data.Info.Size;

            switch (_Type)
            {
                case EAppearTransitionType.Circled:
                    if (!_Position.HasValue)
                        return 0f;
                    return Mathf.Sqrt(
                        Mathf.Pow(Mathf.Abs(mazeSize.X * 0.5f - _Position.Value.X), 2)
                        + Mathf.Pow(Mathf.Abs(mazeSize.Y * 0.5f - _Position.Value.Y), 2)) * coeff;
                case EAppearTransitionType.TopToBottom:
                    if (!_Position.HasValue)
                        return 0f;
                    return (_Appear ? _Position.Value.Y : mazeSize.Y -_Position.Value.Y) * coeff;
                case EAppearTransitionType.Random:
                    return UnityEngine.Random.value * 10f * coeff;
                case EAppearTransitionType.WithoutDelay:
                    return 0;
                default:
                    throw new SwitchCaseNotImplementedException(_Type);
            }
        }
    }
}