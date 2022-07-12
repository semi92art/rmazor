using System.Collections.Generic;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.Common;
using RMAZOR.Views.Coordinate_Converters;

namespace RMAZOR.Views.MazeItemGroups
{
    public interface IViewMazeGravityItemsGroup : IViewMazeMovingItemsGroup
    { }
    
    public class ViewMazeGravityItemsGroup : ViewMazeMovingItemsGroup, IViewMazeGravityItemsGroup
    {
        public ViewMazeGravityItemsGroup(
            ICoordinateConverter _CoordinateConverter,
            IViewMazeCommon _Common)
            : base(_CoordinateConverter, _Common) { }

        public override IEnumerable<EMazeItemType> Types => RmazorUtils.GravityItemTypes;
    }
}