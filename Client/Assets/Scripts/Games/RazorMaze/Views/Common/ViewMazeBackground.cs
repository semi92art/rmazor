using System;
using System.Collections;
using System.Collections.Generic;
using DI.Extensions;
using Exceptions;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Utils;
using Shapes;
using SpawnPools;
using Ticker;
using UnityEngine;
using UnityEngine.Events;
using Utils;
using Object = UnityEngine.Object;
using Random = System.Random;

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
        private bool m_LoadedFirstTime;
        private Transform m_Container;
        private readonly Random m_Random = new Random(); 
        private Color m_BackItemsColor;
        private readonly BehavioursSpawnPool<ShapeRenderer> m_Pool = new BehavioursSpawnPool<ShapeRenderer>();
        private readonly List<Vector2> m_Speeds = new List<Vector2>(PoolSize);
        private Bounds m_ScreenBounds;

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

        private IMazeCoordinateConverter CoordinateConverter { get; }
        private IContainersGetter        ContainersGetter    { get; }
        private IGameTicker              GameTicker          { get; }
        private ICameraProvider          CameraProvider      { get; }
        private IColorProvider           ColorProvider       { get; }

        public ViewMazeBackground(
            IMazeCoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IGameTicker _GameTicker,
            ICameraProvider _CameraProvider,
            IColorProvider _ColorProvider)
        {
            CoordinateConverter = _CoordinateConverter;
            ContainersGetter = _ContainersGetter;
            GameTicker = _GameTicker;
            CameraProvider = _CameraProvider;
            ColorProvider = _ColorProvider;

            GameTicker.Register(this);
        }
        
        #endregion
        
        #region api
        
        public event UnityAction Initialized;

        public Color BackgroundColor
        {
            get => CameraProvider.MainCamera.backgroundColor;
            private set
            {
                CameraProvider.MainCamera.backgroundColor = value;
                BackgroundColorChanged?.Invoke(value);
            }
        }

        public event UnityAction<Color> BackgroundColorChanged;
        
        public void Init()
        {
            ColorProvider.ColorChanged += ColorProviderOnColorChanged;
            m_ScreenBounds = GraphicUtils.GetVisibleBounds(CameraProvider.MainCamera);
            m_BackItemsColor = ColorProvider.GetColor(ColorIds.BackgroundItems);
            InitSources();
            InitShapes();
            Initialized?.Invoke();
            m_Initialized = true;
        }

        private void ColorProviderOnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId == ColorIds.BackgroundItems)
            {
                m_BackItemsColor = _Color;
                foreach (var rend in m_Pool)
                    rend.Color = _Color;
            }
            else if (_ColorId == ColorIds.Background)
                BackgroundColor = _Color;
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage == ELevelStage.Loaded)
            {
                Coroutines.Run(LoadLevelCoroutine(_Args.LevelIndex));
                if (!m_LoadedFirstTime)
                    Coroutines.Run(LoadCoroutine(true));
            }
            m_LoadedFirstTime = true;
        }
        
        public void UpdateTick()
        {
            if (!m_Initialized)
                return;
            ProceedBackground();
        }
        
        #endregion

        #region nonpublic methods

        private List<Disc> InitSources()
        {
            var res = new List<Disc>();
            for (int i = 0; i < 3; i++)
            {
                var sourceGo = new GameObject("Background Item");
                sourceGo.SetParent(ContainersGetter.GetContainer(ContainerNames.Background));
                var source = sourceGo.AddComponent<Disc>();
                source.Color = m_BackItemsColor;
                source.Radius = 0.25f * i;
                source.SortingOrder = SortingOrders.BackgroundItem;
                source.enabled = false;
                res.Add(source);
            }

            return res;
        }

        private void InitShapes()
        {
            var sources = InitSources();
            for (int i = 0; i < PoolSize; i++)
            {
                int randIdx = Mathf.FloorToInt(UnityEngine.Random.value * sources.Count);
                var source = sources[randIdx];
                var newGo = Object.Instantiate(source.gameObject);
                newGo.SetParent(ContainersGetter.GetContainer(ContainerNames.Background));
                var pos = RandomPositionOnScreen();
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
                shape.transform.SetPosXY(RandomPositionOnScreen(false, new Vector2(1f, 1f)));
            }
        }
        
        private Vector2 RandomPositionOnScreen(bool _Inside = true, Vector2? _Padding = null)
        {
            if (!_Inside && !_Padding.HasValue)
                return default;
            
            float xDelta, yDelta;
            if (_Inside)
            {
                xDelta = m_Random.NextFloatAlt() * m_ScreenBounds.size.x * 0.5f;
                yDelta = m_Random.NextFloatAlt() * m_ScreenBounds.size.y * 0.5f;
            }
            else
            {
                int posCase = m_Random.Next(1, 4);

                switch (posCase)
                {
                    case 1: // right
                        xDelta = m_ScreenBounds.size.x * 0.5f + _Padding.Value.x;
                        yDelta = m_ScreenBounds.size.y * 0.5f * m_Random.NextFloatAlt();
                        break;
                    case 2: // left
                        xDelta = -m_ScreenBounds.size.x * 0.5f - _Padding.Value.x;
                        yDelta = m_ScreenBounds.size.y * 0.5f * m_Random.NextFloatAlt();
                        break;
                    case 3: // top
                        xDelta = m_ScreenBounds.size.x * 0.5f * m_Random.NextFloatAlt();
                        yDelta = m_ScreenBounds.size.y * 0.5f + _Padding.Value.y;
                        break;
                    case 4: // bottom
                        xDelta = m_ScreenBounds.size.x * 0.5f * m_Random.NextFloatAlt();
                        yDelta = -m_ScreenBounds.size.y * 0.5f - _Padding.Value.y;
                        break;
                    default:
                        throw new SwitchCaseNotImplementedException(posCase);
                }
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
            new Vector2(m_Random.NextFloatAlt(), m_Random.NextFloatAlt());
        
        private IEnumerator LoadCoroutine(bool _Load)
        {
            var startColor = _Load ? m_BackItemsColor.SetA(0f) : m_BackItemsColor;
            var endColor = !_Load ?  m_BackItemsColor.SetA(0f) : m_BackItemsColor;
            
            yield return Coroutines.Lerp(
                startColor,
                endColor,
                LoadTime,
                _Color =>
                {
                    foreach (var shape in m_Pool)
                        shape.Color = _Color;
                },
                GameTicker,
                (_Finished, _Progress) =>
                {
                    foreach (var shape in m_Pool)
                        shape.Color = endColor;
                });
        }

        private IEnumerator LoadLevelCoroutine(int _Level)
        {
            const float duration = 2f;
            int colorIdx = (_Level / RazorMazeUtils.LevelsInGroup) % 10;
            float hStart = MathUtils.ClampInverse(
                _Level % RazorMazeUtils.LevelsInGroup == 0 ? colorIdx - 1 : colorIdx, 0, 10) / 10f;
            float hEnd = colorIdx / 10f;

            float s = 52f / 100f;
            float v = 42f / 100f;
            var startColor = Color.HSVToRGB(hStart, s, v);
            var endColor = Color.HSVToRGB(hEnd, s, v);
            
            if ((_Level + 1) % 3 == 0)
            {
                BackgroundColor = endColor;
                yield break;
            }
            
            yield return Coroutines.Lerp(
                startColor,
                endColor,
                duration,
                _Color => BackgroundColor = _Color,
                GameTicker);
        }

        #endregion
    }
}