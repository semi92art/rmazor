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
using UnityEngine.Events;
namespace Games.RazorMaze.Views
{
    public class ViewGame : IViewGame
    {
        #region inject
        
        public IContainersGetter              ContainersGetter      { get; }
        public IViewUI                        UI                    { get; }
        public IViewLevelStageController      LevelStageController  { get; }
        public IViewInputConfigurator         InputConfigurator     { get; }
        public IViewCharacter                 Character             { get; }
        public IViewMazeCommon                Common                { get; }
        public IViewMazeBackground            Background            { get; }
        public IViewMazeRotation              MazeRotation          { get; }
        public IViewMazePathItemsGroup        PathItemsGroup        { get; }
        public IViewMazeMovingItemsGroup      MovingItemsGroup      { get; }
        public IViewMazeTrapsReactItemsGroup  TrapsReactItemsGroup  { get; }
        public IViewMazeTrapsIncItemsGroup    TrapsIncItemsGroup    { get; }
        public IViewMazeTurretsGroup          TurretsGroup          { get; }
        public IViewMazePortalsGroup          PortalsGroup          { get; }
        public IViewMazeShredingerBlocksGroup ShredingerBlocksGroup { get; }
        public IViewMazeSpringboardItemsGroup SpringboardItemsGroup { get; }
        public IViewMazeGravityItemsGroup     GravityItemsGroup     { get; }

        private IGameTicker              GameTicker          { get; }
        private IManagersGetter          Managers            { get; }
        private IMazeCoordinateConverter CoordinateConverter { get; }
        private IDebugManager            DebugManager        { get; }
        private IColorProvider           ColorProvider       { get; }

        public ViewGame(
            IContainersGetter _ContainersGetter,
            IViewUI _UI,
            IViewLevelStageController _LevelStageController,
            IViewInputConfigurator _InputConfigurator,
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
            IViewMazeGravityItemsGroup _GravityItemsGroup,
            IGameTicker _GameTicker,
            IManagersGetter _Managers, 
            IMazeCoordinateConverter _CoordinateConverter,
            IDebugManager _DebugManager,
            IColorProvider _ColorProvider)
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
            GravityItemsGroup = _GravityItemsGroup;
            GameTicker = _GameTicker;
            Managers = _Managers;
            CoordinateConverter = _CoordinateConverter;
            DebugManager = _DebugManager;
            ColorProvider = _ColorProvider;
            LevelStageController = _LevelStageController;
        }
        
        #endregion

        #region api
        
        public event UnityAction Initialized;
        
        public void Init()
        {
            ColorProvider.Init();
            DebugManager.Init();
            CoordinateConverter.Init(
                MazeCoordinateConverter.DefaultLeftOffset, 
                MazeCoordinateConverter.DefaultRightOffset,
                MazeCoordinateConverter.DefaultBottomOffset, 
                MazeCoordinateConverter.DefaultTopOffset);
            var proceeders = GetProceeders();
            var iBackColChangedProceeders = GetInterfaceOfProceeders<IOnBackgroundColorChanged>(proceeders);
            foreach (var proceeder in iBackColChangedProceeders)
                Background.BackgroundColorChanged += proceeder.OnBackgroundColorChanged;

            var onLevelStageChangeds = GetInterfaceOfProceeders<IOnLevelStageChanged>(proceeders);
            LevelStageController.RegisterProceeders(onLevelStageChangeds);

            GetInterfaceOfProceeders<IInit>(GetProceeders())
                .ToList()
                .ForEach(_InitObj => _InitObj.Init());
            Initialized?.Invoke();
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
                    Common,
                    UI,                         
                    InputConfigurator,
                    Character,
                    MazeRotation,
                    Background,
                    PathItemsGroup,
                    MovingItemsGroup,
                    TrapsReactItemsGroup,
                    TrapsIncItemsGroup,
                    TurretsGroup,
                    PortalsGroup,
                    ShredingerBlocksGroup,
                    SpringboardItemsGroup,
                    GravityItemsGroup
                }.Where(_Proceeder => _Proceeder != null)
                .ToList();
            return result;
        }

        private static List<T> GetInterfaceOfProceeders<T>(IEnumerable<object> _Proceeders) where T : class
        {
            return _Proceeders.Where(_Proceeder => _Proceeder is T).Cast<T>().ToList();
        }
        
        #endregion
    }
}