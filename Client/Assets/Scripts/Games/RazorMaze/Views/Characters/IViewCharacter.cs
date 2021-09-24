using Entities;
using Games.RazorMaze.Models.ItemProceeders;
using SpawnPools;

namespace Games.RazorMaze.Views.Characters
{
    public interface IViewCharacter : IInit, IActivated, IOnRevivalOrDeath, IOnLevelStageChanged,
        ICharacterMoveStarted, ICharacterMoveContinued, ICharacterMoveFinished
    {
        void OnPositionSet(V2Int _Position);
    }
}