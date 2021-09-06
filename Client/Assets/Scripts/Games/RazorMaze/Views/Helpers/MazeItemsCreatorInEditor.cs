using Games.RazorMaze.Views.ContainerGetters;

namespace Games.RazorMaze.Views.Helpers
{
    public class MazeItemsCreatorInEditor : MazeItemsCreatorProt
    {
        public MazeItemsCreatorInEditor(
            IContainersGetter _ContainersGetter,
            ICoordinateConverter _CoordinateConverter) : base(_ContainersGetter, _CoordinateConverter)
        { }
    }
}