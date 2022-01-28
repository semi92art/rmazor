using Common;
using Common.CameraProviders;
using Common.Exceptions;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.Views.ContainerGetters;
using RMAZOR.Views.Helpers;
using Shapes;
using UnityEngine;
using UnityEngine.Events;

namespace RMAZOR.Views.Common
{
    public abstract class ViewMazeBackgroundItemsBase : IInit, IOnLevelStageChanged, IUpdateTick
    {
        #region constants

        protected const int PoolSize = 100;
        
        #endregion

        #region nonpublic members

        private readonly System.Random m_Random = new System.Random();
        private          Bounds        m_ScreenBounds;
        
        protected readonly System.Type[] PossibleSourceTypes =
        {
            typeof(Disc),
            typeof(Line),
            typeof(Rectangle),
            typeof(Polygon),
            typeof(RegularPolygon)
        };

        #endregion

        #region inject
        
        protected IColorProvider          ColorProvider    { get; }
        protected IViewAppearTransitioner Transitioner     { get; }
        protected IContainersGetter       ContainersGetter { get; }
        protected IViewGameTicker         GameTicker       { get; }
        protected ICameraProvider         CameraProvider   { get; }

        protected ViewMazeBackgroundItemsBase(
            IColorProvider _ColorProvider, 
            IViewAppearTransitioner _Transitioner,
            IContainersGetter _ContainersGetter,
            IViewGameTicker _GameTicker,
            ICameraProvider _CameraProvider)
        {
            ColorProvider = _ColorProvider;
            Transitioner = _Transitioner;
            ContainersGetter = _ContainersGetter;
            GameTicker = _GameTicker;
            CameraProvider = _CameraProvider;
        }


        #endregion

        #region api
        
        public bool              Initialized { get; private set; }
        public event UnityAction Initialize;
       
        public virtual void Init()
        {
            GameTicker.Register(this);
            m_ScreenBounds = GraphicUtils.GetVisibleBounds(CameraProvider.MainCamera);
            ColorProvider.ColorChanged += OnColorChanged;
            InitItems();
            Initialize?.Invoke();
            Initialized = true;
        }
        
        public virtual void UpdateTick()
        {
            if (!Initialized)
                return;
            ProceedItems();
        }

        public abstract void OnLevelStageChanged(LevelStageArgs _Args);

        #endregion

        #region nonpublic methods
        
        protected abstract void InitItems();
        protected abstract void ProceedItems();
        protected virtual void OnColorChanged(int _ColorId, Color _Color) { }
        
        protected bool IsInsideOfScreenBounds(Vector2 _Position, Vector2 _Padding)
        {
            var min = m_ScreenBounds.min;
            var max = m_ScreenBounds.max;
            return _Position.x > min.x - _Padding.x 
                   && _Position.y > min.y - _Padding.y
                   && _Position.x < max.x + _Padding.x
                   && _Position.y < max.y + _Padding.y;
        }
        
        protected Vector2 RandomSpeed()
        {
            return new Vector2(m_Random.NextFloatAlt(), m_Random.NextFloatAlt());
        }
        
        protected Vector2 RandomPositionOnScreen(
            bool _Inside = true,
            Vector2? _Padding = null)
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

        #endregion
    }
}