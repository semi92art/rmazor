using RMAZOR.Models;

namespace RMAZOR.Views.UI
{
    public class ViewUIFake : ViewUIBase
    {
        public override void OnLevelStageChanged(LevelStageArgs _Args) { }

        public ViewUIFake(IViewUIGameControls _GameControls) 
            : base(_GameControls) { }
    }
}