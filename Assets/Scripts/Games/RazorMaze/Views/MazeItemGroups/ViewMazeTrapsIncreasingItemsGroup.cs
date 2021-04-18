using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.MazeCommon;
using Games.RazorMaze.Views.MazeItems;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public class ViewMazeTrapsIncreasingItemsGroup : IViewMazeTrapsIncreasingItemsGroup
    {
        #region inject
        
        private IViewMazeCommon MazeCommon { get; }

        public ViewMazeTrapsIncreasingItemsGroup(IViewMazeCommon _MazeCommon)
        {
            MazeCommon = _MazeCommon;
        }
        
        #endregion
        
        public void Init() { }
        public void OnMazeTrapIncreasingStageChanged(MazeItemTrapIncreasingEventArgs _Args)
        {
            MazeCommon.GetItem<IViewMazeItemTrapIncreasing>(_Args.Item).OnIncreasing(_Args);
        }
    }
}