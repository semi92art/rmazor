using Games.RazorMaze.Models;
using Games.RazorMaze.Views.Common;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public interface IViewMazeGravityItemsGroup : IViewMazeMovingItemsGroup
    { }
    
    public class ViewMazeGravityItemsGroup : ViewMazeMovingItemsGroup, IViewMazeGravityItemsGroup
    {
        public ViewMazeGravityItemsGroup(
            IMazeCoordinateConverter _CoordinateConverter,
            IViewMazeCommon _Common)
            : base(_CoordinateConverter, _Common) { }

        public override EMazeItemType[] Types => RazorMazeUtils.GravityItemTypes();
    }
}