using DialogViewers;
using Entities;
using Extensions;
using Ticker;
using TimeProviders;
using UI.Factories;
using UI.Panels;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Games.RazorMaze.Views.UI
{
    public interface IViewUI
    {
        void OnBeforeLevelStarted(LevelStateChangedArgs _Args, UnityAction _StartLevel);
        void OnLevelStarted(LevelStateChangedArgs _Args);
        void OnLevelFinished(LevelStateChangedArgs _Args, UnityAction _Finish);
    }
    
    public abstract class ViewUIBase : GameObservable, IViewUI, IUpdateTick
    {
        #region nonpublic members
        
        protected IGameDialogViewer DialogViewer;
        protected Canvas Canvas;
        
        #endregion

        #region constructor

        protected ViewUIBase(ITicker _Ticker) : base(_Ticker)
        {
            CreateCanvas();
            CreateDialogViewer();
        }

        #endregion
        
        #region nonpublic methods
        
        protected void CreateCanvas()
        {
            Canvas = UiFactory.UiCanvas(
                "MenuCanvas",
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
        
        protected void CreateDialogViewer()
        {
            DialogViewer = GameDialogViewer.Create(Canvas.RTransform());
        }

        protected void OnGameMenuButtonClick()
        {
            if (GameMenuPanel.PanelState.HasFlag(PanelState.Showing))
            {
                GameMenuPanel.PanelState |= PanelState.NeedToClose;
                return;
            }
            GameTimeProvider.Instance.Pause = true;
            var gameMenuPanel = new GameMenuPanel(DialogViewer,
                () => GameTimeProvider.Instance.Pause = false, Ticker);
            gameMenuPanel.AddObservers(GetObservers());
            gameMenuPanel.Init();
            DialogViewer.Show(gameMenuPanel);
        }
        
        public void UpdateTick()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                OnGameMenuButtonClick();
        }
        
        #endregion
        
        #region api methods

        public abstract void OnBeforeLevelStarted(LevelStateChangedArgs _Args, UnityAction _StartLevel);
        public abstract void OnLevelStarted(LevelStateChangedArgs _Args);
        public abstract void OnLevelFinished(LevelStateChangedArgs _Args, UnityAction _Finish);
        

        #endregion
    }
}