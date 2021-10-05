using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DI.Extensions;
using Entities;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
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
        private const string SoundClipNameBulletFly = "shuriken";
        
        #endregion
        
        #region nonpublic members

        private float m_RotatingSpeed;
        private bool m_BulletRotating;
        
        private Transform m_BulletTr;
        private Transform m_Bullet;
        private Transform m_BulletFakeContainer;
        private Transform m_BulletFake;
        
        #endregion
        
        #region shapes

        protected override object[] Shapes => new object[]
        {
            m_Body,
            m_BulletTail,
            m_BulletHolderBorder,
            m_BulletMask,
            m_BulletMask2,
            m_BulletRenderer,
            m_BulletFakeRenderer
        };

        private Disc m_Body;
        private Triangle m_BulletTail;
        private Disc m_BulletHolderBorder;
        private SpriteRenderer m_BulletRenderer;
        private SpriteRenderer m_BulletFakeRenderer;
        private SpriteMask m_BulletMask;
        private SpriteMask m_BulletMask2;

        #endregion
        
        #region inject
        
        private ITurretBulletTail BulletTail { get; }
        private IViewMazeBackground Background { get; }

        public ViewMazeItemTurret(
            ViewSettings _ViewSettings,
            IModelGame _Model,
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IGameTicker _GameTicker,
            ITurretBulletTail _BulletTail,
            IViewMazeBackground _Background,
            IViewAppearTransitioner _Transitioner,
            IManagersGetter _Managers)
            : base(
                _ViewSettings, 
                _Model, 
                _CoordinateConverter,
                _ContainersGetter,
                _GameTicker,
                _Transitioner,
                _Managers)
        {
            BulletTail = _BulletTail;
            Background = _Background;
        }
        
        #endregion
        
        #region api
        
        public override object Clone() => new ViewMazeItemTurret(
            ViewSettings,
            Model,
            CoordinateConverter, 
            ContainersGetter, 
            GameTicker,
            BulletTail,
            Background,
            Transitioner,
            Managers);

        public override bool Activated
        {
            get => m_Activated;
            set
            {
                m_Activated = value;
                Object.SetActive(value);
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
            if (!Initialized || !Activated)
                return;
            if (AppearingState == EAppearingState.Dissapeared)
                return;
            m_BulletHolderBorder.DashOffset += 2f * Time.deltaTime;
            if (AppearingState == EAppearingState.Appearing)
                return;
            if (!m_BulletRotating)
                m_Bullet.localEulerAngles = m_BulletFake.localEulerAngles;
            else
                m_Bullet.Rotate(Vector3.forward * m_RotatingSpeed * Time.deltaTime);
        }

        #endregion
        
        #region nonpublic methods

        protected override void SetShape()
        {
            var go = Object;

            var sh = ContainersGetter.MazeItemsContainer.gameObject.GetOrAddComponentOnNewChild<Disc>(
                "Turret",
                ref go,
                CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            sh.Radius = CoordinateConverter.GetScale() * 0.5f;
            sh.Type = DiscType.Arc;
            sh.ArcEndCaps = ArcEndCap.Round;
            sh.Color = DrawingUtils.ColorLines;
            sh.Thickness = ViewSettings.LineWidth * CoordinateConverter.GetScale();
            m_Body = sh;
            
            var bhb = sh.gameObject.AddComponentOnNewChild<Disc>("Border", out _, Vector2.zero);
            bhb.Radius = CoordinateConverter.GetScale() * BulletContainerRadius * 0.9f;
            bhb.Dashed = true;
            bhb.DashType = DashType.Rounded;
            bhb.Color = DrawingUtils.ColorLines;
            bhb.Type = DiscType.Ring;
            bhb.Thickness = ViewSettings.LineWidth * CoordinateConverter.GetScale() * 0.5f;
            bhb.SortingOrder = DrawingUtils.GetBlockSortingOrder(Props.Type) + 2;
            bhb.DashSize = 2f;
            m_BulletHolderBorder = bhb;

            var bulletGo = PrefabUtilsEx.InitPrefab(
                ContainersGetter.MazeItemsContainer, "views", "turret_bullet");
            m_BulletTr = bulletGo.transform;
            m_BulletTr.transform.localScale =
                Vector3.one * CoordinateConverter.GetScale() * BulletContainerRadius * 0.9f;
            m_BulletTr.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            m_Bullet = bulletGo.GetContentItem("bullet").transform;
            
            m_BulletRenderer = m_Bullet.GetComponent<SpriteRenderer>();
            m_BulletTail = bulletGo.GetCompItem<Triangle>("tail");

            var bulletFakeGo = PrefabUtilsEx.InitPrefab(
                ContainersGetter.MazeItemsContainer, "views", "turret_bullet");
            bulletFakeGo.name = "Turret Bullet Fake";
            m_BulletFakeContainer = bulletFakeGo.transform;
            m_BulletFakeRenderer = bulletFakeGo.GetCompItem<SpriteRenderer>("bullet");
            
            m_BulletFake = m_BulletFakeRenderer.transform;
            bulletFakeGo.GetCompItem<Triangle>("tail").enabled = false;

            var bulletMaskGo = PrefabUtilsEx.InitPrefab(
                ContainersGetter.MazeItemsContainer, "views", "turret_bullet_mask");
            var bm = bulletMaskGo.GetCompItem<SpriteMask>("mask");
            var bulletMaskGo2 = UnityEngine.Object.Instantiate(bulletMaskGo);
            bulletMaskGo2.SetParent(ContainersGetter.MazeItemsContainer);
            
            var bm2 = bulletMaskGo2.GetCompItem<SpriteMask>("mask");
            bm.enabled = bm2.enabled = false;
            bm.isCustomRangeActive = bm2.isCustomRangeActive = true;
            bm.transform.localScale = bm2.transform.localScale = CoordinateConverter.GetScale() * Vector3.one;
            bm.isCustomRangeActive = bm2.isCustomRangeActive = true;
            
            Object = go;
            m_BulletMask = bm;
            m_BulletMask2 = bm2;
            
            DeactivateShapes();
        }
        
        private IEnumerator HandleTurretPreShootCoroutine()
        {
            Coroutines.Run(IncreaseRotatingSpeed(0.1f));
            yield return ActivateRealBulletAndOpenBarrel(0.2f);
        }
        
        private IEnumerator HandleTurretShootCoroutine(TurretShotEventArgs _Args)
        {
            var toPos = _Args.To.ToVector2();
            EnableBulletMasksAndSetPositions(toPos, toPos + _Args.Direction.ToVector2() * 0.9f);
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
                m_BulletFake.SetGoActive(false);
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
            m_BulletFake.SetGoActive(true);
            m_BulletFakeContainer.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            yield return Coroutines.Lerp(
                0f, 
                1f,
                0.2f,
                _Progress => m_BulletFakeContainer.transform.localScale =
                    Vector3.one * _Progress * CoordinateConverter.GetScale() * BulletContainerRadius * 0.9f,
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
            Managers.Notify(_SM => _SM.PlayClip(
                SoundClipNameBulletFly,
                true));
            
            var fromPos = _Args.From.ToVector2();
            V2Int point = default;
            bool movedToTheEnd = false;
            m_BulletRotating = true;
            m_RotatingSpeed = ViewSettings.TurretBulletRotationSpeed;
            yield return Coroutines.DoWhile(
                () =>
                {
                    if (point == Model.Character.Position)
                    {
                        Model.Character.RaiseDeath();
                        return false;
                    }
                    return !movedToTheEnd;
                },
                () =>
                {
                    fromPos += _Args.Direction.ToVector2() * _Args.ProjectileSpeed;
                    m_BulletTr.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(fromPos));
                    point = V2Int.Round(fromPos);
                    BulletTail.ShowTail(_Args);
                    if (point == _Args.To + _Args.Direction)
                        movedToTheEnd = true;
                }, () =>
                {
                    m_BulletRotating = false;
                    m_Bullet.SetGoActive(false);
                    BulletTail.HideTail(_Args);
                    Managers.Notify(_SM => _SM.StopClip(
                        SoundClipNameBulletFly));
                });
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

        protected override void Appear(bool _Appear)
        {
            AppearingState = _Appear ? EAppearingState.Appearing : EAppearingState.Dissapearing;
            if (_Appear)
            {
                m_BulletRotating = false;
                m_BulletFake.SetGoActive(false);
                Coroutines.Run(AnimateFakeBulletBeforeShoot());
                Coroutines.Run(OpenBarrel(false, true));
            }
            Coroutines.Run(Coroutines.WaitWhile(
                () => !Initialized,
                () =>
                {
                    object[] bulletRenderers;
                    Color bulletRenderersCol;
                    if (_Appear)
                    {
                        m_Bullet.SetGoActive(true);
                        bulletRenderers = new object[] {m_BulletRenderer, m_BulletFakeRenderer};
                        bulletRenderersCol = DrawingUtils.ColorLines;
                    }
                    else
                    {
                        bulletRenderers = new object[] {m_BulletRenderer, m_BulletFakeRenderer};
                        bulletRenderersCol = m_BulletFakeRenderer.color;
                    }
                    
                    Transitioner.DoAppearTransitionSimple(
                        _Appear,
                        GameTicker,
                        new Dictionary<object[], Func<Color>>
                        {
                            {new object [] {m_BulletHolderBorder, m_Body}, () => DrawingUtils.ColorLines},
                            {bulletRenderers, () => bulletRenderersCol}
                        },
                        Props.Position,
                        () =>
                        {
                            if (!_Appear)
                            {
                                m_BulletRotating = false;
                                DeactivateShapes();
                            }
                            AppearingState = _Appear ? EAppearingState.Appeared : EAppearingState.Dissapeared;
                        });
                }));
        }

        #endregion
    }
}