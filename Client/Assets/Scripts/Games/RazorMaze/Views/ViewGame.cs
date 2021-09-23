using System.Collections.Generic;
using System.Linq;
using Games.RazorMaze.Views.Characters;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.InputConfigurators;
using Games.RazorMaze.Views.MazeItemGroups;
using Games.RazorMaze.Views.Rotation;
using Games.RazorMaze.Views.UI;
using Ticker;
using Utils;

namespace Games.RazorMaze.Views
{
    public class ViewGame : IViewGame
    {
        public IViewUI UI { get; }
        public IInputConfigurator InputConfigurator { get; }
        public IViewCharacter Character { get; }
        public IViewMazeCommon MazeCommon { get; }
        public IViewMazeEffector MazeEffector { get; }
        public IViewMazeRotation MazeRotation { get; }
        public IViewMazePathItemsGroup MazePathItemsGroup { get; }
        public IViewMazeMovingItemsGroup MazeMovingItemsGroup { get; }
        public IViewMazeTrapsReactItemsGroup MazeTrapsReactItemsGroup { get; }
        public IViewMazeTrapsIncreasingItemsGroup MazeTrapsIncreasingItemsGroup { get; }
        public IViewMazeTurretsGroup MazeTurretsGroup { get; }
        public IViewMazePortalsGroup PortalsGroup { get; }
        public IViewMazeShredingerBlocksGroup ShredingerBlocksGroup { get; }
        public IViewMazeSpringboardItemsGroup SpringboardItemsGroup { get; }
        
        private IGameTicker GameTicker { get; }

        public ViewGame(
            IViewUI _UI,
            IInputConfigurator _InputConfigurator,
            IViewCharacter _Character,
            IViewMazeCommon _MazeCommon,
            IViewMazeEffector _MazeEffector,
            IViewMazeRotation _MazeRotation,
            IViewMazePathItemsGroup _MazePathItemsGroup,
            IViewMazeMovingItemsGroup _MazeMovingItemsGroup,
            IViewMazeTrapsReactItemsGroup _MazeTrapsReactItemsGroup,
            IViewMazeTrapsIncreasingItemsGroup _MazeTrapsIncreasingItemsGroup,
            IViewMazeTurretsGroup _MazeTurretsGroup,
            IViewMazePortalsGroup _PortalsGroup,
            IViewMazeShredingerBlocksGroup _ShredingerBlocksGroup,
            IViewMazeSpringboardItemsGroup _SpringboardItemsGroup,
            IGameTicker _GameTicker)
        {
            UI = _UI;
            InputConfigurator = _InputConfigurator;
            Character = _Character;
            MazeCommon = _MazeCommon;
            MazeEffector = _MazeEffector;
            MazeRotation = _MazeRotation;
            MazePathItemsGroup = _MazePathItemsGroup;
            MazeMovingItemsGroup = _MazeMovingItemsGroup;
            MazeTrapsReactItemsGroup = _MazeTrapsReactItemsGroup;
            MazeTrapsIncreasingItemsGroup = _MazeTrapsIncreasingItemsGroup;
            MazeTurretsGroup = _MazeTurretsGroup;
            PortalsGroup = _PortalsGroup;
            ShredingerBlocksGroup = _ShredingerBlocksGroup;
            SpringboardItemsGroup = _SpringboardItemsGroup;
            GameTicker = _GameTicker;
        }
        
        public event NoArgsHandler Initialized;

        public void Init()
        {
            var proceeders = GetInterfaceOfProceeders<IInit>();
            int count = proceeders.Count;
            bool[] initialized = new bool[count];
            for (int i = 0; i < count; i++)
            {
                var i1 = i;
                proceeders[i].Initialized += () => initialized[i1] = true;
                proceeders[i].Init();
            }
            
            Coroutines.Run(Coroutines.WaitWhile(
                () => initialized.Any(_Initialized => !_Initialized), 
                () => Initialized?.Invoke()));
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            var proceeders = GetInterfaceOfProceeders<IOnLevelStageChanged>();
            foreach (var proceeder in proceeders)
                proceeder.OnLevelStageChanged(_Args);
            GameTicker.Pause = _Args.Stage == ELevelStage.Paused;
        }
        
        private List<T> GetInterfaceOfProceeders<T>() where T : class
        {
            var result = new List<T>
            {
                UI                                 as T,
                MazeCommon                         as T,
                InputConfigurator                  as T,
                Character                          as T,
                MazeEffector                       as T,
                MazeRotation                       as T,
                MazePathItemsGroup                 as T,
                MazeMovingItemsGroup               as T,
                MazeTrapsReactItemsGroup           as T,
                MazeTrapsIncreasingItemsGroup      as T,
                MazeTurretsGroup                   as T,
                PortalsGroup                       as T,
                ShredingerBlocksGroup              as T,
                SpringboardItemsGroup              as T
            }.Where(_Proceeder => _Proceeder != null).ToList();
            return result;
        } 
    }
}