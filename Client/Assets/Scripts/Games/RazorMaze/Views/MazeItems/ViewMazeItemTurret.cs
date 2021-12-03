using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using DI.Extensions;
using Entities;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.InputConfigurators;
using Games.RazorMaze.Views.MazeItems.Additional;
using Games.RazorMaze.Views.MazeItems.Props;
using Games.RazorMaze.Views.Utils;
using Shapes;
using Ticker;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.MazeItems
{
    public interface IViewMazeItemTurret : IViewMazeItem
    {
        void PreShoot(TurretShotEventArgs _Args);
        void Shoot(TurretShotEventArgs _Args);
        void SetBulletSortingOrder(int _Order);
    }
    
    public class ViewMazeItemTurret : ViewMazeItemBase, IViewMazeItemTurret, IUpdateTick
    {
        #region constants

        private const float BulletContainerRadius = 0.4f;
        
        #endregion
        
        #region nonpublic members

        private static AudioClipArgs AudioClipArgsShurikenFly => new AudioClipArgs("shuriken", EAudioClipType.Sound);
        
        private float     m_RotatingSpeed;
        private bool      m_BulletRotating;
        private Transform m_BulletTr;
        private Transform m_Bullet;
        private Transform m_BulletFakeContainer;
        private Transform m_BulletFakeTr;
        
        #endregion
        
        #region shapes

        protected override string ObjectName => "Turret Block";
        private Disc m_Body;
        private Disc m_BulletHolderBorder;
        private SpriteRenderer m_BulletRenderer;
        private SpriteRenderer m_BulletFakeRenderer;
        private SpriteMask m_BulletMask;
        private SpriteMask m_BulletMask2;

        #endregion
        
        #region inject

        private ModelSettings               ModelSettings     { get; }
        private IViewTurretBulletTail       BulletTail        { get; }
        private IViewMazeBackground         Background        { get; }

        public ViewMazeItemTurret(
            ViewSettings _ViewSettings,
            ModelSettings _ModelSettings,
            IModelGame _Model,
            IMazeCoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IViewGameTicker _GameTicker,
            IViewTurretBulletTail _BulletTail,
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
            ModelSettings = _ModelSettings;
            BulletTail = _BulletTail;
            Background = _Background;
        }
        
        #endregion
        
        #region api
        
        public override Component[] Shapes => new Component[]
        {
            m_Body,
            m_BulletHolderBorder,
            m_BulletRenderer,
            m_BulletFakeRenderer
        };
        
        public override object Clone() => new ViewMazeItemTurret(
            ViewSettings,
            ModelSettings,
            Model,
            CoordinateConverter, 
            ContainersGetter, 
            GameTicker,
            BulletTail,
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
                m_BulletMask.enabled = false;
                m_BulletMask2.enabled = false;
                base.ActivatedInSpawnPool = value;
            }
        }

        public override void Init(ViewMazeItemProps _Props)
        {
            base.Init(_Props);
            BulletTail.Init();
        }

        public void PreShoot(TurretShotEventArgs _Args) => Coroutines.Run(HandleTurretPreShootCoroutine());

        public void Shoot(TurretShotEventArgs _Args)
        {
            Coroutines.Run(HandleTurretShootCoroutine(_Args));
        }

        public void SetBulletSortingOrder(int _Order)
        {
            m_BulletRenderer.sortingOrder = _Order;
            m_BulletMask.frontSortingOrder = m_BulletMask2.frontSortingOrder = _Order;
            m_BulletMask.backSortingOrder = m_BulletMask2.backSortingOrder = _Order - 1;
        }

        public void UpdateTick()
        {
            if (!Initialized || !ActivatedInSpawnPool)
                return;
            if (AppearingState == EAppearingState.Dissapeared)
                return;
            m_BulletHolderBorder.DashOffset = MathUtils.ClampInverse(
                m_BulletHolderBorder.DashOffset += 2f * GameTicker.DeltaTime,
                0f, 10f);
            if (AppearingState == EAppearingState.Appearing)
                return;
            if (!m_BulletRotating)
                m_Bullet.localEulerAngles = m_BulletFakeTr.localEulerAngles;
            else
                m_Bullet.Rotate(Vector3.forward * m_RotatingSpeed * GameTicker.DeltaTime);
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
            var bulletParent = ContainersGetter.GetContainer(ContainerNames.MazeItems);
            var bulletGo = PrefabUtilsEx.InitPrefab(
                bulletParent, "views", "turret_bullet");
            var bulletFakeGo = UnityEngine.Object.Instantiate(bulletGo);
            bulletFakeGo.SetParent(bulletParent);
            bulletFakeGo.name = "Turret Bullet Fake";
            
            m_BulletFakeContainer = bulletFakeGo.transform;
            m_BulletFakeRenderer = bulletFakeGo.GetCompItem<SpriteRenderer>("bullet");
            m_BulletFakeRenderer.color = ColorProvider.GetColor(ColorIds.MazeItem1);
            
            m_BulletTr = bulletGo.transform;
            m_BulletFakeTr = bulletFakeGo.transform;
            
            m_Bullet = bulletGo.GetContentItem("bullet").transform;
            m_BulletRenderer = m_Bullet.GetComponent<SpriteRenderer>();
            m_BulletRenderer.color = ColorProvider.GetColor(ColorIds.MazeItem1);

            var bmGo = PrefabUtilsEx.InitPrefab(
                bulletParent, "views", "turret_bullet_mask");
            var bmGo2 = UnityEngine.Object.Instantiate(bmGo);
            bmGo2.SetParent(bulletParent);
            
            var bm = bmGo.GetCompItem<SpriteMask>("mask");
            var bm2 = bmGo2.GetCompItem<SpriteMask>("mask");
            bm.enabled = bm2.enabled = false;
            bm.isCustomRangeActive = bm2.isCustomRangeActive = true;
            
            (m_Body, m_BulletHolderBorder, m_BulletMask, m_BulletMask2) = (body, bhb, bm, bm2);
        }

        protected override void UpdateShape()
        {
            var scale = CoordinateConverter.Scale;
            m_Body.Radius = CoordinateConverter.Scale * 0.5f;
            m_Body.Thickness = ViewSettings.LineWidth * scale;
            var bulletScale = Vector3.one * scale * BulletContainerRadius * 0.9f;
            m_BulletTr.transform.localScale = m_BulletFakeTr.transform.localScale = bulletScale;
            m_BulletTr.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            m_BulletFakeTr.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            var maskScale = scale * Vector3.one;
            m_BulletMask.transform.localScale = maskScale;
            m_BulletMask2.transform.localScale = maskScale;
            m_BulletHolderBorder.Radius = scale * BulletContainerRadius * 0.9f;
            m_BulletHolderBorder.Thickness = ViewSettings.LineWidth * scale * 0.5f;
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId == ColorIds.MazeItem1)
            {
                m_BulletHolderBorder.Color = _Color;
                m_BulletRenderer.color = _Color;
                m_BulletFakeRenderer.color = _Color;
            }
            else if (_ColorId == ColorIds.Main)
            {
                m_Body.Color = _Color;
            }
        }

        private IEnumerator HandleTurretPreShootCoroutine()
        {
            Coroutines.Run(IncreaseRotatingSpeed(0.1f));
            yield return ActivateRealBulletAndOpenBarrel(0.2f);
        }
        
        private IEnumerator HandleTurretShootCoroutine(TurretShotEventArgs _Args)
        {
            Vector2 toPos = _Args.To;
            EnableBulletMasksAndSetPositions(
                _Args.Direction + toPos, 
                toPos + _Args.Direction + 0.9f * _Args.Direction);
            Coroutines.Run(AnimateFakeBulletAndCloseBarrel(0.2f));
            yield return DoShoot(_Args);
        }

        private IEnumerator IncreaseRotatingSpeed(float _Duration)
        {
            yield return Coroutines.Lerp(
                0f, 
                1f,
                _Duration, 
                _Progress => m_RotatingSpeed = ViewSettings.TurretBulletRotationSpeed * _Progress,
                GameTicker);
        }

        private IEnumerator ActivateRealBulletAndOpenBarrel(float _Delay)
        {
            yield return Coroutines.Delay(() =>
            {
                m_Bullet.SetGoActive(true);
                m_BulletFakeTr.SetGoActive(false);
                var bulletPos = CoordinateConverter.ToLocalMazeItemPosition(Props.Position);
                m_BulletTr.transform.SetLocalPosXY(bulletPos);
                
                Coroutines.Run(OpenBarrel(true, false));
            }, _Delay);
        }

        private IEnumerator AnimateFakeBulletAndCloseBarrel(float _Delay)
        {
            yield return Coroutines.Delay(
                () =>
                {
                    Coroutines.Run(AnimateFakeBulletBeforeShoot());
                    Coroutines.Run(OpenBarrel(false, false));
                },
                _Delay);
        }

        private void EnableBulletMasksAndSetPositions(Vector2 _Mask1Pos, Vector2 _Mask2Pos)
        {
            m_BulletMask.enabled = m_BulletMask2.enabled = true;
            m_BulletMask.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(_Mask1Pos));
            m_BulletMask2.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(_Mask2Pos));
        }
        
        private IEnumerator AnimateFakeBulletBeforeShoot()
        {
            m_BulletFakeContainer.transform.localScale = Vector3.zero;
            m_BulletFakeTr.SetGoActive(true);
            m_BulletFakeContainer.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            yield return Coroutines.Lerp(
                0f, 
                1f,
                0.2f,
                _Progress => m_BulletFakeContainer.transform.localScale =
                    Vector3.one * _Progress * CoordinateConverter.Scale * BulletContainerRadius * 0.9f,
                GameTicker);
        }
        
        private IEnumerator OpenBarrel(bool _Open, bool _Instantly)
        {
            float openedStart, openedEnd, closedStart, closedEnd;
            (openedStart, openedEnd) = GetBarrelDiscAngles(true);
            (closedStart, closedEnd) = GetBarrelDiscAngles(false);

            float startFrom = _Open ? closedStart : openedStart;
            float startTo = !_Open ? closedStart : openedStart;
            float endFrom = _Open ? closedEnd : openedEnd;
            float endTo = !_Open ? closedEnd : openedEnd;

            if (_Open)
                m_BulletRotating = true;

            if (_Instantly)
            {
                m_Body.AngRadiansStart = startTo;
                m_Body.AngRadiansEnd = endTo;
                yield break;
            }

            yield return Coroutines.Lerp(
                0f,
                1f,
                0.1f,
                _Progress =>
                {
                    m_Body.AngRadiansStart = Mathf.Lerp(startFrom, startTo, _Progress);
                    m_Body.AngRadiansEnd = Mathf.Lerp(endFrom, endTo, _Progress);
                },
                GameTicker);
        }

        private IEnumerator DoShoot(TurretShotEventArgs _Args)
        {
            Managers.AudioManager.PlayClip(AudioClipArgsShurikenFly);
            Managers.HapticsManager.PlayPreset(EHapticsPresetType.HeavyImpact);
            Vector2 projectilePos = _Args.From;
            var projectilePosPrev = projectilePos;
            V2Int point = default;
            bool movedToTheEnd = false;
            m_BulletRotating = true;
            m_RotatingSpeed = ViewSettings.TurretBulletRotationSpeed;
            var fullPath = RazorMazeUtils.GetFullPath(_Args.From, _Args.To);
            yield return Coroutines.DoWhile(
                () =>
                {
                    if (Model.Character.IsMoving)
                    {
                        if (fullPath.Contains(point) 
                            && CheckForDeathWhileCharacterMoving(
                            projectilePosPrev,
                            projectilePos,
                            Model.Character.MovingInfo.PreviousPrecisePosition,
                            Model.Character.MovingInfo.PrecisePosition))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (fullPath.Contains(point) 
                            && CheckForCharacterDeathWhileCharacterNotMoving(
                                Model.Character.Position,
                                projectilePos))
                        {
                            return false;
                        }
                    }
                    return !movedToTheEnd;
                },
                () =>
                {
                    projectilePosPrev = projectilePos;
                    projectilePos += (Vector2)_Args.Direction * ModelSettings.TurretProjectileSpeed * GameTicker.FixedDeltaTime;
                    m_BulletTr.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(projectilePos));
                    point = V2Int.Round(projectilePos);
                    BulletTail.ShowTail(_Args);
                    if (point == _Args.To + _Args.Direction + _Args.Direction)
                        movedToTheEnd = true;
                }, () =>
                {
                    m_BulletRotating = false;
                    m_Bullet.SetGoActive(false);
                    BulletTail.HideTail(_Args);
                },
                GameTicker,
                _FixedUpdate: true);
        }

        private Tuple<float, float> GetBarrelDiscAngles(bool _Opened)
        {
            var dir = Props.Directions.First();
            float angleStart = 0f;
            float angleEnd = 0f;
            if (dir == V2Int.left)
            {
                angleStart = _Opened ? -135f : -160f;
                angleEnd = _Opened ? 135f : 160f;
            }
            else if (dir == V2Int.right)
            {
                angleStart = _Opened ? 45f : 20f;
                angleEnd = _Opened ? 315f : 340f;
            }
            else if (dir == V2Int.up)
            {
                angleStart = _Opened ? -225f : -250f;
                angleEnd = _Opened ? 45f : 70f;
            }
            else if (dir == V2Int.down)
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
                m_Bullet.SetGoActive(true);
                m_BulletMask.enabled = true;
                m_BulletMask2.enabled = true;
                m_BulletRotating = false;
                m_BulletFakeTr.SetGoActive(false);
                Coroutines.Run(AnimateFakeBulletBeforeShoot());
                Coroutines.Run(OpenBarrel(false, true));
            }
            base.OnAppearStart(_Appear);
        }

        protected override void OnAppearFinish(bool _Appear)
        {
            if (!_Appear)
            {
                m_BulletMask.enabled = false;
                m_BulletMask2.enabled = false;
                m_BulletRotating = false;
            }
            base.OnAppearFinish(_Appear);
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var bulletRenderers = new Component[] {m_BulletRenderer, m_BulletFakeRenderer};
            var bulletRenderersCol = _Appear ? ColorProvider.GetColor(ColorIds.MazeItem1) : m_BulletFakeRenderer.color;
            return new Dictionary<IEnumerable<Component>, Func<Color>>
            {
                {new [] {m_BulletHolderBorder}, () => ColorProvider.GetColor(ColorIds.MazeItem1)},
                {new [] {m_Body}, () => ColorProvider.GetColor(ColorIds.Main)},
                {bulletRenderers, () => bulletRenderersCol}
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
            if (Model.PathItemsProceeder.AllPathsProceeded)
                return false;
            return true;
        }

        #endregion
    }
}