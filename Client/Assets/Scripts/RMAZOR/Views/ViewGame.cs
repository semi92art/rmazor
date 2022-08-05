using System;
using System.Linq;
using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Enums;
using Common.Helpers;
using Common.Managers.Advertising.AdsProviders;
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
        ICameraProvider               CameraProvider       { get; }
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
        private IViewFullscreenTransitioner   FullscreenTransitioner { get; }
        public  ICameraProvider               CameraProvider         { get; }

        private IViewMazeCommon             Common              { get; }
        private IViewMazeForeground         Foreground          { get; }
        private ICoordinateConverter        CoordinateConverter { get; }
        private IColorProvider              ColorProvider       { get; }
        private IFullscreenDialogViewer            FullscreenDialogViewer     { get; }
        private IRendererAppearTransitioner AppearTransitioner  { get; }
        private IAdsProvidersSet            AdsProvidersSet     { get; }

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
            ICoordinateConverter          _CoordinateConverter,
            IColorProvider                _ColorProvider,
            ICameraProvider               _CameraProvider,
            IFullscreenDialogViewer              _FullscreenDialogViewer,
            IRendererAppearTransitioner   _AppearTransitioner,
            IViewMazeAdditionalBackground _AdditionalBackground,
            IViewFullscreenTransitioner   _FullscreenTransitioner,
            IAdsProvidersSet              _AdsProvidersSet)
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
            FullscreenDialogViewer              = _FullscreenDialogViewer;
            AppearTransitioner           = _AppearTransitioner;
            AdditionalBackground         = _AdditionalBackground;
            FullscreenTransitioner       = _FullscreenTransitioner;
            AdsProvidersSet              = _AdsProvidersSet;
        }
        
        #endregion

        #region nonpublic members

        private object[] m_ProceedersCached;

        #endregion

        #region api

        public override void Init()
        {
            InitAdsProvidersMuteAudioAction();
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

        private void InitAdsProvidersMuteAudioAction()
        {
            foreach (var adsProvider in AdsProvidersSet.GetProviders())
                adsProvider.MuteAudio = MuteAudio;
        }

        private void MuteAudio(bool _Mute)
        {
            var audioManager = Managers.AudioManager;
            if (_Mute)
            {
                audioManager.MuteAudio(EAudioClipType.Music);
                audioManager.MuteAudio(EAudioClipType.UiSound);
                audioManager.MuteAudio(EAudioClipType.GameSound);
            }
            else
            {
                audioManager.UnmuteAudio(EAudioClipType.Music);
                audioManager.UnmuteAudio(EAudioClipType.UiSound);
                audioManager.UnmuteAudio(EAudioClipType.GameSound);
            }
        }
        
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
                AppearTransitioner,
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
            FullscreenDialogViewer.OnClosed = () =>
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
            if (RemotePropertiesRmazor.Notifications == null)
                return;
            var notMan = Managers.NotificationsManager;
            var notifications = RemotePropertiesRmazor.Notifications;
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