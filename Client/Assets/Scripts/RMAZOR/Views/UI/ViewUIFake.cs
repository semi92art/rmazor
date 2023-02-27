using RMAZOR.Models;

namespace RMAZOR.Views.UI
{
    public class ViewUIFake : ViewUIBase
    {
        protected ViewUIFake(IViewUIGameControls _GameControls) 
            : base(_GameControls) { }
        
        public override void OnLevelStageChanged(LevelStageArgs                       _Args) { }
        public override void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args) { }
        public override void OnMazeRotationFinished(MazeRotationEventArgs _Args)             { }
    }
}