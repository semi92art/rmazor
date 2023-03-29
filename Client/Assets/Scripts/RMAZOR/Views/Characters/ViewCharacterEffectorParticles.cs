using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Common;
using Common.Constants;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Utils;
using UnityEngine;

namespace RMAZOR.Views.Characters
{
    public class ViewCharacterEffectorParticles : InitBase, IViewCharacterEffector
    {
        #region constants

        private const int DeathShapesCount = 50;

        #endregion
        
        #region nonpublic members

        private EDirection? m_MoveDirection;
        private Vector2?    m_FromPos;
        private Vector2?    m_DeathPos;

        private Vector2[] m_StartDirections;

        #endregion
        
        #region inject
        
        private ICoordinateConverter        CoordinateConverter { get; }
        private IContainersGetter           ContainersGetter    { get; }
        private IModelGame                  Model               { get; }
        private IColorProvider              ColorProvider       { get; }
        private IViewInputCommandsProceeder CommandsProceeder   { get; }
        private IViewParticlesThrower       ParticlesThrower    { get; }

        private ViewCharacterEffectorParticles(
            ICoordinateConverter        _CoordinateConverter,
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
        
        public bool Activated { get; set; }

        public override void Init()
        {
            ColorProvider.ColorChanged += OnColorChanged;
            CommandsProceeder.Command += OnCommand;
            ParticlesThrower.ParticleType = EParticleType.Bubbles;
            ParticlesThrower.SetPoolSize(500);
            ParticlesThrower.Init();
            ParticlesThrower.SetSortingOrder(SortingOrders.Character - 2);
            ParticlesThrower.SetColors(
                ColorProvider.GetColor(ColorIds.Character),
                ColorProvider.GetColor(ColorIds.Character2));
            InitStartDirections();
            base.Init();
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
            switch (_Args.LevelStage)
            {
                case ELevelStage.CharacterKilled:
                    ThrowParticlesOnCharacterDisappear(true, Model.Character.Position);
                    break;
                case ELevelStage.Finished when !Initialized:
                    Activated = true;
                    break;
            }
        }
        
        public void OnCharacterMoveStarted(CharacterMovingStartedEventArgs _Args)
        {
            m_FromPos = CoordinateConverter.ToLocalCharacterPosition(_Args.From);
            m_MoveDirection = _Args.Direction;
        }
        
        public void OnCharacterMoveContinued(CharacterMovingContinuedEventArgs _Args)
        {
            // ThrowParticlesOnCharacterMoveContinued(_Args);
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
        
        private void OnCommand(EInputCommand _Command, Dictionary<string, object> _Args)
        {
            if (_Command != EInputCommand.KillCharacter)
                return;
            Vector2? deathPosition = default;
            var deathPositionArg = _Args.GetSafe(ComInComArg.KeyDeathPosition, out bool keyExist);
            if (keyExist)
                deathPosition = (V2) deathPositionArg;
            if (_Args == null || !_Args.Any() || !keyExist)
                m_DeathPos = null;
            else
            {
                if (!m_DeathPos.HasValue)
                    m_DeathPos = deathPosition;
                else if (m_FromPos.HasValue)
                {
                    if (Vector2.Distance(deathPosition.Value, m_FromPos.Value) <
                        Vector2.Distance(m_DeathPos.Value, m_FromPos.Value))
                    {
                        m_DeathPos = deathPosition;
                    }
                }
            }
        }

        private void InitStartDirections()
        {
            var startAngles = Enumerable
                .Range(0, DeathShapesCount)
                .Select(_Num => _Num * Mathf.PI * 2f / DeathShapesCount);
            m_StartDirections = startAngles
                .Select(_Ang => new Vector2(Mathf.Cos(_Ang), Mathf.Sin(_Ang)))
                .ToArray();
        }

        private void ThrowParticlesOnCharacterDisappear(bool _Death, V2Int _LastPos = default)
        {
            var center = m_MoveDirection.HasValue && _Death
                ? (Vector2)ContainersGetter.GetContainer(ContainerNamesMazor.Character).position
                : CoordinateConverter.ToGlobalMazeItemPosition(_LastPos);
            Activated = true;
            var speeds = new Vector3[m_StartDirections.Length];
            if (m_MoveDirection.HasValue && _Death)
            {
                float sqrt2 = Mathf.Sqrt(2f);
                Vector2 moveDir = RmazorUtils.GetDirectionVector(m_MoveDirection.Value, EMazeOrientation.North);
                for (int i = 0; i < DeathShapesCount; i++)
                {
                    float dist = Vector2.Distance(moveDir, m_StartDirections[i]);
                    float coeff;
                    if (dist < sqrt2)
                        coeff = (sqrt2 - dist) * (sqrt2 - dist) * 5f + Random.value * 5f;
                    else coeff = Random.value * 5f;
                    speeds[i] = ((Vector3) m_StartDirections[i] + (Vector3)moveDir) * coeff;
                }
            }
            else
            {
                for (int i = 0; i < DeathShapesCount; i++)
                    speeds[i] = (Vector3) m_StartDirections[i] * Random.value * 5f;
            }
            for (int i = 0; i < DeathShapesCount; i++)
            {
                float randScale = 0.5f + 0.5f * Random.value;
                float throwTime = 0.5f + 0.5f * Random.value;
                ParticlesThrower.ThrowParticle(
                    center + m_StartDirections[i] * Random.value, 
                    speeds[i],
                    randScale,
                    throwTime);
            }
        }
        
        private void ThrowParticlesOnMoveFinished(EDirection _MoveDirection)
        {
            const float directSpeedCoefficient = 2f;
            const float orthogonalSpeedCoeficient = 2f;
            float GetDirectSpeedAddict(float _DirectionCoordinate)
            {
                return -_DirectionCoordinate * directSpeedCoefficient * (0.5f * (1f + Random.value));
            }
            float GetOrthogonalSpeedAddict(float _DirectionCoordinate, float _OrthogonalDirection)
            {
                return MathUtils.Equals(_DirectionCoordinate, 0f) ? 
                    _OrthogonalDirection * Random.value * orthogonalSpeedCoeficient : 0f;
            }
            Vector2 moveDir = RmazorUtils.GetDirectionVector(_MoveDirection, EMazeOrientation.North);
            for (int i = 0; i < 20; i++)
            {
                float orthDirCoeff = i % 2 == 0 ? 1f : -1f;
                var throwSpeed = new Vector2(
                    GetDirectSpeedAddict(moveDir.x) +
                    GetOrthogonalSpeedAddict(moveDir.x, orthDirCoeff),
                    GetDirectSpeedAddict(moveDir.y) +
                    GetOrthogonalSpeedAddict(moveDir.y, orthDirCoeff));
                throwSpeed = -throwSpeed;
                throwSpeed *= 2f;
                var cont = ContainersGetter.GetContainer(ContainerNamesMazor.Character);
                var orthDir = 0.5f * new Vector2(moveDir.y, moveDir.x) * orthDirCoeff;
                float orthDirCoeff2 = _MoveDirection switch
                {
                    EDirection.Left  => -1f,
                    EDirection.Down  => -1f,
                    EDirection.Right => 1f,
                    EDirection.Up    => 1f,
                    _ => throw new SwitchExpressionException(_MoveDirection)
                };
                orthDir *= orthDirCoeff2;
                var pos = (Vector2)cont.position + moveDir * CoordinateConverter.Scale * 0.5f + orthDir * (Random.value * 1.1f);
                float randScale = 0.6f + 0.4f * Random.value;
                ParticlesThrower.ThrowParticle(pos, throwSpeed, randScale, 0.8f);
            }
        }

        #endregion
    }
}