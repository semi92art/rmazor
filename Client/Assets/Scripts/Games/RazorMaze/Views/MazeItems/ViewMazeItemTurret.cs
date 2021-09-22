using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DI.Extensions;
using Entities;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.MazeItems.Additional;
using Games.RazorMaze.Views.MazeItems.Props;
using Games.RazorMaze.Views.Utils;
using Shapes;
using Ticker;
using TimeProviders;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.MazeItems
{
    public interface IViewMazeItemTurret : IViewMazeItem
    {
        void PreShoot(TurretShotEventArgs _Args);
        void Shoot(TurretShotEventArgs _Args);
    }
    
    public class ViewMazeItemTurret : ViewMazeItemBase, IViewMazeItemTurret, IUpdateTick
    {
        #region constants
        
        private const float BulletContainerRadius = 0.4f;
        
        #endregion
        
        #region nonpublic members

        private static int _bulletSortingOrder;
        private float m_RotatingSpeed;
        private bool m_Moving;
        private bool m_Rotating;
        
        private Rectangle m_Barrel;
        private Triangle m_BulletTail;
        private Disc m_BulletHolderBorder;
        
        private Transform m_BulletContainer;
        private Transform m_Bullet;
        private Transform m_BulletFakeContainer;
        private Transform m_BulletFake;
        private SpriteMask m_BulletMask;
        private SpriteMask m_BulletMask2;
        
        #endregion
        
        #region inject
        
        private ITurretBulletTail BulletTail { get; }
        private IModelCharacter ModelCharacter { get; }

        public ViewMazeItemTurret(
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            ITicker _Ticker,
            IModelMazeData _Data,
            IGameTimeProvider _GameTimeProvider,
            ITurretBulletTail _BulletTail,
            IModelCharacter _ModelCharacter,
            ViewSettings _ViewSettings)
            : base(_ViewSettings, _Data, _CoordinateConverter, _ContainersGetter, _GameTimeProvider, _Ticker)
        {
            BulletTail = _BulletTail;
            ModelCharacter = _ModelCharacter;
        }
        
        #endregion
        
        #region api

        public override void Init(ViewMazeItemProps _Props)
        {
            base.Init(_Props);
            BulletTail.Init();
            Coroutines.Run(OpenBarrel(false, true));
        }

        public void PreShoot(TurretShotEventArgs _Args) => Coroutines.Run(HandleTurretPreShootCoroutine());

        public void Shoot(TurretShotEventArgs _Args) => Coroutines.Run(HandleTurretShootCoroutine(_Args));

        public void UpdateTick()
        {
            if (!Initialized || !Activated)
                return;
            float rotSpeed = m_RotatingSpeed * Time.deltaTime;
            if (!m_Moving)
                m_Bullet.localEulerAngles = m_BulletFake.localEulerAngles;
            else
                m_Bullet.Rotate(Vector3.forward * rotSpeed);
            m_BulletHolderBorder.DashOffset += 2f * Time.deltaTime;
        }

        public override object Clone() => new ViewMazeItemTurret(
            CoordinateConverter, ContainersGetter, Ticker, Data, GameTimeProvider, BulletTail, ModelCharacter, ViewSettings);
        
        #endregion
        
        #region nonpublic members

        protected override void SetShape()
        {
            var go = Object;
            var sh = ContainersGetter.MazeItemsContainer.gameObject
                .GetOrAddComponentOnNewChild<Rectangle>("Path Item", ref go,
                    CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            go.DestroyChildrenSafe();
            sh.Width = sh.Height = CoordinateConverter.GetScale() * 1.03f;
            sh.Type = Rectangle.RectangleType.RoundedSolid;
            sh.CornerRadius = ViewSettings.CornerRadius * CoordinateConverter.GetScale();
            sh.Color = DrawingUtils.ColorLines;
            sh.SortingOrder = DrawingUtils.GetBlockSortingOrder(Props.Type);

            var bullHold = go.AddComponentOnNewChild<Disc>("Bullet Holder", out _, Vector2.zero);
            bullHold.Radius = CoordinateConverter.GetScale() * BulletContainerRadius;
            bullHold.Color = DrawingUtils.ColorBack;
            bullHold.Type = DiscType.Disc;
            bullHold.SortingOrder = DrawingUtils.GetBlockSortingOrder(Props.Type) + 1;
            var bcb = bullHold.gameObject.AddComponentOnNewChild<Disc>("Border", out _, Vector2.zero);
            bcb.Radius = CoordinateConverter.GetScale() * BulletContainerRadius * 0.9f;
            bcb.Dashed = true;
            bcb.DashType = DashType.Rounded;
            bcb.Color = DrawingUtils.ColorLines;
            bcb.Type = DiscType.Ring;
            bcb.Thickness = ViewSettings.LineWidth * CoordinateConverter.GetScale() * 0.5f;
            bcb.SortingOrder = DrawingUtils.GetBlockSortingOrder(Props.Type) + 2;
            bcb.DashSize = 2f;
            m_BulletHolderBorder = bcb;
            
            var barrel = go.AddComponentOnNewChild<Rectangle>("Barrel", out _, Vector2.zero);
            barrel.Type = Rectangle.RectangleType.HardSolid;
            barrel.Color = DrawingUtils.ColorBack;
            barrel.SortingOrder = DrawingUtils.GetBlockSortingOrder(Props.Type) + 1;

            var tr = barrel.transform;
            (tr.localPosition, tr.localEulerAngles) = GetBarrelLocalPositionAndRotation();
            (barrel.Width, barrel.Height) = GetBarrelSize();
            barrel.Height *= 0.1f;

            var bulletGo = PrefabUtilsEx.InitPrefab(
                ContainersGetter.MazeItemsContainer, "views", "turret_bullet");
            m_BulletContainer = bulletGo.transform;
            m_BulletContainer.transform.localScale =
                Vector3.one * CoordinateConverter.GetScale() * BulletContainerRadius * 0.9f;
            m_BulletContainer.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            m_Bullet = bulletGo.GetContentItem("bullet").transform;

            var bulletSprRend = m_Bullet.GetComponent<SpriteRenderer>();
            bulletSprRend.sortingOrder = GetBulletSortingOrder(DrawingUtils.GetBlockSortingOrder(Props.Type) + 2);
            m_BulletTail = bulletGo.GetCompItem<Triangle>("tail");

            var bulletFakeGo = PrefabUtilsEx.InitPrefab(
                ContainersGetter.MazeItemsContainer, "views", "turret_bullet");
            m_BulletFakeContainer = bulletFakeGo.transform;
            
            m_BulletFake = bulletFakeGo.GetContentItem("bullet").transform;
            m_BulletFake.GetComponent<SpriteRenderer>().sortingOrder = DrawingUtils.GetBlockSortingOrder(Props.Type) + 2;
            bulletFakeGo.GetCompItem<Triangle>("tail").enabled = false;

            var bulletMaskGo = PrefabUtilsEx.InitPrefab(
                ContainersGetter.MazeItemsContainer, "views", "turret_bullet_mask");
            var bm = bulletMaskGo.GetCompItem<SpriteMask>("mask");
            var bulletMaskGo2 = PrefabUtilsEx.InitPrefab(
                ContainersGetter.MazeItemsContainer, "views", "turret_bullet_mask");
            var bm2 = bulletMaskGo2.GetCompItem<SpriteMask>("mask");
            bm.enabled = bm2.enabled = false;
            bm.transform.localScale = bm2.transform.localScale = CoordinateConverter.GetScale() * Vector3.one;
            bm.isCustomRangeActive = bm2.isCustomRangeActive = true;
            bm.frontSortingOrder = bm2.frontSortingOrder = bulletSprRend.sortingOrder;
            
            Object = go;
            m_BulletMask = bm;
            m_BulletMask2 = bm2;
            m_Barrel = barrel;
            
            Coroutines.Run(HandleTurretPrePreShootCoroutine());
        }

        protected override void Appear(bool _Appear)
        {
            Coroutines.Run(Coroutines.WaitWhile(
                () => !Initialized,
                () =>
                {
                    RazorMazeUtils.DoAppearTransitionSimple(
                        _Appear,
                        GameTimeProvider,
                        new Dictionary<IEnumerable<ShapeRenderer>, Color>
                        {
                            {new [] {m_Barrel}, DrawingUtils.ColorBack},
                            {new [] {m_BulletHolderBorder}, DrawingUtils.ColorLines}
                        });

                }));
        }

        private IEnumerator HandleTurretPrePreShootCoroutine()
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
                GameTimeProvider);
        }
        
        private IEnumerator HandleTurretPreShootCoroutine()
        {
            yield return Coroutines.Lerp(
                0f, 
                1f, 
                0.2f, 
                _Progress => { },
                GameTimeProvider,
                (_Breaked, _Progress) =>
                {
                    m_Bullet.SetGoActive(true);
                    m_BulletContainer.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
                    m_BulletFake.SetGoActive(false);
                    Coroutines.Run(OpenBarrel(true, false));
                });
        }
        
        private IEnumerator HandleTurretShootCoroutine(TurretShotEventArgs _Args)
        {
            m_BulletMask.enabled = m_BulletMask2.enabled = true;
            m_BulletMask.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(_Args.To));
            m_BulletMask2.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(
                _Args.To.ToVector2() + _Args.Direction.ToVector2() * 0.9f));
            
            var pos = _Args.From.ToVector2();
            V2Int point = default;
            bool movedToTheEnd = false;

            Coroutines.Run(Coroutines.Delay(
                () =>
                {
                    Coroutines.Run(OpenBarrel(false, false));
                    Coroutines.Run(HandleTurretPrePreShootCoroutine());
                },
                0.2f));

            m_Moving = true;
            m_RotatingSpeed = ViewSettings.TurretBulletRotationSpeed;
            yield return Coroutines.DoWhile(
                () =>
                {
                    if (point == ModelCharacter.Position)
                    {
                        ModelCharacter.RaiseDeath();
                        return false;
                    }
                    return !movedToTheEnd;
                },
                () =>
                {
                    pos += _Args.Direction.ToVector2() * _Args.ProjectileSpeed;
                    m_BulletContainer.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(pos));
                    point = V2Int.Round(pos);
                    BulletTail.ShowTail(_Args);
                    if (point == _Args.To + _Args.Direction)
                        movedToTheEnd = true;
                }, () =>
                {
                    m_Moving = false;
                    m_Bullet.SetGoActive(false);
                    BulletTail.HideTail(_Args);
                });
        }
        
        private IEnumerator OpenBarrel(bool _Open, bool _Instantly)
        {
            const float closedWidth = 0.1f;
            const float openedWidth = 0.85f;
            
            float barrelHeight = GetBarrelSize().Item2;
            if (_Instantly)
            {
                m_Barrel.Height = barrelHeight * (_Open ? openedWidth : closedWidth);
                yield break;
            }

            if (_Open)
            {
                m_Moving = true;
                Coroutines.Run(Coroutines.Lerp(
                    0f, 
                    1f,
                    0.1f, 
                    _Progress => m_RotatingSpeed = ViewSettings.TurretBulletRotationSpeed * _Progress,
                    GameTimeProvider));
            }
            
            yield return Coroutines.Lerp(
                _Open ? closedWidth : openedWidth, _Open ? openedWidth : closedWidth, 0.1f,
                _Progress => m_Barrel.Height = barrelHeight * _Progress,
                GameTimeProvider);
        }
        
        private Tuple<Vector2, Vector3> GetBarrelLocalPositionAndRotation()
        {
            var dir = Props.Directions.First().ToVector2();
            var localPos = dir * (0.25f + 0.02f) * CoordinateConverter.GetScale();
            float angle = dir.x == 0 ? 90 : 0;
            return new Tuple<Vector2, Vector3>(localPos, Vector3.forward * angle);
        }

        private Tuple<float, float> GetBarrelSize()
        {
            float width = CoordinateConverter.GetScale() * (0.5f + 0.02f);
            float height = BulletContainerRadius * 2f * CoordinateConverter.GetScale();
            return new Tuple<float, float>(width, height);
        }

        private int GetBulletSortingOrder(int _DefaultSortingOrder)
        {
            _bulletSortingOrder++;
            _bulletSortingOrder = MathUtils.ClampInverse(
                _bulletSortingOrder, _DefaultSortingOrder, _DefaultSortingOrder + 30);
            return _bulletSortingOrder;
        }

        #endregion
    }
}