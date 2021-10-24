using Games.RazorMaze.Views.InputConfigurators;

namespace Games.RazorMaze.Views.UI
{
    public class ViewUIGameControlsProt : ViewUIGameControlsBase
    {
        public ViewUIGameControlsProt(IViewInputConfigurator _InputConfigurator) 
            : base(_InputConfigurator) { }

        public override void OnLevelStageChanged(LevelStageArgs _Args) { }
    }
}