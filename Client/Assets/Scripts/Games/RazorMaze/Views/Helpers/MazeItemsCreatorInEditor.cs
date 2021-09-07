using System.Collections.Generic;
using Entities;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.MazeItems;

namespace Games.RazorMaze.Views.Helpers
{
    public class MazeItemsCreatorInEditor : MazeItemsCreatorProt
    {
        public MazeItemsCreatorInEditor(
            IContainersGetter _ContainersGetter,
            ICoordinateConverter _CoordinateConverter) 
            : base(_ContainersGetter, _CoordinateConverter)
        { }

        protected override void AddMazeItemProt(
            ICollection<IViewMazeItem> _Items,
            V2Int _MazeSize, 
            ViewMazeItemProps _Props)
        { }
    }
}