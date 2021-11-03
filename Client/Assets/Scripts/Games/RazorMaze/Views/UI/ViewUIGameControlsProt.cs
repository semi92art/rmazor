using Games.RazorMaze.Views.InputConfigurators;

namespace Games.RazorMaze.Views.UI
{
    public class ViewUIGameControlsProt : ViewUIGameControlsBase
    {
        public ViewUIGameControlsProt(IViewInput _Input) 
            : base(_Input) { }

        public override void OnLevelStageChanged(LevelStageArgs _Args) { }
    }
}