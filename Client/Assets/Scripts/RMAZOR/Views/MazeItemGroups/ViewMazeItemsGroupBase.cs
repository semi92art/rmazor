using System.Collections.Generic;
using System.Linq;
using Common.Helpers;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.Common;
using RMAZOR.Views.MazeItems;

namespace RMAZOR.Views.MazeItemGroups
{
    public abstract class ViewMazeItemsGroupBase : InitBase, IViewMazeItemGroup
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
        
        public List<IViewMazeItem> GetItems()
        {
            var result = new List<IViewMazeItem>();
            var items = Common.GetItems(false);
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (!Types.Contains(item.Props.Type))
                    continue;
                result.Add(item);
            }
            return result;
        }
        
        public List<IViewMazeItem> GetActiveItems()
        {
            var result = new List<IViewMazeItem>();
            var items = Common.GetItems();
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (!Types.Contains(item.Props.Type))
                    continue;
                result.Add(item);
            }
            return result;
        }
        
        public virtual void OnLevelStageChanged(LevelStageArgs _Args)
        {
            var items = GetItems();
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                item.OnLevelStageChanged(_Args);
            }
        }

        #endregion
    }
}