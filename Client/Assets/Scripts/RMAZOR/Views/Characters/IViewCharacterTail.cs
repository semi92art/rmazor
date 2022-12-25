using System;
using Common;
using Common.Entities;
using Common.Helpers;
using mazing.common.Runtime;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.SpawnPools;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders.Additional;

namespace RMAZOR.Views.Characters
{
    public interface IViewCharacterTail :
        IInit,
        IActivated,
        ICharacterMoveStarted,
        ICharacterMoveContinued,
        ICharacterMoveFinished,
        IOnLevelStageChanged,
        IOnPathCompleted
    {
        Func<ViewCharacterInfo> GetCharacterObjects { get; set; }
    }

    public class ViewCharacterTailFake : InitBase, IViewCharacterTail
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