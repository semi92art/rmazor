using Games.RazorMaze.Views.Characters;
using Games.RazorMaze.Views.InputConfigurators;
using Games.RazorMaze.Views.MazeCommon;
using Games.RazorMaze.Views.MazeItemGroups;
using Games.RazorMaze.Views.Rotation;
using Games.RazorMaze.Views.UI;

namespace Games.RazorMaze.Views
{
    public interface IViewGame : IInit
    {
        IViewUI UI { get; }
        IInputConfigurator InputConfigurator { get; }
        IViewCharacter Character { get; }
        IViewMazeCommon MazeCommon { get; }
        IViewMazeRotation MazeRotation { get; }
        IViewMazeMovingItemsGroup MazeMovingItemsGroup { get; }
        IViewMazeTrapsReactItemsGroup MazeTrapsReactItemsGroup { get; }
        IViewMazeTrapsIncreasingItemsGroup MazeTrapsIncreasingItemsGroup { get; }
    }
}