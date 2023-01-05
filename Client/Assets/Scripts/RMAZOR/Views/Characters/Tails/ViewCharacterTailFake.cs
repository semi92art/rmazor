using System;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Helpers;
using RMAZOR.Models;

namespace RMAZOR.Views.Characters.Tails
{
    public interface IViewCharacterTailFake : IViewCharacterTail { }
    
    public class ViewCharacterTailFake : InitBase, IViewCharacterTailFake
    {
        public Func<ViewCharacterInfo> GetCharacterObjects { get; set; }
        public bool                    Activated           { get; set; }
        
        public void OnCharacterMoveStarted(CharacterMovingStartedEventArgs     _Args)        { }
        public void OnCharacterMoveContinued(CharacterMovingContinuedEventArgs _Args)        { }
        public void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs   _Args)        { }
        public void OnLevelStageChanged(LevelStageArgs                         _Args)        { }
        public void ShowTail(CharacterMoveEventArgsBase                        _Args)        { }
        public void HideTail(CharacterMovingFinishedEventArgs                  _Args = null) { }
        public void OnPathCompleted(V2Int _LastPath)                                         { }
    }
}