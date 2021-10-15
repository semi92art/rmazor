using UI.Factories;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Games.RazorMaze.Views.UI
{
    public interface IViewUI : IInit, IOnLevelStageChanged { }
    
    public abstract class ViewUIBase : IViewUI
    {
        #region nonpublic members
        
        protected Canvas m_Canvas;
        
        #endregion
        
        #region api

        public event UnityAction Initialized;
        
        public virtual void Init()
        {
            CreateCanvas();
            Initialized?.Invoke();
        }
        
        public abstract void OnLevelStageChanged(LevelStageArgs _Args);

        #endregion
        
        #region nonpublic methods
        
        protected void CreateCanvas()
        {
            m_Canvas = UiFactory.UiCanvas(
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
            Initialized?.Invoke();
        }

        #endregion
    }
}