using Games.RazorMaze.Views.Characters;
using Games.RazorMaze.Views.InputConfigurators;
using Games.RazorMaze.Views.MazeCommon;
using Games.RazorMaze.Views.MazeItemGroups;
using Games.RazorMaze.Views.Rotation;
using Games.RazorMaze.Views.UI;

namespace Games.RazorMaze.Views
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

        public ViewGame(
            IViewUI _UI,
            IInputConfigurator _InputConfigurator,
            IViewCharacter _Character,
            IViewMazeCommon _MazeCommon,
            IViewMazeRotation _MazeRotation,
            IViewMazeMovingItemsGroup _MazeMovingItemsGroup,
            IViewMazeTrapsReactItemsGroup _MazeTrapsReactItemsGroup)
        {
            UI = _UI;
            InputConfigurator = _InputConfigurator;
            Character = _Character;
            MazeCommon = _MazeCommon;
            MazeRotation = _MazeRotation;
            MazeMovingItemsGroup = _MazeMovingItemsGroup;
            MazeTrapsReactItemsGroup = _MazeTrapsReactItemsGroup;
        }

        public void Init()
        {
            MazeRotation.Init();
            MazeMovingItemsGroup.Init();
            Character.Init();
            InputConfigurator.ConfigureInput();
        }
    }
}