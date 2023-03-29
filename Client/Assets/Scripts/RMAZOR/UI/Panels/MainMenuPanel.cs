using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Common;
using Common.Constants;
using Common.Entities;
using mazing.common.Runtime;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.UI.DialogViewers;
using mazing.common.Runtime.Utils;
using RMAZOR.Constants;
using RMAZOR.Helpers;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.UI.PanelItems.Main_Menu_Panel_Items;
using RMAZOR.UI.Panels.ShopPanels;
using RMAZOR.UI.Utils;
using RMAZOR.Views.Common;
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.UI.Panels
{
    public interface IMainMenuPanel : IDialogPanel { }

    public class MainMenuPanelFake : FakeDialogPanel, IMainMenuPanel { }
    
    public class MainMenuPanel : DialogPanelBase, IMainMenuPanel
    {
        #region nonpublic members

        private Animator                      m_Animator, m_FirstLaunchHandAnimator;
        private RectTransform                 m_FirstLaunchPanel;
        private MainMenuCharacterSubPanelView m_CharacterSubPanel;

        private Button
            m_ButtonSettings,
            m_ButtonReviewMessage;

        private MainMenuPanelButtonDailyGift           m_ButtonDailyGift;
        private MainMenuPanelButtonPlayMain            m_ButtonPlayMain;
        private MainMenuPanelButtonPlayDailyChallenges m_ButtonPlayDailyChallenges;
        private MainMenuPanelButtonPlayRandomLevels    m_ButtonPlayRandomLevels;
        private MainMenuPanelButtonPlayPuzzles         m_ButtonPlayPuzzles;

        private TextMeshProUGUI m_ReviewMessageText, m_ReviewMessageButtonText;
        
        protected override string PrefabName => "main_menu_panel";
        
        #endregion

        #region inject

        private ViewSettings                    ViewSettings             { get; }
        private IViewLevelStageSwitcher         LevelStageSwitcher       { get; }
        private IDailyChallengePanel            DailyChallengePanel      { get; }
        private ILevelsDialogPanelPuzzles       LevelsDialogPanelPuzzles { get; }
        private IDailyGiftPanel                 DailyGiftPanel           { get; }
        private ISettingDialogPanel             SettingDialogPanel       { get; }
        private IShopDialogPanel                ShopDialogPanel          { get; }
        private ICustomizeCharacterPanel        CustomizeCharacterPanel  { get; }
        private IDialogViewersController        DialogViewersController  { get; }
        private IModelGame                      Model                    { get; }
        private ILevelsLoader                   LevelsLoader             { get; }
        private IContainersGetter               ContainersGetter         { get; }
        private ICoordinateConverter            CoordinateConverter      { get; }
        private IViewFullscreenTransitioner     FullscreenTransitioner   { get; }
        private IViewGameUiCreatingLevelMessage CreatingLevelMessage     { get; }

        private MainMenuPanel(
            ViewSettings                    _ViewSettings,
            IManagersGetter                 _Managers,
            IUITicker                       _Ticker,
            ICameraProvider                 _CameraProvider,
            IColorProvider                  _ColorProvider,
            IViewTimePauser                 _TimePauser,
            IViewInputCommandsProceeder     _CommandsProceeder,
            IViewLevelStageSwitcher         _LevelStageSwitcher,
            IDailyChallengePanel            _DailyChallengePanel,
            ILevelsDialogPanelPuzzles       _LevelsDialogPanelPuzzles,
            IDailyGiftPanel                 _DailyGiftPanel,
            ISettingDialogPanel             _SettingDialogPanel,
            IShopDialogPanel                _ShopDialogPanel,
            ICustomizeCharacterPanel        _CustomizeCharacterPanel,
            IDialogViewersController        _DialogViewersController,
            IModelGame                      _Model,
            ILevelsLoader                   _LevelsLoader,
            IContainersGetter               _ContainersGetter,
            ICoordinateConverter            _CoordinateConverter,
            IViewFullscreenTransitioner     _FullscreenTransitioner,
            IViewGameUiCreatingLevelMessage _CreatingLevelMessage)
            : base(
                _Managers, 
                _Ticker,
                _CameraProvider,
                _ColorProvider,
                _TimePauser,
                _CommandsProceeder)
        {
            ViewSettings                = _ViewSettings;
            LevelStageSwitcher          = _LevelStageSwitcher;
            DailyChallengePanel         = _DailyChallengePanel;
            LevelsDialogPanelPuzzles    = _LevelsDialogPanelPuzzles;
            DailyGiftPanel              = _DailyGiftPanel;
            SettingDialogPanel          = _SettingDialogPanel;
            ShopDialogPanel             = _ShopDialogPanel;
            CustomizeCharacterPanel     = _CustomizeCharacterPanel;
            DialogViewersController     = _DialogViewersController;
            Model                       = _Model;
            LevelsLoader                = _LevelsLoader;
            ContainersGetter            = _ContainersGetter;
            CoordinateConverter         = _CoordinateConverter;
            FullscreenTransitioner      = _FullscreenTransitioner;
            CreatingLevelMessage        = _CreatingLevelMessage;
        }

        #endregion

        #region api
        
        public override int      DialogViewerId => DialogViewerIdsMazor.Fullscreen2;
        public override Animator Animator       => m_Animator;
        
        public override void LoadPanel(RectTransform _Container, ClosePanelAction _OnClose)
        {
            base.LoadPanel(_Container, _OnClose);
            InitDailyGiftButton();
            InitPlayButtons();
            InitCharacterSubPanel();
            ProceedFirstLaunchPanelOnLoad();
        }

        #endregion

        #region nonpublic methods

        protected override void OnDialogStartAppearing()
        {
            m_ButtonDailyGift          .UpdateState();
            m_ButtonPlayMain           .UpdateState();
            m_ButtonPlayDailyChallenges.UpdateState();
            m_ButtonPlayPuzzles        .UpdateState();
            m_ButtonPlayRandomLevels   .UpdateState();
            m_CharacterSubPanel        .UpdateState();
        }

        protected override void OnDialogAppeared()
        {
            base.OnDialogAppeared();
            LoadDailyChallengePanelOnAppearedIfNeed();
        }

        private void InitDailyGiftButton()
        {
            var (t, am, lm) = 
                (Ticker, Managers.AudioManager, Managers.LocalizationManager);
            m_ButtonDailyGift.Init(t, am, lm, 
                Managers.PrefabSetManager,
                DailyGiftPanel,
                OnDailyGiftButtonClick);
        }

        private void InitPlayButtons()
        {
            var (t, am, lm) = 
                (Ticker, Managers.AudioManager, Managers.LocalizationManager);
            m_ButtonPlayMain.Init(t, am, lm, GetMainGameModeStagesTotalCount, GetMainGameModeCurrentStageIndex);
            m_ButtonPlayRandomLevels.Init(t, am, lm, Managers.ScoreManager);
            m_ButtonPlayDailyChallenges.Init(t, am, lm, GetDailyChTodayTotalCount, GetDailyChTodayFinishedCount);
            m_ButtonPlayPuzzles.Init(t, am, lm, Managers.ScoreManager, GetPuzzlesTotalCount, GetPuzzlesFinishedCount);
        }

        private void InitCharacterSubPanel()
        {
            var (t, am, lm) = 
                (Ticker, Managers.AudioManager, Managers.LocalizationManager);
            m_CharacterSubPanel.Init(t, am, lm, ContainersGetter,
                Managers.PrefabSetManager, Managers.ScoreManager, DailyGiftPanel,
                CoordinateConverter, CustomizeCharacterPanel, ShopDialogPanel,
                OnAddMoneyButtonClick,
                OnCustomizeCharacterButtonClick,
                OnShopButtonClick);
        }

        private int GetMainGameModeCurrentStageIndex()
        {
            var savedGame = Managers.ScoreManager.GetSavedGame(MazorCommonData.SavedGameFileName);
            if (savedGame == null)
            {
                savedGame = new SavedGameV2 {Arguments = new Dictionary<string, object>()};
                Managers.ScoreManager.SaveGame(savedGame);
            }
            object levelIndexArg = savedGame.Arguments.GetSafe(KeyLevelIndexMainLevels, out _);
            long levelIndex = Convert.ToInt64(levelIndexArg);
            string levelType = (string) savedGame.Arguments.GetSafe(KeyCurrentLevelType, out _);
            return levelType == ParameterLevelTypeBonus ? (int)levelIndex + 1 : RmazorUtils.GetLevelsGroupIndex(levelIndex);
        }

        private int GetMainGameModeStagesTotalCount()
        {
            var levelInfoArgs = new LevelInfoArgs
            {
                GameMode  = ParameterGameModeMain,
                LevelType = ParameterLevelTypeDefault
            };
            int levelsCount = LevelsLoader.GetLevelsCount(levelInfoArgs);
            return RmazorUtils.GetLevelsGroupIndex(levelsCount - 1);
        }
        
        private int GetPuzzlesTotalCount()
        {
            var levelInfoArgs = new LevelInfoArgs
            {
                GameMode  = ParameterGameModePuzzles,
                LevelType = ParameterLevelTypeDefault
            };
            int levelsCount = LevelsLoader.GetLevelsCount(levelInfoArgs);
            return levelsCount;
        }

        private static int GetPuzzlesFinishedCount()
        {
            List<long> puzzlelevelsFinishedOnceIndices = SaveUtils.GetValue(SaveKeysRmazor.LevelsFinishedOncePuzzles);
            if (puzzlelevelsFinishedOnceIndices == null)
            {
                puzzlelevelsFinishedOnceIndices = new List<long> { -1 };
                SaveUtils.PutValue(SaveKeysRmazor.LevelsFinishedOncePuzzles, puzzlelevelsFinishedOnceIndices);
            }
            return (int) puzzlelevelsFinishedOnceIndices.Max() + 1;
        }

        private int GetDailyChTodayTotalCount()
        {
            return DailyChallengePanel.GetChallengesCountTodayTotal();
        }

        private int GetDailyChTodayFinishedCount()
        {
            return DailyChallengePanel.GetChallengesCountTodayFinished();
        }
        
        protected override void GetPrefabContentObjects(GameObject _Go)
        {
            m_Animator                  = _Go.GetCompItem<Animator>("animator");
            m_ButtonReviewMessage       = _Go.GetCompItem<Button>("button_review_message");
            m_ButtonSettings            = _Go.GetCompItem<Button>("button_settings");
            m_ButtonDailyGift           = _Go.GetCompItem<MainMenuPanelButtonDailyGift>("button_daily_gift");
            m_ButtonPlayMain            = _Go.GetCompItem<MainMenuPanelButtonPlayMain>("button_play_main_levels");
            m_ButtonPlayDailyChallenges = _Go.GetCompItem<MainMenuPanelButtonPlayDailyChallenges>("button_play_daily_challenge");
            m_ButtonPlayRandomLevels    = _Go.GetCompItem<MainMenuPanelButtonPlayRandomLevels>("button_play_random_levels");
            m_ButtonPlayPuzzles         = _Go.GetCompItem<MainMenuPanelButtonPlayPuzzles>("button_play_puzzle_levels");
            m_CharacterSubPanel         = _Go.GetCompItem<MainMenuCharacterSubPanelView>("character_sub_panel");
            m_FirstLaunchPanel          = _Go.GetCompItem<RectTransform>("first_launch_panel");
            m_FirstLaunchHandAnimator   = _Go.GetCompItem<Animator>("first_launch_hand_animator");
            m_ReviewMessageText         = _Go.GetCompItem<TextMeshProUGUI>("review_message_text");
            m_ReviewMessageButtonText   = _Go.GetCompItem<TextMeshProUGUI>("review_message_button_text");
        }

        protected override void LocalizeTextObjectsOnLoad()
        {
            var locTextInfos = new[]
            {
                new LocTextInfo(m_ReviewMessageText,       ETextType.MenuUI_H3, "review_message_text"),
                new LocTextInfo(m_ReviewMessageButtonText, ETextType.MenuUI_H2, "suggest_idea", 
                    _T =>
                    {
                        var textCol = Managers.LocalizationManager.GetCurrentLanguage() == ELanguage.Russian
                            ? new Color(0.29f, 0.19f, 0.17f)
                            : Color.white;
                        m_ReviewMessageButtonText.color = textCol;

                        return _T.FirstCharToUpper(CultureInfo.CurrentUICulture);
                    }),
            };
            foreach (var locTextInfo in locTextInfos)
                Managers.LocalizationManager.AddLocalization(locTextInfo);
        }

        protected override void SubscribeButtonEvents()
        {
            m_ButtonReviewMessage      .SetOnClick(OnRateGameButtonClick);
            m_ButtonSettings           .SetOnClick(OnSettingsButtonClick);
            m_ButtonPlayMain           .SetOnClick(OnPlayMainLevelsButtonClick);
            m_ButtonPlayDailyChallenges.SetOnClick(OnPlayDailyChallengeButtonClick);
            m_ButtonPlayRandomLevels   .SetOnClick(OnPlayRandomLevelsButtonClick);
            m_ButtonPlayPuzzles        .SetOnClick(OnPlayPuzzleLevelsButtonClick);
        }

        private void OnRateGameButtonClick()
        {
            PlayButtonClickSound();
            Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.MainMenuRateGameButtonClick);
            Managers.ShopManager.RateGame();
            SaveUtils.PutValue(SaveKeysCommon.GameWasRated, true);
        }

        private void OnSettingsButtonClick()
        {
            PlayButtonClickSound();
            Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.MainMenuSettingsButtonClick);
            var dv = DialogViewersController.GetViewer(SettingDialogPanel.DialogViewerId);
            dv.Show(SettingDialogPanel, 100f, _AdditionalCameraEffectsAction: AdditionalCameraEffectsActionDefaultCoroutine);
        }

        private void OnDailyGiftButtonClick()
        {
            PlayButtonClickSound();
            Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.MainMenuSettingsButtonClick);
            var dv = DialogViewersController.GetViewer(DailyGiftPanel.DialogViewerId);
            dv.Show(DailyGiftPanel, _AdditionalCameraEffectsAction: AdditionalCameraEffectsActionDefaultCoroutine);
        }

        private void OnAddMoneyButtonClick()
        {
            OnShopButtonClick();
        }

        private void OnCustomizeCharacterButtonClick()
        {
            PlayButtonClickSound();
            Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.MainMenuCustomizeCharacterClick);
            var dv = DialogViewersController.GetViewer(CustomizeCharacterPanel.DialogViewerId);
            dv.Show(CustomizeCharacterPanel, 100f, _AdditionalCameraEffectsAction: AdditionalCameraEffectsActionDefaultCoroutine);
        }
        
        private void OnShopButtonClick()
        {
            PlayButtonClickSound();
            Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.MainMenuShopButtonClick);
            var dv = DialogViewersController.GetViewer(ShopDialogPanel.DialogViewerId);
            dv.Show(ShopDialogPanel, 100f, _AdditionalCameraEffectsAction: AdditionalCameraEffectsActionDefaultCoroutine);
        }

        private void OnPlayMainLevelsButtonClick()
        {
            PlayButtonClickSound();
            Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.PlayMainLevelsButtonClick);
            LoadLastMainLevel(() => OnClose(() =>
            {
                var saveKeyMainMenuTutFinished = SaveKeysRmazor.IsTutorialFinished("main_menu");
                if (SaveUtils.GetValue(saveKeyMainMenuTutFinished))
                    return;
                m_FirstLaunchPanel       .SetGoActive(false);
                m_FirstLaunchHandAnimator.SetGoActive(false);
                SaveUtils.PutValue(saveKeyMainMenuTutFinished, true);
            }));
        }
        
        private void OnPlayDailyChallengeButtonClick()
        {
            PlayButtonClickSound();
            Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.PlayDailyChallengeButtonClick);
            DailyChallengePanel.OnReadyToLoadLevelAction = () => OnClose();
            var dv = DialogViewersController.GetViewer(DailyChallengePanel.DialogViewerId);
            dv.Show(DailyChallengePanel, 100f, _AdditionalCameraEffectsAction: AdditionalCameraEffectsActionDefaultCoroutine);
        }

        private void OnPlayRandomLevelsButtonClick()
        {
            PlayButtonClickSound();
            Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.PlayRandomLevelsButtonClick);
            var args = new Dictionary<string, object>
            {
                {KeyGameMode,                       ParameterGameModeRandom},
                {KeyLevelIndex,                     0L},
                {KeyRandomLevelSize,                -1},
                {KeyOnReadyToLoadLevelFinishAction, (UnityAction)(() => OnClose())},
                {KeyAiSimulation,                   false}
            };
            var loadLevelCoroutine = MainMenuUtils.LoadLevelCoroutine(
                args, ViewSettings, FullscreenTransitioner, Ticker, LevelStageSwitcher);
            Cor.Run(Cor.Delay(
                ViewSettings.betweenLevelTransitionTime * 0.8f, Ticker, CreatingLevelMessage.ShowMessage));
            Cor.Run(loadLevelCoroutine);
        }

        private void OnPlayPuzzleLevelsButtonClick()
        {
            PlayButtonClickSound();
            Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.PlayPuzzleLevelsButtonClick);
            LevelsDialogPanelPuzzles.OnReadyToLoadLevelAction = () => OnClose();
            var dv = DialogViewersController.GetViewer(LevelsDialogPanelPuzzles.DialogViewerId);
            dv.Show(LevelsDialogPanelPuzzles, 100f, _AdditionalCameraEffectsAction: AdditionalCameraEffectsActionDefaultCoroutine);
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
                args, ViewSettings, FullscreenTransitioner, Ticker, LevelStageSwitcher);
            Cor.Run(loadLevelCoroutine);
        }

        private void LoadDailyChallengePanelOnAppearedIfNeed()
        {
            object mustLoadDailyChallengesPanelArg = Model.LevelStaging.Arguments.GetSafe(
                KeyIsDailyChallengeSuccess, out bool keyExist);
            if (!keyExist || !(bool) mustLoadDailyChallengesPanelArg)
                return;
            Cor.Run(Cor.Delay(0.2f, Ticker, OnPlayDailyChallengeButtonClick));
        }
        
        private void AdditionalCameraEffectsActionDefaultCoroutine(bool _Appear, float _Time)
        {
            Cor.Run(MainMenuUtils.SubPanelsAdditionalCameraEffectsActionCoroutine(_Appear, _Time,
                CameraProvider, Ticker));
        }

        private void ProceedFirstLaunchPanelOnLoad()
        {
            bool mainMenuTutorialFinished = SaveUtils.GetValue(SaveKeysRmazor.IsTutorialFinished("main_menu"));
            m_FirstLaunchPanel       .SetGoActive(!mainMenuTutorialFinished);
            m_FirstLaunchHandAnimator.SetGoActive(!mainMenuTutorialFinished);
        }

        #endregion
    }
}