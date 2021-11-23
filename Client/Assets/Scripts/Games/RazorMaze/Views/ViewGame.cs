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
using UnityEngine.Events;
namespace Games.RazorMaze.Views
{
    public interface IViewGame :
        IInit,
        IOnLevelStageChanged,
        ICharacterMoveStarted,
        ICharacterMoveContinued,
        ICharacterMoveFinished
    {
        IContainersGetter              ContainersGetter      { get; }
        IViewUI                        UI                    { get; }
        IViewLevelStageController      LevelStageController  { get; }
        IViewInputCommandsProceeder    CommandsProceeder     { get; }
        IViewCharacter                 Character             { get; }
        IViewMazeCommon                Common                { get; }
        IViewMazeBackground            Background            { get; }
        IViewMazeRotation              MazeRotation          { get; }
        IViewMazePathItemsGroup        PathItemsGroup        { get; }
        IViewMazeMovingItemsGroup      MovingItemsGroup      { get; }
        IViewMazeTrapsReactItemsGroup  TrapsReactItemsGroup  { get; }
        IViewMazeTrapsIncItemsGroup    TrapsIncItemsGroup    { get; }
        IViewMazeTurretsGroup          TurretsGroup          { get; }
        IViewMazePortalsGroup          PortalsGroup          { get; }
        IViewMazeShredingerBlocksGroup ShredingerBlocksGroup { get; }
        IViewMazeSpringboardItemsGroup SpringboardItemsGroup { get; }
        IViewMazeGravityItemsGroup     GravityItemsGroup     { get; }
        IManagersGetter                Managers              { get; }
    }
    
    public class ViewGame : IViewGame
    {
        #region inject
        
        public IContainersGetter              ContainersGetter      { get; }
        public IViewUI                        UI                    { get; }
        public IViewLevelStageController      LevelStageController  { get; }
        public IViewInputController           InputController       { get; }
        public IViewInputCommandsProceeder    CommandsProceeder     { get; }
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
        public IManagersGetter                Managers              { get; }

        private IMazeCoordinateConverter CoordinateConverter { get; }
        private IColorProvider           ColorProvider       { get; }
        private ICameraProvider          CameraProvider      { get; }

        public ViewGame(
            IContainersGetter _ContainersGetter,
            IViewUI _UI,
            IViewLevelStageController _LevelStageController,
            IViewInputController _InputController,
            IViewInputCommandsProceeder _CommandsProceeder,
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
            IManagersGetter _Managers, 
            IMazeCoordinateConverter _CoordinateConverter,
            IColorProvider _ColorProvider,
            ICameraProvider _CameraProvider)
        {
            ContainersGetter = _ContainersGetter;
            UI = _UI;
            InputController = _InputController;
            LevelStageController = _LevelStageController;
            CommandsProceeder = _CommandsProceeder;
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
            Managers = _Managers;
            CoordinateConverter = _CoordinateConverter;
            ColorProvider = _ColorProvider;
            CameraProvider = _CameraProvider;
        }
        
        #endregion

        #region api
        
        public event UnityAction Initialized;
        
        public void Init()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Managers.DebugManager.Init();
#endif
            Managers.AudioManager.Init();
            ColorProvider.Init();
            CoordinateConverter.Init();
            LevelStageController.RegisterProceeders(GetInterfaceOfProceeders<IOnLevelStageChanged>());
            GetInterfaceOfProceeders<IInit>().ForEach(_InitObj => _InitObj.Init());
            LevelStageController.Init();
            Initialized?.Invoke();
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            LevelStageController.OnLevelStageChanged(_Args);
        }
        
        public void OnCharacterMoveStarted(CharacterMovingEventArgs _Args)
        {
            var proceeders = GetInterfaceOfProceeders<ICharacterMoveStarted>();
            foreach (var proceeder in proceeders)
                proceeder.OnCharacterMoveStarted(_Args);
        }

        public void OnCharacterMoveContinued(CharacterMovingEventArgs _Args)
        {
            var proceeders = GetInterfaceOfProceeders<ICharacterMoveContinued>();
            foreach (var proceeder in proceeders)
                proceeder.OnCharacterMoveContinued(_Args);
        }
        
        public void OnCharacterMoveFinished(CharacterMovingEventArgs _Args)
        {
            var proceeders = GetInterfaceOfProceeders<ICharacterMoveFinished>();
            foreach (var proceeder in proceeders)
                proceeder.OnCharacterMoveFinished(_Args);
        }

        #endregion
        
        #region nonpublic methods

        private List<T> GetInterfaceOfProceeders<T>() where T : class
        {
            var proceeders = new List<object>
                {
                    ContainersGetter,
                    Common,
                    UI,                         
                    InputController,
                    Character,
                    MazeRotation,
                    PathItemsGroup,
                    MovingItemsGroup,
                    TrapsReactItemsGroup,
                    TrapsIncItemsGroup,
                    TurretsGroup,
                    PortalsGroup,
                    ShredingerBlocksGroup,
                    SpringboardItemsGroup,
                    GravityItemsGroup,
                    Background,
                    CameraProvider
                }.Where(_Proceeder => _Proceeder != null);
            return proceeders.Where(_Proceeder => _Proceeder is T).Cast<T>().ToList();
        }
        
        #endregion
    }
}