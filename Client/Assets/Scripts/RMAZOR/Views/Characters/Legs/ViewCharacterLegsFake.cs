using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Helpers;
using RMAZOR.Models;

namespace RMAZOR.Views.Characters.Legs
{
    public interface IViewCharacterLegsFake: IViewCharacterLegs { }
    
    public class ViewCharacterLegsFake : InitBase, IViewCharacterLegsFake
    {
        public void OnRotationFinished(MazeRotationEventArgs _Args) { }

        public EAppearingState AppearingState => EAppearingState.Dissapeared;
        public bool            Activated      { get; set; }
        
        public void Appear(bool _Appear)                                                  { }
        public void OnCharacterMoveStarted(CharacterMovingStartedEventArgs     _Args)     { }
        public void OnCharacterMoveContinued(CharacterMovingContinuedEventArgs _Args)     { }
        public void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs   _Args)     { }
        public void OnLevelStageChanged(LevelStageArgs                         _Args)     { }
        public void OnPathCompleted(V2Int                                      _LastPath) { }
    }
}