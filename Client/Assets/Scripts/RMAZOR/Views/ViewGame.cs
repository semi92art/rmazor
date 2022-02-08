// ReSharper disable ClassNeverInstantiated.Global

using System;
using System.Linq;
using Common;
using Common.CameraProviders;
using Common.Helpers;
using Common.Providers;
using Common.UI;
using Managers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Views.Characters;
using RMAZOR.Views.Common;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.MazeItemGroups;
using RMAZOR.Views.Rotation;
using RMAZOR.Views.UI;
using UnityEngine.Events;

namespace RMAZOR.Views
{
    public interface IViewGame :
        IInit,
        IOnLevelStageChanged,
        ICharacterMoveStarted,
        ICharacterMoveContinued,
        ICharacterMoveFinished
    {
        ViewSettings                   Settings              { get; }
        IViewUI                        UI                    { get; }
        IViewLevelStageController      LevelStageController  { get; }
        IViewInputController           InputController       { get; }
        IViewInputCommandsProceeder    CommandsProceeder     { get; }
        IViewCharacter                 Character             { get; }
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

        public ViewSettings                   Settings              { get; }
        public IContainersGetter              ContainersGetter      { get; }
        public IViewUI                        UI                    { get; }
        public IViewLevelStageController      LevelStageController  { get; }
        public IViewInputController           InputController       { get; }
        public IViewInputCommandsProceeder    CommandsProceeder     { get; }
        public IViewCharacter                 Character             { get; }
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

        private IViewMazeCommon          Common              { get; }
        private IViewMazeBackground      Background          { get; }
        private IViewMazeForeground      Foreground          { get; }
        private IMazeCoordinateConverter CoordinateConverter { get; }
        private IColorProvider           ColorProvider       { get; }
        private ICameraProvider          CameraProvider      { get; }
        private IDebugManager            DebugManager        { get; }
        private IBigDialogViewer         BigDialogViewer     { get; }

        public ViewGame(
            ViewSettings                          _Settings,
            IContainersGetter                     _ContainersGetter,
            IViewUI                               _UI,
            IViewLevelStageController             _LevelStageController,
            IViewInputController                  _InputController,
            IViewInputCommandsProceeder           _CommandsProceeder,
            IViewCharacter                        _Character,
            IViewMazeCommon                       _Common,
            IViewMazeBackground                   _Background,
            IViewMazeForeground                   _Foreground,
            IViewMazeRotation                     _MazeRotation,
            IViewMazePathItemsGroup               _PathItemsGroup,
            IViewMazeMovingItemsGroup             _MovingItemsGroup,
            IViewMazeTrapsReactItemsGroup         _TrapsReactItemsGroup,
            IViewMazeTrapsIncItemsGroup           _TrapsIncItemsGroup,
            IViewMazeTurretsGroup                 _TurretsGroup,
            IViewMazePortalsGroup                 _PortalsGroup,
            IViewMazeShredingerBlocksGroup        _ShredingerBlocksGroup,
            IViewMazeSpringboardItemsGroup        _SpringboardItemsGroup,
            IViewMazeGravityItemsGroup            _GravityItemsGroup,
            IManagersGetter                       _Managers, 
            IMazeCoordinateConverter              _CoordinateConverter,
            IColorProvider                        _ColorProvider,
            ICameraProvider                       _CameraProvider,
            IDebugManager                         _DebugManager,
            IBigDialogViewer _BigDialogViewer)
        {
            Settings               = _Settings;
            ContainersGetter       = _ContainersGetter;
            UI                     = _UI;
            InputController        = _InputController;
            LevelStageController   = _LevelStageController;
            CommandsProceeder      = _CommandsProceeder;
            Character              = _Character;
            Common                 = _Common;
            Background             = _Background;
            Foreground             = _Foreground;
            MazeRotation           = _MazeRotation;
            PathItemsGroup         = _PathItemsGroup;
            MovingItemsGroup       = _MovingItemsGroup;
            TrapsReactItemsGroup   = _TrapsReactItemsGroup;
            TrapsIncItemsGroup     = _TrapsIncItemsGroup;
            TurretsGroup           = _TurretsGroup;
            PortalsGroup           = _PortalsGroup;
            ShredingerBlocksGroup  = _ShredingerBlocksGroup;
            SpringboardItemsGroup  = _SpringboardItemsGroup;
            GravityItemsGroup      = _GravityItemsGroup;
            Managers               = _Managers;
            CoordinateConverter    = _CoordinateConverter;
            ColorProvider          = _ColorProvider;
            CameraProvider         = _CameraProvider;
            DebugManager           = _DebugManager;
            BigDialogViewer = _BigDialogViewer;
        }
        
        #endregion

        #region nonpublic members

        private object[] m_ProceedersCached;

        #endregion

        #region api

        public bool              Initialized { get; private set; }
        public event UnityAction Initialize;
        
        public void Init()
        {
            InitDebugManager();
            InitProceeders();
            Initialize?.Invoke();
            Initialized = true;
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            LevelStageController.OnLevelStageChanged(_Args);
        }
        
        public void OnCharacterMoveStarted(CharacterMovingStartedEventArgs _Args)
        {
            var proceeders = GetInterfaceOfProceeders<ICharacterMoveStarted>();
            foreach (var proceeder in proceeders)
                proceeder?.OnCharacterMoveStarted(_Args);
        }

        public void OnCharacterMoveContinued(CharacterMovingContinuedEventArgs _Args)
        {
            var proceeders = GetInterfaceOfProceeders<ICharacterMoveContinued>();
            foreach (var proceeder in proceeders)
                proceeder?.OnCharacterMoveContinued(_Args);
        }
        
        public void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            var proceeders = GetInterfaceOfProceeders<ICharacterMoveFinished>();
            foreach (var proceeder in proceeders)
                proceeder?.OnCharacterMoveFinished(_Args);
        }

        #endregion
        
        #region nonpublic methods

        private void InitDebugManager()
        {
            if (Managers.RemoteConfigManager.Initialized)
                DebugManager.Init();
            else
                Managers.RemoteConfigManager.Initialize += DebugManager.Init;
        }

        private void InitProceeders()
        {
            m_ProceedersCached = new object[]
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
                Foreground,
                CameraProvider,
                Managers
            };
            ColorProvider.Init();
            CoordinateConverter.Init();
            LevelStageController
                .RegisterProceeders(GetInterfaceOfProceeders<IOnLevelStageChanged>()
                .Where(_P => _P != null));
            foreach (var initObj in GetInterfaceOfProceeders<IInit>())
                initObj?.Init();
            LevelStageController.Init();
            BigDialogViewer.OnClosed = () =>
            {
                CommandsProceeder.RaiseCommand(EInputCommand.UnPauseLevel, null, true);
            };
        }

        private T[] GetInterfaceOfProceeders<T>() where T : class
        {
            return Array.ConvertAll(m_ProceedersCached, _Item => _Item as T);
        }
        
        #endregion
    }
}