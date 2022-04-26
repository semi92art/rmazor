using System.Collections.Generic;
using Common;
using RMAZOR.Models.ProceedInfos;
using RMAZOR.Views.MazeItems;

namespace RMAZOR.Views.Common
{
    public interface IViewMazeCommon : IInit, IOnLevelStageChanged
    {
        IViewMazeItem       GetItem(IMazeItemProceedInfo    _Item);
        T                   GetItem<T>(IMazeItemProceedInfo _Item) where T : class, IViewMazeItem;
        List<IViewMazeItem> GetItems(bool                   _OnlyActive = true, bool _OnlyInitialized = true);
        List<T>             GetItems<T>(bool                _OnlyActive = true) where T : class, IViewMazeItem;
    }
}