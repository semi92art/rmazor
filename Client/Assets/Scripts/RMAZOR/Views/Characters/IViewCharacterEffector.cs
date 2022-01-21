using Common.Entities;
using RMAZOR.Models.ItemProceeders;
using SpawnPools;

namespace RMAZOR.Views.Characters
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