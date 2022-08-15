using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Common;
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
    
    public class ViewMazeItemTurret : ViewMazeItemBase, IViewMazeItemTurret, IUpdateTick
    {
        #region constants

        private const float ProjectileContainerRadius = 0.4f;
        private const int   ParticlesThrowerSize      = 30;
        
        #endregion
        
        #region nonpublic members

        protected override string ObjectName => "Turret Block";
        
        private float               m_RotatingSpeed;
        private bool                m_ProjRotating;
        private Disc                m_HolderBorder;
        private Rectangle           m_ProjectileMask;
        private BoxCollider2D       m_ProjectileMaskCollider;
        private TurretShotEventArgs m_LastShotArgs;
        private Vector3             m_MaskColliderCurrentPosition;

        #endregion
        
        #region inject

        private IViewBackground               Background           { get; }
        private IViewMazeAdditionalBackground AdditionalBackground { get; }
        private IViewTurretProjectile         Projectile           { get; }
        private IViewTurretProjectile         ProjectileFake       { get; }
        private IViewTurretBody               TurretBody           { get; }
        private IViewParticlesThrower         ParticlesThrower     { get; }

        private ViewMazeItemTurret(
            ViewSettings                  _ViewSettings,
            IModelGame                    _Model,
            ICoordinateConverter          _CoordinateConverter,
            IContainersGetter             _ContainersGetter,
            IViewGameTicker               _GameTicker,
            IViewBackground               _Background,
            IRendererAppearTransitioner   _Transitioner,
            IManagersGetter               _Managers,
            IColorProvider                _ColorProvider,
            IViewInputCommandsProceeder   _CommandsProceeder,
            IViewMazeAdditionalBackground _AdditionalBackground,
            IViewTurretProjectile         _Projectile,
            IViewTurretBody               _TurretBody,
            IViewParticlesThrower         _ParticlesThrower)
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
            ParticlesThrower.Clone() as IViewParticlesThrower);

        public override bool ActivatedInSpawnPool
        {
            get => base.ActivatedInSpawnPool;
            set
            {
                if (!value)
                {
                    m_ProjectileMaskCollider.enabled = false;
                    m_ProjectileMask.enabled = false;
                    m_HolderBorder.enabled   = false;
                }
                TurretBody.ActivatedInSpawnPool = value;
                Projectile.Activated            = value;
                ProjectileFake.Activated        = value;
                base.ActivatedInSpawnPool       = value;
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
            if (_Args.LevelStage == ELevelStage.Finished && _Args.PreviousStage != ELevelStage.Paused)
                StopProceedingTurret();
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
            if (AppearingState == EAppearingState.Dissapeared)
                return;
            m_HolderBorder.DashOffset = MathUtils.ClampInverse(
                m_HolderBorder.DashOffset += 2f * GameTicker.DeltaTime,
                0f, 10f);
            if (AppearingState == EAppearingState.Appearing)
                return;
            if (!m_ProjRotating)
                Projectile.ProjectileTransform.localRotation = ProjectileFake.ProjectileTransform.localRotation;
            else
            {
                var angles = Vector3.forward * m_RotatingSpeed * GameTicker.DeltaTime;
                Projectile.ProjectileTransform.Rotate(angles);
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
            int sortingOrder = GetSortingOrder();
            m_HolderBorder = Object.AddComponentOnNewChild<Disc>("Border", out _)
                .SetColor(ColorProvider.GetColor(ColorIds.MazeItem1))
                .SetSortingOrder(sortingOrder + 1)
                .SetType(DiscType.Ring)
                .SetDashed(true)
                .SetDashType(DashType.Rounded)
                .SetDashSize(2f);
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
            m_ProjectileMaskCollider.gameObject.layer = LayerMask.NameToLayer("χ Hi");
            m_ProjectileMaskCollider.isTrigger = true;
            SetProjectileMaskProperties(m_ProjectileMask);
            AdditionalBackground.GroupsCollected += SetStencilRefValues;
            Projectile.WallCollision += OnProjectileOnWallCollision;
        }

        private void OnProjectileOnWallCollision(Collider2D _Collider)
        {
            if (m_LastShotArgs == null)
                return;
            bool isThisCollider = _Collider.transform.localPosition == m_MaskColliderCurrentPosition;
            if (!isThisCollider)
                return;
            ThrowParticlesOnProjectileAndWallCollision(m_LastShotArgs.To, m_LastShotArgs.Direction);
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
            Projectile.ContainerTransform.SetLocalScaleXY(projectileScale);
            ProjectileFake.ContainerTransform.SetLocalScaleXY(projectileScale);
            var pos = CoordinateConverter.ToLocalMazeItemPosition(Props.Position);
            Projectile.ContainerTransform.SetLocalPosXY(pos);
            ProjectileFake.ContainerTransform.SetLocalPosXY(pos);
            m_ProjectileMask.SetWidth(scale).SetHeight(scale).enabled = false;
            m_ProjectileMaskCollider.size = Vector2.one * scale;
            m_HolderBorder.SetRadius(scale * ProjectileContainerRadius * 0.9f)
                .SetThickness(ViewSettings.LineThickness * scale * 0.5f);
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
            switch (_ColorId)
            {
                case ColorIds.Main: m_HolderBorder.Color = _Color; break;
            }
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
                    Projectile.ProjectileTransform.SetLocalPosXY(Vector2.zero);
                    Projectile.Show(true);
                    ProjectileFake.Show(false);
                    var projectilePos = CoordinateConverter.ToLocalMazeItemPosition(Props.Position);
                    Projectile.ContainerTransform.SetLocalPosXY(projectilePos);
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
            m_ProjectileMask.enabled = true;
            var maskTr = m_ProjectileMask.transform;
            var pos = CoordinateConverter.ToLocalMazeItemPosition(_Mask1Pos);
            maskTr.SetLocalPosXY(pos).SetPosZ(-0.1f);
            m_MaskColliderCurrentPosition = m_ProjectileMaskCollider.transform.localPosition;
        }
        
        private IEnumerator AnimateFakeProjectileBeforeShoot()
        {
            ProjectileFake.ContainerTransform.localScale = Vector3.zero;
            ProjectileFake.Show(true);
            ProjectileFake.ContainerTransform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            yield return Cor.Lerp(
                GameTicker,
                0.2f,
                _OnProgress: _P => ProjectileFake.ContainerTransform.localScale =
                    Vector3.one * _P * CoordinateConverter.Scale * ProjectileContainerRadius * 0.9f);
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
            m_LastShotArgs = _Args;
            Managers.AudioManager.PlayClip(GetAudioClipArgsShurikenFly());
            Vector2 projectilePos = _Args.From;
            Projectile.Tail.ShowTail(_Args, projectilePos);
            var projectilePosPrev = projectilePos;
            bool movedToTheEnd = false;
            m_ProjRotating = true;
            m_RotatingSpeed = ViewSettings.turretProjectileRotationSpeed;
            var fullPath = RmazorUtils.GetFullPath(_Args.From, _Args.To);
            V2Int pointCheck = default;
            bool CorPredicate()
            {
                return ProjectileMovingPredicate(
                    fullPath, 
                    projectilePos,
                    projectilePosPrev,
                    ref movedToTheEnd);
            }
            void CorAction()
            {
                projectilePosPrev = projectilePos;
                projectilePos += (Vector2)_Args.Direction
                                 * Model.Settings.turretProjectileSpeed
                                 * GameTicker.FixedDeltaTime;
                Projectile.ContainerTransform.SetLocalPosXY(
                    CoordinateConverter.ToLocalMazeItemPosition(projectilePos));
                var point = V2Int.Round(projectilePos);
                Projectile.Tail.ShowTail(_Args, projectilePos);
                if (point == _Args.To && point != pointCheck)
                {
                    // ThrowParticlesOnProjectileAndWallCollision(_Args.To, _Args.Direction);
                }

                pointCheck = point;
                if (point == _Args.To + _Args.Direction)
                    movedToTheEnd = true;
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

        private bool ProjectileMovingPredicate(
            ICollection<V2Int> _ProjectilePath,
            Vector2 _ProjectilePosition,
            Vector2 _PrevProjectilePosition,
            ref bool _MovedToTheEnd)
        {
            var point = (V2Int)_ProjectilePosition;
            if (Model.Character.IsMoving)
            {
                if (_ProjectilePath.Contains(point) 
                    && CheckForDeathWhileCharacterMoving(
                        _PrevProjectilePosition,
                        _ProjectilePosition,
                        Model.Character.MovingInfo.PreviousPrecisePosition,
                        Model.Character.MovingInfo.PrecisePosition))
                {
                    return false;
                }
            }
            else
            {
                if (_ProjectilePath.Contains(point) 
                    && CheckForCharacterDeathWhileCharacterNotMoving(
                        Model.Character.Position,
                        _ProjectilePosition))
                {
                    return false;
                }
            }
            return !_MovedToTheEnd;
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
                m_ProjectileMask.enabled = false;
                m_ProjectileMaskCollider.enabled = false;
            }
            base.OnAppearFinish(_Appear);
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            return new Dictionary<IEnumerable<Component>, Func<Color>>
            {
                {new [] {m_HolderBorder}, () => ColorProvider.GetColor(ColorIds.Main)},
            };
        }

        private bool CheckForCharacterDeathWhileCharacterNotMoving(Vector2 _CharacterPos, Vector2 _ProjectilePos)
        {
            if (!IsDeathPossible())
                return false;
            bool result = Vector2.Distance(_CharacterPos, _ProjectilePos) + MathUtils.Epsilon < 0.9f;
            if (result)
                CommandsProceeder.RaiseCommand(EInputCommand.KillCharacter, null);
            return result;
        }

        private bool CheckForDeathWhileCharacterMoving(
            Vector2 _ProjectileStart,
            Vector2 _ProjectileEnd,
            Vector2 _CharacterStart,
            Vector2 _CharacterEnd)
        {
            if (!IsDeathPossible())
                return false;
            var intersection = MathUtils.LineSegementsIntersect(
                _ProjectileStart, 
                _ProjectileEnd,
                _CharacterStart,
                _CharacterEnd);
            if (intersection.HasValue)
            {
                CommandsProceeder.RaiseCommand(EInputCommand.KillCharacter, 
                    new object[] { CoordinateConverter.ToLocalMazeItemPosition(intersection.Value) });
                return true;
            }
            var projAveragePos = (_ProjectileStart + _ProjectileEnd) * 0.5f;
            var charAveragePos = (_CharacterStart + _CharacterEnd) * 0.5f;
            if (!(Vector2.Distance(projAveragePos, charAveragePos) + MathUtils.Epsilon < 0.9f)) 
                return false;
            CommandsProceeder.RaiseCommand(EInputCommand.KillCharacter, 
                new object[] { CoordinateConverter.ToLocalMazeItemPosition(charAveragePos) });
            return true;
        }

        private bool IsDeathPossible()
        {
            if (!Initialized || !ActivatedInSpawnPool)
                return false;
            if (ProceedingStage != EProceedingStage.ActiveAndWorking)
                return false;
            if (!Model.Character.Alive)
                return false;
            if (Model.LevelStaging.LevelStage == ELevelStage.Finished)
                return false;
            return !Model.PathItemsProceeder.AllPathsProceeded;
        }

        private static AudioClipArgs GetAudioClipArgsShurikenFly()
        {
            return new AudioClipArgs("shuriken", EAudioClipType.GameSound);
        }
        
        private void ThrowParticlesOnProjectileAndWallCollision(
            V2Int _ProjectileFinishPosition,
            V2Int _ProjectileDirection)
        {
            var a = _ProjectileFinishPosition + _ProjectileDirection * 0.5f;
            a = CoordinateConverter.ToGlobalMazeItemPosition(a);
            var projMoveDir = RmazorUtils.GetMoveDirection(_ProjectileDirection, MazeOrientation.North);
            var projRealDirVec = RmazorUtils.GetDirectionVector(projMoveDir, Model.MazeRotation.Orientation);
            Vector2 throwDir = -projRealDirVec;
            if (Model.MazeRotation.Orientation == MazeOrientation.East || Model.MazeRotation.Orientation == MazeOrientation.West)
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