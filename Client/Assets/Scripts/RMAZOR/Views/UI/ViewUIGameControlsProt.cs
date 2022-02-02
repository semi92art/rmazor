using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;

namespace RMAZOR.Views.UI
{
    public class ViewUIGameControlsProt : ViewUIGameControlsBase
    {
        public ViewUIGameControlsProt(IModelGame _Model, IViewInputCommandsProceeder _CommandsProceeder) 
            : base(_Model, _CommandsProceeder) { }

        public override void OnLevelStageChanged(LevelStageArgs _Args) { }
    }
}