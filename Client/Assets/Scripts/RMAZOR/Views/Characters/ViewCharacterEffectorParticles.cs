using System.Linq;
using System.Runtime.CompilerServices;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Helpers;
using Common.Providers;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Utils;
using UnityEngine;

namespace RMAZOR.Views.Characters
{
    public class ViewCharacterEffectorParticles : IViewCharacterEffector
    {
        #region nonpublic members

        private bool                m_Initialized;
        private bool                m_Activated;
        private EMazeMoveDirection? m_MoveDirection;
        private Vector2?            m_FromPos;
        private Vector2?            m_DeathPos;

        #endregion
        
        #region inject
        
        private ICoordinateConverter  CoordinateConverter { get; }
        private IContainersGetter           ContainersGetter    { get; }
        private IModelGame                  Model               { get; }
        private IColorProvider              ColorProvider       { get; }
        private IViewInputCommandsProceeder CommandsProceeder   { get; }
        private IViewParticlesThrower       ParticlesThrower    { get; }

        private ViewCharacterEffectorParticles(
            ICoordinateConverter  _CoordinateConverter,
            IContainersGetter           _ContainersGetter,
            IModelGame                  _Model,
            IColorProvider              _ColorProvider,
            IViewInputCommandsProceeder _CommandsProceeder,
            IViewParticlesThrower       _ParticlesThrower)
        {
            CoordinateConverter = _CoordinateConverter;
            ContainersGetter    = _ContainersGetter;
            Model               = _Model;
            ColorProvider       = _ColorProvider;
            CommandsProceeder   = _CommandsProceeder;
            ParticlesThrower    = _ParticlesThrower;
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
                        ColorProvider.ColorChanged += OnColorChanged;
                        CommandsProceeder.Command += OnCommand;
                        ParticlesThrower.ParticleType = EParticleType.Bubbles;
                        ParticlesThrower.SetPoolSize(100);
                        ParticlesThrower.Init();
                        ParticlesThrower.SetSortingOrder(SortingOrders.Character + 2);
                        m_Initialized = true;
                    }
                }
                m_Activated = value;
            }
        }

        private void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId == ColorIds.Character || _ColorId == ColorIds.Character2)
            {
                ParticlesThrower.SetColors(
                    ColorProvider.GetColor(ColorIds.Character),
                    ColorProvider.GetColor(ColorIds.Character2));
            }
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.LevelStage == ELevelStage.CharacterKilled)
            {
                ThrowParticlesOnCharacterDisappear(true, Model.Character.Position);
            }
            else if (_Args.LevelStage != ELevelStage.Finished)
            {
                if (!m_Initialized)
                    Activated = true;
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
            if (_Args.BlockOnFinish != null && _Args.BlockOnFinish.Type == EMazeItemType.Portal)
                return;
            if (Model.PathItemsProceeder.AllPathsProceeded)
                return;
            ThrowParticlesOnMoveFinished(_Args.Direction);
            m_MoveDirection = null;
        }

        public void OnPathCompleted(V2Int _LastPos)
        {
            m_FromPos = null;
            ThrowParticlesOnCharacterDisappear(false, _LastPos);
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

        private void ThrowParticlesOnCharacterDisappear(bool _Death, V2Int _LastPos = default)
        {
            Vector3 center;
            if (m_MoveDirection.HasValue && _Death)
                center = ContainersGetter.GetContainer(ContainerNames.Character).position;
            else
                center = CoordinateConverter.ToGlobalMazeItemPosition(_LastPos);
            Activated = true;
            const int deathShapesCount = 50;
            var startAngles = Enumerable
                .Range(0, deathShapesCount)
                .Select(_Num => _Num * Mathf.PI * 2f / deathShapesCount);
            var startDirections = startAngles
                .Select(_Ang => new Vector2(Mathf.Cos(_Ang), Mathf.Sin(_Ang)))
                .ToList();
            var localStartPositions = startDirections
                .Select(_Dir => (Vector3) _Dir * Random.value)
                .ToList();
            var speeds = new Vector3[localStartPositions.Count];
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
                    speeds[i] = ((Vector3) startDirections[i] + (Vector3)moveDir) * coeff;
                }
            }
            else
            {
                for (int i = 0; i < deathShapesCount; i++)
                    speeds[i] = (Vector3) startDirections[i] * Random.value * 5f;
            }

            for (int i = 0; i < deathShapesCount; i++)
            {
                float randScale = 0.5f + 0.5f * Random.value;
                float throwTime = 0.5f + 0.5f * Random.value;
                ParticlesThrower.ThrowParticle(
                    center + localStartPositions[i], 
                    speeds[i],
                    randScale,
                    throwTime);
            }
        }
        
        private void ThrowParticlesOnMoveFinished(EMazeMoveDirection _MoveDirection)
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
            Vector2 moveDir = RmazorUtils.GetDirectionVector(_MoveDirection, MazeOrientation.North);
            for (int i = 0; i < 6; i++)
            {
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
                float randScale = 0.4f + 0.3f * Random.value;
                ParticlesThrower.ThrowParticle(pos, throwSpeed, randScale, 0.2f);
            }
        }

        #endregion
    }
}