using System;
using System.Collections;
using System.Collections.Generic;
using DI.Extensions;
using Games.RazorMaze.Views.Characters;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Utils;
using Shapes;
using SpawnPools;
using Ticker;
using UnityEngine;
using Utils;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Games.RazorMaze.Views.Common
{
    public class ViewMazeBackground : IViewMazeBackground, IUpdateTick
    {
        #region constants

        private const float LoadTime = 1f;
        private const int PoolSize = 100;
        
        #endregion
        
        #region nonpublic members

        private bool m_Initialized;
        private Transform m_Container;
        private readonly Color m_Color = DrawingUtils.ColorLines.SetA(0.05f);
        private readonly List<ShapeRenderer> m_Sources = new List<ShapeRenderer>();
        private readonly BehavioursSpawnPool<ShapeRenderer> m_Pool = new BehavioursSpawnPool<ShapeRenderer>();
        private readonly List<Vector2> m_Speeds = new List<Vector2>(PoolSize);
        private Bounds m_ScreenBounds;
        private readonly System.Random m_Random = new System.Random(); 

        private readonly Type[] m_PossibleSourceTypes = 
        {
            typeof(Disc),
            typeof(Line),
            typeof(Rectangle),
            typeof(Polygon),
            typeof(RegularPolygon)
        };
        
        #endregion
        
        #region inject

        private ICoordinateConverter CoordinateConverter { get; }
        private IContainersGetter ContainersGetter { get; }
        private IViewMazeCommon ViewMazeCommon { get; }
        private IViewCharacter Character { get; }
        private IGameTicker GameTicker { get; }

        public ViewMazeBackground(
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IViewMazeCommon _ViewMazeCommon, 
            IViewCharacter _Character,
            IGameTicker _GameTicker)
        {
            CoordinateConverter = _CoordinateConverter;
            ContainersGetter = _ContainersGetter;
            ViewMazeCommon = _ViewMazeCommon;
            Character = _Character;
            GameTicker = _GameTicker;
            
            GameTicker.Register(this);
        }
        
        #endregion
        
        #region api
        
        public event NoArgsHandler Initialized;
        
        public void Init()
        {
            m_ScreenBounds = GameUtils.GetVisibleBounds();
            InitSources();
            InitShapes();
            Initialized?.Invoke();
            m_Initialized = true;
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage == ELevelStage.Loaded)
                OnLevelLoad();
            else if (_Args.Stage == ELevelStage.Unloaded)
                OnLevelUnload();
        }
        
        public void UpdateTick()
        {
            if (!m_Initialized)
                return;
            ProceedBackground();
        }
        
        #endregion

        #region nonpublic methods

        private void InitSources()
        {
            int sortingOrder = DrawingUtils.GetBackgroundItemSortingOrder();

            var source1Go = new GameObject("Background Source 1");
            var source1 = source1Go.AddComponent<Disc>();
            source1.Color = m_Color;
            source1.Radius = CoordinateConverter.GetScale() * 0.3f;
            source1.SortingOrder = sortingOrder;
            source1.enabled = false;
            m_Sources.Add(source1);
            
            var source2Go = new GameObject("Background Source 2");
            var source2 = source2Go.AddComponent<Disc>();
            source2.Color = m_Color;
            source2.Radius = CoordinateConverter.GetScale() * 0.2f;
            source2.SortingOrder = sortingOrder;
            source2.enabled = false;
            m_Sources.Add(source2);
            
            var source3Go = new GameObject("Background Source 3");
            var source3 = source3Go.AddComponent<Disc>();
            source3.Color = m_Color;
            source3.Radius = CoordinateConverter.GetScale() * 0.1f;
            source3.SortingOrder = sortingOrder;
            source3.enabled = false;
            m_Sources.Add(source3);
        }

        private void InitShapes()
        {
            for (int i = 0; i < PoolSize; i++)
            {
                int randIdx = Mathf.FloorToInt(Random.value * m_Sources.Count);
                var source = m_Sources[randIdx];
                var newGo = Object.Instantiate(source.gameObject);
                newGo.SetParent(ContainersGetter.BackgroundContainer);
                var pos = RandomPositionInMarginRect();
                newGo.transform.SetPosXY(pos);
                Component newSourceRaw = null;
                foreach (var possibleType in m_PossibleSourceTypes)
                {
                    newSourceRaw = newGo.GetComponent(possibleType);
                    if (newSourceRaw != null)
                        break;
                }
                if (newSourceRaw == null)
                    return;
                var newSource = (ShapeRenderer)newSourceRaw;
                if (newSource == null)
                    return;
                newSource.enabled = true;
                m_Pool.Add(newSource);
                m_Speeds.Add(RandomSpeed());
            }
        }
        
        private void ProceedBackground()
        {
            int k = 0;
            foreach (var shape in m_Pool)
            {
                var speed = m_Speeds[k++] * Time.deltaTime;
                shape.transform.PlusPosXY(speed.x, speed.y);
                if (IsInsideOfScreenBounds(shape.transform.position.XY(), new Vector2(1f, 1f)))
                    continue;
                shape.transform.SetPosXY(RandomPositionInMarginRect(false, new Vector2(1f, 1f)));
            }
        }
        
        private void OnLevelLoad() => Coroutines.Run(LoadCoroutine(true));

        private void OnLevelUnload() => Coroutines.Run(LoadCoroutine(false));
        
        private Vector2 RandomPositionInMarginRect(bool _Inside = true, Vector2? _Padding = null)
        {
            if (!_Inside && !_Padding.HasValue)
                return default;
            
            float xDelta, yDelta;
            if (_Inside)
            {
                xDelta = m_Random.NextFloatAlt() * m_ScreenBounds.size.x;
                yDelta = m_Random.NextFloatAlt() * m_ScreenBounds.size.y;
            }
            else
            {
                bool right = m_Random.NextFloatAlt() > 0;
                bool top = m_Random.NextFloatAlt() > 0;

                xDelta = m_Random.NextFloatAlt() * _Padding.Value.x +
                         (right ? 1f : -1f) * 0.5f * (m_ScreenBounds.size.x + _Padding.Value.x);
                yDelta = m_Random.NextFloatAlt() * _Padding.Value.y +
                         (top ? 1f : -1f) * 0.5f * (m_ScreenBounds.size.y + _Padding.Value.y);
            }
            
            float x = m_ScreenBounds.center.x + xDelta;
            float y = m_ScreenBounds.center.y + yDelta;
            return new Vector2(x, y);
        }

        private bool IsInsideOfScreenBounds(Vector2 _Position, Vector2 _Padding)
        {
            var min = (Vector2) m_ScreenBounds.min;
            var max = (Vector2) m_ScreenBounds.max;
            return _Position.x > min.x - _Padding.x 
                   && _Position.y > min.y - _Padding.y
                   && _Position.x < max.x + _Padding.x
                   && _Position.y < max.y + _Padding.y;
        }

        private Vector2 RandomSpeed() =>
            new Vector2(m_Random.NextFloatAlt() * 2f, m_Random.NextFloatAlt() * 2f);
        
        private IEnumerator LoadCoroutine(bool _Load)
        {
            yield return null;
        
            // var startColor = _Load ? m_Color.SetA(0f) : m_Color;
            // var endColor = !_Load ?  m_Color.SetA(0f) : m_Color;
            //
            // yield return Coroutines.Lerp(
            //     startColor,
            //     endColor,
            //     LoadTime,
            //     _Color =>
            //     {
            //         foreach (var shape in m_Pool)
            //             shape.Color = _Color;
            //     },
            //     GameTicker,
            //     (_Finished, _Progress) =>
            //     {
            //         foreach (var shape in m_Pool)
            //             shape.Color = endColor;
            //     });
        }

        // TODO взято из RandomPositionGenerator, 05ab3159
        // private Vector2 Next(float _Indent)
        // {
        //     bool generated = false;
        //     Vector2 result = default;
        //     for (int i = 0; i < 10000; i++)
        //     {
        //         bool intersects = false;
        //         Vector2 pos = RandomPositionInMarginRect(_Indent);
        //         foreach (var pool in m_Pools)
        //         {
        //             foreach (var point in pool)
        //             {
        //                 if (!point.Activated)
        //                     continue;
        //                 var dscPos = point.transform.position;
        //                 if (!GeometryUtils.CirclesIntersect(
        //                     pos, _Indent, dscPos, point.Radius))
        //                     continue;
        //                 intersects = true;
        //                 break;
        //             }
        //         }
        //         if (intersects) 
        //             continue;
        //
        //         generated = true;
        //         result = pos;
        //         break;
        //     }
        //
        //     if (!generated)
        //         Debug.LogWarning("Disc was not generated because of not enough space");
        //     return result;
        // }
        

        
        #endregion
    }
}