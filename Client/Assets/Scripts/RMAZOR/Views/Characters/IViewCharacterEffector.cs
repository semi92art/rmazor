using Common.Entities;
using Common.SpawnPools;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Models.ItemProceeders.Additional;

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