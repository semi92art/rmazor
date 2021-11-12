using Games.RazorMaze.Views.InputConfigurators;

namespace Games.RazorMaze.Views.UI
{
    public class ViewUIGameControlsProt : ViewUIGameControlsBase
    {
        public ViewUIGameControlsProt(IViewInputCommandsProceeder _CommandsProceeder) 
            : base(_CommandsProceeder) { }

        public override void OnLevelStageChanged(LevelStageArgs _Args) { }
    }
}