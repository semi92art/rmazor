using System;
using System.Linq;
using Common;
using Common.CameraProviders;
using Common.Helpers;
using Common.Providers;
using Common.UI;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Views.Characters;
using RMAZOR.Views.Common;
using RMAZOR.Views.Helpers;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.MazeItemGroups;
using RMAZOR.Views.Rotation;
using RMAZOR.Views.UI;

namespace RMAZOR.Views
{
    public interface IViewGame :
        IInit,
        IOnLevelStageChanged,
        ICharacterMoveStarted,
        ICharacterMoveContinued,
        ICharacterMoveFinished
    {
        ViewSettings                Settings             { get; }
        IViewUI                     UI                   { get; }
        IViewLevelStageController   LevelStageController { get; }
        IViewInputController        InputController      { get; }
        IViewInputCommandsProceeder CommandsProceeder    { get; }
        IViewCharacter              Character            { get; }
        IViewMazeRotation           MazeRotation         { get; }
        IViewMazeItemsGroupSet      MazeItemsGroupSet    { get; }
        IViewMazePathItemsGroup     PathItemsGroup       { get; }
        IManagersGetter             Managers             { get; }
    }
    
    public class ViewGame : InitBase, IViewGame
    {
        #region inject

        public  ViewSettings                  Settings                 { get; }
        public  IContainersGetter             ContainersGetter         { get; }
        public  IViewUI                       UI                       { get; }
        public  IViewLevelStageController     LevelStageController     { get; }
        public  IViewInputController          InputController          { get; }
        public  IViewInputCommandsProceeder   CommandsProceeder        { get; }
        public  IViewCharacter                Character                { get; }
        public  IViewMazeRotation             MazeRotation             { get; }
        public  IViewMazeItemsGroupSet        MazeItemsGroupSet        { get; }
        public  IViewMazePathItemsGroup       PathItemsGroup           { get; }
        public  IManagersGetter               Managers                 { get; }
        
        private IViewMazeCommon               Common                   { get; }
        private IViewBackground               Background               { get; }
        private IViewMazeForeground           Foreground               { get; }
        private IMazeCoordinateConverter      CoordinateConverter      { get; }
        private IColorProvider                ColorProvider            { get; }
        private ICameraProvider               CameraProvider           { get; }
        private IDebugManager                 DebugManager             { get; }
        private IBigDialogViewer              BigDialogViewer          { get; }
        private IViewBetweenLevelTransitioner BetweenLevelTransitioner { get; }

        public ViewGame(
            ViewSettings                  _Settings,
            IContainersGetter             _ContainersGetter,
            IViewUI                       _UI,
            IViewLevelStageController     _LevelStageController,
            IViewInputController          _InputController,
            IViewInputCommandsProceeder   _CommandsProceeder,
            IViewCharacter                _Character,
            IViewMazeCommon               _Common,
            IViewBackground               _Background,
            IViewMazeForeground           _Foreground,
            IViewMazeRotation             _MazeRotation,
            IViewMazeItemsGroupSet        _MazeItemsGroupSet, 
            IViewMazePathItemsGroup       _PathItemsGroup,
            IManagersGetter               _Managers, 
            IMazeCoordinateConverter      _CoordinateConverter,
            IColorProvider                _ColorProvider,
            ICameraProvider               _CameraProvider,
            IDebugManager                 _DebugManager,
            IBigDialogViewer              _BigDialogViewer,
            IViewBetweenLevelTransitioner _BetweenLevelTransitioner)
        {
            Settings                     = _Settings;
            ContainersGetter             = _ContainersGetter;
            UI                           = _UI;
            InputController              = _InputController;
            LevelStageController         = _LevelStageController;
            CommandsProceeder            = _CommandsProceeder;
            Character                    = _Character;
            Common                       = _Common;
            Background                   = _Background;
            Foreground                   = _Foreground;
            MazeRotation                 = _MazeRotation;
            MazeItemsGroupSet            = _MazeItemsGroupSet;
            PathItemsGroup               = _PathItemsGroup;
            Managers                     = _Managers;
            CoordinateConverter          = _CoordinateConverter;
            ColorProvider                = _ColorProvider;
            CameraProvider               = _CameraProvider;
            DebugManager                 = _DebugManager;
            BigDialogViewer              = _BigDialogViewer;
            BetweenLevelTransitioner     = _BetweenLevelTransitioner;
        }
        
        #endregion

        #region nonpublic members

        private object[] m_ProceedersCached;

        #endregion

        #region api

        public override void Init()
        {
            InitDebugManager();
            InitProceeders();
            base.Init();
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
            CoordinateConverter.Init();
            var proceedersToExecuteOnLevelStageChangedBeforeGroups = new object[]
            {
                ContainersGetter,
                Common,
                UI,
                InputController,
                Character,
                MazeRotation,
                PathItemsGroup,
                MazeItemsGroupSet,
                BetweenLevelTransitioner,
                Background,
                Foreground
            };
            var proceedersToExecuteOnLevelStageChangedAfretGroups = new object[]
            {
                CameraProvider,
                Managers
            };
            m_ProceedersCached = proceedersToExecuteOnLevelStageChangedBeforeGroups
                .Concat(proceedersToExecuteOnLevelStageChangedAfretGroups)
                .ToArray();
            LevelStageController
                .RegisterProceeders(GetInterfaceOfProceeders<IOnLevelStageChanged>(
                        proceedersToExecuteOnLevelStageChangedBeforeGroups)
                .Where(_P => _P != null).ToList(), -1);
            LevelStageController
                .RegisterProceeders(GetInterfaceOfProceeders<IOnLevelStageChanged>(
                        proceedersToExecuteOnLevelStageChangedAfretGroups)
                    .Where(_P => _P != null).ToList(), 2);
            ColorProvider.Init();
            foreach (var initObj in GetInterfaceOfProceeders<IInit>())
                initObj?.Init();
            LevelStageController.Init();
            BigDialogViewer.OnClosed = () =>
            {
                CommandsProceeder.RaiseCommand(EInputCommand.UnPauseLevel, null, true);
            };
        }

        private T[] GetInterfaceOfProceeders<T>(object[] _Proceeders = null) where T : class
        {
            return Array.ConvertAll(_Proceeders ?? m_ProceedersCached, _Item => _Item as T);
        }
        
        #endregion
    }
}