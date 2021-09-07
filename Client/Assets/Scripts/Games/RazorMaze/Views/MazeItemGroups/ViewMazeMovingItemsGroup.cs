using System.Collections.Generic;
using Entities;
using Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.MazeCommon;
using Games.RazorMaze.Views.Utils;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public class ViewMazeMovingItemsGroup : ViewMazeMovingItemsGroupBase
    {
        #region inject
        
        private ViewSettings ViewSettings { get; }

        public ViewMazeMovingItemsGroup(
            IModelMazeData _Data,
            IMovingItemsProceeder _MovingItemsProceeder,
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IViewMazeCommon _MazeCommon,
            ViewSettings _ViewSettings) 
            : base(_Data, _MovingItemsProceeder, _CoordinateConverter, _ContainersGetter, _MazeCommon)
        {
            ViewSettings = _ViewSettings;
        }
        
        #endregion
        
        #region api

        public override void Init()
        {
            DrawWallBlockMovingPaths(DrawingUtils.ColorLines);
        }

        #endregion

        #region nonpublic methods

        protected override void MarkMazeItemBusyPositions(MazeItem _Item, IEnumerable<V2Int> _Positions) { }

        #endregion
    }
}