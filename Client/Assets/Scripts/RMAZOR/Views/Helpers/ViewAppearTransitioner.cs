using System;
using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using Common.Exceptions;
using Common.Extensions;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Models;
using Shapes;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace RMAZOR.Views.Helpers
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
        void DoAppearTransition(
            bool _Appear,
            Dictionary<IEnumerable<Component>, Func<Color>> _Sets,
            V2Int? _ItemPosition = null,
            UnityAction _OnFinish = null,
            EAppearTransitionType _Type = EAppearTransitionType.Circled);
    }

    public class ViewAppearTransitioner : IViewAppearTransitioner
    {
        #region inject
        
        private IModelGame               Model               { get; }
        private IMazeCoordinateConverter CoordinateConverter { get; }
        private IViewGameTicker          GameTicker          { get; }

        public ViewAppearTransitioner(
            IModelGame _Model, 
            IMazeCoordinateConverter _CoordinateConverter, 
            IViewGameTicker _GameTicker)
        {
            Model = _Model;
            CoordinateConverter = _CoordinateConverter;
            GameTicker = _GameTicker;
        }

        #endregion

        #region api

        public void DoAppearTransition(
            bool _Appear,
            Dictionary<IEnumerable<Component>, Func<Color>> _Sets,
            V2Int? _ItemPosition = null,
            UnityAction _OnFinish = null,
            EAppearTransitionType _Type = EAppearTransitionType.Circled)
        {
            const float transitionTime = 0.3f;
            float delay = GetDelay(_Appear, _Type, _ItemPosition);
            
            foreach (var set in _Sets)
            {
                Color ToAlpha0() => set.Value().SetA(0f);

                var startCol = _Appear ? ToAlpha0 : set.Value;
                var endCol = !_Appear ? ToAlpha0 : set.Value;
                var shapes = set.Key.Where(_Shape => _Shape.IsNotNull()).ToList();
                
                foreach (var shape in shapes)
                {
                    if (shape is ShapeRenderer shapeRenderer)        shapeRenderer.Color  = startCol();
                    else if (shape is SpriteRenderer spriteRenderer) spriteRenderer.color = startCol();
                    else if (shape is TextMeshPro textMeshPro)       textMeshPro.color    = startCol();
                }
                
                if (_Appear)
                    foreach (var shape in shapes)
                    {
                        if (shape is Behaviour beh) beh.enabled = true;
                        else if (shape is Renderer rend) rend.enabled = true;
                    }

                Cor.Run(Cor.Delay(
                    delay,
                    () =>
                    {
                        Cor.Run(Cor.Lerp(
                            0f,
                            1f,
                            transitionTime,
                            _Progress =>
                            {
                                var col = Color.Lerp(startCol(), endCol(), _Progress);
                                foreach (var shape in shapes)
                                {
                                    if (shape is ShapeRenderer shapeRenderer)        shapeRenderer.Color  = col;
                                    else if (shape is SpriteRenderer spriteRenderer) spriteRenderer.color = col;
                                    else if (shape is TextMeshPro textMeshPro)       textMeshPro.color    = col;
                                }
                            },
                            GameTicker,
                            (_Finished, _Progress) =>
                            {
                                foreach (var shape in shapes)
                                {
                                    if (shape is ShapeRenderer shapeRenderer)
                                        shapeRenderer.Color = endCol();
                                    else if (shape is SpriteRenderer spriteRenderer)
                                        spriteRenderer.color = endCol();
                                    else if (shape is TextMeshPro textMeshPro)
                                        textMeshPro.color = endCol();
                                }
                                
                                if (!_Appear)
                                    foreach (var shape in shapes)
                                    {
                                        if (shape is Behaviour beh)      beh.enabled  = false;
                                        else if (shape is Renderer rend) rend.enabled = false;
                                    }
                                _OnFinish?.Invoke();
                            }));
                    }));
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
                    return Random.value * 10f * coeff;
                case EAppearTransitionType.WithoutDelay:
                    return 0;
                default:
                    throw new SwitchCaseNotImplementedException(_Type);
            }
        }
    }
}