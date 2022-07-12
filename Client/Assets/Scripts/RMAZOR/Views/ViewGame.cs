using System;
using System.Linq;
using Common;
using Common.CameraProviders;
using Common.CameraProviders.Camera_Effects_Props;
using Common.Constants;
using Common.Helpers;
using Common.Managers.Notifications;
using Common.Providers;
using Common.UI;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders.Additional;
using RMAZOR.Views.Characters;
using RMAZOR.Views.Common;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.MazeItemGroups;
using RMAZOR.Views.Rotation;
using RMAZOR.Views.UI;
using UnityEngine;

namespace RMAZOR.Views
{
    public interface IViewGame :
        IInit,
        IOnLevelStageChanged,
        ICharacterMoveStarted,
        ICharacterMoveContinued,
        ICharacterMoveFinished
    {
        ViewSettings                  Settings             { get; }
        IViewUI                       UI                   { get; }
        IViewLevelStageController     LevelStageController { get; }
        IViewInputTouchProceeder      TouchProceeder       { get; }
        IViewInputCommandsProceeder   CommandsProceeder    { get; }
        IViewCharacter                Character            { get; }
        IViewMazeRotation             MazeRotation         { get; }
        IViewMazeItemsGroupSet        MazeItemsGroupSet    { get; }
        IViewMazePathItemsGroup       PathItemsGroup       { get; }
        IManagersGetter               Managers             { get; }
        IViewBackground               Background           { get; }
        IViewMazeAdditionalBackground AdditionalBackground { get; }
    }
    
    public class ViewGame : InitBase, IViewGame
    {
        #region inject

        private IRemotePropertiesRmazor       RemotePropertiesRmazor { get; }
        public  ViewSettings                  Settings               { get; }
        public  IContainersGetter             ContainersGetter       { get; }
        public  IViewUI                       UI                     { get; }
        public  IViewLevelStageController     LevelStageController   { get; }
        public  IViewInputCommandsProceeder   CommandsProceeder      { get; }
        public  IViewInputTouchProceeder      TouchProceeder         { get; }
        public  IViewCharacter                Character              { get; }
        public  IViewMazeRotation             MazeRotation           { get; }
        public  IViewMazeItemsGroupSet        MazeItemsGroupSet      { get; }
        public  IViewMazePathItemsGroup       PathItemsGroup         { get; }
        public  IManagersGetter               Managers               { get; }
        public  IViewBackground               Background             { get; }
        public  IViewMazeAdditionalBackground AdditionalBackground   { get; }

        private IViewMazeCommon             Common                 { get; }
        private IViewMazeForeground         Foreground             { get; }
        private ICoordinateConverter  CoordinateConverter    { get; }
        private IColorProvider              ColorProvider          { get; }
        private ICameraProvider             CameraProvider         { get; }
        private IBigDialogViewer            BigDialogViewer        { get; }
        private IViewFullscreenTransitioner FullscreenTransitioner { get; }

        private ViewGame(
            IRemotePropertiesRmazor       _RemotePropertiesRmazor,
            ViewSettings                  _Settings,
            IContainersGetter             _ContainersGetter,
            IViewUI                       _UI,
            IViewLevelStageController     _LevelStageController,
            IViewInputCommandsProceeder   _CommandsProceeder,
            IViewInputTouchProceeder      _TouchProceeder,
            IViewCharacter                _Character,
            IViewMazeCommon               _Common,
            IViewBackground               _Background,
            IViewMazeForeground           _Foreground,
            IViewMazeRotation             _MazeRotation,
            IViewMazeItemsGroupSet        _MazeItemsGroupSet,
            IViewMazePathItemsGroup       _PathItemsGroup,
            IManagersGetter               _Managers,
            ICoordinateConverter    _CoordinateConverter,
            IColorProvider                _ColorProvider,
            ICameraProvider               _CameraProvider,
            IBigDialogViewer              _BigDialogViewer,
            IViewFullscreenTransitioner   _FullscreenTransitioner,
            IViewMazeAdditionalBackground _AdditionalBackground)
        {
            RemotePropertiesRmazor       = _RemotePropertiesRmazor;
            Settings                     = _Settings;
            ContainersGetter             = _ContainersGetter;
            UI                           = _UI;
            LevelStageController         = _LevelStageController;
            CommandsProceeder            = _CommandsProceeder;
            TouchProceeder               = _TouchProceeder;
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
            BigDialogViewer              = _BigDialogViewer;
            FullscreenTransitioner       = _FullscreenTransitioner;
            AdditionalBackground         = _AdditionalBackground;
        }
        
        #endregion

        #region nonpublic members

        private object[] m_ProceedersCached;

        #endregion

        #region api

        public override void Init()
        {
            CameraProvider.GetMazeBounds = CoordinateConverter.GetMazeBounds;
            CameraProvider.GetConverterScale = () => CoordinateConverter.Scale;
            CameraProvider.Init();
            CameraProvider.UpdateState();
            InitProceeders();
            SendNotificationsOnInit();
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

        private void InitProceeders()
        {
            CoordinateConverter.GetContainer = ContainersGetter.GetContainer;
            CoordinateConverter.Init();
            CommonUtils.DoOnInitializedEx(
                Managers.RemoteConfigManager, 
                () => Managers.AnalyticsManager.Init());
            var proceedersToExecuteOnLevelStageChangedBeforeGroups = new object[]
            {
                ContainersGetter,
                Common,
                UI,
                TouchProceeder,
                CommandsProceeder,
                Character,
                MazeRotation,
                PathItemsGroup,
                MazeItemsGroupSet,
                FullscreenTransitioner,
                Foreground,
                Background
            };
            var proceedersToExecuteOnLevelStageChangedAfretGroups = new object[]
            {
                AdditionalBackground,
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
            TouchProceeder.ProceedRotation = Application.isEditor;
            CameraProvider.Follow = ContainersGetter.GetContainer(ContainerNames.Character);
        }

        private T[] GetInterfaceOfProceeders<T>(object[] _Proceeders = null) where T : class
        {
            return Array.ConvertAll(_Proceeders ?? m_ProceedersCached, _Item => _Item as T);
        }

        private void SendNotificationsOnInit()
        {
            if (!CommonData.Release)
                return;
            if (RemotePropertiesRmazor.Nofifications == null)
                return;
            var notMan = Managers.NotificationsManager;
            var notifications = RemotePropertiesRmazor.Nofifications;
            notMan.OperatingMode = Application.platform == RuntimePlatform.Android
                ? ENotificationsOperatingMode.QueueClearAndReschedule
                : ENotificationsOperatingMode.NoQueue;
            if (notMan.OperatingMode.HasFlag(ENotificationsOperatingMode.RescheduleAfterClearing))
                notMan.LastNotificationsCountToReschedule = notifications.Count;
            notMan.ClearAllNotifications();
            foreach (var notification in notifications)
            {
                notMan.SendNotification(
                    notification.Message, 
                    string.Empty, 
                    DateTime.Now.Add(notification.TimeSpan),
                    _Reschedule: Application.platform == RuntimePlatform.Android,
                    _SmallIcon: "main_icon");
            }
        }
        
        #endregion
    }
}