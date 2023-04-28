using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Constants;
using Common.Managers.Advertising.AdsProviders;
using mazing.common.Runtime;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Helpers;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders.Additional;
using RMAZOR.Settings;
using RMAZOR.UI.Panels;
using RMAZOR.UI.Utils;
using RMAZOR.Views.Characters;
using RMAZOR.Views.Common;
using RMAZOR.Views.Common.Additional_Background;
using RMAZOR.Views.Common.ViewLevelStageController;
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using RMAZOR.Views.Controllers;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.MazeItemGroups;
using RMAZOR.Views.Rotation;
using RMAZOR.Views.UI;
using RMAZOR.Views.Utils;
using UnityEngine;
using UnityEngine.Events;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.Views
{
    public interface IViewGame :
        IInit,
        IOnLevelStageChanged,
        ICharacterMoveStarted,
        ICharacterMoveContinued,
        ICharacterMoveFinished,
        IMazeRotationStarted,
        IMazeRotationFinished
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
        ILevelsLoader                 LevelsLoader         { get; }
        ICameraProvider               CameraProvider       { get; }
    }
    
    public class ViewGame : InitBase, IViewGame
    {
        #region nonpublic members

        private float    m_SecondsInLevelWithoutInputActions;
        private object[] m_ProceedersCached; 

        #endregion
        
        #region inject

        public  ViewSettings                   Settings                  { get; }
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
        public  ICameraProvider                CameraProvider            { get; }
        public  ILevelsLoader                  LevelsLoader              { get; }
        
        private IViewFullscreenTransitioner    FullscreenTransitioner    { get; }
        private IDialogPanelsSet               DialogPanelsSet           { get; }
        private IViewIdleAdsPlayer             IdleAdsPlayer             { get; }
        private IViewGameIdleQuitter           IdleQuitter               { get; }
        private IViewMobileNotificationsSender MobileNotificationsSender { get; }
        private IViewInputCommandsRecorder     InputCommandsRecorder     { get; }
        private IViewInputCommandsReplayer       InputCommandsReplayer       { get; }
        private IUITicker                      UiTicker                  { get; }
        private IRewardCounter                 RewardCounter             { get; }
        private IRemoteConfigManager           RemoteConfigManager       { get; }
        private IViewLevelStageSwitcher        LevelStageSwitcher        { get; }
        private IRetroModeSetting              RetroModeSetting          { get; }
        private IModelGame                     Model                     { get; }
        private IContainersGetter              ContainersGetter          { get; }
        private IViewMazeCommon                Common                    { get; }
        private ICoordinateConverter           CoordinateConverter       { get; }
        private IColorProvider                 ColorProvider             { get; }
        private IRendererAppearTransitioner    AppearTransitioner        { get; }
        private IAdsProvidersSet               AdsProvidersSet           { get; }
        private ICommonTicker                  CommonTicker              { get; }

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
            IViewMobileNotificationsSender _MobileNotificationsSender,
            IViewInputCommandsRecorder     _InputCommandsRecorder,
            IViewInputCommandsReplayer     _InputCommandsReplayer,
            IUITicker                      _UiTicker,
            IRewardCounter                 _RewardCounter,
            IRemoteConfigManager           _RemoteConfigManager,
            IViewLevelStageSwitcher        _LevelStageSwitcher,
            IRetroModeSetting              _RetroModeSetting,
            IModelGame                     _Model)
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
            InputCommandsRecorder        = _InputCommandsRecorder;
            InputCommandsReplayer        = _InputCommandsReplayer;
            UiTicker                     = _UiTicker;
            RewardCounter                = _RewardCounter;
            RemoteConfigManager          = _RemoteConfigManager;
            LevelStageSwitcher           = _LevelStageSwitcher;
            RetroModeSetting             = _RetroModeSetting;
            Model                        = _Model;
        }
        
        #endregion

        #region api

        public override void Init()
        {
            if (Initialized)
                return;
            RetroModeSetting.ValueSet += OnRetroModeValueSet;
            CommonDataRmazor.LevelsInGroupArray = new[] {3, 3, 3};
            InitAdsProvidersMuteAudioAction();
            CommonTicker.Register(this);
            RegisterLevelsStagingProceeders();
            InitProceeders();
            TouchProceeder.ProceedRotation = Application.isEditor;
            CameraProvider.Follow = ContainersGetter.GetContainer(ContainerNamesMazor.Character);
            CommonUtils.DoOnInitializedEx(DialogPanelsSet, () =>
            {
                bool mainGameModeLoadedAtLeastOnce = SaveUtils.GetValue(SaveKeysRmazor.MainGameModeLoadedAtLeastOnce);
                if (Settings.loadMainGameModeOnStart && !mainGameModeLoadedAtLeastOnce)
                {
                    LoadLastMainLevel(null);
                    SaveUtils.PutValue(SaveKeysRmazor.MainGameModeLoadedAtLeastOnce, true);
                }
                else
                {
                    Cor.Run(LoadMainMenuOnStart());
                }
            });
            base.Init();
        }

        private void OnRetroModeValueSet(bool _Value)
        {
            if (Model.LevelStaging.LevelStage == ELevelStage.None)
                return;
            string gameMode = ViewLevelStageSwitcherUtils.GetGameMode(Model.LevelStaging.Arguments);
            if (gameMode == ParameterGameModeRandom)
                return;
            var defaultClip = GetAudioClipArgsLevelTheme(false);
            var retroClip   = GetAudioClipArgsLevelTheme(true);
            Managers.AudioManager.PlayClip(_Value ? retroClip : defaultClip);
            Managers.AudioManager.PauseClip(_Value ? defaultClip : retroClip);
        }
        
        private static AudioClipArgs GetAudioClipArgsLevelTheme(bool _Retro)
        {
            string prefabName = _Retro ? "synthwave_theme" : "main_theme";
            float volume = _Retro ? 0.1f : 0.25f;
            return new AudioClipArgs(prefabName, EAudioClipType.Music, volume, true);
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
        
        public void OnMazeRotationStarted(MazeRotationEventArgs _Args)
        {
            var proceeders = GetInterfaceOfProceeders<IMazeRotationStarted>();
            foreach (var proceeder in proceeders)
                proceeder?.OnMazeRotationStarted(_Args);
        }
        
        public void OnMazeRotationFinished(MazeRotationEventArgs _Args)
        {
            var proceeders = GetInterfaceOfProceeders<IMazeRotationFinished>();
            foreach (var proceeder in proceeders)
                proceeder?.OnMazeRotationFinished(_Args);
        }

        #endregion
        
        #region nonpublic methods
        
        private IEnumerator LoadMainMenuOnStart()
        {
            if (!CameraProvider.Initialized)
                yield return null;
            var args = new Dictionary<string, object>
            {
                {KeyAdditionalCameraEffectAction, 
                    (UnityAction<bool, float>)AdditionalCameraEffectsActionDefaultCoroutine}
            };
            CommandsProceeder.RaiseCommand(EInputCommand.MainMenuPanel, args);
        }

        private void AdditionalCameraEffectsActionDefaultCoroutine(bool _Appear, float _Time)
        {
            Cor.Run(MainMenuUtils.SubPanelsAdditionalCameraEffectsActionCoroutine(_Appear, _Time,
                CameraProvider, UiTicker));
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
            CommonUtils.DoOnInitializedEx(RemoteConfigManager, Managers.AnalyticsManager.Init);
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
                {-01, Background},
                // Maze item groups execute here
                {+01, AdditionalBackground},
                {+02, CameraProvider},
                {+03, Managers},
                {+04, InputCommandsRecorder},
                {+05, InputCommandsReplayer},
                {+06, RewardCounter}
            };
        }
        
        private T[] GetInterfaceOfProceeders<T>() where T : class
        {
            return Array.ConvertAll(m_ProceedersCached, _Item => _Item as T);
        }
        
        private void LoadLastMainLevel(UnityAction _OnReadyToLoadLevel)
        {
            var sgCache = Managers.ScoreManager.GetSavedGame( MazorCommonData.SavedGameFileName);
            object levelIndexArg = sgCache.Arguments.GetSafe(KeyLevelIndexMainLevels, out _);
            long levelIndex = Convert.ToInt64(levelIndexArg);
            LoadLevelByIndex(levelIndex, ParameterGameModeMain, sgCache.Arguments, _OnReadyToLoadLevel);
        }

        private void LoadLevelByIndex(
            long                       _LevelIndex,
            string                     _GameMode,
            Dictionary<string, object> _SavedArgs,
            UnityAction                _OnReadyToLoadLevel)
        {
            var args = new Dictionary<string, object>
            {
                {KeyGameMode,                       _GameMode},
                {KeyLevelIndex,                     _LevelIndex},
                {KeyOnReadyToLoadLevelFinishAction, _OnReadyToLoadLevel},
                {KeySource,                         ParameterSourceMainMenu}
            };
            switch (_GameMode)
            {
                case ParameterGameModeMain:
                {
                    string levelType = (string) _SavedArgs.GetSafe(KeyNextLevelType, out _);
                    if (levelType != ParameterLevelTypeDefault && levelType != ParameterLevelTypeBonus)
                        levelType = ParameterLevelTypeDefault;
                    args.SetSafe(KeyNextLevelType, levelType);
                    break;
                }
                case ParameterGameModePuzzles:
                    args.SetSafe(KeyNextLevelType, ParameterLevelTypeDefault);
                    break;
            }
            var loadLevelCoroutine = MainMenuUtils.LoadLevelCoroutine(
                args, Settings, FullscreenTransitioner, UiTicker, LevelStageSwitcher);
            Cor.Run(loadLevelCoroutine);
        }
        
        #endregion
    }
}