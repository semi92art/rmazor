using System;
using System.Collections.Generic;
using System.Linq;
using Constants;
using DI.Extensions;
using Exceptions;
using GameHelpers;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
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
    public interface IViewMazeBackground : IInit, IOnLevelStageChanged
    { }
    
    public class ViewMazeBackground : IViewMazeBackground, IUpdateTick
    {
        #region constants

        private const int   PoolSize          = 100;
        private const float CongratsAnimSpeed = 1f;
        
        #endregion
        
        #region nonpublic members

        private readonly BehavioursSpawnPool<ShapeRenderer> m_BackIdleItemsPool = 
            new BehavioursSpawnPool<ShapeRenderer>();
        private readonly BehavioursSpawnPool<Animator> m_BackCongratsItemsPool = 
            new BehavioursSpawnPool<Animator>();
        private readonly Dictionary<Animator, ShapeRenderer[]> m_BackCongratsItemsDict = 
            new Dictionary<Animator, ShapeRenderer[]>();

        private readonly List<Vector2> m_BackIdleItemSpeeds = new List<Vector2>(PoolSize);
        private readonly Random        m_Random             = new Random();
        private readonly Type[] m_PossibleSourceTypes =
        {
            typeof(Disc),
            typeof(Line),
            typeof(Rectangle),
            typeof(Polygon),
            typeof(RegularPolygon)
        };
        private Color BackgroundColor
        {
            set => CameraProvider.MainCamera.backgroundColor = value;
        }
        
        private Color     m_BackItemsColor;
        private Bounds    m_ScreenBounds;
        private float     m_LastCongratsItemAnimTime;
        private float     m_NextRandomCongratsItemAnimInterval;
        private bool      m_DoAnimateCongrats;

        
        #endregion
        
        #region inject

        private IMazeCoordinateConverter CoordinateConverter { get; }
        private IContainersGetter        ContainersGetter    { get; }
        private IViewGameTicker          GameTicker          { get; }
        private ICameraProvider          CameraProvider      { get; }
        private IColorProvider           ColorProvider       { get; }
        private IViewAppearTransitioner  Transitioner        { get; }
        private IPrefabSetManager        PrefabSetManager    { get; }

        public ViewMazeBackground(
            IMazeCoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IViewGameTicker _GameTicker,
            ICameraProvider _CameraProvider,
            IColorProvider _ColorProvider,
            IViewAppearTransitioner _Transitioner,
            IPrefabSetManager _PrefabSetManager)
        {
            CoordinateConverter = _CoordinateConverter;
            ContainersGetter = _ContainersGetter;
            GameTicker = _GameTicker;
            CameraProvider = _CameraProvider;
            ColorProvider = _ColorProvider;
            Transitioner = _Transitioner;
            PrefabSetManager = _PrefabSetManager;

            GameTicker.Register(this);
        }
        
        #endregion
        
        #region api

        public bool              Initialized { get; private set; }
        public event UnityAction Initialize;

        public void Init()
        {
            ColorProvider.ColorChanged += OnColorChanged;
            m_ScreenBounds = GraphicUtils.GetVisibleBounds(CameraProvider.MainCamera);
            m_BackItemsColor = ColorProvider.GetColor(ColorIds.BackgroundIdleItems);
            InitBackgroundIdleItems();
            InitBackgroundCongratsItems();
            Initialize?.Invoke();
            Initialized = true;
        }

        private void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId == ColorIds.BackgroundIdleItems)
            {
                m_BackItemsColor = _Color;
                foreach (var rend in m_BackIdleItemsPool)
                    rend.Color = _Color;
            }
            else if (_ColorId == ColorIds.Background)
                BackgroundColor = _Color;
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            m_DoAnimateCongrats = false;
            if (_Args.Stage == ELevelStage.Loaded)
            {
                SetForegroundColors(_Args.LevelIndex);
                AppearBackgroundIdleItems(true);
            }
            else if (_Args.Stage == ELevelStage.Finished)
            {
                m_DoAnimateCongrats = true;
                AppearBackgroundIdleItems(false);
            }
            else if (_Args.Stage == ELevelStage.Unloaded)
            {
                foreach (var item in m_BackCongratsItemsPool)
                    m_BackCongratsItemsPool.Deactivate(item);
            }
        }
        
        public void UpdateTick()
        {
            if (!Initialized)
                return;
            ProceedBackgroundIdleItems();
            if (!m_DoAnimateCongrats)
                return;
            ProceedBackgroundCongratsItems();
        }

        #endregion

        #region nonpublic methods
        
        private void AppearBackgroundIdleItems(bool _Appear)
        {
            Transitioner.DoAppearTransition(_Appear,
                new Dictionary<IEnumerable<Component>, Func<Color>>
                {
                    {m_BackIdleItemsPool, () => m_BackItemsColor}
                },
                _Type: EAppearTransitionType.WithoutDelay);
        }
        
        private void InitBackgroundIdleItems()
        {
            var sources = new List<Disc>();
            for (int i = 0; i < 3; i++)
            {
                var sourceGo = new GameObject("Background Item");
                sourceGo.SetParent(ContainersGetter.GetContainer(ContainerNames.Background));
                var source = sourceGo.AddComponent<Disc>();
                source.Color = m_BackItemsColor;
                source.Radius = 0.25f * i;
                source.SortingOrder = SortingOrders.BackgroundItem;
                source.enabled = false;
                sources.Add(source);
            }
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
                m_BackIdleItemsPool.Add(newSource);
                m_BackIdleItemSpeeds.Add(RandomSpeed());
            }
        }

        private void InitBackgroundCongratsItems()
        {
            var sourceGos = new List<GameObject>();
            for (int i = 0; i < 2; i++)
            {
                var sourceGo = PrefabSetManager.GetPrefab(
                    "views", $"background_item_congrats_{i + 1}");
                sourceGos.Add(sourceGo);
            }
            for (int i = 0; i < PoolSize; i++)
            {
                int randIdx = Mathf.FloorToInt(UnityEngine.Random.value * sourceGos.Count);
                var sourceGo = sourceGos[randIdx];
                var newGo = Object.Instantiate(sourceGo);
                newGo.SetParent(ContainersGetter.GetContainer(ContainerNames.Background));
                var sourceAnim = newGo.GetCompItem<Animator>("animator");
                sourceAnim.speed = CongratsAnimSpeed;
                var triggerer = newGo.GetCompItem<AnimationTriggerer>("triggerer");
                var content = newGo.GetCompItem<Transform>("content");
                var shapes = m_PossibleSourceTypes
                    .SelectMany(_T => content.GetComponentsInChildren(_T))
                    .Cast<ShapeRenderer>()
                    .ToArray();
                
                int colIdx = Mathf.FloorToInt(UnityEngine.Random.value * m_CongradColorSet.Length);
                var col = m_CongradColorSet[colIdx];

                triggerer.Trigger1 = () =>
                {
                    foreach (var shape in shapes)
                        shape.Color = col;
                };
                triggerer.Trigger2 = () =>
                {
                    var startColA = col.a;
                    Coroutines.Run(Coroutines.Lerp(
                        1f,
                        0f,
                        1f / (6f * CongratsAnimSpeed),
                        _Progress =>
                        {
                            foreach (var shape in shapes)
                                shape.Color = shape.Color.SetA(_Progress * startColA);
                        },
                        GameTicker));
                };
                triggerer.Trigger3 = () => m_BackCongratsItemsPool.Deactivate(sourceAnim);
                m_BackCongratsItemsPool.Add(sourceAnim);
                m_BackCongratsItemsDict.Add(sourceAnim, shapes);
            }

            foreach (var item in m_BackCongratsItemsPool)
                m_BackCongratsItemsPool.Deactivate(item);
        }
        
        private void ProceedBackgroundIdleItems()
        {
            for (int i = 0; i < m_BackIdleItemsPool.Count; i++)
            {
                var shape = m_BackIdleItemsPool[i];
                var speed = m_BackIdleItemSpeeds[i] * GameTicker.DeltaTime;
                shape.transform.PlusPosXY(speed.x, speed.y);
                if (IsInsideOfScreenBounds(shape.transform.position.XY(), Vector2.one))
                    continue;
                shape.transform.SetPosXY(RandomPositionOnScreen(false, Vector2.one));
            }
        }

        private void ProceedBackgroundCongratsItems()
        {
            if (!(GameTicker.Time > m_NextRandomCongratsItemAnimInterval + m_LastCongratsItemAnimTime)) 
                return;
            m_LastCongratsItemAnimTime = GameTicker.Time;
            m_NextRandomCongratsItemAnimInterval = 0.05f + UnityEngine.Random.value * 0.15f;
            var item = m_BackCongratsItemsPool.FirstInactive;
            if (item.IsNull())
                return;
            var tr = item.transform;
            tr.position = RandomPositionOnScreen();
            tr.localScale = Vector3.one * (0.5f + 2f * UnityEngine.Random.value);
            m_BackCongratsItemsPool.Activate(item);
            item.SetTrigger(AnimKeys.Anim);
        }

        private static readonly Color[] m_CongradColorSet =
        {
            new Color(0.72f, 1f, 0.58f),
            new Color(1f, 0.81f, 0.45f),
            new Color(0.56f, 1f, 0.86f),
            new Color(0.44f, 0.55f, 1f),
            new Color(0.91f, 0.52f, 1f),
            new Color(1f, 0.49f, 0.52f),
        };
        
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
            Vector2 min = m_ScreenBounds.min;
            Vector2 max = m_ScreenBounds.max;
            return _Position.x > min.x - _Padding.x 
                   && _Position.y > min.y - _Padding.y
                   && _Position.x < max.x + _Padding.x
                   && _Position.y < max.y + _Padding.y;
        }

        private Vector2 RandomSpeed()
        {
            return new Vector2(m_Random.NextFloatAlt(), m_Random.NextFloatAlt());
        }

        private void SetForegroundColors(int _LevelIndex)
        {
            int group = RazorMazeUtils.GetGroupIndex(_LevelIndex);
            float h = GetHForHSV(group);
            float s = 100f / 100f;
            float v = 100f / 100f;
            var newMainColor = Color.HSVToRGB(h, s, v);
            ColorProvider.SetColor(ColorIds.Main, newMainColor);
            ColorProvider.SetColor(ColorIds.Background, Color.HSVToRGB(h, s, 5f / 100f));
        }

        private static float GetHForHSV(int _Group)
        {
            var values = new float[]
            {
                30,  // 1
                185, // 5
                55,  // 2
                225, // 6
                80,  // 3
                265, // 7
                140, // 4
                305, // 8
                // 330  // 9
            }.Select(_H => _H / 360f).ToArray();
            int idx = _Group % values.Length;
            return values[idx];
        }

        #endregion
    }
}