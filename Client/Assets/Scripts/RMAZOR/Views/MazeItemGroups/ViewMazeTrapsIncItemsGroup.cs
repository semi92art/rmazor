using System.Collections.Generic;
using Common;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.Common;
using RMAZOR.Views.MazeItems;
using UnityEngine.Events;

namespace RMAZOR.Views.MazeItemGroups
{
    public interface IViewMazeTrapsIncItemsGroup :
        IInit,
        IViewMazeItemGroup
    {
        void OnMazeTrapIncreasingStageChanged(MazeItemTrapIncreasingEventArgs _Args);
    }
    
    public class ViewMazeTrapsIncItemsGroup : ViewMazeItemsGroupBase, IViewMazeTrapsIncItemsGroup
    {
        #region inject
        
        public ViewMazeTrapsIncItemsGroup(IViewMazeCommon _Common) : base(_Common) { }
        
        #endregion

        public override IEnumerable<EMazeItemType> Types => new[] {EMazeItemType.TrapIncreasing};

        public void OnMazeTrapIncreasingStageChanged(MazeItemTrapIncreasingEventArgs _Args)
        {
            Common.GetItem<IViewMazeItemTrapIncreasing>(_Args.Item).OnIncreasing(_Args);
        }

    }
}