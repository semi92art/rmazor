using Games.RazorMaze.Views.Characters;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.InputConfigurators;
using Games.RazorMaze.Views.MazeItemGroups;
using Games.RazorMaze.Views.Rotation;
using Games.RazorMaze.Views.UI;
using Utils;

namespace Games.RazorMaze.Views
{
    public class ViewGame : IViewGame
    {
        public IViewUI UI { get; }
        public IInputConfigurator InputConfigurator { get; }
        public IViewCharacter Character { get; }
        public IViewMazeCommon MazeCommon { get; }
        public IViewMazeTransitioner MazeTransitioner { get; }
        public IViewMazeRotation MazeRotation { get; }
        public IViewMazeMovingItemsGroup MazeMovingItemsGroup { get; }
        public IViewMazeTrapsReactItemsGroup MazeTrapsReactItemsGroup { get; }
        public IViewMazeTrapsIncreasingItemsGroup MazeTrapsIncreasingItemsGroup { get; }
        public IViewMazeTurretsGroup MazeTurretsGroup { get; }
        public IViewMazePortalsGroup PortalsGroup { get; }
        public IViewMazeShredingerBlocksGroup ShredingerBlocksGroup { get; }
        public IViewMazeSpringboardItemsGroup SpringboardItemsGroup { get; }

        public ViewGame(
            IViewUI _UI,
            IInputConfigurator _InputConfigurator,
            IViewCharacter _Character,
            IViewMazeCommon _MazeCommon,
            IViewMazeTransitioner _MazeTransitioner,
            IViewMazeRotation _MazeRotation,
            IViewMazeMovingItemsGroup _MazeMovingItemsGroup,
            IViewMazeTrapsReactItemsGroup _MazeTrapsReactItemsGroup,
            IViewMazeTrapsIncreasingItemsGroup _MazeTrapsIncreasingItemsGroup,
            IViewMazeTurretsGroup _MazeTurretsGroup,
            IViewMazePortalsGroup _PortalsGroup,
            IViewMazeShredingerBlocksGroup _ShredingerBlocksGroup,
            IViewMazeSpringboardItemsGroup _SpringboardItemsGroup)
        {
            UI = _UI;
            InputConfigurator = _InputConfigurator;
            Character = _Character;
            MazeCommon = _MazeCommon;
            MazeTransitioner = _MazeTransitioner;
            MazeRotation = _MazeRotation;
            MazeMovingItemsGroup = _MazeMovingItemsGroup;
            MazeTrapsReactItemsGroup = _MazeTrapsReactItemsGroup;
            MazeTrapsIncreasingItemsGroup = _MazeTrapsIncreasingItemsGroup;
            MazeTurretsGroup = _MazeTurretsGroup;
            PortalsGroup = _PortalsGroup;
            ShredingerBlocksGroup = _ShredingerBlocksGroup;
            SpringboardItemsGroup = _SpringboardItemsGroup;
        }
        
        public event NoArgsHandler Initialized;

        public void Init()
        {
            bool mazeCommonInitialized = false;
            bool mazeRotationInitialized = false;
            bool mazeMovingItemsGroupInitialized = false;
            bool mazeTrapsReactItemsGroupInitialized = false;
            bool mazeTrapsIncreasingItemsGroupInitialized = false;
            bool characterInitialized = false;
            bool inputConfiguratorInitialized = false;

            MazeCommon.Initialized                    += () => mazeCommonInitialized = true;
            MazeRotation.Initialized                  += () => mazeRotationInitialized = true;
            MazeMovingItemsGroup.Initialized          += () => mazeMovingItemsGroupInitialized = true;
            MazeTrapsReactItemsGroup.Initialized      += () => mazeTrapsReactItemsGroupInitialized = true;
            MazeTrapsIncreasingItemsGroup.Initialized += () => mazeTrapsIncreasingItemsGroupInitialized = true;
            Character.Initialized                     += () => characterInitialized = true;
            InputConfigurator.Initialized             += () => inputConfiguratorInitialized = true;
            
            MazeCommon.Init();
            MazeRotation.Init();
            MazeMovingItemsGroup.Init();
            MazeTrapsReactItemsGroup.Init();
            MazeTrapsIncreasingItemsGroup.Init();
            Character.Init();
            InputConfigurator.Init();

            System.Func<bool> allInitialized = () =>
                mazeCommonInitialized
                && mazeRotationInitialized
                && mazeMovingItemsGroupInitialized
                && mazeTrapsReactItemsGroupInitialized
                && mazeTrapsIncreasingItemsGroupInitialized
                && characterInitialized
                && inputConfiguratorInitialized;

            Coroutines.Run(Coroutines.WaitWhile(
                () => !allInitialized.Invoke(), 
                () => Initialized?.Invoke()));
        }
    }
}