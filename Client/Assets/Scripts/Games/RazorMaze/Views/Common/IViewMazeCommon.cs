using System.Collections.Generic;
using Games.RazorMaze.Models.ProceedInfos;
using Games.RazorMaze.Views.MazeItems;

namespace Games.RazorMaze.Views.Common
{
    public interface IViewMazeCommon : IInit, IOnLevelStageChanged
    {
        List<IViewMazeItem> MazeItems { get; }
        IViewMazeItem GetItem(IMazeItemProceedInfo _Item);
        T GetItem<T>(IMazeItemProceedInfo _Item) where T : IViewMazeItem;
    }
}