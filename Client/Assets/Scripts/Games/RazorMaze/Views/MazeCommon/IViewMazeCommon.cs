using System.Collections.Generic;
using Entities;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.MazeItems;

namespace Games.RazorMaze.Views.MazeCommon
{
    public interface IViewMazeCommon : IInit
    {
        List<IViewMazeItem> MazeItems { get; }
        void OnPathProceed(V2Int _PathItem);
        IViewMazeItem GetItem(MazeItem _Item);
        T GetItem<T>(MazeItem _Item) where T : IViewMazeItem;
        event NoArgsHandler GameLoopUpdate;
    }
}