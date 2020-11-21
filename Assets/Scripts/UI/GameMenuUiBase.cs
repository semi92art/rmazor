using DialogViewers;
using Extensions;
using UI.Factories;
using UI.Panels;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    public interface IGameMenuUi
    {
        void OnGameStarted(LevelStateChangedArgs _Args, UnityAction _StartGame);
        void OnLevelStarted(LevelStateChangedArgs _Args);
        void OnLevelFinished(LevelStateChangedArgs _Args);
        void OnLifesChanged(LifeEventArgs _Args);
    }
    
    public abstract class GameMenuUiBase : IGameMenuUi
    {
        protected ILifesPanel LifesPanel;
        protected IGameDialogViewer GameDialogViewer;
        protected Canvas Canvas;
        
        protected GameMenuUiBase()
        {
            CreateCanvas();
            CreateDialogViewer();
            CreateStatsPanel();
        }
        
        protected virtual void CreateCanvas()
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
        
        protected virtual void CreateDialogViewer()
        {
            GameDialogViewer = DialogViewers.GameDialogViewer.Create(Canvas.RTransform());
        }

        protected virtual void CreateStatsPanel()
        {
            LifesPanel = Panels.LifesPanel.Create(Canvas.RTransform(), GameDialogViewer);
        }
        
        public virtual void OnGameStarted(LevelStateChangedArgs _Args, UnityAction _StartGame)
        {
            IGameDialogPanel gameStartPanel = new GameStartPanel(GameDialogViewer, _StartGame);
            gameStartPanel.Show();
        }

        public virtual void OnLevelStarted(LevelStateChangedArgs _Args)
        {
            
        }

        public virtual void OnLevelFinished(LevelStateChangedArgs _Args)
        {
            
        }

        public virtual void OnLifesChanged(LifeEventArgs _Args)
        {
            LifesPanel.SetLifes(_Args.Lifes);
        }
    }
}