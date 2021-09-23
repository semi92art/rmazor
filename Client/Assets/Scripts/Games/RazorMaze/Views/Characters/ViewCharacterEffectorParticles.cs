using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Constants;
using DI.Extensions;
using Exceptions;
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

        private bool m_Activated;
        private GameObject m_DeathShapesContainer;
        private List<ShapeRenderer> m_DeathShapes;
        
        #endregion
        
        #region inject
        
        private ICoordinateConverter CoordinateConverter { get; }
        private IContainersGetter ContainersGetter { get; }
        public IGameTicker GameTicker { get; }
        public IModelCharacter ModelCharacter { get; }

        public ViewCharacterEffectorParticles(
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IGameTicker _GameTicker,
            IModelCharacter _ModelCharacter)
        {
            CoordinateConverter = _CoordinateConverter;
            ContainersGetter = _ContainersGetter;
            GameTicker = _GameTicker;
            ModelCharacter = _ModelCharacter;
        }
        
        #endregion

        #region api
        
        public event NoArgsHandler Initialized;

        public bool Activated
        {
            get => m_Activated;
            set
            {
                m_Activated = value;
                m_DeathShapes.ForEach(_Shape => _Shape.enabled = value);
            }
        }
        public void Init()
        {
            InitPrefab();
            Initialized?.Invoke();
        }

        public void OnRevivalOrDeath(bool _Alive)
        {
            if (_Alive)
                m_DeathShapes.ForEach(_Shape => _Shape.enabled = false);
            else Coroutines.Run(DeathCoroutine());
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:
                    Activated = true;
                    break;
                case ELevelStage.Paused:
                    break;
                case ELevelStage.StartedOrContinued:
                    break;
                case ELevelStage.Finished:
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Args.Stage);
            }
        }
        
        #endregion
        
        #region nonpublic methods

        private void InitPrefab()
        {
            var prefab = PrefabUtilsEx.InitPrefab(
                ContainersGetter.MazeContainer.transform,
                CommonPrefabSetNames.Views,
                "character_death");
            m_DeathShapesContainer = prefab.GetContentItem("death items");
            var localScale = Vector3.one * CoordinateConverter.GetScale() * 0.98f;
            m_DeathShapesContainer.transform.localScale = localScale;
            var deathShapeTypes = new[]
            {
                typeof(Disc), typeof(Rectangle), typeof(Line), 
                typeof(Polyline), typeof(Polygon), typeof(RegularPolygon)
            };
            m_DeathShapes = deathShapeTypes
                .SelectMany(_T => m_DeathShapesContainer.GetComponentsInChildren(_T))
                .Cast<ShapeRenderer>()
                .ToList();
            m_DeathShapes.ForEach(_Shape => _Shape.enabled = false);
            m_DeathShapes.Shuffle();
        }
        
        private IEnumerator DeathCoroutine()
        {
            Activated = true;
            int deathShapesCount = m_DeathShapes.Count;
            var startAngles = Enumerable
                .Range(0, deathShapesCount)
                .Select(_Num => _Num * Mathf.PI * 2f / deathShapesCount)
                .ToList();
            var startDirections = startAngles
                .Select(_Ang => new Vector2(Mathf.Cos(_Ang), Mathf.Sin(_Ang)))
                .ToList();
            var startPositions = startDirections
                .Select(_Dir => ContainersGetter.CharacterContainer.position + (Vector3) _Dir * Random.value)
                .ToList();
            var endPositions = startPositions.ToList();
            for (int i = 0; i < deathShapesCount; i++)
                endPositions[i] += (Vector3) startDirections[i] * Random.value * 5f;
            yield return Coroutines.Lerp(
                0f,
                1f,
                1f,
                _Progress =>
                {
                    for (int i = 0; i < deathShapesCount; i++)
                    {
                        var shape = m_DeathShapes[i];
                        shape.transform.position =
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