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
        IViewMazeCommon                    Common { get; }
        IViewMazeBackground                  Background { get; }
        IViewMazeRotation                  Rotation { get; }
        IViewMazePathItemsGroup            PathItemsGroup { get; }
        IViewMazeMovingItemsGroup          MovingItemsGroup { get; }
        IViewMazeTrapsReactItemsGroup      TrapsReactItemsGroup { get; }
        IViewMazeTrapsIncreasingItemsGroup TrapsIncreasingItemsGroup { get; }
        IViewMazeTurretsGroup              TurretsGroup { get; }
        IViewMazePortalsGroup              PortalsGroup { get; }
        IViewMazeShredingerBlocksGroup     ShredingerBlocksGroup { get; }
        IViewMazeSpringboardItemsGroup     SpringboardItemsGroup { get; }
    }
}