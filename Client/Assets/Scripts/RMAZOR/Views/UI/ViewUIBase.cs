using Common;
using Common.Utils;
using RMAZOR.Models;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.Views.UI
{
    public interface IViewUI : IInit, IOnLevelStageChanged
    {
        IViewUIGameControls GameControls { get; }
    }
    
    public abstract class ViewUIBase : IViewUI
    {
        #region nonpublic members
        
        protected Canvas m_Canvas;
        
        #endregion

        #region constructor

        public IViewUIGameControls GameControls { get; }

        protected ViewUIBase(IViewUIGameControls _GameControls)
        {
            GameControls = _GameControls;
        }

        #endregion
        
        #region api

        public bool              Initialized { get; private set; }
        public event UnityAction Initialize;
        
        public virtual void Init()
        {
            CreateCanvas();
            Initialize?.Invoke();
            Initialized = true;
        }
        
        public abstract void OnLevelStageChanged(LevelStageArgs _Args);

        #endregion
        
        #region nonpublic methods
        
        protected void CreateCanvas()
        {
            m_Canvas = UIUtils.UiCanvas(
                GetType().Name + " Canvas",
                RenderMode.ScreenSpaceOverlay,
                false,
                0,
                AdditionalCanvasShaderChannels.None,
                CanvasScaler.ScaleMode.ScaleWithScreenSize,
                new Vector2Int(1920, 1080),
                CanvasScaler.ScreenMatchMode.Shrink,
                0f,
                100,
                true,
                GraphicRaycaster.BlockingObjects.None);
        }

        protected void RaiseInitializedEvent()
        {
            Initialize?.Invoke();
        }

        #endregion

    }
}
