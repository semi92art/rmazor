using System.Collections.Generic;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.MazeItems;
using SpawnPools;

namespace Games.RazorMaze.Views.Helpers.MazeItemsCreators
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