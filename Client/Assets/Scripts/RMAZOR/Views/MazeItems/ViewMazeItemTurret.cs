using System;
using System.Collections;
using System.Collections.Generic;
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
using RMAZOR.Views.Common;
using RMAZOR.Views.Helpers;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.MazeItems.Additional;
using RMAZOR.Views.MazeItems.Props;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

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
        
        #endregion
        
        #region nonpublic members

        private static AudioClipArgs AudioClipArgsShurikenFly => new AudioClipArgs("shuriken", EAudioClipType.GameSound);
        
        private float     m_RotatingSpeed;
        private bool      m_ProjectileRotating;
        private Transform m_ProjectileTr;
        private Transform m_Projectile;
        private Transform m_ProjectileFakeContainer;
        private Transform m_ProjectileFakeTr;
        
        #endregion
        
        #region shapes

        protected override string         ObjectName => "Turret Block";
        private            Disc           m_Body;
        private            Disc           m_ProjectileHolderBorder;
        private            SpriteRenderer m_ProjectileRenderer;
        private            SpriteRenderer m_ProjectileFakeRenderer;
        private            SpriteMask     m_ProjectileMask;
        private            SpriteMask     m_ProjectileMask2;

        #endregion
        
        #region inject

        private IViewTurretProjectileTail ProjectileTail { get; }
        private IViewMazeBackground       Background     { get; }

        public ViewMazeItemTurret(
            ViewSettings _ViewSettings,
            IModelGame _Model,
            IMazeCoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IViewGameTicker _GameTicker,
            IViewTurretProjectileTail _ProjectileTail,
            IViewMazeBackground _Background,
            IViewAppearTransitioner _Transitioner,
            IManagersGetter _Managers,
            IColorProvider _ColorProvider,
            IViewInputCommandsProceeder _CommandsProceeder)
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
            ProjectileTail = _ProjectileTail;
            Background = _Background;
        }
        
        #endregion
        
        #region api
        
        public override Component[] Shapes => new Component[]
        {
            m_Body,
            m_ProjectileHolderBorder,
            m_ProjectileRenderer,
            m_ProjectileFakeRenderer
        };
        
        public override object Clone() => new ViewMazeItemTurret(
            ViewSettings,
            Model,
            CoordinateConverter, 
            ContainersGetter, 
            GameTicker,
            ProjectileTail.Clone() as IViewTurretProjectileTail, 
            Background,
            Transitioner,
            Managers,
            ColorProvider,
            CommandsProceeder);

        public override bool ActivatedInSpawnPool
        {
            get => base.ActivatedInSpawnPool;
            set
            {
                m_ProjectileMask.enabled = false;
                m_ProjectileMask2.enabled = false;
                base.ActivatedInSpawnPool = value;
            }
        }

        public override void Init(ViewMazeItemProps _Props)
        {
            base.Init(_Props);
            ProjectileTail.Init();
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            base.OnLevelStageChanged(_Args);
            if (_Args.Stage == ELevelStage.Finished && _Args.PreviousStage != ELevelStage.Paused)
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
            m_ProjectileRenderer.sortingOrder = _Order;
            m_ProjectileMask.frontSortingOrder = m_ProjectileMask2.frontSortingOrder = _Order;
            m_ProjectileMask.backSortingOrder = m_ProjectileMask2.backSortingOrder = _Order - 1;
            ProjectileTail.SetSortingOrder(_Order);
        }

        public void UpdateTick()
        {
            if (!Initialized || !ActivatedInSpawnPool)
                return;
            if (AppearingState == EAppearingState.Dissapeared)
                return;
            m_ProjectileHolderBorder.DashOffset = MathUtils.ClampInverse(
                m_ProjectileHolderBorder.DashOffset += 2f * GameTicker.DeltaTime,
                0f, 10f);
            if (AppearingState == EAppearingState.Appearing)
                return;
            if (!m_ProjectileRotating)
                m_Projectile.localEulerAngles = m_ProjectileFakeTr.localEulerAngles;
            else
                m_Projectile.Rotate(Vector3.forward * m_RotatingSpeed * GameTicker.DeltaTime);
        }

        #endregion
        
        #region nonpublic methods

        protected override void InitShape()
        {
            var body = Object.gameObject.AddComponentOnNewChild<Disc>("Turret", out _);
            body.Type = DiscType.Arc;
            body.ArcEndCaps = ArcEndCap.Round;
            body.Color = ColorProvider.GetColor(ColorIds.Main);
            body.SortingOrder = SortingOrders.GetBlockSortingOrder(Props.Type);
            var bhb = Object.gameObject.AddComponentOnNewChild<Disc>("Border", out _);
            bhb.Dashed = true;
            bhb.DashType = DashType.Rounded;
            bhb.Color = ColorProvider.GetColor(ColorIds.MazeItem1);
            bhb.Type = DiscType.Ring;
            bhb.SortingOrder = SortingOrders.GetBlockSortingOrder(Props.Type) + 1;
            bhb.DashSize = 2f;
            var projectileParent = ContainersGetter.GetContainer(ContainerNames.MazeItems);
            var projectileGo = Managers.PrefabSetManager.InitPrefab(
                projectileParent, "views", "turret_bullet");
            var projectileFakeGo = UnityEngine.Object.Instantiate(projectileGo);
            projectileFakeGo.SetParent(projectileParent);
            projectileFakeGo.name = "Turret Projectile Fake";
            m_ProjectileFakeContainer = projectileFakeGo.transform;
            m_ProjectileFakeRenderer = projectileFakeGo.GetCompItem<SpriteRenderer>("bullet");
            m_ProjectileFakeRenderer.color = ColorProvider.GetColor(ColorIds.MazeItem1);
            m_ProjectileTr = projectileGo.transform;
            m_ProjectileFakeTr = projectileFakeGo.transform;
            m_Projectile = projectileGo.GetContentItem("bullet").transform;
            m_ProjectileRenderer = m_Projectile.GetComponent<SpriteRenderer>();
            m_ProjectileRenderer.color = ColorProvider.GetColor(ColorIds.MazeItem1);
            var bmGo = Managers.PrefabSetManager.InitPrefab(
                projectileParent, "views", "turret_bullet_mask");
            var bmGo2 = UnityEngine.Object.Instantiate(bmGo);
            bmGo2.SetParent(projectileParent);
            var bm = bmGo.GetCompItem<SpriteMask>("mask");
            var bm2 = bmGo2.GetCompItem<SpriteMask>("mask");
            bm.enabled = bm2.enabled = false;
            bm.isCustomRangeActive = bm2.isCustomRangeActive = true;
            (m_Body, m_ProjectileHolderBorder, m_ProjectileMask, m_ProjectileMask2) = (body, bhb, bm, bm2);
        }

        protected override void UpdateShape()
        {
            float scale = CoordinateConverter.Scale;
            m_Body.Radius = CoordinateConverter.Scale * 0.5f;
            m_Body.Thickness = ViewSettings.LineWidth * scale;
            var projectileScale = Vector3.one * scale * ProjectileContainerRadius * 0.9f;
            m_ProjectileTr.transform.localScale = m_ProjectileFakeTr.transform.localScale = projectileScale;
            m_ProjectileTr.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            m_ProjectileFakeTr.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            var maskScale = scale * Vector3.one;
            m_ProjectileMask.transform.localScale = maskScale;
            m_ProjectileMask2.transform.localScale = maskScale;
            m_ProjectileHolderBorder.Radius = scale * ProjectileContainerRadius * 0.9f;
            m_ProjectileHolderBorder.Thickness = ViewSettings.LineWidth * scale * 0.5f;
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId == ColorIds.MazeItem1)
            {
                m_ProjectileHolderBorder.Color = _Color;
                m_ProjectileRenderer.color = _Color;
                m_ProjectileFakeRenderer.color = _Color;
            }
            else if (_ColorId == ColorIds.Main)
            {
                m_Body.Color = _Color;
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
                _Args.Direction + toPos, 
                toPos + _Args.Direction + 0.9f * _Args.Direction);
            Cor.Run(AnimateFakeProjectileAndCloseBarrel(0.2f));
            yield return DoShoot(_Args);
        }

        private IEnumerator IncreaseRotatingSpeed(float _Duration)
        {
            yield return Cor.Lerp(
                0f, 
                1f,
                _Duration, 
                _Progress => m_RotatingSpeed = ViewSettings.TurretProjectileRotationSpeed * _Progress,
                GameTicker);
        }

        private IEnumerator ActivateRealProjectileAndOpenBarrel(float _Delay)
        {
            yield return Cor.Delay(_Delay,
                () =>
            {
                m_Projectile.SetGoActive(true);
                m_ProjectileFakeTr.SetGoActive(false);
                var projectilePos = CoordinateConverter.ToLocalMazeItemPosition(Props.Position);
                m_ProjectileTr.transform.SetLocalPosXY(projectilePos);
                Cor.Run(HighlightBarrel(true));
                Cor.Run(OpenBarrel(true, false));
            });
        }

        private IEnumerator AnimateFakeProjectileAndCloseBarrel(float _Delay)
        {
            yield return Cor.Delay(
                _Delay,
                () =>
                {
                    Cor.Run(AnimateFakeProjectileBeforeShoot());
                    Cor.Run(HighlightBarrel(false));
                    Cor.Run(OpenBarrel(false, false));
                });
        }

        private void EnableProjectileMasksAndSetPositions(Vector2 _Mask1Pos, Vector2 _Mask2Pos)
        {
            m_ProjectileMask.enabled = m_ProjectileMask2.enabled = true;
            m_ProjectileMask.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(_Mask1Pos));
            m_ProjectileMask2.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(_Mask2Pos));
        }
        
        private IEnumerator AnimateFakeProjectileBeforeShoot()
        {
            m_ProjectileFakeContainer.transform.localScale = Vector3.zero;
            m_ProjectileFakeTr.SetGoActive(true);
            m_ProjectileFakeContainer.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            yield return Cor.Lerp(
                0f, 
                1f,
                0.2f,
                _Progress => m_ProjectileFakeContainer.transform.localScale =
                    Vector3.one * _Progress * CoordinateConverter.Scale * ProjectileContainerRadius * 0.9f,
                GameTicker);
        }
        
        private IEnumerator OpenBarrel(bool _Open, bool _Instantly, bool _Forced = false)
        {
            float openedStart, openedEnd, closedStart, closedEnd;
            (openedStart, openedEnd) = GetBarrelDiscAngles(true);
            (closedStart, closedEnd) = GetBarrelDiscAngles(false);
            float startFrom = _Open ? closedStart : openedStart;
            float startTo = !_Open ? closedStart : openedStart;
            float endFrom = _Open ? closedEnd : openedEnd;
            float endTo = !_Open ? closedEnd : openedEnd;
            if (_Open)
                m_ProjectileRotating = true;
            if (_Instantly && (_Forced || Model.LevelStaging.LevelStage != ELevelStage.Finished))
            {
                m_Body.AngRadiansStart = startTo;
                m_Body.AngRadiansEnd = endTo;
                yield break;
            }
            yield return Cor.Lerp(
                0f,
                1f,
                0.1f,
                _Progress =>
                {
                    m_Body.AngRadiansStart = Mathf.Lerp(startFrom, startTo, _Progress);
                    m_Body.AngRadiansEnd = Mathf.Lerp(endFrom, endTo, _Progress);
                },
                GameTicker,
                _BreakPredicate: () =>
                {
                    if (_Forced)
                        return false;
                    return Model.LevelStaging.LevelStage == ELevelStage.Finished;
                });
        }

        private IEnumerator HighlightBarrel(bool _Open, bool _Instantly = false, bool _Forced = false)
        {
            Color DefCol() => ColorProvider.GetColor(ColorIds.Main);
            var highlightCol = ColorProvider.GetColor(ColorIds.MazeItem1);
            Color StartCol() => _Open ? DefCol() : highlightCol;
            Color EndCol() => !_Open ? DefCol() : highlightCol;
            if (_Instantly && (_Forced || Model.LevelStaging.LevelStage != ELevelStage.Finished))
            {
                m_Body.Color = EndCol();
                yield break;
            }
            yield return Cor.Lerp(
                0f,
                1f,
                0.1f,
                _P =>
                {
                    m_Body.Color = Color.Lerp(StartCol(), EndCol(), _P);
                },
                GameTicker,
                _BreakPredicate: () => AppearingState != EAppearingState.Appeared
                                       || Model.LevelStaging.LevelStage == ELevelStage.Finished);
        }

        private IEnumerator DoShoot(TurretShotEventArgs _Args)
        {
            Managers.AudioManager.PlayClip(AudioClipArgsShurikenFly);
            Vector2 projectilePos = _Args.From;
            var projectilePosPrev = projectilePos;
            V2Int point;
            bool movedToTheEnd = false;
            m_ProjectileRotating = true;
            m_RotatingSpeed = ViewSettings.TurretProjectileRotationSpeed;
            var fullPath = RazorMazeUtils.GetFullPath(_Args.From, _Args.To);
            yield return Cor.DoWhile(
                () =>
                {
                    return ProjectileMovingPredicate(
                        fullPath, 
                        projectilePos,
                        projectilePosPrev,
                        ref movedToTheEnd);
                },
                () =>
                {
                    projectilePosPrev = projectilePos;
                    projectilePos += (Vector2)_Args.Direction * Model.Settings.TurretProjectileSpeed * GameTicker.FixedDeltaTime;
                    m_ProjectileTr.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(projectilePos));
                    point = V2Int.Round(projectilePos);
                    ProjectileTail.ShowTail(_Args, projectilePos);
                    if (point == _Args.To + 2 * _Args.Direction)
                        movedToTheEnd = true;
                }, () =>
                {
                    m_ProjectileRotating = false;
                    m_Projectile.SetGoActive(false);
                    ProjectileTail.HideTail();
                },
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
            Cor.Run(Cor.WaitEndOfFrame(
                () =>
                {
                    Cor.Run(OpenBarrel(false, true, true));
                    Cor.Run(HighlightBarrel(false, true, true));
                }));
            m_ProjectileRotating = false;
        }

        private Tuple<float, float> GetBarrelDiscAngles(bool _Opened)
        {
            var dir = Props.Directions.First();
            float angleStart = 0f;
            float angleEnd = 0f;
            if (dir == V2Int.Left)
            {
                angleStart = _Opened ? -135f : -160f;
                angleEnd = _Opened ? 135f : 160f;
            }
            else if (dir == V2Int.Right)
            {
                angleStart = _Opened ? 45f : 20f;
                angleEnd = _Opened ? 315f : 340f;
            }
            else if (dir == V2Int.Up)
            {
                angleStart = _Opened ? -225f : -250f;
                angleEnd = _Opened ? 45f : 70f;
            }
            else if (dir == V2Int.Down)
            {
                angleStart = _Opened ? -45f : -70f;
                angleEnd = _Opened ? 225f : 250f;
            }
            return new Tuple<float, float>(angleStart * Mathf.Deg2Rad, angleEnd * Mathf.Deg2Rad);
        }

        protected override void OnAppearStart(bool _Appear)
        {
            if (_Appear)
            {
                m_Projectile.SetGoActive(true);
                m_ProjectileMask.enabled = true;
                m_ProjectileMask2.enabled = true;
                m_ProjectileRotating = false;
                m_ProjectileFakeTr.SetGoActive(false);
                Cor.Run(AnimateFakeProjectileBeforeShoot());
                Cor.Run(OpenBarrel(false, true));
            }
            base.OnAppearStart(_Appear);
        }

        protected override void OnAppearFinish(bool _Appear)
        {
            if (!_Appear)
            {
                m_ProjectileMask.enabled = false;
                m_ProjectileMask2.enabled = false;
                m_ProjectileRotating = false;
            }
            base.OnAppearFinish(_Appear);
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var projectileRenderers = new Component[] {m_ProjectileRenderer, m_ProjectileFakeRenderer};
            var projectileRenderersCol = _Appear ? ColorProvider.GetColor(ColorIds.MazeItem1) : m_ProjectileFakeRenderer.color;
            return new Dictionary<IEnumerable<Component>, Func<Color>>
            {
                {new [] {m_ProjectileHolderBorder}, () => ColorProvider.GetColor(ColorIds.MazeItem1)},
                {new [] {m_Body}, () => ColorProvider.GetColor(ColorIds.Main)},
                {projectileRenderers, () => projectileRenderersCol}
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

        #endregion
    }
}