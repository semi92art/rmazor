using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.Constants;
using Common.Entities;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.Common;
using RMAZOR.Views.ContainerGetters;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.Characters
{
    public class ViewCharacterEffectorParticles : IViewCharacterEffector
    {
        #region nonpublic members

        private bool                m_Initialized;
        private bool                m_Activated;
        private GameObject          m_DeathShapesContainer;
        private List<ShapeRenderer> m_DeathShapes;
        private EMazeMoveDirection? m_MoveDirection;
        private Vector2?            m_FromPos;
        private Vector2?            m_DeathPos;
        
        #endregion
        
        #region inject
        
        private IMazeCoordinateConverter    CoordinateConverter { get; }
        private IContainersGetter           ContainersGetter    { get; }
        private IViewGameTicker             GameTicker          { get; }
        private IModelGame                  Model               { get; }
        private IColorProvider              ColorProvider       { get; }
        private IViewInputCommandsProceeder CommandsProceeder   { get; }
        private IPrefabSetManager           PrefabSetManager    { get; }

        public ViewCharacterEffectorParticles(
            IMazeCoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IViewGameTicker _GameTicker,
            IModelGame _Model,
            IColorProvider _ColorProvider,
            IViewInputCommandsProceeder _CommandsProceeder,
            IPrefabSetManager _PrefabSetManager)
        {
            CoordinateConverter = _CoordinateConverter;
            ContainersGetter = _ContainersGetter;
            GameTicker = _GameTicker;
            Model = _Model;
            ColorProvider = _ColorProvider;
            CommandsProceeder = _CommandsProceeder;
            PrefabSetManager = _PrefabSetManager;
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
                    {
                        CommandsProceeder.Command += OnCommand;
                        InitPrefab();
                        m_Initialized = true;
                    }
                    UpdatePrefab();    
                }
                m_Activated = value;
                m_DeathShapes.ForEach(_Shape => _Shape.enabled = value);
            }
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage == ELevelStage.CharacterKilled)
                Cor.Run(DisappearCoroutine(true, Model.Character.Position));
            else if (_Args.Stage != ELevelStage.Finished)
            {
                if (!m_Initialized)
                    Activated = true;
                m_DeathShapes.ForEach(_Shape => _Shape.enabled = false);
            }
        }
        
        public void OnCharacterMoveStarted(CharacterMovingStartedEventArgs _Args)
        {
            m_FromPos = CoordinateConverter.ToLocalCharacterPosition(_Args.From);
            m_MoveDirection = _Args.Direction;
        }

        public void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            if (_Args.BlockOnFinish != null && _Args.BlockOnFinish.Type == EMazeItemType.Springboard)
                return;
            m_MoveDirection = null;
        }

        public void OnAllPathProceed(V2Int _LastPos)
        {
            m_FromPos = null;
            Cor.Run(DisappearCoroutine(false, _LastPos));
        }

        #endregion
        
        #region nonpublic methods

        private void InitPrefab()
        {
            var prefab = PrefabSetManager.InitPrefab(
                null,
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
                _Shape.SortingOrder = SortingOrders.Character + 2;
                _Shape.Color = ColorProvider.GetColor(ColorIds.Character);
                _Shape.enabled = false;
            });
            m_DeathShapes.Shuffle();
        }

        private void UpdatePrefab()
        {
            var localScale = Vector3.one * (CoordinateConverter.Scale * 0.98f);
            m_DeathShapesContainer.transform.localScale = localScale;
        }
        
        private IEnumerator DisappearCoroutine(bool _Death, V2Int _LastPos = default)
        {
            Vector3 cent;
            if (m_MoveDirection.HasValue && _Death)
            {
                cent = ContainersGetter.GetContainer(ContainerNames.Character).position;
                m_DeathShapesContainer.transform.position = cent;
            }
            else
            {
                cent = CoordinateConverter.ToGlobalMazeItemPosition(_LastPos);
                m_DeathShapesContainer.transform.position = cent;
            }
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
                Vector2 moveDir = RazorMazeUtils.GetDirectionVector(m_MoveDirection.Value, MazeOrientation.North);
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

            var col = ColorProvider.GetColor(ColorIds.Character);
            yield return Cor.Lerp(
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
                        shape.Color = col.SetA(1f - _Progress);
                    }
                },
                GameTicker,
                (_Finished, _Progress) => Activated = false);
        }
        
        private void OnCommand(EInputCommand _Command, object[] _Args)
        {
            if (_Command != EInputCommand.KillCharacter)
                return;
            if (_Args == null || !_Args.Any())
                m_DeathPos = null;
            else
            {
                var newDeathPos = (Vector2) _Args[0];
                if (!m_DeathPos.HasValue)
                    m_DeathPos = newDeathPos;
                else if (m_FromPos.HasValue)
                {
                    if (Vector2.Distance(newDeathPos, m_FromPos.Value) <
                        Vector2.Distance(m_DeathPos.Value, m_FromPos.Value))
                    {
                        m_DeathPos = newDeathPos;
                    }
                }
            }
        }
        
        #endregion
    }
}