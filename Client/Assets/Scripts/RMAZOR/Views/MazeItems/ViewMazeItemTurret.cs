using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Common.Constants;
using Common.Entities;
using Common.Enums;
using Common.Extensions;
using Common.Helpers;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Views.Characters;
using RMAZOR.Views.Common;
using RMAZOR.Views.Common.Additional_Background;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.MazeItems.Additional;
using RMAZOR.Views.MazeItems.Props;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;
using UnityEngine.Rendering;

namespace RMAZOR.Views.MazeItems
{
    public interface IViewMazeItemTurret : IViewMazeItem
    {
        void PreShoot(TurretShotEventArgs _Args);
        void Shoot(TurretShotEventArgs _Args);
        void SetProjectileSortingOrder(int _Order);
    }
    
    public class ViewMazeItemTurret : 
        ViewMazeItemBase,
        IViewMazeItemTurret,
        IUpdateTick
    {
        #region constants

        private const float ProjectileContainerRadius = 0.4f;
        private const int   ParticlesThrowerSize      = 30;
        
        #endregion
        
        #region nonpublic members

        private static AudioClipArgs AudioClipArgsShurikenFly =>
            new AudioClipArgs("shuriken", EAudioClipType.GameSound);
        
        protected override string ObjectName => "Turret Block";
        
        private float               m_RotatingSpeed;
        private bool                m_ProjRotating;
        private bool                m_ProjectileMovingLocked = true;
        private Rectangle           m_ProjectileMask;
        private BoxCollider2D       m_ProjectileMaskCollider;
        private TurretShotEventArgs m_LastShotArgs;
        private Vector3             m_MaskColliderCurrentPosition;

        #endregion
        
        #region inject

        private IViewBackground                     Background                     { get; }
        private IViewMazeAdditionalBackground       AdditionalBackground           { get; }
        private IViewTurretProjectile               Projectile                     { get; }
        private IViewTurretProjectile               ProjectileFake                 { get; }
        private IViewTurretBody                     TurretBody                     { get; }
        private IViewParticlesThrower               ParticlesThrower               { get; }
        private IViewCharacter                      Character                      { get; }
        private IViewSwitchLevelStageCommandInvoker SwitchLevelStageCommandInvoker { get; }

        private ViewMazeItemTurret(
            ViewSettings                        _ViewSettings,
            IModelGame                          _Model,
            ICoordinateConverter                _CoordinateConverter,
            IContainersGetter                   _ContainersGetter,
            IViewGameTicker                     _GameTicker,
            IViewBackground                     _Background,
            IRendererAppearTransitioner         _Transitioner,
            IManagersGetter                     _Managers,
            IColorProvider                      _ColorProvider,
            IViewInputCommandsProceeder         _CommandsProceeder,
            IViewMazeAdditionalBackground       _AdditionalBackground,
            IViewTurretProjectile               _Projectile,
            IViewTurretBody                     _TurretBody,
            IViewParticlesThrower               _ParticlesThrower,
            IViewCharacter                      _Character,
            IViewSwitchLevelStageCommandInvoker _SwitchLevelStageCommandInvoker)
            : base(
                _ViewSettings,
                _Model,
                _CoordinateConverter,
                _ContainersGetter,
                _GameTicker,
                _Transitioner,
                _Managers,
                _ColorProvider,
                _CommandsProceeder)
        {
            Background           = _Background;
            AdditionalBackground = _AdditionalBackground;
            Projectile           = _Projectile;
            TurretBody           = _TurretBody;
            ParticlesThrower     = _ParticlesThrower;
            Character            = _Character;
            SwitchLevelStageCommandInvoker = _SwitchLevelStageCommandInvoker;
            ProjectileFake       = Projectile.Clone() as IViewTurretProjectile;
        }
        
        #endregion
        
        #region api

        public override Component[] Renderers => new Component[] { };
        
        public override object Clone() => new ViewMazeItemTurret(
            ViewSettings,
            Model,
            CoordinateConverter, 
            ContainersGetter, 
            GameTicker,
            Background,
            Transitioner,
            Managers,
            ColorProvider,
            CommandsProceeder,
            AdditionalBackground,
            Projectile.Clone()       as IViewTurretProjectile,
            TurretBody.Clone()       as IViewTurretBody,
            ParticlesThrower.Clone() as IViewParticlesThrower,
            Character,
            SwitchLevelStageCommandInvoker);

        public override bool ActivatedInSpawnPool
        {
            get => base.ActivatedInSpawnPool;
            set
            {
                if (!value)
                {
                    m_ProjectileMaskCollider.enabled = false;
                    m_ProjectileMask.enabled = false;
                }
                TurretBody    .Activated  = value;
                Projectile    .Activated  = value;
                ProjectileFake.Activated  = value;
                base.ActivatedInSpawnPool = value;
            }
        }

        public override void Init()
        {
            InitParticlesThrower();
            base.Init();
        }

        public override void UpdateState(ViewMazeItemProps _Props)
        {
            if (!Initialized)
            {
                Projectile.Init(false);
                ProjectileFake.Init(true);
                ProjectileFake.Tail.HideTail();
            }
            base.UpdateState(_Props);
            TurretBody.Update(_Props);
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            base.OnLevelStageChanged(_Args);
            switch (_Args.LevelStage)
            {
                case ELevelStage.Finished when 
                    _Args.PreviousStage != ELevelStage.Paused:
                {
                    StopProceedingTurret();
                }
                    break;
                case ELevelStage.Finished when 
                    _Args.PreviousStage == ELevelStage.Paused
                    && _Args.PrePreviousStage == ELevelStage.StartedOrContinued:
                {
                    StopProceedingTurret();
                }
                    break;
            }
        }

        public void PreShoot(TurretShotEventArgs _Args)
        {
            Cor.Run(HandleTurretPreShootCoroutine());
        }

        public void Shoot(TurretShotEventArgs _Args)
        {
            Cor.Run(HandleTurretShootCoroutine(_Args));
        }

        public void SetProjectileSortingOrder(int _Order)
        {
            Projectile.SetSortingOrder(_Order);
            ProjectileFake.SetSortingOrder(_Order);
        }

        public void UpdateTick()
        {
            if (!Initialized || !ActivatedInSpawnPool)
                return;
            if (AppearingState == EAppearingState.Dissapeared || AppearingState == EAppearingState.Appearing)
                return;
            if (!m_ProjRotating)
                Projectile.Rotation = ProjectileFake.Rotation;
            else
            {
                var angles = Vector3.forward * m_RotatingSpeed * GameTicker.DeltaTime;
                Projectile.Rotation = Quaternion.Euler(Projectile.Rotation.eulerAngles + angles);
            }
        }

        public override void Appear(bool _Appear)
        {
            base          .Appear(_Appear);
            Projectile    .Appear(_Appear);
            ProjectileFake.Appear(_Appear);
            TurretBody    .Appear(_Appear);
        }

        #endregion
        
        #region nonpublic methods

        protected override void InitShape()
        {
            var projParent = ContainersGetter.GetContainer(ContainerNames.MazeItems);
            static void SetProjectileMaskProperties(Rectangle _Mask)
            {
                _Mask.SetBlendMode(ShapesBlendMode.Subtractive)
                    .SetRenderQueue(-1)
                    .SetZTest(CompareFunction.Less)
                    .SetColor(new Color(0f, 0f, 0f, 1f / 255f))
                    .SetSortingOrder(SortingOrders.AdditionalBackgroundTexture)
                    .SetStencilComp(CompareFunction.Greater)
                    .SetStencilOpPass(StencilOp.Replace);
                _Mask.transform.SetPosZ(-0.1f);
                _Mask.enabled = false;
            }
            m_ProjectileMask = projParent.AddComponentOnNewChild<Rectangle>(
                "Turret Projectile Mask", out _);
            int maskHash = CommonUtils.StringToHash(UnityEngine.Random.value.ToString(CultureInfo.InvariantCulture));
            string maskName = "Turret Projectile Mask Collider " + maskHash;
            m_ProjectileMaskCollider = m_ProjectileMask.transform.AddComponentOnNewChild<BoxCollider2D>(
                "Turret Projectile Mask Collider " + maskName, out _);
            m_ProjectileMaskCollider.gameObject.layer = LayerMask.NameToLayer(LayerNamesCommon.Hi);
            m_ProjectileMaskCollider.isTrigger = true;
            SetProjectileMaskProperties(m_ProjectileMask);
            AdditionalBackground.GroupsCollected += SetStencilRefValues;
            Projectile.WallCollision += OnProjectileOnWallCollision;
        }

        private void OnProjectileOnWallCollision(Collider2D _Collider)
        {
            if (m_LastShotArgs == null)
                return;
            void StopProjectile()
            {
                m_ProjectileMovingLocked = true;
                Projectile.SetVelocity(Vector2.zero);
            }
            if (Character.GetObjects().Colliders.Contains(_Collider))
            {
                SwitchLevelStageCommandInvoker.SwitchLevelStage(EInputCommand.KillCharacter);
                StopProjectile();
                return;
            }
            bool isThisCollider = _Collider.transform.localPosition == m_MaskColliderCurrentPosition;
            if (!isThisCollider)
                return;
            StopProjectile();
            ThrowParticlesOnProjectileAndWallCollision(
                m_LastShotArgs.To,
                m_LastShotArgs.Direction);
        }

        private void InitParticlesThrower()
        {
            int sortingOrder = SortingOrders.GetBlockSortingOrder(Props.Type);
            ParticlesThrower.ParticleType = EParticleType.Bubbles;
            ParticlesThrower.SetPoolSize(ParticlesThrowerSize);
            ParticlesThrower.Init();
            ParticlesThrower.SetSortingOrder(sortingOrder + 3);
        }

        protected override void UpdateShape()
        {
            TurretBody.SetTurretContainer(Object);
            float scale = CoordinateConverter.Scale;
            var projectileScale = Vector2.one * scale * ProjectileContainerRadius * 0.9f;
            Projectile.SetScale(projectileScale);
            ProjectileFake.SetScale(projectileScale);
            var pos = CoordinateConverter.ToLocalMazeItemPosition(Props.Position);
            Projectile.SetPosition(pos);
            ProjectileFake.SetPosition(pos);
            m_ProjectileMask.SetWidth(scale).SetHeight(scale).enabled = false;
            m_ProjectileMaskCollider.size = Vector2.one * scale;
        }

        private void SetStencilRefValues(List<PointsGroupArgs> _Groups)
        {
            int GetGroupIndexByPoint()
            {
                if (_Groups == null)
                    return -2;
                foreach (var group in _Groups
                    .Where(_Group => _Group.Points.Contains(Props.Position)))
                {
                    return group.GroupIndex;
                }
                return -1;
            }
            int stencilRef = GetGroupIndexByPoint();
            if (stencilRef < 0)
                return;
            m_ProjectileMask.StencilRefID = Convert.ToByte(stencilRef + 1);
            Projectile.SetStencilRefId(stencilRef);
            ProjectileFake.SetStencilRefId(stencilRef);
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            
        }

        private IEnumerator HandleTurretPreShootCoroutine()
        {
            Cor.Run(IncreaseRotatingSpeed(0.1f));
            yield return ActivateRealProjectileAndOpenBarrel(0.2f);
        }
        
        private IEnumerator HandleTurretShootCoroutine(TurretShotEventArgs _Args)
        {
            Vector2 toPos = _Args.To;
            EnableProjectileMasksAndSetPositions(
                _Args.Direction + toPos);
            Cor.Run(AnimateFakeProjectileAndCloseBarrel(0.2f));
            yield return DoShoot(_Args);
        }

        private IEnumerator IncreaseRotatingSpeed(float _Duration)
        {
            yield return Cor.Lerp(
                GameTicker,
                _Duration, 
                _OnProgress: _P => m_RotatingSpeed = ViewSettings.turretProjectileRotationSpeed * _P);
        }

        private IEnumerator ActivateRealProjectileAndOpenBarrel(float _Delay)
        {
            yield return Cor.Delay(_Delay,
                GameTicker,
                () =>
                {
                    Projectile.Show(true);
                    ProjectileFake.Show(false);
                    var projectilePos = CoordinateConverter.ToLocalMazeItemPosition(Props.Position);
                    Projectile.SetPosition(projectilePos);
                    Projectile.SetVelocity(Vector2.zero);
                    HighlightBarrel(true);
                    OpenBarrel(true);
                });
        }

        private IEnumerator AnimateFakeProjectileAndCloseBarrel(float _Delay)
        {
            yield return Cor.Delay(
                _Delay,
                GameTicker,
                () =>
                {
                    Cor.Run(AnimateFakeProjectileBeforeShoot());
                    HighlightBarrel(false);
                    OpenBarrel(false);
                });
        }

        private void EnableProjectileMasksAndSetPositions(Vector2 _Mask1Pos)
        {
            m_ProjectileMask.enabled = false;
            var maskTr = m_ProjectileMask.transform;
            var pos = CoordinateConverter.ToLocalMazeItemPosition(_Mask1Pos);
            maskTr.SetLocalPosXY(pos).SetPosZ(-0.1f);
            m_MaskColliderCurrentPosition = m_ProjectileMaskCollider.transform.localPosition;
        }
        
        private IEnumerator AnimateFakeProjectileBeforeShoot()
        {
            ProjectileFake.SetScale(Vector2.zero);
            ProjectileFake.Show(true);
            ProjectileFake.SetPosition(CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            yield return Cor.Lerp(
                GameTicker,
                0.2f,
                _OnProgress: _P =>
                {
                    ProjectileFake.SetScale(
                        Vector2.one * _P * CoordinateConverter.Scale * ProjectileContainerRadius * 0.9f);
                });
        }
        
        private void OpenBarrel(bool _Open, bool _Instantly = false, bool _Forced = false)
        {
            if (_Open)
                m_ProjRotating = true;
            TurretBody.OpenBarrel(_Open, _Instantly, _Forced);
        }

        private void HighlightBarrel(bool _Open, bool _Instantly = false, bool _Forced = false)
        {
            TurretBody.HighlightBarrel(_Open, _Instantly, _Forced);
        }
        
        private IEnumerator DoShoot(TurretShotEventArgs _Args)
        {
            m_ProjectileMovingLocked = false;
            m_LastShotArgs = _Args;
            Managers.AudioManager.PlayClip(AudioClipArgsShurikenFly);
            Vector2 projectilePos = _Args.From;
            Projectile.Tail.ShowTail(_Args);
            m_ProjRotating = true;
            m_RotatingSpeed = ViewSettings.turretProjectileRotationSpeed;
            bool CorPredicate()
            {
                return !m_ProjectileMovingLocked;
            }
            void CorAction()
            {
                projectilePos += (Vector2)_Args.Direction
                                 * Model.Settings.turretProjectileSpeed
                                 * GameTicker.FixedDeltaTime;
                var newProjPos = CoordinateConverter.ToLocalMazeItemPosition(projectilePos);
                Projectile.SetPosition(newProjPos);
            }
            void CorFinish()
            {
                m_ProjRotating = false;
                Projectile.Show(false);
                Projectile.Tail.HideTail();
            }
            yield return Cor.DoWhile(
                CorPredicate,
                CorAction,
                CorFinish,
                GameTicker,
                _FixedUpdate: true);
        }
        
        private void StopProceedingTurret()
        {
            Cor.Run(Cor.WaitNextFrame(
                () =>
                {
                    OpenBarrel(false, true, true);
                    HighlightBarrel(false, true, true);
                }));
            m_ProjRotating = false;
        }

        protected override void OnAppearStart(bool _Appear)
        {
            if (_Appear)
            {
                Projectile.Tail.HideTail();
                ProjectileFake.Tail.HideTail();
                m_ProjRotating = false;
                m_ProjectileMask.enabled = true;
                m_ProjectileMaskCollider.enabled = true;
                OpenBarrel(false, true);
                Cor.Run(AnimateFakeProjectileBeforeShoot());
            }
            base.OnAppearStart(_Appear);
        }

        protected override void OnAppearFinish(bool _Appear)
        {
            m_ProjectileMask.enabled = false;
            if (!_Appear)
            {
                m_ProjRotating = false;
                m_ProjectileMaskCollider.enabled = false;
            }
            base.OnAppearFinish(_Appear);
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            return null;
        }
        
        private void ThrowParticlesOnProjectileAndWallCollision(
            V2Int _ProjectileFinishPosition,
            V2Int _ProjectileDirection)
        {
            var a = _ProjectileFinishPosition + _ProjectileDirection * 0.5f;
            a = CoordinateConverter.ToGlobalMazeItemPosition(a);
            var projMoveDir = RmazorUtils.GetDirection(_ProjectileDirection, EMazeOrientation.North);
            var projRealDirVec = RmazorUtils.GetDirectionVector(projMoveDir, Model.MazeRotation.Orientation);
            Vector2 throwDir = -projRealDirVec;
            if (Model.MazeRotation.Orientation == EMazeOrientation.East || Model.MazeRotation.Orientation == EMazeOrientation.West)
                throwDir = projRealDirVec;
            for (int i = 0; i < ParticlesThrowerSize; i++)
            {
                var orthDir = 0.5f * new Vector2(throwDir.y, throwDir.x);
                float alterationCoeff = i % 2 == 0 ? 1f : -1f;
                var directSpeedVector = throwDir * 16f;
                var orthSpeedVector = orthDir * alterationCoeff * (UnityEngine.Random.value + 0.5f) * 16f;
                var fullSpeedVector = directSpeedVector + orthSpeedVector;
                fullSpeedVector *= new Vector2(0.5f * UnityEngine.Random.value + 0.5f, 0.5f * UnityEngine.Random.value + 0.5f);
                var pos = a + orthDir * alterationCoeff * (UnityEngine.Random.value * 0.8f);
                float randScale = 0.5f + 0.3f * UnityEngine.Random.value;
                ParticlesThrower.ThrowParticle(pos, fullSpeedVector, randScale, 0.1f);
            }
        }

        #endregion
    }
}