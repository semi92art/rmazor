using System.Collections.Generic;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.MazeItems;
using SpawnPools;

namespace Games.RazorMaze.Views.Helpers
{
    public interface IMazeItemsCreator
    {
        List<IViewMazeItem> CreateMazeItems(MazeInfo _Info);
        void InitMazeItems(
            MazeInfo _Info, 
            SpawnPool<IViewMazeItemPath> _PathPool,
            Dictionary<EMazeItemType, SpawnPool<IViewMazeItem>> _BlockPools);
        IViewMazeItem CloneDefaultBlock(EMazeItemType _Type);
        IViewMazeItemPath CloneDefaultPath();
    }
}