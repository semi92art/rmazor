using System;
using System.Collections;
using System.Linq;
using Entities;
using Extensions;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Utils;
using Shapes;
using TimeProviders;
using UnityEngine;
using UnityGameLoopDI;
using Utils;

namespace Games.RazorMaze.Views.MazeItems
{
    public interface IViewMazeItemTurret : IViewMazeItem
    {
        void PreShoot();
        void Shoot(float _Speed);
    }
    
    public class ViewMazeItemTurret : ViewMazeItemBase, IViewMazeItemTurret, IUpdateTick
    {
        #region nonpublic members

        private const float BulletContainerRadius = 0.4f;
        private Disc m_BulletHolder;
        private Rectangle m_Barrel;
        private Transform m_BulletContainer;
        private Transform m_Bullet;
        private bool m_Initialized;

        #endregion
        
        #region inject
        
        private ViewSettings ViewSettings { get; }
        private IModelMazeData Data { get; }
        private IGameTimeProvider GameTimeProvider { get; }

        public ViewMazeItemTurret(
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            ViewSettings _ViewSettings,
            IModelMazeData _Data,
            IGameTimeProvider _GameTimeProvider)
            : base(_CoordinateConverter, _ContainersGetter)
        {
            ViewSettings = _ViewSettings;
            Data = _Data;
            GameTimeProvider = _GameTimeProvider;
        }
        
        #endregion
        
        #region api
        
        public void PreShoot()
        {
            Coroutines.Run(HandleTurretPreShotCoroutine());
        }

        public void Shoot(float _Speed)
        {
            var pos = Props.Position;
            bool isEnd = false;
            while (!isEnd)
            {
                pos += Props.Directions.First();
                var item = Data.Info.MazeItems.FirstOrDefault(_Itm => _Itm.Position == pos);
                if (item == null)
                    continue;
                isEnd = true;
            }
            Coroutines.Run(HandleTurretShotCoroutine(
                m_BulletContainer.transform, Props.Position, pos, Props.Directions.First(), _Speed));
        }
        
        public void UpdateTick()
        {
            if (!m_Initialized)
                return;
            float rotSpeed = ViewSettings.MovingTrapRotationSpeed * Time.deltaTime; 
            m_Bullet.Rotate(Vector3.forward * rotSpeed);
        }

        public object Clone() => new ViewMazeItemTurret(
            CoordinateConverter, ContainersGetter, ViewSettings, Data, GameTimeProvider);
        
        #endregion
        
        #region nonpublic members

        protected override void SetShape()
        {
            var go = Object;
            var sh = ContainersGetter.MazeItemsContainer.gameObject
                .GetOrAddComponentOnNewChild<Rectangle>("Path Item", ref go, 
                    CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            go.DestroyChildrenSafe();
            sh.Width = sh.Height = CoordinateConverter.GetScale() * 0.99f;
            sh.Type = Rectangle.RectangleType.RoundedSolid;
            sh.CornerRadius = ViewSettings.CornerRadius * CoordinateConverter.GetScale();
            sh.Color = ViewUtils.ColorLines;
            sh.SortingOrder = ViewUtils.GetBlockSortingOrder(Props.Type);

            var bullCont = go.AddComponentOnNewChild<Disc>("Bullet Container", out _, Vector2.zero);
            bullCont.Radius = CoordinateConverter.GetScale() * BulletContainerRadius;
            bullCont.Color = ViewUtils.ColorMain;
            bullCont.Type = DiscType.Disc;
            bullCont.SortingOrder = ViewUtils.GetBlockSortingOrder(Props.Type) + 1;

            var barrel = go.AddComponentOnNewChild<Rectangle>("Barrel", out _, Vector2.zero);
            barrel.Type = Rectangle.RectangleType.HardSolid;
            barrel.Color = ViewUtils.ColorMain;
            barrel.SortingOrder = ViewUtils.GetBlockSortingOrder(Props.Type) + 1;

            var tr = barrel.transform;
            (tr.localPosition, tr.localEulerAngles) = GetBarrelLocalPositionAndRotation();
            (barrel.Width, barrel.Height) = GetBarrelSize();

            var bulletGo = PrefabUtilsEx.InitPrefab(
                ContainersGetter.MazeItemsContainer, "views", "turret_bullet");
            m_BulletContainer = bulletGo.transform;
            m_Bullet = bulletGo.GetContentItem("bullet").transform;
            m_Bullet.GetComponent<SpriteRenderer>().sortingOrder = ViewUtils.GetBlockSortingOrder(Props.Type) + 2;
            
            Object = go;
            m_BulletHolder = bullCont;
            m_Barrel = barrel;

            Coroutines.Run(HandleBulletPrepareToShootCoroutine());

            m_Initialized = true;
        }

        private Tuple<Vector2, Vector3> GetBarrelLocalPositionAndRotation()
        {
            var dir = Props.Directions.First().ToVector2();
            var localPos = dir * 0.25f * CoordinateConverter.GetScale();
            float angle = dir.x == 0 ? 90 : 0;
            return new Tuple<Vector2, Vector3>(localPos, Vector3.forward * angle);
        }

        private Tuple<float, float> GetBarrelSize()
        {
            float width = CoordinateConverter.GetScale() * 0.5f;
            float height = BulletContainerRadius * 2f * CoordinateConverter.GetScale();
            return new Tuple<float, float>(width, height);
        }

        private IEnumerator HandleTurretPreShotCoroutine()
        {
            //m_BulletContainer.gameObject.SetActive(true);
            // m_BulletContainer.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            yield return Coroutines.Lerp(
                0f,
                1f,
                0.2f,
                _Progress =>
                {

                },
                GameTimeProvider,
                (_Breaked, _Progress) => Coroutines.Run(OpenBarrel(true)));
        }

        private IEnumerator HandleTurretShotCoroutine(
            Transform _Projectile,
            V2Int _From,
            V2Int _To,
            V2Int _Direction,
            float _Speed)
        {
            var pos = _From.ToVector2();
            V2Int point = default;
            bool finish = false;

            Coroutines.Run(Coroutines.Delay(
                () => Coroutines.Run(OpenBarrel(false)),
                0.2f));
            
            yield return Coroutines.DoWhile(
                () => !finish && point != Data.CharacterInfo.Position,
                () =>
                {
                    pos += _Direction.ToVector2() * _Speed;
                    _Projectile.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(pos));
                    point = pos.ToV2IntRound();
                    if (point == _To)
                        finish = true;
                },
                () => Coroutines.Run(HandleBulletPrepareToShootCoroutine()));
        }

        private IEnumerator HandleBulletPrepareToShootCoroutine()
        {
            //m_BulletContainer.gameObject.SetActive(true);
            m_BulletContainer.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            yield return Coroutines.Lerp(
                0f,
                1f,
                0.2f,
                _Progress =>
                {
                    m_BulletContainer.transform.localScale =
                        Vector3.one * _Progress * CoordinateConverter.GetScale() * BulletContainerRadius * 0.9f;
                },
                GameTimeProvider);
        }

        private IEnumerator OpenBarrel(bool _Open)
        {
            if (_Open)
                m_Barrel.enabled = true;

            float barrelHeight = GetBarrelSize().Item2;
            
            yield return Coroutines.Lerp(
                0f,
                1f,
                0.1f,
                _Progress =>
                {
                    m_Barrel.Height = barrelHeight * (_Open ? _Progress : 1 - _Progress);
                },
                GameTimeProvider,
                (_Breaked, _Progress) =>
                {
                    if (!_Open)
                        m_Barrel.enabled = false;
                });
        }
        
        #endregion
    }
}