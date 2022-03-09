using System.Collections.Generic;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.MazeItems;

namespace RMAZOR.Views.MazeItemGroups
{
    public interface IViewMazeItemGroup : IOnLevelStageChanged
    {
        IEnumerable<EMazeItemType> Types { get; }
        IEnumerable<IViewMazeItem> GetActiveItems();
    }
}