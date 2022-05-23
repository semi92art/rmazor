using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.SpawnPools;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.Characters
{
    public class ViewCharacterEffectorParticles : IViewCharacterEffector
    {
        #region constants

        private const int BubblesPoolSize = 100;

        #endregion
        
        #region nonpublic members

        private bool                m_Initialized;
        private bool                m_Activated;
        private GameObject          m_DeathShapesContainer;
        private List<ShapeRenderer> m_DeathShapes;
        private EMazeMoveDirection? m_MoveDirection;
        private Vector2?            m_FromPos;
        private Vector2?            m_DeathPos;

        private readonly SpawnPool<IViewBubbleItem> m_BubblesPool =
            new SpawnPool<IViewBubbleItem>();
        
        #endregion
        
        #region inject
        
        private IMazeCoordinateConverter    CoordinateConverter { get; }
        private IContainersGetter           ContainersGetter    { get; }
        private IViewGameTicker             GameTicker          { get; }
        private IModelGame                  Model               { get; }
        private IColorProvider              ColorProvider       { get; }
        private IViewInputCommandsProceeder CommandsProceeder   { get; }
        private IPrefabSetManager           PrefabSetManager    { get; }
        private IViewBubbleItem             BubbleItem          { get; }

        public ViewCharacterEffectorParticles(
            IMazeCoordinateConverter    _CoordinateConverter,
            IContainersGetter           _ContainersGetter,
            IViewGameTicker             _GameTicker,
            IModelGame                  _Model,
            IColorProvider              _ColorProvider,
            IViewInputCommandsProceeder _CommandsProceeder,
            IPrefabSetManager           _PrefabSetManager,
            IViewBubbleItem            _BubbleItem)
        {
            CoordinateConverter = _CoordinateConverter;
            ContainersGetter    = _ContainersGetter;
            GameTicker          = _GameTicker;
            Model               = _Model;
            ColorProvider       = _ColorProvider;
            CommandsProceeder   = _CommandsProceeder;
            PrefabSetManager    = _PrefabSetManager;
            BubbleItem          = _BubbleItem;
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
                        InitCharacterDeathPrefab();
                        InitBubblesPool();
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
            ThrowBubblesOnMoveFinished(_Args.Direction);
        }

        public void OnAllPathProceed(V2Int _LastPos)
        {
            m_FromPos = null;
            Cor.Run(DisappearCoroutine(false, _LastPos));
        }

        #endregion
        
        #region nonpublic methods
        
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

        private void InitCharacterDeathPrefab()
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
        
        private void InitBubblesPool()
        {
            for (int i = 0; i < BubblesPoolSize; i++)
            {
                var item = (IViewBubbleItem)BubbleItem.Clone();
                item.Init();
                m_BubblesPool.Add(item);
            }
        }

        private void UpdatePrefab()
        {
            var localScale = Vector3.one * (CoordinateConverter.Scale * 0.98f);
            m_DeathShapesContainer.transform.localScale = localScale;
        }
        
        private IEnumerator DisappearCoroutine(bool _Death, V2Int _LastPos = default)
        {
            Vector3 center;
            if (m_MoveDirection.HasValue && _Death)
            {
                center = ContainersGetter.GetContainer(ContainerNames.Character).position;
                m_DeathShapesContainer.transform.position = center;
            }
            else
            {
                center = CoordinateConverter.ToGlobalMazeItemPosition(_LastPos);
                m_DeathShapesContainer.transform.position = center;
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
                Vector2 moveDir = RmazorUtils.GetDirectionVector(m_MoveDirection.Value, MazeOrientation.North);
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
                GameTicker,
                1f,
                _OnProgress: _P =>
                {
                    for (int i = 0; i < deathShapesCount; i++)
                    {
                        var shape = m_DeathShapes[i];
                        shape.transform.localPosition =
                            Vector3.Lerp(startPositions[i], endPositions[i], _P);
                        shape.Color = col.SetA(1f - _P);
                    }
                },
                _OnFinish: () => Activated = false);
        }
        
        private void ThrowBubblesOnMoveFinished(EMazeMoveDirection _MoveDirection)
        {
            const float directSpeedCoefficient = 2f;
            const float orthogonalSpeedCoeficient = 4f;

            float GetDirectSpeedAddict(float _DirectionCoordinate)
            {
                return -_DirectionCoordinate * directSpeedCoefficient;
            }
            float GetOrthogonalSpeedAddict(float _DirectionCoordinate, float _OrthogonalDirection)
            {
                return Mathf.Abs(_DirectionCoordinate) < MathUtils.Epsilon ? 
                    _OrthogonalDirection * Random.value * orthogonalSpeedCoeficient : 0f;
            }
            for (int i = 0; i < 6; i++)
            {
                Vector2 moveDir = RmazorUtils.GetDirectionVector(_MoveDirection, MazeOrientation.North);
                float orthDirCoeff = i % 2 == 0 ? 1f : -1f;
                var throwSpeed = new Vector2(
                    GetDirectSpeedAddict(moveDir.x) +
                    GetOrthogonalSpeedAddict(moveDir.x, orthDirCoeff),
                    GetDirectSpeedAddict(moveDir.y) +
                    GetOrthogonalSpeedAddict(moveDir.y, orthDirCoeff));
                var cont = ContainersGetter.GetContainer(ContainerNames.Character);
                var orthDir = 0.5f * new Vector2(moveDir.y, moveDir.x) * orthDirCoeff;
                float orthDirCoeff2 = _MoveDirection switch
                {
                    EMazeMoveDirection.Left  => -1f,
                    EMazeMoveDirection.Down  => -1f,
                    EMazeMoveDirection.Right => 1f,
                    EMazeMoveDirection.Up    => 1f,
                    _ => throw new SwitchExpressionException(_MoveDirection)
                };
                orthDir *= orthDirCoeff2;
                var pos = (Vector2)cont.position + moveDir * CoordinateConverter.Scale * 0.5f + orthDir;
                ThrowBubble(pos, throwSpeed);
            }
        }

        private void ThrowBubble(Vector2 _Position, Vector2 _Speed)
        {
            var item = m_BubblesPool.FirstInactive;
            m_BubblesPool.Activate(item);
            float randScale = 0.4f + 0.3f * Random.value;
            item.Throw(_Position, _Speed, randScale);
        }
        

        
        #endregion
    }
}