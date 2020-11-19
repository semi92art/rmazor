using DialogViewers;
using Extensions;
using UI.Factories;
using UI.Panels;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public interface IGameMenuUi
    {
        ILifesPanel LifesPanel { get; }
    }
    
    public abstract class GameMenuUiBase : IGameMenuUi
    {
        public ILifesPanel LifesPanel { get; protected set; }
        protected IDialogViewer DialogViewer;
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
            DialogViewer = GameDialogViewer.Create(Canvas.RTransform());
        }

        protected virtual void CreateStatsPanel()
        {
            LifesPanel = Panels.LifesPanel.Create(Canvas.RTransform(), DialogViewer);
        }
    }
}