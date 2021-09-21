using System.Collections.Generic;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.MazeItems;

namespace Games.RazorMaze.Views.Common
{
    public interface IViewMazeCommon : IInit
    {
        List<IViewMazeItem> MazeItems { get; }
        IViewMazeItem GetItem(MazeItem _Item);
        T GetItem<T>(MazeItem _Item) where T : IViewMazeItem;
        event NoArgsHandler GameLoopUpdate;
    }
}