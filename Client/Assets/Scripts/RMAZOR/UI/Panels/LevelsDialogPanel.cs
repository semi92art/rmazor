using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Common.Constants;
using Common.Utils;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.UI.DialogViewers;
using mazing.common.Runtime.Utils;
using RMAZOR.Helpers;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.UI.PanelItems.Levels_Panel_Items;
using RMAZOR.Views.Common;
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using RMAZOR.Views.InputConfigurators;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.UI.Panels
{
    public interface ILevelsDialogPanelMain : IDialogPanel { }
    
    public class LevelsDialogPanelMainFake : 
        DialogPanelFake, ILevelsDialogPanelMain { }
     
    public class LevelsDialogPanelMain : DialogPanelBase, ILevelsDialogPanelMain
    {
        #region constants

        private const int LevelGroupItemsCountOnPage = 4;

        #endregion
        
        #region nonpublic members

        private Animator        m_PanelAnimator;
        private TextMeshProUGUI m_TitleText;
        private Button         
            m_ButtonPreviousGroups,
            m_ButtonNextGroups, 
            m_ButtonClose;
        private Sprite
            m_SpriteLevelGroupItemEnabled,
            m_SpriteLevelGroupItemDisabled,
            m_SpriteLevelGroupItemSelected,
            m_SpriteLevelGroupItemStarEnabled,
            m_SpriteLevelGroupItemStarDisabled;
        
        private readonly Dictionary<int, LevelsPanelGroupItem> m_LevelPanelGroupItemsDict 
            = new Dictionary<int, LevelsPanelGroupItem>();

        private int m_SelectedLevelGroupGroupIndex = -1;
        
        protected override string PrefabName => "levels_panel";

        #endregion
        
        #region inject

        private ILevelsLoader                LevelsLoader                { get; }
        private IRawLevelInfoGetter          RawLevelInfoGetter          { get; }
        private IModelGame                   Model                       { get; }
        private IViewLevelStageSwitcher      LevelStageSwitcher          { get; }
        private IConfirmLoadLevelDialogPanel ConfirmLoadLevelDialogPanel { get; }
        private IDialogViewersController     DialogViewersController     { get; }

        private LevelsDialogPanelMain(
            IManagersGetter              _Managers,
            IUITicker                    _Ticker,
            ICameraProvider              _CameraProvider,
            IColorProvider               _ColorProvider,
            IViewTimePauser              _TimePauser,
            IViewInputCommandsProceeder  _CommandsProceeder,
            ILevelsLoader                _LevelsLoader,
            IRawLevelInfoGetter          _RawLevelInfoGetter,
            IModelGame                   _Model,
            IViewLevelStageSwitcher      _LevelStageSwitcher,
            IConfirmLoadLevelDialogPanel _ConfirmLoadLevelDialogPanel,
            IDialogViewersController     _DialogViewersController)
            : base(
                _Managers,
                _Ticker,
                _CameraProvider, 
                _ColorProvider,
                _TimePauser,
                _CommandsProceeder)
        {
            LevelsLoader                = _LevelsLoader;
            RawLevelInfoGetter          = _RawLevelInfoGetter;
            Model                       = _Model;
            LevelStageSwitcher          = _LevelStageSwitcher;
            ConfirmLoadLevelDialogPanel = _ConfirmLoadLevelDialogPanel;
            DialogViewersController     = _DialogViewersController;
        }

        #endregion

        #region api
        
        public override    int      DialogViewerId => DialogViewerIdsCommon.MediumCommon;
        public override    Animator Animator       => m_PanelAnimator;

        public override void LoadPanel(RectTransform _Container, ClosePanelAction _OnClose)
        {
            base.LoadPanel(_Container, _OnClose);
            LoadLevelGroupButtonSprites();
        }

        #endregion

        #region nonpublic methods
        
        protected override void OnDialogStartAppearing()
        {
            if (m_SelectedLevelGroupGroupIndex == -1)
                m_SelectedLevelGroupGroupIndex = GetCurrentLevelGroupGroupIndex();
            UpdatePrevAndNextButtonStates();
            UpdateLevelGroupItems();
            base.OnDialogStartAppearing();
        }
        
        private void OnButtonPrevClick()
        {
            m_SelectedLevelGroupGroupIndex--;
            UpdatePrevAndNextButtonStates();
            UpdateLevelGroupItems();
        }

        private void OnButtonNextClick()
        {
            m_SelectedLevelGroupGroupIndex++;
            UpdatePrevAndNextButtonStates();
            UpdateLevelGroupItems();
        }

        private void OnButtonCloseClick()
        {
            OnClose(() =>
            {
                LevelStageSwitcher.SwitchLevelStage(EInputCommand.UnPauseLevel);
            });
            PlayButtonClickSound();
        }
        
        protected override void GetPrefabContentObjects(GameObject _GameObject)
        {
            var go = _GameObject;
            m_PanelAnimator        = go.GetCompItem<Animator>("animator");
            m_TitleText            = go.GetCompItem<TextMeshProUGUI>("title_text");
            m_ButtonPreviousGroups = go.GetCompItem<Button>("button_previous_stages");
            m_ButtonNextGroups     = go.GetCompItem<Button>("button_next_stages");
            m_ButtonClose          = go.GetCompItem<Button>("button_close");
            for (int i = 1; i <= LevelGroupItemsCountOnPage; i++)
            {
                var levelGroupItem = go.GetCompItem<LevelsPanelGroupItem>($"stage_item_{i}");
                levelGroupItem.Init(
                    LevelsLoader,
                    RawLevelInfoGetter,
                    Model,
                    Ticker,
                    Managers.AudioManager,
                    Managers.LocalizationManager,
                    m_SpriteLevelGroupItemEnabled, 
                    m_SpriteLevelGroupItemDisabled,
                    m_SpriteLevelGroupItemSelected,
                    m_SpriteLevelGroupItemStarEnabled,
                    m_SpriteLevelGroupItemStarDisabled);
                m_LevelPanelGroupItemsDict.Add(i, levelGroupItem);
            }
        }

        protected override void LocalizeTextObjectsOnLoad()
        {
            var locMan = Managers.LocalizationManager;
            locMan.AddLocalization(new LocTextInfo(
                m_TitleText,
                ETextType.MenuUI, 
                "stages",
                _Text => _Text.ToUpper(CultureInfo.CurrentUICulture)));
        }

        private void LoadLevelGroupButtonSprites()
        {
            var psm = Managers.PrefabSetManager;
            m_SpriteLevelGroupItemEnabled = psm.GetObject<Sprite>(
                CommonPrefabSetNames.Views, 
                "level_stage_item_enabled_sprite");
            m_SpriteLevelGroupItemDisabled = psm.GetObject<Sprite>(
                CommonPrefabSetNames.Views, 
                "level_stage_item_disabled_sprite");
            m_SpriteLevelGroupItemSelected = psm.GetObject<Sprite>(
                CommonPrefabSetNames.Views, 
                "level_stage_item_selected_sprite");
            m_SpriteLevelGroupItemStarEnabled = psm.GetObject<Sprite>(
                CommonPrefabSetNames.Views, 
                "level_stage_item_star_enabled_sprite");
            m_SpriteLevelGroupItemStarDisabled = psm.GetObject<Sprite>(
                CommonPrefabSetNames.Views, 
                "level_stage_item_star_disabled_sprite");
        }

        protected override void SubscribeButtonEvents()
        {
            m_ButtonPreviousGroups.onClick.AddListener(OnButtonPrevClick);
            m_ButtonNextGroups    .onClick.AddListener(OnButtonNextClick);
            m_ButtonClose         .onClick.AddListener(OnButtonCloseClick);
        }

        private void UpdatePrevAndNextButtonStates()
        {
            m_ButtonPreviousGroups.interactable = true;
            m_ButtonNextGroups.interactable = true;
            if (m_SelectedLevelGroupGroupIndex == 0)
                m_ButtonPreviousGroups.interactable = false;
            if (m_SelectedLevelGroupGroupIndex > GetCurrentLevelGroupGroupIndex() + 1)
                m_ButtonNextGroups.interactable = false;
        }

        private void UpdateLevelGroupItems()
        {
            var levelsFinishedOnce = SaveUtils.GetValue(SaveKeysRmazor.LevelsFinishedOnce);
            long maxLevelFinishedOnce = levelsFinishedOnce.Max();
            int maxLevelGroup = RmazorUtils.GetLevelsGroupIndex(maxLevelFinishedOnce);
            string currentLevelType = (string)Model.LevelStaging.Arguments.GetSafe(
                KeyCurrentLevelType, out _);
            bool isCurrentLevelBonus = currentLevelType == ParameterLevelTypeBonus;
            int currentLevelGroupIdx = isCurrentLevelBonus ? 
                (int)Model.LevelStaging.LevelIndex + 1 
                : RmazorUtils.GetLevelsGroupIndex(Model.LevelStaging.LevelIndex);
            int currentLevelsGroupGroupIndex = GetCurrentLevelGroupGroupIndex();
            int startLevelGroupIndexInGroup = m_SelectedLevelGroupGroupIndex * LevelGroupItemsCountOnPage;
            bool isUnknownStage = m_SelectedLevelGroupGroupIndex > currentLevelsGroupGroupIndex + 1;
            for (int i = 1; i <= LevelGroupItemsCountOnPage; i++)
            {
                bool? stageEnabled = null;
                int iLevelsGroup = startLevelGroupIndexInGroup + i;
                if (!isUnknownStage)
                    stageEnabled = iLevelsGroup <= maxLevelGroup;
                m_LevelPanelGroupItemsDict[i].UpdateItem(
                    iLevelsGroup,
                    currentLevelGroupIdx,
                    stageEnabled, 
                    _LevelInGroupIndex =>
                    {
                        long currentLevelIdxInGroup = RmazorUtils.GetIndexInGroup(Model.LevelStaging.LevelIndex);   
                        if (RmazorUtils.WasLevelGroupFinishedBefore(currentLevelGroupIdx) 
                            || currentLevelIdxInGroup == 0 || isCurrentLevelBonus)
                        {
                            LoadLevel(iLevelsGroup, _LevelInGroupIndex);
                        }
                        else
                        {
                            ConfirmLoadLevelDialogPanel.SetLevelGroupAndIndex(iLevelsGroup, _LevelInGroupIndex);
                            var dv = DialogViewersController.GetViewer(ConfirmLoadLevelDialogPanel.DialogViewerId);
                            dv.Show(ConfirmLoadLevelDialogPanel);
                        }
                    });
            }
        }

        private void LoadLevel(int _LevelGroupIndex, int _LevelInGroupIndex)
        {
            PlayButtonClickSound();
            long levelIndex = RmazorUtils.GetFirstLevelInGroupIndex(_LevelGroupIndex) + _LevelInGroupIndex;
            string levelType = _LevelGroupIndex == -1
                ? ParameterLevelTypeBonus
                : ParameterLevelTypeDefault;
            var args = new LevelInfoArgs
            {
                GameMode = ParameterGameModeMain,
                LevelType = levelType
            };
            bool isLevelExist = levelIndex < LevelsLoader.GetLevelsCount(args);
            if (isLevelExist)
            {
                OnButtonCloseClick();
                InvokeStartUnloadingLevel(_LevelGroupIndex, _LevelInGroupIndex);
            }
            else
            {
                 MazorCommonUtils.ShowAlertDialog("OOPS", "I didn't create this level yet. Try to load it some later.");   
            }
        }
        
        private void InvokeStartUnloadingLevel(int _LevelGroupIndex, int _LevelInGroupIndex)
        {
            var args = new Dictionary<string, object>();
            long levelIndex;
            if (_LevelInGroupIndex == -1)
            {
                args.SetSafe(KeyNextLevelType, ParameterLevelTypeBonus);
                levelIndex = _LevelGroupIndex - 1;
            }
            else
            {
                args.SetSafe(KeyNextLevelType, ParameterLevelTypeDefault);
                levelIndex = RmazorUtils.GetFirstLevelInGroupIndex(_LevelGroupIndex) + _LevelInGroupIndex;
            }
            args.SetSafe(KeyLevelIndex, levelIndex);
            args.SetSafe(KeySource, ParameterSourceLevelsPanel);
            LevelStageSwitcher.SwitchLevelStage(EInputCommand.StartUnloadingLevel, args);
        }

        private int GetCurrentLevelGroupGroupIndex()
        {
            int levelGroupIndex = RmazorUtils.GetLevelsGroupIndex(Model.LevelStaging.LevelIndex);
            return Mathf.FloorToInt(levelGroupIndex / (float)LevelGroupItemsCountOnPage);
        }

        #endregion
    }
}