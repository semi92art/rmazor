using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.CameraProviders;
using Common.Helpers;
using Common.Providers;
using Common.Ticker;
using Common.UI;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders.Additional;
using RMAZOR.Views.Characters;
using RMAZOR.Views.Common;
using RMAZOR.Views.Helpers;
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
    
    public class ViewGame : InitBase, IViewGame, IApplicationFocus
    {
        #region inject

        public ViewSettings                  Settings             { get; }
        public IContainersGetter             ContainersGetter     { get; }
        public IViewUI                       UI                   { get; }
        public IViewLevelStageController     LevelStageController { get; }
        public IViewInputCommandsProceeder   CommandsProceeder    { get; }
        public IViewInputTouchProceeder      TouchProceeder       { get; }
        public IViewCharacter                Character            { get; }
        public IViewMazeRotation             MazeRotation         { get; }
        public IViewMazeItemsGroupSet        MazeItemsGroupSet    { get; }
        public IViewMazePathItemsGroup       PathItemsGroup       { get; }
        public IManagersGetter               Managers             { get; }
        public IViewBackground               Background           { get; }
        public IViewMazeAdditionalBackground AdditionalBackground { get; }
        
        private IViewMazeCommon               Common                   { get; }
        private IViewMazeForeground           Foreground               { get; }
        private IMazeCoordinateConverter      CoordinateConverter      { get; }
        private IColorProvider                ColorProvider            { get; }
        private ICameraProvider               CameraProvider           { get; }
        private IBigDialogViewer              BigDialogViewer          { get; }
        private IViewBetweenLevelTransitioner BetweenLevelTransitioner { get; }
        private IViewGameTicker               ViewGameTicker           { get; }

        public ViewGame(
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
            IMazeCoordinateConverter      _CoordinateConverter,
            IColorProvider                _ColorProvider,
            ICameraProvider               _CameraProvider,
            IBigDialogViewer              _BigDialogViewer,
            IViewBetweenLevelTransitioner _BetweenLevelTransitioner,
            IViewMazeAdditionalBackground _AdditionalBackground,
            IViewGameTicker               _ViewGameTicker)
        {
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
            BetweenLevelTransitioner     = _BetweenLevelTransitioner;
            AdditionalBackground         = _AdditionalBackground;
            ViewGameTicker               = _ViewGameTicker;
        }
        
        #endregion

        #region nonpublic members

        private object[] m_ProceedersCached;

        #endregion

        #region api

        public override void Init()
        {
            ViewGameTicker.Register(this);
            InitProceeders();
            SendNotifications();
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
        
        public void OnApplicationFocus(bool _Focus)
        {
            if (!_Focus)
                return;
            Cor.Run(Cor.Delay(1f, SendNotifications));
        }

        #endregion
        
        #region nonpublic methods

        private void InitProceeders()
        {
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
                BetweenLevelTransitioner,
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
        }

        private T[] GetInterfaceOfProceeders<T>(object[] _Proceeders = null) where T : class
        {
            return Array.ConvertAll(_Proceeders ?? m_ProceedersCached, _Item => _Item as T);
        }

        private void SendNotifications()
        {
            if (!CommonData.Release)
                return;
            var locMan = Managers.LocalizationManager;
            var notMan = Managers.NotificationsManager;
            string title = Application.productName;
            var dict = new Dictionary<string, DateTime>
            {
                {locMan.GetTranslation("notification_1"), DateTime.Now.AddDays(1)},
                {locMan.GetTranslation("notification_2"), DateTime.Now.AddDays(2)}
            };
            foreach ((string body, var dateTime) in dict)
            {
                notMan.SendNotification(
                    title, 
                    body, 
                    dateTime,
                    _Reschedule: true,
                    _SmallIcon: "main_icon",
                    _LargeIcon: "main_icon_large");
            }
        }
        
        #endregion
    }
}