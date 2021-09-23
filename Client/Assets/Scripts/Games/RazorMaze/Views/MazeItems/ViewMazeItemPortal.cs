using System.Collections.Generic;
using System.Linq;
using DI.Extensions;
using Entities;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Utils;
using Shapes;
using SpawnPools;
using Ticker;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.MazeItems
{
    public interface IViewMazeItemPortal : IViewMazeItem
    {
        void DoTeleport(PortalEventArgs _Args);
    }
    
    public class ViewMazeItemPortal : ViewMazeItemBase, IViewMazeItemPortal, IUpdateTick
    {
        #region constants
        
        private const float RotationSpeed = 5f;
        private const int OrbitsCount = 14;
        private const int GravityItemsCount = 30;
        private const float GravitySpawnTime = 1f;
        private const float GravityItemsSpeed = 1f;
        
        #endregion
        
        #region nonpublic members

        private bool m_Appeared;
        private float m_GravitySpawnTimer;
        
        private Disc m_Center;
        private readonly List<Disc> m_Orbits = new List<Disc>();
        private readonly BehavioursSpawnPool<Disc> m_GravityItems = new BehavioursSpawnPool<Disc>();
        
        #endregion
        
        #region shapes

        protected override object[] Shapes => new object[] {m_Center}
            .Concat(m_Orbits)
            .Concat(m_GravityItems)
            .ToArray();
        
        #endregion
        
        #region inject

        public ViewMazeItemPortal(
            ViewSettings _ViewSettings,
            IModelGame _Model,
            ICoordinateConverter _CoordinateConverter, 
            IContainersGetter _ContainersGetter,
            IGameTicker _GameTicker)
            : base(
                _ViewSettings,
                _Model,
                _CoordinateConverter,
                _ContainersGetter,
                _GameTicker) { }
        
        #endregion

        #region api
        
        public override object Clone() => new ViewMazeItemPortal(
            ViewSettings,
            Model, 
            CoordinateConverter,
            ContainersGetter,
            GameTicker);
        
        public override bool Activated
        {
            get => m_Activated;
            set
            {
                m_Activated = value;
                if (value) 
                    return;
                m_Center.enabled = false;
                m_Orbits.ForEach(_Orbit => _Orbit.enabled = false);
                foreach (var item in m_GravityItems)
                    m_GravityItems.Deactivate(item);
            }
        }
        
        public void UpdateTick()
        {
            if (!Initialized || !Activated)
                return;
            for (int i = 0; i < m_Orbits.Count; i++)
            {
                bool clockwise = i.InRange(new V2Int(0, 3), new V2Int(7, 10));
                float c = clockwise ? -1f : 1f;
                m_Orbits[i].transform.Rotate(Vector3.forward * RotationSpeed * c * Time.deltaTime * 50f);
            }
            if (m_Appeared)
                UpdateGravityItems();
        }

        public void DoTeleport(PortalEventArgs _Args)
        {
            Coroutines.Run(Coroutines.Lerp(
                1f, 
                3f,
                0.07f,
                _Progress => m_Center.Radius = CoordinateConverter.GetScale() * 0.2f * _Progress,
                GameTicker,
                (_Breaked, _Prgrss) =>
                {
                    Coroutines.Run(Coroutines.Lerp(
                        3f,
                        1f,
                        0.07f,
                        _Progress => m_Center.Radius = CoordinateConverter.GetScale() * 0.2f * _Progress,
                        GameTicker));
                }));
        }

        #endregion

        #region nonpublic methods

        protected override void SetShape()
        {
            var go = Object;
            var center = ContainersGetter.MazeItemsContainer.gameObject
                .GetOrAddComponentOnNewChild<Disc>(
                    "Portal Item",
                    ref go,
                    CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            center.Type = DiscType.Disc;
            center.Color = DrawingUtils.ColorLines;
            center.Radius = CoordinateConverter.GetScale() * 0.2f;
            go.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            go.DestroyChildrenSafe();
            m_Center = center;
            Object = go;

            var scale = CoordinateConverter.GetScale();
            for (int i = 0; i < OrbitsCount; i++)
            {
                var orbit = go.AddComponentOnNewChild<Disc>($"Orbit {i + 1}", out _, Vector2.zero);
                orbit.Thickness = ViewSettings.LineWidth * scale * 0.5f;
                orbit.Type = DiscType.Arc;
                orbit.Color = DrawingUtils.ColorLines;
                m_Orbits.Add(orbit);
            }

            m_Orbits[0].Radius = m_Orbits[1].Radius = m_Orbits[2].Radius = m_Orbits[3].Radius = scale * 0.45f;
            m_Orbits[4].Radius = m_Orbits[5].Radius = m_Orbits[6].Radius = scale * 0.4f;
            m_Orbits[7].Radius = m_Orbits[8].Radius = m_Orbits[9].Radius = m_Orbits[10].Radius = scale * 0.35f;
            m_Orbits[11].Radius = m_Orbits[12].Radius = m_Orbits[13].Radius = scale * 0.3f;

            const float deg2rad = Mathf.Deg2Rad;
            m_Orbits[0].AngRadiansStart = 0f;
            m_Orbits[0].AngRadiansEnd = 60f * deg2rad;
            m_Orbits[1].AngRadiansStart = 90f * deg2rad;
            m_Orbits[1].AngRadiansEnd = 100f * deg2rad;
            m_Orbits[2].AngRadiansStart = 130f * deg2rad;
            m_Orbits[2].AngRadiansEnd = 180f * deg2rad;
            m_Orbits[3].AngRadiansStart = 270f * deg2rad;
            m_Orbits[3].AngRadiansEnd = 350f * deg2rad;
            
            m_Orbits[4].AngRadiansStart = 10f * deg2rad;
            m_Orbits[4].AngRadiansEnd = 70f * deg2rad;
            m_Orbits[5].AngRadiansStart = 140f * deg2rad;
            m_Orbits[5].AngRadiansEnd = 210f * deg2rad;
            m_Orbits[6].AngRadiansStart = 230f * deg2rad;
            m_Orbits[6].AngRadiansEnd = 280f * deg2rad;

            m_Orbits[7].AngRadiansStart = 5f * deg2rad;
            m_Orbits[7].AngRadiansEnd = 45f * deg2rad;
            m_Orbits[8].AngRadiansStart = 115f * deg2rad;
            m_Orbits[8].AngRadiansEnd = 135f * deg2rad;
            m_Orbits[9].AngRadiansStart = 165f * deg2rad;
            m_Orbits[9].AngRadiansEnd = 190f * deg2rad;
            m_Orbits[10].AngRadiansStart = 220f * deg2rad;
            m_Orbits[10].AngRadiansEnd = 275f * deg2rad;
            
            m_Orbits[11].AngRadiansStart = 75f * deg2rad;
            m_Orbits[11].AngRadiansEnd = 115f * deg2rad;
            m_Orbits[12].AngRadiansStart = 145f * deg2rad;
            m_Orbits[12].AngRadiansEnd = 205f * deg2rad;
            m_Orbits[13].AngRadiansStart = 270f * deg2rad;
            m_Orbits[13].AngRadiansEnd = 325f * deg2rad;
            
            InitGravitySpawnPool();
        }

        protected override void Appear(bool _Appear)
        {
            Coroutines.Run(Coroutines.WaitWhile(
                () => !Initialized,
                () =>
                {
                    RazorMazeUtils.DoAppearTransitionSimple(
                        _Appear,
                        GameTicker,
                        new Dictionary<object[], Color>
                        {
                            {new [] {m_Center}, DrawingUtils.ColorLines},
                            {m_Orbits.Cast<object>().ToArray(), DrawingUtils.ColorLines}
                        },
                        _OnFinish: () =>
                        {
                            m_Appeared = _Appear;
                            if (!_Appear)
                                DeactivateShapes();
                        });

                }));
        }

        private void InitGravitySpawnPool()
        {
            for (int i = 0; i < GravityItemsCount; i++)
            {
                var gItem = Object.AddComponentOnNewChild<Disc>("Gravity Item", out _, Vector2.zero);
                gItem.Radius = 0.025f * CoordinateConverter.GetScale();
                gItem.Color = DrawingUtils.ColorLines;
                gItem.Type = DiscType.Disc;
                m_GravityItems.Add(gItem);
                m_GravityItems.Deactivate(gItem);
            }
        }

        private void UpdateGravityItems()
        {
            m_GravitySpawnTimer += Time.deltaTime;
            if (m_GravitySpawnTimer < GravitySpawnTime)
                return;

            var item = m_GravityItems.FirstInactive;
            m_GravityItems.Activate(item);
            float angle = Random.value * 2f * Mathf.PI;
            var v = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            float dist = 1f + Random.value * CoordinateConverter.GetScale() * 1f;
            item.transform.SetLocalPosXY(v * dist);

            Coroutines.Run(Coroutines.Lerp(
                0f,
                1f,
                0.5f,
                _Progress => item.Color = item.Color.SetA(_Progress * 0.7f),
                GameTicker));
            
            Coroutines.Run(Coroutines.Lerp(
                1f,
                0f,
                dist * GravityItemsSpeed,
                _Progress => item.transform.SetLocalPosXY(v * dist * _Progress),
                GameTicker,
                (_Breaked, _Progress) =>
                {
                    item.Color = item.Color.SetA(0f);
                    m_GravityItems.Deactivate(item);
                }));
            m_GravitySpawnTimer = 0f;
        }

        #endregion
    }
}