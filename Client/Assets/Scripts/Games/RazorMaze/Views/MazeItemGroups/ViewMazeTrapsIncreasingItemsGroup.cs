using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.MazeItems;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public class ViewMazeTrapsIncreasingItemsGroup : ViewMazeItemsGroupBase, IViewMazeTrapsIncreasingItemsGroup
    {
        #region inject
        
        public ViewMazeTrapsIncreasingItemsGroup(IViewMazeCommon _Common) : base(_Common) { }
        
        #endregion

        public override EMazeItemType[] Types => new[] {EMazeItemType.TrapIncreasing};
        public event NoArgsHandler Initialized;
        
        public void Init() => Initialized?.Invoke();
        
        public void OnMazeTrapIncreasingStageChanged(MazeItemTrapIncreasingEventArgs _Args)
        {
            Common.GetItem<IViewMazeItemTrapIncreasing>(_Args.Item).OnIncreasing(_Args);
        }

    }
}