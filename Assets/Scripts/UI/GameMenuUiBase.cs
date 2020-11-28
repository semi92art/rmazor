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
        IStatsMiniPanel StatsMiniPanel { get; }
        IRevenueMiniPanel RevenueMiniPanel { get; }
        void OnBeforeLevelStarted(LevelStateChangedArgs _Args, UnityAction<long> _GetLifes, UnityAction _StartLevel);
        void OnLevelStarted(LevelStateChangedArgs _Args);
        void OnLevelFinished(LevelStateChangedArgs _Args);
        void OnTimeEnded(UnityAction<float> _SetAdditionalTime, UnityAction _Continue);
        void OnLifesEnded(UnityAction<long> _SetAdditionalLifes, UnityAction _Continue);
    }
    
    public abstract class GameMenuUiBase : IGameMenuUi
    {
        public IStatsMiniPanel StatsMiniPanel { get; protected set; }
        public IRevenueMiniPanel RevenueMiniPanel { get; protected set; }
        
        protected IGameDialogViewer DialogViewer;
        protected Canvas Canvas;
        
        protected GameMenuUiBase()
        {
            CreateCanvas();
            CreateDialogViewer();
            CreateStatsMiniPanel();
            CreateRevenueMiniPanel();
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

        protected virtual void CreateStatsMiniPanel()
        {
            StatsMiniPanel = Panels.StatsMiniPanel.Create(Canvas.RTransform(), StatsPanelPosition.Top);
        }
        
        protected virtual void CreateDialogViewer()
        {
            DialogViewer = GameDialogViewer.Create(Canvas.RTransform());
        }

        protected virtual void CreateRevenueMiniPanel()
        {
            RevenueMiniPanel = Panels.RevenueMiniPanel.Create(Canvas.RTransform(), RevenuePanelPosition.TopRight);
            RevenueMiniPanel.Hide();
        }

        public virtual void OnBeforeLevelStarted(
            LevelStateChangedArgs _Args,
            UnityAction<long> _GetLifes,
            UnityAction _StartLevel)
        {
            IGameDialogPanel startLevelPanel = new LevelStartPanel(
                DialogViewer, _Args.Level, _GetLifes, _StartLevel);
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

        public virtual void OnTimeEnded(UnityAction<float> _SetAdditionalTime, UnityAction _Continue)
        {
            if (TimeOrLifesEndedPanel.TimesPanelCalled < TimeOrLifesEndedPanel.TimesCanTimeOrLifeBeAdded)
            {
                IGameDialogPanel timeFinishedPanel = new TimeOrLifesEndedPanel(
                    DialogViewer,
                    true,
                    _Continue,
                    _SetAdditionalTime);
                timeFinishedPanel.Show();
            }
            else
            {
                // TODO show game results panel
            }
        }

        public void OnLifesEnded(UnityAction<long> _SetAdditionalLifes, UnityAction _Continue)
        {
            if (TimeOrLifesEndedPanel.TimesPanelCalled < TimeOrLifesEndedPanel.TimesCanTimeOrLifeBeAdded)
            {
                IGameDialogPanel timeFinishedPanel = new TimeOrLifesEndedPanel(
                    DialogViewer,
                    false,
                    _Continue,
                    _SetAdditionalLife: _SetAdditionalLifes);
                timeFinishedPanel.Show();
            }
            else
            {
                // TODO show game results panel
            }
        }
    }
}