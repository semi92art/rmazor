using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Constants;
using DI.Extensions;
using Entities;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Utils;
using Shapes;
using Ticker;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.Characters
{
    public class ViewCharacterEffectorParticles : IViewCharacterEffector
    {
        #region nonpublic members

        private bool m_Initialized;
        private bool m_Activated;
        private GameObject m_DeathShapesContainer;
        private List<ShapeRenderer> m_DeathShapes;
        private EMazeMoveDirection? m_MoveDirection;
        
        #endregion
        
        #region inject
        
        private IMazeCoordinateConverter CoordinateConverter { get; }
        private IContainersGetter ContainersGetter { get; }
        private IGameTicker GameTicker { get; }
        private IModelGame Model { get; }

        public ViewCharacterEffectorParticles(
            IMazeCoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IGameTicker _GameTicker,
            IModelGame _Model)
        {
            CoordinateConverter = _CoordinateConverter;
            ContainersGetter = _ContainersGetter;
            GameTicker = _GameTicker;
            Model = _Model;
        }
        
        #endregion

        #region api
        

        public bool Activated
        {
            get => m_Activated;
            set
            {
                if (value)
                {
                    if (!m_Initialized)
                        InitPrefab();
                    UpdatePrefab();    
                }
                m_Activated = value;
                m_DeathShapes.ForEach(_Shape => _Shape.enabled = value);
            }
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage == ELevelStage.CharacterKilled)
                Coroutines.Run(DisappearCoroutine(true, Model.Character.Position));
            else if (_Args.Stage != ELevelStage.Finished)
                m_DeathShapes.ForEach(_Shape => _Shape.enabled = false);
        }
        
        public void OnCharacterMoveStarted(CharacterMovingEventArgs _Args)
        {
            m_MoveDirection = _Args.Direction;
        }

        public void OnCharacterMoveFinished(CharacterMovingEventArgs _Args)
        {
            m_MoveDirection = null;
        }

        public void OnAllPathProceed(V2Int _LastPos)
        {
            Coroutines.Run(DisappearCoroutine(false, _LastPos));
        }

        #endregion
        
        #region nonpublic methods

        private void InitPrefab()
        {
            var prefab = PrefabUtilsEx.InitPrefab(
                ContainersGetter.GetContainer(ContainerNames.Maze).transform,
                CommonPrefabSetNames.Views,
                "character_death");
            m_DeathShapesContainer = prefab.GetContentItem("death items");
            var deathShapeTypes = new[]
            {
                typeof(Disc), typeof(Rectangle), typeof(Line), 
                typeof(Polyline), typeof(Polygon), typeof(RegularPolygon)
            };
            m_DeathShapes = deathShapeTypes
                .SelectMany(_T => m_DeathShapesContainer.GetComponentsInChildren(_T))
                .Cast<ShapeRenderer>()
                .ToList();
            m_DeathShapes.ForEach(_Shape =>
            {
                _Shape.SortingOrder = DrawingUtils.GetCharacterSortingOrder() + 2;
                _Shape.Color = DrawingUtils.ColorLines;
                _Shape.enabled = false;
            });
            m_DeathShapes.Shuffle();
            
            m_Initialized = true;
        }

        private void UpdatePrefab()
        {
            var localScale = Vector3.one * CoordinateConverter.Scale * 0.98f;
            m_DeathShapesContainer.transform.localScale = localScale;
        }
        
        private IEnumerator DisappearCoroutine(bool _Death, V2Int _LastPos = default)
        {
            var center = m_MoveDirection.HasValue && _Death ? 
                ContainersGetter.GetContainer(ContainerNames.Character).localPosition : 
                (Vector3)CoordinateConverter.ToLocalCharacterPosition(_LastPos);
            m_DeathShapesContainer.transform.localPosition = center;
            Activated = true;
            int deathShapesCount = m_DeathShapes.Count;
            var startAngles = Enumerable
                .Range(0, deathShapesCount)
                .Select(_Num => _Num * Mathf.PI * 2f / deathShapesCount);
            var startDirections = startAngles
                .Select(_Ang => new Vector2(Mathf.Cos(_Ang), Mathf.Sin(_Ang)))
                .ToList();
            var startPositions = startDirections
                .Select(_Dir => (Vector3) _Dir * Random.value)
                .ToList();
            var endPositions = startPositions.ToList();
            if (m_MoveDirection.HasValue && _Death)
            {
                float sqrt2 = Mathf.Sqrt(2f);
                var moveDir = RazorMazeUtils.GetDirectionVector(m_MoveDirection.Value, MazeOrientation.North).ToVector2();
                for (int i = 0; i < deathShapesCount; i++)
                {
                    float dist = Vector2.Distance(moveDir, startDirections[i]);
                    float coeff;
                    if (dist < sqrt2)
                        coeff = (sqrt2 - dist) * (sqrt2 - dist) * 5f + Random.value * 5f;
                    else coeff = Random.value * 5f;
                    endPositions[i] += ((Vector3) startDirections[i] + (Vector3)moveDir) * coeff;
                }
            }
            else
            {
                for (int i = 0; i < deathShapesCount; i++)
                    endPositions[i] += (Vector3) startDirections[i] * Random.value * 5f;
            }

            yield return Coroutines.Lerp(
                0f,
                1f,
                1f,
                _Progress =>
                {
                    for (int i = 0; i < deathShapesCount; i++)
                    {
                        var shape = m_DeathShapes[i];
                        shape.transform.localPosition =
                            Vector3.Lerp(startPositions[i], endPositions[i], _Progress);
                        shape.Color = DrawingUtils.ColorLines.SetA(1f - _Progress);
                    }
                },
                GameTicker,
                (_Finished, _Progress) => Activated = false);
        }
        
        #endregion
    }
}