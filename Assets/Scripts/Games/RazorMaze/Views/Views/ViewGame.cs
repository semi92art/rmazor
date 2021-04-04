using Games.RazorMaze.Views.Characters;
using Games.RazorMaze.Views.InputConfigurators;
using Games.RazorMaze.Views.MazeCommon;
using Games.RazorMaze.Views.MazeItemGroups;
using Games.RazorMaze.Views.Rotation;
using Games.RazorMaze.Views.UI;

namespace Games.RazorMaze.Views.Views
{
    public class ViewGame : IViewGame
    {
        public IViewUI UI { get; }
        public IInputConfigurator InputConfigurator { get; }
        public IViewCharacter Character { get; }
        public IViewMazeCommon MazeCommon { get; }
        public IViewMazeRotation MazeRotation { get; }
        public IViewMazeMovingItemsGroup MazeMovingItemsGroup { get; }
        public IViewMazeTrapsReactItemsGroup MazeTrapsReactItemsGroup { get; }
        public IViewMazeTrapsIncreasingItemsGroup MazeTrapsIncreasingItemsGroup { get; }
        public IViewMazeTurretsGroup MazeTurretsGroup { get; }
        public IViewMazePortalsGroup PortalsGroup { get; }

        public ViewGame(
            IViewUI _UI,
            IInputConfigurator _InputConfigurator,
            IViewCharacter _Character,
            IViewMazeCommon _MazeCommon,
            IViewMazeRotation _MazeRotation,
            IViewMazeMovingItemsGroup _MazeMovingItemsGroup,
            IViewMazeTrapsReactItemsGroup _MazeTrapsReactItemsGroup,
            IViewMazeTrapsIncreasingItemsGroup _MazeTrapsIncreasingItemsGroup,
            IViewMazeTurretsGroup _MazeTurretsGroup,
            IViewMazePortalsGroup _PortalsGroup)
        {
            UI = _UI;
            InputConfigurator = _InputConfigurator;
            Character = _Character;
            MazeCommon = _MazeCommon;
            MazeRotation = _MazeRotation;
            MazeMovingItemsGroup = _MazeMovingItemsGroup;
            MazeTrapsReactItemsGroup = _MazeTrapsReactItemsGroup;
            MazeTrapsIncreasingItemsGroup = _MazeTrapsIncreasingItemsGroup;
            MazeTurretsGroup = _MazeTurretsGroup;
            PortalsGroup = _PortalsGroup;
        }

        public void Init()
        {
            MazeCommon.Init();
            MazeRotation.Init();
            MazeMovingItemsGroup.Init();
            MazeTrapsReactItemsGroup.Init();
            MazeTrapsIncreasingItemsGroup.Init();
            Character.Init();
            InputConfigurator.ConfigureInput();
        }
    }
}