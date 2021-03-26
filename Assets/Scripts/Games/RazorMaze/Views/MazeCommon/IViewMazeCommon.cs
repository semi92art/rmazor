using System.Collections.Generic;
using Games.RazorMaze.Views.MazeItems;

namespace Games.RazorMaze.Views.MazeCommon
{
    public interface IViewMazeCommon : IInit
    {
        List<IViewMazeItem> MazeItems { get; }
    }
}