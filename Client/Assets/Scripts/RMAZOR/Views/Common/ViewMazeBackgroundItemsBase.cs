using System;
using Common.Constants;
using Common.Helpers;
using Common.Utils;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Exceptions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Views.Helpers;
using Shapes;
using UnityEngine;
using Random = System.Random;

namespace RMAZOR.Views.Common
{
    public abstract class ViewMazeBackgroundItemsBase : InitBase, IUpdateTick
    {
        #region nonpublic members

        protected virtual int PoolSize => 100;
        protected          Transform Container => ContainersGetter.GetContainer(ContainerNamesCommon.Background);
        protected readonly Random    RandomGen = new Random();
        private            Bounds    m_ScreenBounds;
        
        protected readonly Type[] PossibleSourceTypes =
        {
            typeof(Disc),
            typeof(Line),
            typeof(Rectangle),
            typeof(Polygon),
            typeof(RegularPolygon)
        };

        #endregion

        #region inject

        protected IColorProvider              ColorProvider    { get; }
        protected IRendererAppearTransitioner Transitioner     { get; }
        protected IContainersGetter           ContainersGetter { get; }
        protected IViewGameTicker             GameTicker       { get; }
        protected ICameraProvider             CameraProvider   { get; }

        protected ViewMazeBackgroundItemsBase(
            IColorProvider              _ColorProvider,
            IRendererAppearTransitioner _Transitioner,
            IContainersGetter           _ContainersGetter,
            IViewGameTicker             _GameTicker,
            ICameraProvider             _CameraProvider)
        {
            ColorProvider    = _ColorProvider;
            Transitioner     = _Transitioner;
            ContainersGetter = _ContainersGetter;
            GameTicker       = _GameTicker;
            CameraProvider   = _CameraProvider;
        }
        
        #endregion

        #region api

        public override void Init()
        {
            GameTicker.Register(this);
            m_ScreenBounds = GraphicUtils.GetVisibleBounds(CameraProvider.Camera);
            ColorProvider.ColorChanged += OnColorChanged;
            InitItems();
            base.Init();
        }
        
        public virtual void UpdateTick()
        {
            if (!Initialized)
                return;
            ProceedItems();
        }
        
        #endregion

        #region nonpublic methods
        
        protected abstract void InitItems();
        protected abstract void ProceedItems();
        protected virtual void OnColorChanged(int _ColorId, Color _Color) { }

        protected Vector2 RandomPositionOnScreen(
            bool _Inside = true,
            Vector2? _Padding = null)
        {
            if (!_Inside && !_Padding.HasValue)
                return default;
            
            float xDelta, yDelta;
            if (_Inside)
            {
                xDelta = RandomGen.NextFloatAlt() * m_ScreenBounds.size.x * 0.5f;
                yDelta = RandomGen.NextFloatAlt() * m_ScreenBounds.size.y * 0.5f;
            }
            else
            {
                int posCase = RandomGen.Next(1, 4);

                switch (posCase)
                {
                    case 1: // right
                        xDelta = m_ScreenBounds.size.x * 0.5f + _Padding.Value.x;
                        yDelta = m_ScreenBounds.size.y * 0.5f * RandomGen.NextFloatAlt();
                        break;
                    case 2: // left
                        xDelta = -m_ScreenBounds.size.x * 0.5f - _Padding.Value.x;
                        yDelta = m_ScreenBounds.size.y * 0.5f * RandomGen.NextFloatAlt();
                        break;
                    case 3: // top
                        xDelta = m_ScreenBounds.size.x * 0.5f * RandomGen.NextFloatAlt();
                        yDelta = m_ScreenBounds.size.y * 0.5f + _Padding.Value.y;
                        break;
                    case 4: // bottom
                        xDelta = m_ScreenBounds.size.x * 0.5f * RandomGen.NextFloatAlt();
                        yDelta = -m_ScreenBounds.size.y * 0.5f - _Padding.Value.y;
                        break;
                    default:
                        throw new SwitchCaseNotImplementedException(posCase);
                }
            }
            
            float x = m_ScreenBounds.center.x + xDelta;
            float y = m_ScreenBounds.center.y + yDelta;
            return (Vector2)CameraProvider.Camera.transform.position + new Vector2(x, y);
        }

        #endregion
    }
}