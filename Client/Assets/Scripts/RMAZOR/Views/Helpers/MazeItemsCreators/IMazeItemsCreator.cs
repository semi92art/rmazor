using System.Collections.Generic;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.MazeItems;
using SpawnPools;

namespace RMAZOR.Views.Helpers.MazeItemsCreators
{
    public interface IMazeItemsCreator
    {
        List<IViewMazeItem> CreateMazeItems(MazeInfo _Info);
        void InitPathItems(MazeInfo _Info, SpawnPool<IViewMazeItemPath> _PathPool);
        void InitAndActivateBlockItems(MazeInfo _Info, Dictionary<EMazeItemType, SpawnPool<IViewMazeItem>> _BlockPools);
        IViewMazeItem CloneDefaultBlock(EMazeItemType _Type);
        IViewMazeItemPath CloneDefaultPath();
    }
}