using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.MazeItems;
using UnityEngine.Events;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public class ViewMazeTrapsIncItemsGroup : ViewMazeItemsGroupBase, IViewMazeTrapsIncItemsGroup
    {
        #region inject
        
        public ViewMazeTrapsIncItemsGroup(IViewMazeCommon _Common) : base(_Common) { }
        
        #endregion

        public override EMazeItemType[] Types => new[] {EMazeItemType.TrapIncreasing};
        public event UnityAction Initialized;
        
        public void Init() => Initialized?.Invoke();
        
        public void OnMazeTrapIncreasingStageChanged(MazeItemTrapIncreasingEventArgs _Args)
        {
            Common.GetItem<IViewMazeItemTrapIncreasing>(_Args.Item).OnIncreasing(_Args);
        }

    }
}