using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.Constants;
using Common.Managers.Advertising.AdsProviders;
using mazing.common.Runtime;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Helpers;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders.Additional;
using RMAZOR.UI.Panels;
using RMAZOR.Views.Characters;
using RMAZOR.Views.Common;
using RMAZOR.Views.Common.Additional_Background;
using RMAZOR.Views.Common.ViewLevelStageController;
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
        ILevelsLoader                 LevelsLoader         { get; }
    }
    
    public class ViewGame : InitBase, IViewGame
    {
        #region nonpublic members

        private float    m_SecondsInLevelWithoutInputActions;
        private object[] m_ProceedersCached; 

        #endregion
        
        #region inject

        public  ViewSettings                   Settings                  { get; }
        public  IContainersGetter              ContainersGetter          { get; }
        public  IViewUI                        UI                        { get; }
        public  IViewLevelStageController      LevelStageController      { get; }
        public  IViewInputCommandsProceeder    CommandsProceeder         { get; }
        public  IViewInputTouchProceeder       TouchProceeder            { get; }
        public  IViewCharacter                 Character                 { get; }
        public  IViewMazeRotation              MazeRotation              { get; }
        public  IViewMazeItemsGroupSet         MazeItemsGroupSet         { get; }
        public  IViewMazePathItemsGroup        PathItemsGroup            { get; }
        public  IManagersGetter                Managers                  { get; }
        public  IViewBackground                Background                { get; }
        public  IViewMazeAdditionalBackground  AdditionalBackground      { get; }
        private IViewFullscreenTransitioner    FullscreenTransitioner    { get; }
        public  ICameraProvider                CameraProvider            { get; }
        public  ILevelsLoader                  LevelsLoader              { get; }
        private IDialogPanelsSet               DialogPanelsSet           { get; }
        private IViewIdleAdsPlayer             IdleAdsPlayer             { get; }
        private IViewGameIdleQuitter           IdleQuitter               { get; }
        private IViewMobileNotificationsSender MobileNotificationsSender { get; }

        private IViewMazeCommon             Common                 { get; }
        private IViewMazeForeground         Foreground             { get; }
        private ICoordinateConverter        CoordinateConverter    { get; }
        private IColorProvider              ColorProvider          { get; }
        private IRendererAppearTransitioner AppearTransitioner     { get; }
        private IAdsProvidersSet            AdsProvidersSet        { get; }
        private ICommonTicker               CommonTicker           { get; }

        private ViewGame(
            ViewSettings                   _Settings,
            IContainersGetter              _ContainersGetter,
            IViewUI                        _UI,
            IViewLevelStageController      _LevelStageController,
            IViewInputCommandsProceeder    _CommandsProceeder,
            IViewInputTouchProceeder       _TouchProceeder,
            IViewCharacter                 _Character,
            IViewMazeCommon                _Common,
            IViewBackground                _Background,
            IViewMazeForeground            _Foreground,
            IViewMazeRotation              _MazeRotation,
            IViewMazeItemsGroupSet         _MazeItemsGroupSet,
            IViewMazePathItemsGroup        _PathItemsGroup,
            IManagersGetter                _Managers,
            ICoordinateConverter           _CoordinateConverter,
            IColorProvider                 _ColorProvider,
            ICameraProvider                _CameraProvider,
            IRendererAppearTransitioner    _AppearTransitioner,
            IViewMazeAdditionalBackground  _AdditionalBackground,
            IViewFullscreenTransitioner    _FullscreenTransitioner,
            IAdsProvidersSet               _AdsProvidersSet,
            ICommonTicker                  _CommonTicker,
            ILevelsLoader                  _LevelsLoader,
            IDialogPanelsSet               _DialogPanelsSet,
            IViewIdleAdsPlayer             _IdleAdsPlayer,
            IViewGameIdleQuitter           _IdleQuitter,
            IViewMobileNotificationsSender _MobileNotificationsSender)
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
            AppearTransitioner           = _AppearTransitioner;
            AdditionalBackground         = _AdditionalBackground;
            FullscreenTransitioner       = _FullscreenTransitioner;
            AdsProvidersSet              = _AdsProvidersSet;
            CommonTicker                 = _CommonTicker;
            LevelsLoader                 = _LevelsLoader;
            DialogPanelsSet              = _DialogPanelsSet;
            IdleAdsPlayer                = _IdleAdsPlayer;
            IdleQuitter                  = _IdleQuitter;
            MobileNotificationsSender    = _MobileNotificationsSender;
        }
        
        #endregion

        #region api

        public override void Init()
        {
            if (Initialized)
                return;
            CommonDataRmazor.LevelsInGroupArray = Settings.LevelsInGroup; 
            InitAdsProvidersMuteAudioAction();
            CommonTicker.Register(this);
            RegisterLevelsStagingProceeders();
            InitProceeders();
            TouchProceeder.ProceedRotation = Application.isEditor;
            CameraProvider.Follow = ContainersGetter.GetContainer(ContainerNamesMazor.Character);
            CommonUtils.DoOnInitializedEx(DialogPanelsSet, () => Cor.Run(LoadMainMenuOnStart()));
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
        
        private IEnumerator LoadMainMenuOnStart()
        {
            if (!CameraProvider.Initialized)
                yield return null;
            CommandsProceeder.RaiseCommand(EInputCommand.MainMenuPanel, null);
        }

        private void InitAdsProvidersMuteAudioAction()
        {
            foreach (var adsProvider in AdsProvidersSet.GetProviders())
                adsProvider.MuteAudio = MuteAudio;
        }

        private void MuteAudio(bool _Mute)
        {
            var audioManager = Managers.AudioManager;
            var audioClipTypes = Enum
                .GetValues(typeof(EAudioClipType))
                .Cast<EAudioClipType>()
                .ToList();
            foreach (var audioClipType in audioClipTypes)
            {
                if (_Mute) audioManager.MuteAudio(audioClipType);
                else       audioManager.UnmuteAudio(audioClipType);
            }
        }
        
        private void InitProceeders()
        {
            CoordinateConverter.GetContainer = ContainersGetter.GetContainer;
            CoordinateConverter.Init();
            CommonUtils.DoOnInitializedEx(
                Managers.RemoteConfigManager, 
                () => Managers.AnalyticsManager.Init());
            // RegisterLevelsStagingProceeders();
            m_ProceedersCached = GetOrderedViewGameProceedersToExecute().Values.ToArray();
            ColorProvider            .Init();
            CameraProvider           .Init();
            IdleAdsPlayer            .Init();
            IdleQuitter              .Init();
            MobileNotificationsSender.Init();
            foreach (var initObj in GetInterfaceOfProceeders<IInit>())
                initObj?.Init();
            LevelStageController.Init();
        }

        private void RegisterLevelsStagingProceeders()
        {
            var allProceeders = GetOrderedViewGameProceedersToExecute();
            foreach ((int key, var value) in allProceeders)
            {
                if (value is IOnLevelStageChanged onLevelStageChanged)
                    LevelStageController.RegisterProceeder(onLevelStageChanged, key);
            }
        }
        
        private SortedDictionary<int, object> GetOrderedViewGameProceedersToExecute()
        {
            return new SortedDictionary<int, object>
            {
                {-13, ContainersGetter},
                {-12, Common},
                {-11, UI},
                {-10, TouchProceeder},
                {-09, CommandsProceeder},
                {-08, Character},
                {-07, MazeRotation},
                {-06, PathItemsGroup},
                {-05, MazeItemsGroupSet},
                {-04, AppearTransitioner},
                {-03, FullscreenTransitioner},
                {-02, Foreground},
                {-01, Background},
                
                {+01, AdditionalBackground},
                {+02, CameraProvider},
                {+03, Managers},
            };
        }
        
        private T[] GetInterfaceOfProceeders<T>() where T : class
        {
            return Array.ConvertAll(m_ProceedersCached, _Item => _Item as T);
        }
        
        #endregion
    }
}