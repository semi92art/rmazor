using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.Utils;
using RMAZOR.Helpers;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.UI.PanelItems.Levels_Panel_Items;
using RMAZOR.UI.Utils;
using RMAZOR.Views.Common;
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using RMAZOR.Views.InputConfigurators;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.UI.Panels
{
    public interface ILevelsDialogPanelPuzzles : IDialogPanel
    {
        UnityAction OnReadyToLoadLevelAction { set; }
    }
    
    public class LevelsDialogPanelPuzzlesFake 
        : DialogPanelFake, ILevelsDialogPanelPuzzles
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public UnityAction OnReadyToLoadLevelAction { get; set; }
    }

    public class LevelsDialogPanelPuzzles : DialogPanelBase, ILevelsDialogPanelPuzzles
    {
        #region constants

        private const int LevelsItemCountOnPage = 20;

        #endregion

        #region nonpublic members

        private TextMeshProUGUI m_TitleText;
        private Animator        m_PanelAnimator;

        private Button         
            m_ButtonPreviousPage,
            m_ButtonNextPage, 
            m_ButtonClose;

        private readonly Dictionary<int, LevelsPanelLevelItem> m_LevelsPanelLevelItemsDict
            = new Dictionary<int, LevelsPanelLevelItem>();

        private int m_CurrentPageIdx;
        private int m_PuzzlesTotalCountCached;

        protected override string PrefabName => "levels_panel_puzzles";
        
        #endregion

        #region inject

        private ViewSettings                ViewSettings           { get; }
        private IViewFullscreenTransitioner FullscreenTransitioner { get; }
        private ILevelsLoader               LevelsLoader           { get; }
        private IViewLevelStageSwitcher     LevelStageSwitcher     { get; }
        
        private LevelsDialogPanelPuzzles(
            ViewSettings                _ViewSettings,
            IViewFullscreenTransitioner _FullscreenTransitioner,
            IManagersGetter             _Managers,
            IUITicker                   _Ticker,
            ICameraProvider             _CameraProvider,
            IColorProvider              _ColorProvider,
            IViewTimePauser             _TimePauser,
            IViewInputCommandsProceeder _CommandsProceeder,
            ILevelsLoader               _LevelsLoader,
            IViewLevelStageSwitcher     _LevelStageSwitcher) 
            : base(
                _Managers,
                _Ticker, 
                _CameraProvider,
                _ColorProvider,
                _TimePauser, 
                _CommandsProceeder)
        {
            ViewSettings           = _ViewSettings;
            FullscreenTransitioner = _FullscreenTransitioner;
            LevelsLoader           = _LevelsLoader;
            LevelStageSwitcher     = _LevelStageSwitcher;
        }

        #endregion

        #region api
        
        public UnityAction OnReadyToLoadLevelAction { private get; set; }

        public override int DialogViewerId => DialogViewerIdsCommon.FullscreenCommon;

        public override Animator Animator => m_PanelAnimator;

        #endregion

        #region nonpublic methods
        
        protected override void LoadPanelCore(RectTransform _Container, ClosePanelAction _OnClose)
        {
            CachePuzzlesTotalCount();
            base.LoadPanelCore(_Container, _OnClose);
        }
        
        private void CachePuzzlesTotalCount()
        {
            var levelInfoArgs = new LevelInfoArgs
            {
                GameMode  = ParameterGameModePuzzles,
                LevelType = ParameterLevelTypeDefault
            };
            m_PuzzlesTotalCountCached = LevelsLoader.GetLevelsCount(levelInfoArgs);
        }
        
        protected override void OnDialogStartAppearing()
        {
            UpdatePrevAndNextButtonStates();
            UpdatePage();
            base.OnDialogStartAppearing();
        }
        
        private void OnButtonPrevClick()
        {
            PlayButtonClickSound();
            m_CurrentPageIdx--;
            UpdatePrevAndNextButtonStates();
            UpdatePage();
        }

        private void OnButtonNextClick()
        {
            PlayButtonClickSound();
            m_CurrentPageIdx++;
            UpdatePrevAndNextButtonStates();
            UpdatePage();
        }

        private void OnButtonCloseClick()
        {
            PlayButtonClickSound();
            OnClose();
            Managers.AudioManager.PlayClip(CommonAudioClipArgs.UiButtonClick);
        }
        
        protected override void GetPrefabContentObjects(GameObject _GameObject)
        {
            var go = _GameObject;
            m_TitleText          = go.GetCompItem<TextMeshProUGUI>("title_text");
            m_PanelAnimator      = go.GetCompItem<Animator>("animator");
            m_ButtonPreviousPage = go.GetCompItem<Button>("button_previous_page");
            m_ButtonNextPage     = go.GetCompItem<Button>("button_next_page");
            m_ButtonClose        = go.GetCompItem<Button>("button_close");

            var content = go.GetCompItem<RectTransform>("content");
            var levelPanelItems = content
                .GetChildren()
                .Select(_C => _C.GetComponent<LevelsPanelLevelItem>())
                .ToList();
            for (int i = 0; i < LevelsItemCountOnPage; i++)
            {
                var levelsPanelItem = levelPanelItems[i];
                levelsPanelItem.Init(
                    Ticker,
                    Managers.AudioManager,
                    Managers.LocalizationManager);
                m_LevelsPanelLevelItemsDict.Add(i, levelsPanelItem);
            }
        }

        protected override void LocalizeTextObjectsOnLoad()
        {
            var locTextInfo = new LocTextInfo(m_TitleText, ETextType.MenuUI_H1, "puzzles", 
                _T => _T.ToUpper(CultureInfo.CurrentUICulture));
            Managers.LocalizationManager.AddLocalization(locTextInfo);
        }
        
        protected override void SubscribeButtonEvents()
        {
            m_ButtonPreviousPage.onClick.AddListener(OnButtonPrevClick);
            m_ButtonNextPage    .onClick.AddListener(OnButtonNextClick);
            m_ButtonClose       .onClick.AddListener(OnButtonCloseClick);
        }

        private void UpdatePrevAndNextButtonStates()
        {
            m_ButtonPreviousPage.interactable = IsValidPageIndex(m_CurrentPageIdx - 1);
            m_ButtonNextPage.interactable     = IsValidPageIndex(m_CurrentPageIdx + 1);
        }

        private void UpdatePage()
        {
            var levelsFinishedOnce = SaveUtils.GetValue(SaveKeysRmazor.LevelsFinishedOncePuzzles)
                                     ?? new List<long>();
            long maxLevelFinishedOnce = levelsFinishedOnce.Any() ? levelsFinishedOnce.Max() : -1L;
            int startLevelIdxOnPage = m_CurrentPageIdx * LevelsItemCountOnPage;
            for (int i = 0; i < LevelsItemCountOnPage; i++)
            {
                long levelIndex = startLevelIdxOnPage + i;
                bool? enabled = null;
                if (IsValidLevelIndex(levelIndex))
                    enabled = levelIndex <= maxLevelFinishedOnce;
                var levelItem = m_LevelsPanelLevelItemsDict[i];
                levelItem.UpdateState(
                    levelIndex,
                    enabled,
                    levelIndex == maxLevelFinishedOnce + 1,
                    () =>
                    {
                        PlayButtonClickSound();
                        LoadLevel(levelIndex);
                    });
            }
        }
        
        private void LoadLevel(long _LevelIndex)
        {
            var args = new Dictionary<string, object>
            {
                {KeyGameMode,                       ParameterGameModePuzzles},
                {KeyNextLevelType,                  ParameterLevelTypeDefault},
                {KeyLevelIndex,                     _LevelIndex},
                {KeyOnReadyToLoadLevelFinishAction, OnReadyToLoadLevelAction}
            };
            OnClose();
            var loadLevelCoroutine = MainMenuUtils.LoadLevelCoroutine(
                args, ViewSettings, FullscreenTransitioner, Ticker, LevelStageSwitcher);
            Cor.Run(loadLevelCoroutine);
        }

        private bool IsValidPageIndex(int _Page)
        {
            return _Page.InRange(0, (m_PuzzlesTotalCountCached - 1) / LevelsItemCountOnPage);
        }
        
        private bool IsValidLevelIndex(long _LevelIndex)
        {
            return MathUtils.IsInRange(_LevelIndex, 0, m_PuzzlesTotalCountCached - 1);
        }
        
        #endregion
    }
}