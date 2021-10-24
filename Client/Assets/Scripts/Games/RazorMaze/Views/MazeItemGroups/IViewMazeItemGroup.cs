using System.Collections.Generic;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.MazeItems;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public interface IViewMazeItemGroup : IOnLevelStageChanged
    {
        EMazeItemType[] Types { get; }
        IEnumerable<IViewMazeItem> GetItems();
        IEnumerable<IViewMazeItem> GetActiveItems();
    }
}