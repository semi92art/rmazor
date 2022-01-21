using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;

namespace RMAZOR.Views.UI
{
    public class ViewUIGameControlsProt : ViewUIGameControlsBase
    {
        public ViewUIGameControlsProt(IViewInputCommandsProceeder _CommandsProceeder) 
            : base(_CommandsProceeder) { }

        public override void OnLevelStageChanged(LevelStageArgs _Args) { }
    }
}