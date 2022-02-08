using System.Collections.Generic;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.MazeItems;

namespace RMAZOR.Views.MazeItemGroups
{
    public interface IViewMazeItemGroup : IOnLevelStageChanged
    {
        EMazeItemType[]     Types { get; }
        List<IViewMazeItem> GetItems();
        List<IViewMazeItem> GetActiveItems();
    }
}