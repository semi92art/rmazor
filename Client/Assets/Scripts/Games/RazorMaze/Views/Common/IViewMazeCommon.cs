using System.Collections.Generic;
using Games.RazorMaze.Models.ProceedInfos;
using Games.RazorMaze.Views.MazeItems;

namespace Games.RazorMaze.Views.Common
{
    public interface IViewMazeCommon : IInit, IOnLevelStageChanged
    {
        IViewMazeItem       GetItem(IMazeItemProceedInfo _Item);
        T                   GetItem<T>(IMazeItemProceedInfo _Item) where T : class, IViewMazeItem;
        List<IViewMazeItem> GetItems(bool _OnlyActive = true);
        List<T>             GetItems<T>(bool _OnlyActive = true) where T : class, IViewMazeItem;
    }
}