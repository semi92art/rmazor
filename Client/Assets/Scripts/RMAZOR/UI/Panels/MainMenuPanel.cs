using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Constants;
using mazing.common.Runtime;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.CameraProviders.Camera_Effects_Props;
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
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.UI.Panels
{
    public interface IMainMenuPanel : IDialogPanel { }

    public class MainMenuPanelFake : FakeDialogPanel, IMainMenuPanel { }
    
    public class MainMenuPanel : DialogPanelBase, IMainMenuPanel, IUpdateTick
    {
        #region nonpublic members

        private Animator                      m_Animator;
        private Image                         m_Background;
        private RectTransform                 m_FirstLaunchPanel;
        private MainMenuCharacterSubPanelView m_CharacterSubPanel;

        private Button
            m_ButtonDisableAds,
            m_ButtonRateGame,
            m_ButtonSettings,
            m_ButtonShop;

        private MainMenuPanelButtonPlayMain            m_ButtonPlayMain;
        private MainMenuPanelButtonPlayDailyChallenges m_ButtonPlayDailyChallenges;
        private MainMenuPanelButtonPlayRandomLevels    m_ButtonPlayRandomLevels;
        private MainMenuPanelButtonPlayPuzzles         m_ButtonPlayPuzzles;
        private MainMenuPanelButtonPlayVsAi            m_ButtonPlayVsAi;
        
        protected override string PrefabName => "main_menu_panel";
        
        #endregion

        #region inject

        private ViewSettings                 ViewSettings                { get; }
        private IViewLevelStageSwitcher      LevelStageSwitcher          { get; }
        private IDailyChallengePanel         DailyChallengePanel         { get; }
        private IRandomGenerationParamsPanel RandomGenerationParamsPanel { get; }
        private ILevelsDialogPanelPuzzles    LevelsDialogPanelPuzzles    { get; }
        private IDailyGiftPanel              DailyGiftPanel              { get; }
        private IDisableAdsDialogPanel       DisableAdsDialogPanel       { get; }
        private ISettingDialogPanel          SettingDialogPanel          { get; }
        private IShopDialogPanel             ShopDialogPanel             { get; }
        private ICustomizeCharacterPanel     CustomizeCharacterPanel     { get; }
        private IDialogViewersController     DialogViewersController     { get; }
        private IModelGame                   Model                       { get; }
        private ILevelsLoader                LevelsLoader                { get; }
        private IContainersGetter            ContainersGetter            { get; }
        private ICoordinateConverter         CoordinateConverter         { get; }
        private IViewFullscreenTransitioner  FullscreenTransitioner      { get; }

        private MainMenuPanel(
            ViewSettings                 _ViewSettings,
            IManagersGetter              _Managers,
            IUITicker                    _Ticker,
            ICameraProvider              _CameraProvider,
            IColorProvider               _ColorProvider,
            IViewTimePauser              _TimePauser,
            IViewInputCommandsProceeder  _CommandsProceeder,
            IViewLevelStageSwitcher      _LevelStageSwitcher,
            IDailyChallengePanel         _DailyChallengePanel,
            IRandomGenerationParamsPanel _RandomGenerationParamsPanel,
            ILevelsDialogPanelPuzzles    _LevelsDialogPanelPuzzles,
            IDailyGiftPanel              _DailyGiftPanel,
            IDisableAdsDialogPanel       _DisableAdsDialogPanel,
            ISettingDialogPanel          _SettingDialogPanel,
            IShopDialogPanel             _ShopDialogPanel,
            ICustomizeCharacterPanel     _CustomizeCharacterPanel,
            IDialogViewersController     _DialogViewersController,
            IModelGame                   _Model,
            ILevelsLoader                _LevelsLoader,
            IContainersGetter            _ContainersGetter,
            ICoordinateConverter         _CoordinateConverter,
            IViewFullscreenTransitioner  _FullscreenTransitioner)
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
            RandomGenerationParamsPanel = _RandomGenerationParamsPanel;
            LevelsDialogPanelPuzzles    = _LevelsDialogPanelPuzzles;
            DailyGiftPanel              = _DailyGiftPanel;
            DisableAdsDialogPanel       = _DisableAdsDialogPanel;
            SettingDialogPanel          = _SettingDialogPanel;
            ShopDialogPanel             = _ShopDialogPanel;
            CustomizeCharacterPanel     = _CustomizeCharacterPanel;
            DialogViewersController     = _DialogViewersController;
            Model                       = _Model;
            LevelsLoader                = _LevelsLoader;
            ContainersGetter            = _ContainersGetter;
            CoordinateConverter         = _CoordinateConverter;
            FullscreenTransitioner      = _FullscreenTransitioner;
        }

        #endregion

        #region api
        
        public override int      DialogViewerId => MazorCommonDialogViewerIds.Fullscreen2;
        public override Animator Animator       => m_Animator;
        
        public override void LoadPanel(RectTransform _Container, ClosePanelAction _OnClose)
        {
            base.LoadPanel(_Container, _OnClose);
            InitPlayButtons();
            InitCharacterSubPanel();
            ProceedDisableAdsButtonOnLoad();
            ProceedFirstLaunchPanelOnLoad();
        }
        
        public void UpdateTick()
        {
            ChangeBackgroundColorOnUpdateTick();
        }
        
        #endregion

        #region nonpublic methods

        private void ChangeBackgroundColorOnUpdateTick()
        {
            float h = 0.5f + 0.15f * Mathf.Cos(Ticker.Time * 0.1f);
            const float s = 80f / 100f;
            const float v = 100f / 100f;
            var backColor = Color.HSVToRGB(h, s, v);
            m_Background.color = backColor;
        }

        protected override void OnDialogStartAppearing()
        {
            m_ButtonPlayMain           .UpdateState();
            m_ButtonPlayDailyChallenges.UpdateState();
            m_ButtonPlayPuzzles        .UpdateState();
            m_ButtonPlayRandomLevels   .UpdateState();
            m_CharacterSubPanel        .UpdateState();
            m_ButtonPlayVsAi           .UpdateState();
        }

        protected override void OnDialogAppeared()
        {
            base.OnDialogAppeared();
            LoadDailyChallengePanelOnAppearedIfNeed();
        }

        private void InitPlayButtons()
        {
            var (t, am, lm) = 
                (Ticker, Managers.AudioManager, Managers.LocalizationManager);
            m_ButtonPlayMain.Init(t, am, lm, GetMainGameModeStagesTotalCount, GetMainGameModeCurrentStageIndex);
            m_ButtonPlayRandomLevels.Init(t, am, lm);
            m_ButtonPlayDailyChallenges.Init(t, am, lm, GetDailyChTodayTotalCount, GetDailyChTodayFinishedCount);
            m_ButtonPlayPuzzles.Init(t, am, lm, GetPuzzlesTotalCount, GetPuzzlesFinishedCount);
            m_ButtonPlayVsAi.Init(t, am, lm);
        }

        private void InitCharacterSubPanel()
        {
            var (t, am, lm) = 
                (Ticker, Managers.AudioManager, Managers.LocalizationManager);
            m_CharacterSubPanel.Init(t, am, lm, ContainersGetter,
                Managers.PrefabSetManager, Managers.ScoreManager, DailyGiftPanel, CoordinateConverter,
                OnDailyGiftButtonClick, OnAddMoneyButtonClick, OnCustomizeCharacterButtonClick);
        }

        private int GetMainGameModeCurrentStageIndex()
        {
            var savedGame = Managers.ScoreManager.GetSavedGame( MazorCommonData.SavedGameFileName);
            object levelIndexArg = savedGame.Arguments.GetSafe(KeyLevelIndexMainLevels, out _);
            long levelIndex = Convert.ToInt64(levelIndexArg);
            string levelType = (string) savedGame.Arguments.GetSafe(KeyCurrentLevelType, out _);
            return levelType == ParameterLevelTypeBonus ? (int)levelIndex : RmazorUtils.GetLevelsGroupIndex(levelIndex);
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
            m_Background                = _Go.GetCompItem<Image>("background");
            m_ButtonDisableAds          = _Go.GetCompItem<Button>("button_no_ads");
            m_ButtonRateGame            = _Go.GetCompItem<Button>("button_rate_game");
            m_ButtonSettings            = _Go.GetCompItem<Button>("button_settings");
            m_ButtonShop                = _Go.GetCompItem<Button>("button_shop");
            m_ButtonPlayMain            = _Go.GetCompItem<MainMenuPanelButtonPlayMain>("button_play_main_levels");
            m_ButtonPlayDailyChallenges = _Go.GetCompItem<MainMenuPanelButtonPlayDailyChallenges>("button_play_daily_challenge");
            m_ButtonPlayRandomLevels    = _Go.GetCompItem<MainMenuPanelButtonPlayRandomLevels>("button_play_random_levels");
            m_ButtonPlayPuzzles         = _Go.GetCompItem<MainMenuPanelButtonPlayPuzzles>("button_play_puzzle_levels");
            m_ButtonPlayVsAi            = _Go.GetCompItem<MainMenuPanelButtonPlayVsAi>("button_play_vs_ai");
            m_CharacterSubPanel         = _Go.GetCompItem<MainMenuCharacterSubPanelView>("character_sub_panel");
            m_FirstLaunchPanel          = _Go.GetCompItem<RectTransform>("first_launch_panel");
        }

        protected override void LocalizeTextObjectsOnLoad()
        {

        }

        protected override void SubscribeButtonEvents()
        {
            m_ButtonDisableAds         .SetOnClick(OnDisableAdsButtonClick);
            m_ButtonRateGame           .SetOnClick(OnRateGameButtonClick);
            m_ButtonSettings           .SetOnClick(OnSettingsButtonClick);
            m_ButtonShop               .SetOnClick(OnAddMoneyButtonClick);
            m_ButtonPlayMain           .SetOnClick(OnPlayMainLevelsButtonClick);
            m_ButtonPlayDailyChallenges.SetOnClick(OnPlayDailyChallengeButtonClick);
            m_ButtonPlayRandomLevels   .SetOnClick(OnPlayRandomLevelsButtonClick);
            m_ButtonPlayPuzzles        .SetOnClick(OnPlayPuzzleLevelsButtonClick);
            m_ButtonPlayVsAi           .SetOnClick(OnPlayVsAiButtonClick);
        }

        private void OnDisableAdsButtonClick()
        {
            PlayButtonClickSound();
            Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.MainMenuDisableAdsButtonClick);
            var dv = DialogViewersController.GetViewer(DisableAdsDialogPanel.DialogViewerId);
            dv.Show(DisableAdsDialogPanel, 100f, _AdditionalCameraEffectsAction: AdditionalCameraEffectsActionDefaultCoroutine);
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
            PlayButtonClickSound();
            Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.MainMenuShopButtonClick);
            var dv = DialogViewersController.GetViewer(ShopDialogPanel.DialogViewerId);
            dv.Show(ShopDialogPanel, 100f, _AdditionalCameraEffectsAction: AdditionalCameraEffectsActionDefaultCoroutine);
        }

        private void OnCustomizeCharacterButtonClick()
        {
            PlayButtonClickSound();
            Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.MainMenuCustomizeCharacterClick);
            var dv = DialogViewersController.GetViewer(CustomizeCharacterPanel.DialogViewerId);
            dv.Show(CustomizeCharacterPanel, 100f, _AdditionalCameraEffectsAction: AdditionalCameraEffectsActionDefaultCoroutine);
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
                m_FirstLaunchPanel.SetGoActive(false);
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
            RandomGenerationParamsPanel.OnReadyToLoadLevelAction = () => OnClose();
            var dv = DialogViewersController.GetViewer(RandomGenerationParamsPanel.DialogViewerId);
            dv.Show(RandomGenerationParamsPanel, 100f, _AdditionalCameraEffectsAction: AdditionalCameraEffectsActionDefaultCoroutine);
        }

        private void OnPlayPuzzleLevelsButtonClick()
        {
            PlayButtonClickSound();
            Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.PlayPuzzleLevelsButtonClick);
            LevelsDialogPanelPuzzles.OnReadyToLoadLevelAction = () => OnClose();
            var dv = DialogViewersController.GetViewer(LevelsDialogPanelPuzzles.DialogViewerId);
            dv.Show(LevelsDialogPanelPuzzles, 100f, _AdditionalCameraEffectsAction: AdditionalCameraEffectsActionDefaultCoroutine);
        }
        
        private void OnPlayVsAiButtonClick()
        {
            PlayButtonClickSound();
            Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.PlayVsAiButtonClick);
            // TODO
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

        private void ProceedDisableAdsButtonOnLoad()
        {
            m_ButtonDisableAds.SetGoActive(Managers.AdsManager.ShowAds);
        }

        private void ProceedFirstLaunchPanelOnLoad()
        {
            bool mainMenuTutorialFinished = SaveUtils.GetValue(SaveKeysRmazor.IsTutorialFinished("main_menu"));
            m_FirstLaunchPanel.SetGoActive(!mainMenuTutorialFinished);
        }

        #endregion
    }
}