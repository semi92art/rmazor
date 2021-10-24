using System.Collections.Generic;
using Entities;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.MazeItems;
using Games.RazorMaze.Views.MazeItems.Props;

namespace Games.RazorMaze.Views.Helpers.MazeItemsCreators
{
    public class MazeItemsCreatorInEditor : MazeItemsCreatorProt
    {
        public MazeItemsCreatorInEditor(
            IContainersGetter _ContainersGetter,
            IMazeCoordinateConverter _CoordinateConverter) 
            : base(_ContainersGetter, _CoordinateConverter)
        { }

        protected override void AddMazeItemProt(
            ICollection<IViewMazeItem> _Items, 
            V2Int _MazeSize,
            ViewMazeItemProps _Props)
        {
            AddMazeItemProtCore(_Items, _MazeSize, _Props);
        }
    }
}