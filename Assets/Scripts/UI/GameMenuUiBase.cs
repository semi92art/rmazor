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
        IStatsPanel StatsPanel { get; }
        void OnBeforeLevelStarted(LevelStateChangedArgs _Args, UnityAction<long> _GetLifes, UnityAction _StartLevel);
        void OnLevelStarted(LevelStateChangedArgs _Args);
        void OnLevelFinished(LevelStateChangedArgs _Args);
    }
    
    public abstract class GameMenuUiBase : IGameMenuUi
    {
        public IStatsPanel StatsPanel { get; protected set; }
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

        protected virtual void CreateStatsPanel()
        {
            StatsPanel = Panels.StatsPanel.Create(Canvas.RTransform(), StatsPanelPosition.Top);
        }
        
        protected virtual void CreateDialogViewer()
        {
            GameDialogViewer = DialogViewers.GameDialogViewer.Create(Canvas.RTransform());
        }

        public virtual void OnBeforeLevelStarted(LevelStateChangedArgs _Args, UnityAction<long> _GetLifes, UnityAction _StartLevel)
        {
            IGameDialogPanel startLevelPanel = new LevelStartPanel(GameDialogViewer, _Args.Level, _GetLifes, _StartLevel);
            startLevelPanel.Show();
        }

        public virtual void OnLevelStarted(LevelStateChangedArgs _Args)
        {
            // TODO
        }

        public virtual void OnLevelFinished(LevelStateChangedArgs _Args)
        {
            // TODO
        }
    }
}