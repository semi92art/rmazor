using Common.Entities;
using Common.SpawnPools;
using RMAZOR.Models.ItemProceeders;

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