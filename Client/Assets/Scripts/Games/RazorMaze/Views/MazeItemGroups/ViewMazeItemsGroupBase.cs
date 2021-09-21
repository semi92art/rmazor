using System.Collections.Generic;
using System.Linq;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.MazeItems;
using Utils;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public abstract class ViewMazeItemsGroupBase : IOnLevelStageChanged, IMazeItemTypes
    {
        public abstract EMazeItemType[] Types { get; }
        protected IViewMazeCommon Common { get; }
        private bool m_CommonInitialized;

        protected ViewMazeItemsGroupBase(IViewMazeCommon _Common)
        {
            Common = _Common;
            Common.Initialized += () => m_CommonInitialized = true;
        }
        
        
        public virtual void OnLevelStageChanged(LevelStageArgs _Args)
        {
            Coroutines.Run(Coroutines.WaitWhile(
                () => !m_CommonInitialized,
                () =>
                {
                    foreach (var item in GetItems())
                        item.OnLevelStageChanged(_Args);
                }));
        }

        protected IEnumerable<IViewMazeItem> GetItems() => 
            Common.MazeItems.Where(_Item => Types.Contains(_Item.Props.Type)).ToList();
    }
}