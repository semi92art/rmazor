using System;
using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using Common.Enums;
using Common.Extensions;
using Common.Helpers;
using Common.Providers;
using Common.SpawnPools;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Views.Common;
using RMAZOR.Views.Helpers;
using RMAZOR.Views.InputConfigurators;
using Shapes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RMAZOR.Views.MazeItems
{
    public interface IViewMazeItemPortal : IViewMazeItem
    {
        void DoTeleport(PortalEventArgs _Args);
    }
    
    public class ViewMazeItemPortal : ViewMazeItemBase, IViewMazeItemPortal, IUpdateTick
    {
        #region constants
        
        private const float  RotationSpeed       = 5f;
        private const int    OrbitsCount         = 14;
        private const int    GravityItemsCount   = 30;
        private const float  GravitySpawnTime    = 1f;
        private const float  GravityItemsSpeed   = 1f;
        
        #endregion
        
        #region nonpublic members

        private static AudioClipArgs AudioClipArgsPortal => new AudioClipArgs("portal", EAudioClipType.GameSound);

        private float m_GravitySpawnTimer;
        
        #endregion
        
        #region shapes

        protected override string                    ObjectName => "Portal Block";
        private            Disc                      m_Center;
        private readonly   List<Disc>                m_Orbits       = new List<Disc>();
        private readonly   BehavioursSpawnPool<Disc> m_GravityItems = new BehavioursSpawnPool<Disc>();
        
        #endregion
        
        #region inject

        public ViewMazeItemPortal(
            ViewSettings                  _ViewSettings,
            IModelGame                    _Model,
            IMazeCoordinateConverter      _CoordinateConverter,
            IContainersGetter             _ContainersGetter,
            IViewGameTicker               _GameTicker,
            IViewBetweenLevelTransitioner _Transitioner,
            IManagersGetter               _Managers,
            IColorProvider                _ColorProvider,
            IViewInputCommandsProceeder   _CommandsProceeder)
            : base(
                _ViewSettings,
                _Model,
                _CoordinateConverter,
                _ContainersGetter,
                _GameTicker,
                _Transitioner,
                _Managers,
                _ColorProvider,
                _CommandsProceeder) { }

        #endregion

        #region api
        
        public override object Clone() => new ViewMazeItemPortal(
            ViewSettings,
            Model, 
            CoordinateConverter,
            ContainersGetter,
            GameTicker,
            Transitioner,
            Managers,
            ColorProvider,
            CommandsProceeder);
        
        public override Component[] Shapes => new Component[] {m_Center}.Concat(m_Orbits).ToArray();

        public override bool ActivatedInSpawnPool
        {
            get => base.ActivatedInSpawnPool;
            set
            {
                m_GravityItems.DeactivateAll();
                base.ActivatedInSpawnPool = value;
            }
        }

        public void UpdateTick()
        {
            if (!Initialized || !ActivatedInSpawnPool)
                return;
            for (int i = 0; i < m_Orbits.Count; i++)
            {
                bool clockwise = i.InRange(new V2Int(0, 3), new V2Int(7, 10));
                float c = clockwise ? -1f : 1f;
                m_Orbits[i].transform.Rotate(Vector3.forward * RotationSpeed * c * GameTicker.DeltaTime * 50f);
            }
            if (AppearingState == EAppearingState.Appeared)
                UpdateGravityItems();
        }

        public void DoTeleport(PortalEventArgs _Args)
        {
            if (_Args.IsPortFrom)
                Managers.AudioManager.PlayClip(AudioClipArgsPortal);
            Cor.Run(Cor.Lerp(
                1f, 
                3f,
                0.07f,
                _Progress => m_Center.Radius = CoordinateConverter.Scale * 0.2f * _Progress,
                GameTicker,
                (_Broken, _Progress) =>
                {
                    Cor.Run(Cor.Lerp(
                        3f,
                        1f,
                        0.07f,
                        _P => m_Center.Radius = CoordinateConverter.Scale * 0.2f * _P,
                        GameTicker));
                }));
        }

        #endregion

        #region nonpublic methods

        protected override void InitShape()
        {
            m_Center = Object.AddComponentOnNewChild<Disc>("Portal Item", out _)
                .SetType(DiscType.Disc)
                .SetColor(GetMainColor());
            for (int i = 0; i < OrbitsCount; i++)
            {
                var orbit = Object.AddComponentOnNewChild<Disc>($"Orbit {i + 1}", out _, Vector2.zero)
                    .SetType(DiscType.Arc)
                    .SetColor(GetMainColor())
                    .SetThickness(ViewSettings.LineWidth * CoordinateConverter.Scale * 0.5f);
                m_Orbits.Add(orbit);
            }
            void SetRadius(float _Radius, params int[] _OrbitIndices)
            {
                foreach (int idx in _OrbitIndices)
                    m_Orbits[idx].Radius = _Radius * CoordinateConverter.Scale;
            }
            SetRadius(0.45f, 0, 1, 2, 3);
            SetRadius(0.4f, 4, 5, 6);
            SetRadius(0.35f, 7, 8, 9, 10);
            SetRadius(0.3f, 11, 12, 13);
            void SetOrbitAngles(int _OrbitIndex, float _StartAngle, float _EndAngle)
            {
                const float deg2Rad = Mathf.Deg2Rad;
                m_Orbits[_OrbitIndex].AngRadiansStart = _StartAngle * deg2Rad;
                m_Orbits[_OrbitIndex].AngRadiansEnd = _EndAngle * deg2Rad;
            }
            SetOrbitAngles(0, 0f, 60f);
            SetOrbitAngles(1, 90f, 100f);
            SetOrbitAngles(2, 130f, 180f);
            SetOrbitAngles(3, 270f, 350f);
            SetOrbitAngles(4, 10f, 700f);
            SetOrbitAngles(5, 140f, 210f);
            SetOrbitAngles(6, 230f, 280f);
            SetOrbitAngles(7, 5f, 45f);
            SetOrbitAngles(8, 115f, 135f);
            SetOrbitAngles(9, 165f, 190f);
            SetOrbitAngles(10, 220f, 275f);
            SetOrbitAngles(11, 75f, 115f);
            SetOrbitAngles(12, 145f, 205f);
            SetOrbitAngles(13, 270f, 325f);
            InitGravitySpawnPool();
        }

        protected override void UpdateShape()
        {
            Object.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            m_Center.Radius = CoordinateConverter.Scale * 0.2f;
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId != ColorIds.Main)
                return;
            m_Center.Color = _Color;
            foreach (var item in m_Orbits)
                item.Color = _Color;
            foreach (var item in m_GravityItems)
                item.Color = _Color.SetA(item.Color.a);
        }

        private void InitGravitySpawnPool()
        {
            for (int i = 0; i < GravityItemsCount; i++)
            {
                var gItem = Object.AddComponentOnNewChild<Disc>("Gravity Item", out _, Vector2.zero)
                    .SetType(DiscType.Disc)
                    .SetColor(GetMainColor())
                    .SetRadius(0.025f * CoordinateConverter.Scale);
                m_GravityItems.Add(gItem);
                m_GravityItems.Deactivate(gItem);
            }
        }

        private void UpdateGravityItems()
        {
            m_GravitySpawnTimer += GameTicker.DeltaTime;
            if (m_GravitySpawnTimer < GravitySpawnTime)
                return;
            var item = m_GravityItems.FirstInactive;
            m_GravityItems.Activate(item);
            float angle = Random.value * 2f * Mathf.PI;
            var v = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            float dist = 1f + Random.value * CoordinateConverter.Scale * 1f;
            item.transform.SetLocalPosXY(v * dist);
            Cor.Run(Cor.Lerp(
                0f,
                1f,
                0.5f,
                _Progress => item.Color = item.Color.SetA(_Progress * 0.7f),
                GameTicker));
            Cor.Run(Cor.Lerp(
                1f,
                0f,
                dist * GravityItemsSpeed,
                _Progress => item.transform.SetLocalPosXY(v * dist * _Progress),
                GameTicker,
                (_Broken, _Progress) =>
                {
                    item.Color = item.Color.SetA(0f);
                    m_GravityItems.Deactivate(item);
                }));
            m_GravitySpawnTimer = 0f;
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var col = GetMainColor();
            return new Dictionary<IEnumerable<Component>, Func<Color>>
            {
                {new[] {m_Center}, () => col},
                {m_Orbits,         () => col}
            };
        }

        private Color GetMainColor()
        {
            return ColorProvider.GetColor(ColorIds.Main);
        }

        #endregion
    }
}