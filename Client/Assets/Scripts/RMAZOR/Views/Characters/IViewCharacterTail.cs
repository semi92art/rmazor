using System;
using Common;
using Common.Entities;
using Common.Helpers;
using Common.SpawnPools;
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
        IOnLevelStageChanged
    {
        Func<ViewCharacterInfo> GetCharacterObjects { get; set; }
        void                    OnAllPathProceed(V2Int                    _LastPath);
        void                    ShowTail(CharacterMoveEventArgsBase       _Args);
        void                    HideTail(CharacterMovingFinishedEventArgs _Args = null);
    }

    public class ViewCharacterTailFake : InitBase, IViewCharacterTail
    {
        public Func<ViewCharacterInfo> GetCharacterObjects { get; set; }
        public bool                    Activated           { get; set; }
        
        public void                    OnCharacterMoveStarted(CharacterMovingStartedEventArgs     _Args) { }
        public void                    OnCharacterMoveContinued(CharacterMovingContinuedEventArgs _Args) { }
        public void                    OnCharacterMoveFinished(CharacterMovingFinishedEventArgs   _Args) { }
        public void                    OnLevelStageChanged(LevelStageArgs                         _Args) { }
        public void                    OnAllPathProceed(V2Int                    _LastPath)              { }
        public void                    ShowTail(CharacterMoveEventArgsBase       _Args)                  { }
        public void                    HideTail(CharacterMovingFinishedEventArgs _Args = null)           { }
    }
}