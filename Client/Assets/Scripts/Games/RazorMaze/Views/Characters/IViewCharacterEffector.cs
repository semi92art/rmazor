using Entities;
using Games.RazorMaze.Models.ItemProceeders;
using SpawnPools;

namespace Games.RazorMaze.Views.Characters
{
    public interface IViewCharacterEffector :
        IActivated,
        IOnLevelStageChanged,
        ICharacterMoveStarted,
        ICharacterMoveFinished
    {
        void OnAllPathProceed(V2Int _LastPos);
    }
}