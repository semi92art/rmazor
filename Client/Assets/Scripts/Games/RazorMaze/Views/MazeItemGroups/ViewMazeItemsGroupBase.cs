using System.Collections.Generic;
using System.Linq;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.MazeItems;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public abstract class ViewMazeItemsGroupBase : IViewMazeItemGroup
    {
        #region nonpublic members

        protected IViewMazeCommon Common { get; }

        #endregion

        #region constructor

        protected ViewMazeItemsGroupBase(IViewMazeCommon _Common)
        {
            Common = _Common;
        }

        #endregion

        #region api
        
        public abstract EMazeItemType[] Types { get; }
        
        public IEnumerable<IViewMazeItem> GetItems()
        {
            return Common.MazeItems.Where(_Item => Types.Contains(_Item.Props.Type)).ToList();
        }
        
        public IEnumerable<IViewMazeItem> GetActiveItems()
        {
            return GetItems().Where(_Item => _Item.ActivatedInSpawnPool);
        }
        
        public virtual void OnLevelStageChanged(LevelStageArgs _Args)
        {
            foreach (var item in GetItems())
                item.OnLevelStageChanged(_Args);
        }

        #endregion
    }
}