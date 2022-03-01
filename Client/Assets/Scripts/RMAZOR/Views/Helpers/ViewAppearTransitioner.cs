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
    
    public interface IViewBetweenLevelMazeTransitioner
    {
        float FullTransitionTime { get; }
        void DoAppearTransition(
            bool _Appear,
            Dictionary<IEnumerable<Component>, Func<Color>> _Sets,
            V2Int? _ItemPosition = null,
            UnityAction _OnFinish = null,
            EAppearTransitionType _Type = EAppearTransitionType.Circled);
    }

    public class ViewBetweenLevelMazeTransitioner : IViewBetweenLevelMazeTransitioner, IOnLevelStageChanged
    {
        #region inject
        
        private IModelGame      Model        { get; }
        private IViewGameTicker GameTicker   { get; }
        private ViewSettings    ViewSettings { get; }

        public ViewBetweenLevelMazeTransitioner(
            IModelGame      _Model, 
            IViewGameTicker _GameTicker,
            ViewSettings    _ViewSettings)
        {
            Model        = _Model;
            GameTicker   = _GameTicker;
            ViewSettings = _ViewSettings;
        }

        #endregion

        #region api

        public float FullTransitionTime { get; private set; }

        public void DoAppearTransition(
            bool _Appear,
            Dictionary<IEnumerable<Component>, Func<Color>> _Sets,
            V2Int? _ItemPosition = null,
            UnityAction _OnFinish = null,
            EAppearTransitionType _Type = EAppearTransitionType.Circled)
        {
            float delay = GetDelay(_Appear, _Type, _ItemPosition);
            foreach (var set in _Sets)
            {
                Color ToAlpha0() => set.Value().SetA(0f);
                var startCol = _Appear ? ToAlpha0 : set.Value;
                var endCol = !_Appear ? ToAlpha0 : set.Value;
                var shapes = set.Key.Where(_Shape => _Shape.IsNotNull()).ToList();
                foreach (var shape in shapes)
                {
                    switch (shape)
                    {
                        case ShapeRenderer shapeRenderer: shapeRenderer.Color    = startCol(); break;
                        case SpriteRenderer spriteRenderer: spriteRenderer.color = startCol(); break;
                        case TextMeshPro textMeshPro: textMeshPro.color          = startCol(); break;
                    }
                }
                if (_Appear)
                    foreach (var shape in shapes)
                    {
                        switch (shape)
                        {
                            case Behaviour beh: beh.enabled = true; break;
                            case Renderer rend: rend.enabled = true; break;
                        }
                    }
                Cor.Run(Cor.Delay(
                    delay,
                    () =>
                    {
                        Cor.Run(Cor.Lerp(
                            0f,
                            1f,
                            ViewSettings.mazeItemTransitionTime,
                            _Progress =>
                            {
                                var col = Color.Lerp(startCol(), endCol(), _Progress);
                                foreach (var shape in shapes)
                                {
                                    switch (shape)
                                    {
                                        case ShapeRenderer shapeRenderer:   shapeRenderer.Color  = col; break;
                                        case SpriteRenderer spriteRenderer: spriteRenderer.color = col; break;
                                        case TextMeshPro textMeshPro:       textMeshPro.color    = col; break;
                                    }
                                }
                            },
                            GameTicker,
                            (_Finished, _Progress) =>
                            {
                                foreach (var shape in shapes)
                                {
                                    switch (shape)
                                    {
                                        case ShapeRenderer shapeRenderer:   shapeRenderer.Color  = endCol(); break;
                                        case SpriteRenderer spriteRenderer: spriteRenderer.color = endCol(); break;
                                        case TextMeshPro textMeshPro:       textMeshPro.color    = endCol(); break;
                                    }
                                }
                                if (!_Appear)
                                    foreach (var shape in shapes)
                                    {
                                        switch (shape)
                                        {
                                            case Behaviour beh: beh.enabled  = false; break;
                                            case Renderer rend: rend.enabled = false; break;
                                        }
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
                    return (_Appear ? _Position.Value.Y : mazeSize.Y - _Position.Value.Y) * coeff;
                case EAppearTransitionType.Random:
                    return Random.value * 10f * coeff;
                case EAppearTransitionType.WithoutDelay:
                    return 0;
                default:
                    throw new SwitchCaseNotImplementedException(_Type);
            }
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage != ELevelStage.Loaded)
                return;
            var blockPositions = Model.GetAllProceedInfos().Select(_Info => _Info.StartPosition);
            var pathPositions = Model.PathItemsProceeder.PathProceeds.Keys;
            var allPositions = blockPositions.Concat(pathPositions).Distinct();
            float maxSqrDistanceFromCenter = 0f;
            V2Int positionWithMaxDistance = default;
            var mazeSize = Model.Data.Info.Size;
            var center = new Vector2(mazeSize.X * 0.5f, mazeSize.Y * 0.5f);
            foreach (var pos in allPositions)
            {
                float dX = pos.X - center.x;
                float dY = pos.Y - center.y;
                float sqrDist = dX * dX + dY * dY;
                if (sqrDist < maxSqrDistanceFromCenter)
                    continue;
                maxSqrDistanceFromCenter = sqrDist;
                positionWithMaxDistance = pos;
            }
            FullTransitionTime = GetDelay(true, EAppearTransitionType.Circled, positionWithMaxDistance);
        }
    }
}