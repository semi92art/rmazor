using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.MazeItems;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public sealed class ViewMazeSpringboardItemsGroup : ViewMazeSpringboardItemsGroupBase
    {
        #region inject

        private IViewMazeCommon ViewMazeCommon { get; }
        
        public ViewMazeSpringboardItemsGroup(IViewMazeCommon _ViewMazeCommon)
        {
            ViewMazeCommon = _ViewMazeCommon;
        }
        
        #endregion
        
        
        public override void OnSpringboardEvent(SpringboardEventArgs _Args)
        {
            var item = ViewMazeCommon.GetItem<IViewMazeItemSpringboard>(_Args.Item);
            item.MakeJump(_Args);
        }
    }
}