using System.Collections.Generic;
using System.Linq;
using Entities;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Characters;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;
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
        #region nonpublic members

        private readonly List<bool> m_InitChecks = new List<bool>();
        
        #endregion
        
        #region inject
        
        public IContainersGetter ContainersGetter { get; }
        public IViewUI UI { get; }
        public IViewLevelStageController LevelStageController { get; }
        public IInputConfigurator InputConfigurator { get; }
        public IViewCharacter Character { get; }
        public IViewMazeCommon Common { get; }
        public IViewMazeBackground Background { get; }
        public IViewMazeRotation MazeRotation { get; }
        public IViewMazePathItemsGroup PathItemsGroup { get; }
        public IViewMazeMovingItemsGroup MovingItemsGroup { get; }
        public IViewMazeTrapsReactItemsGroup TrapsReactItemsGroup { get; }
        public IViewMazeTrapsIncItemsGroup TrapsIncItemsGroup { get; }
        public IViewMazeTurretsGroup TurretsGroup { get; }
        public IViewMazePortalsGroup PortalsGroup { get; }
        public IViewMazeShredingerBlocksGroup ShredingerBlocksGroup { get; }
        public IViewMazeSpringboardItemsGroup SpringboardItemsGroup { get; }
        
        private IGameTicker GameTicker { get; }
        private IManagersGetter Managers { get; }

        public ViewGame(
            IContainersGetter _ContainersGetter,
            IViewUI _UI,
            IInputConfigurator _InputConfigurator,
            IViewCharacter _Character,
            IViewMazeCommon _Common,
            IViewMazeBackground _Background,
            IViewMazeRotation _MazeRotation,
            IViewMazePathItemsGroup _PathItemsGroup,
            IViewMazeMovingItemsGroup _MovingItemsGroup,
            IViewMazeTrapsReactItemsGroup _TrapsReactItemsGroup,
            IViewMazeTrapsIncItemsGroup _TrapsIncItemsGroup,
            IViewMazeTurretsGroup _TurretsGroup,
            IViewMazePortalsGroup _PortalsGroup,
            IViewMazeShredingerBlocksGroup _ShredingerBlocksGroup,
            IViewMazeSpringboardItemsGroup _SpringboardItemsGroup,
            IGameTicker _GameTicker,
            IManagersGetter _Managers, IViewLevelStageController _LevelStageController)
        {
            ContainersGetter = _ContainersGetter;
            UI = _UI;
            InputConfigurator = _InputConfigurator;
            Character = _Character;
            Common = _Common;
            Background = _Background;
            MazeRotation = _MazeRotation;
            PathItemsGroup = _PathItemsGroup;
            MovingItemsGroup = _MovingItemsGroup;
            TrapsReactItemsGroup = _TrapsReactItemsGroup;
            TrapsIncItemsGroup = _TrapsIncItemsGroup;
            TurretsGroup = _TurretsGroup;
            PortalsGroup = _PortalsGroup;
            ShredingerBlocksGroup = _ShredingerBlocksGroup;
            SpringboardItemsGroup = _SpringboardItemsGroup;
            GameTicker = _GameTicker;
            Managers = _Managers;
            LevelStageController = _LevelStageController;
        }
        
        #endregion

        #region api
        
        public event NoArgsHandler Initialized;
        
        public void Init()
        {
            var proceeders = GetProceeders();
            var iBackColChangedProceeders = GetInterfaceOfProceeders<IOnBackgroundColorChanged>(proceeders);
            foreach (var proceeder in iBackColChangedProceeders)
                Background.BackgroundColorChanged += proceeder.OnBackgroundColorChanged;

            var mazeItemGroups = GetInterfaceOfProceeders<IOnLevelStageChanged>(proceeders);
            LevelStageController.RegisterProceeders(mazeItemGroups);
            
            CallInits<IInit>();
            Coroutines.Run(Coroutines.WaitWhile(
                () => m_InitChecks.Any(_Inited => !_Inited), 
                () => Initialized?.Invoke()));
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            LevelStageController.OnLevelStageChanged(_Args);

        }
        
        public void OnCharacterMoveStarted(CharacterMovingEventArgs _Args)
        {
            var proceeders = GetInterfaceOfProceeders<ICharacterMoveStarted>(GetProceeders());
            foreach (var proceeder in proceeders)
                proceeder.OnCharacterMoveStarted(_Args);
        }

        public void OnCharacterMoveContinued(CharacterMovingEventArgs _Args)
        {
            var proceeders = GetInterfaceOfProceeders<ICharacterMoveContinued>(GetProceeders());
            foreach (var proceeder in proceeders)
                proceeder.OnCharacterMoveContinued(_Args);
        }
        
        public void OnCharacterMoveFinished(CharacterMovingEventArgs _Args)
        {
            var proceeders = GetInterfaceOfProceeders<ICharacterMoveFinished>(GetProceeders());
            foreach (var proceeder in proceeders)
                proceeder.OnCharacterMoveFinished(_Args);
        }

        #endregion
        
        #region nonpublic methods
        
        private List<object> GetProceeders()
        {
            var result = new List<object>
                {
                    UI,                         
                    Common,
                    InputConfigurator,
                    Character,
                    Background,
                    MazeRotation,
                    PathItemsGroup,
                    MovingItemsGroup,
                    TrapsReactItemsGroup,
                    TrapsIncItemsGroup,
                    TurretsGroup,
                    PortalsGroup,
                    ShredingerBlocksGroup,
                    SpringboardItemsGroup    
                }.Where(_Proceeder => _Proceeder != null)
                .ToList();
            return result;
        }
        
        private void CallInits<T>() 
            where T : class
        {
            var type = typeof(T);
            var initObjects = GetInterfaceOfProceeders<T>(GetProceeders()).ToList();
            int count = initObjects.Count;
            m_InitChecks.Clear();
            for (int i = 0; i < count; i++)
                m_InitChecks.Add(false);
            for (int i = 0; i < count; i++)
            {
                var i1 = i;
                if (type == typeof(IInit) && initObjects[i] is IInit initObj)
                {
                    initObj.Initialized += () => m_InitChecks[i1] = true;
                    initObj.Init(); 
                }
            }
        }
        
        private static List<T> GetInterfaceOfProceeders<T>(IEnumerable<object> _Proceeders) where T : class
        {
            return _Proceeders.Where(_Proceeder => _Proceeder is T).Cast<T>().ToList();
        }
        
        #endregion
    }
}