using Entities;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Common;
using SpawnPools;

namespace Games.RazorMaze.Views.Characters
{
    public interface IViewCharacter : IInit, IActivated, IOnRevivalOrDeath, IOnLevelStageChanged,
        ICharacterMoveStarted, ICharacterMoveContinued, ICharacterMoveFinished, IOnBackgroundColorChanged
    {
        void OnPositionSet(V2Int _Position);
    }
}