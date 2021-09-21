using Games.RazorMaze.Views.Characters;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.InputConfigurators;
using Games.RazorMaze.Views.MazeItemGroups;
using Games.RazorMaze.Views.Rotation;
using Games.RazorMaze.Views.UI;

namespace Games.RazorMaze.Views
{
    public interface IViewGame : IInit, IOnLevelStageChanged
    {
        IViewUI                            UI { get; }
        IInputConfigurator                 InputConfigurator { get; }
        IViewCharacter                     Character { get; }
        IViewMazeCommon                    MazeCommon { get; }
        IViewMazeEffector                  MazeEffector { get; }
        IViewMazeRotation                  MazeRotation { get; }
        IViewMazePathItemsGroup            MazePathItemsGroup { get; }
        IViewMazeMovingItemsGroup          MazeMovingItemsGroup { get; }
        IViewMazeTrapsReactItemsGroup      MazeTrapsReactItemsGroup { get; }
        IViewMazeTrapsIncreasingItemsGroup MazeTrapsIncreasingItemsGroup { get; }
        IViewMazeTurretsGroup              MazeTurretsGroup { get; }
        IViewMazePortalsGroup              PortalsGroup { get; }
        IViewMazeShredingerBlocksGroup     ShredingerBlocksGroup { get; }
        IViewMazeSpringboardItemsGroup     SpringboardItemsGroup { get; }
    }
}