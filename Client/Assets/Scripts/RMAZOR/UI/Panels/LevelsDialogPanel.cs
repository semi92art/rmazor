using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Common.Constants;
using Common.Entities;
using Common.Extensions;
using Common.UI;
using Common.Utils;
using mazing.common.Runtime;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Entities.UI;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
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
using RMAZOR.Views.InputConfigurators;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RMAZOR.UI.Panels
{
    public interface ILevelsDialogPanel : IDialogPanel { }
    
    public class LevelsDialogPanelFake : ILevelsDialogPanel
    {
        public EDialogViewerType DialogViewerType   => default;
        public EAppearingState   AppearingState     { get; set; }
        public RectTransform     PanelRectTransform => null;
        public Animator          Animator           => null;
        
        public void LoadPanel(RectTransform _Container, ClosePanelAction _OnClose) { }
    }
     
    public class LevelsDialogPanel : DialogPanelBase, ILevelsDialogPanel
    {
        #region constants

        private const int LevelGroupItemsCount = 4;

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
        
        private readonly Dictionary<int, LevelsPanelGroupItem> m_LevelPanelGroupItems 
            = new Dictionary<int, LevelsPanelGroupItem>();

        private int m_SelectedLevelGroupGroupIndex = -1;

        #endregion
        
        #region inject

        private ILevelsLoader                       LevelsLoader                   { get; }
        private IRawLevelInfoGetter                 RawLevelInfoGetter             { get; }
        private IModelGame                          Model                          { get; }
        private IViewSwitchLevelStageCommandInvoker SwitchLevelStageCommandInvoker { get; }
        private IConfirmLoadLevelDialogPanel        ConfirmLoadLevelDialogPanel    { get; }
        private IDialogViewersController            DialogViewersController        { get; }
        private IFontProvider                       FontProvider                   { get; }

        private LevelsDialogPanel(
            ILevelsLoader                       _LevelsLoader,
            IRawLevelInfoGetter                 _RawLevelInfoGetter,
            IManagersGetter                     _Managers,
            IUITicker                           _Ticker,
            ICameraProvider                     _CameraProvider,
            IColorProvider                      _ColorProvider,
            IViewTimePauser                     _TimePauser,
            IModelGame                          _Model,
            IViewInputCommandsProceeder         _CommandsProceeder,
            IViewSwitchLevelStageCommandInvoker _SwitchLevelStageCommandInvoker,
            IConfirmLoadLevelDialogPanel        _ConfirmLoadLevelDialogPanel,
            IDialogViewersController            _DialogViewersController,
            IFontProvider                       _FontProvider)
            : base(
                _Managers,
                _Ticker,
                _CameraProvider, 
                _ColorProvider,
                _TimePauser,
                _CommandsProceeder)
        {
            LevelsLoader                   = _LevelsLoader;
            RawLevelInfoGetter             = _RawLevelInfoGetter;
            Model                          = _Model;
            SwitchLevelStageCommandInvoker = _SwitchLevelStageCommandInvoker;
            ConfirmLoadLevelDialogPanel    = _ConfirmLoadLevelDialogPanel;
            DialogViewersController        = _DialogViewersController;
            FontProvider = _FontProvider;
        }

        #endregion

        #region api
        
        public override EDialogViewerType DialogViewerType => EDialogViewerType.Medium1;
        public override Animator          Animator         => m_PanelAnimator;

        public override void LoadPanel(RectTransform _Container, ClosePanelAction _OnClose)
        {
            base.LoadPanel(_Container, _OnClose);
            var go  = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(_Container, RectTransformLite.FullFill),
                CommonPrefabSetNames.DialogPanels, "levels_panel");
            PanelRectTransform = go.RTransform();
            PanelRectTransform.SetGoActive(false);
            LoadLevelGroupButtonSprites();
            GetPrefabContentObjects(go);
            LocalizeTexts();
            SubscribeButtonEvents();
        }

        public override void OnDialogStartAppearing()
        {
            if (m_SelectedLevelGroupGroupIndex == -1)
                m_SelectedLevelGroupGroupIndex = GetCurrentLevelGroupGroupIndex();
            UpdatePrevAndNextButtonStates();
            UpdateLevelGroupItems();
            base.OnDialogStartAppearing();
        }

        #endregion

        #region nonpublic methods
        
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
            base.OnClose(() =>
            {
                SwitchLevelStageCommandInvoker.SwitchLevelStage(EInputCommand.UnPauseLevel);
            });
            Managers.AudioManager.PlayClip(CommonAudioClipArgs.UiButtonClick);
        }
        
        private void GetPrefabContentObjects(GameObject _GameObject)
        {
            var go = _GameObject;
            m_PanelAnimator        = go.GetCompItem<Animator>("animator");
            m_TitleText            = go.GetCompItem<TextMeshProUGUI>("title_text");
            m_ButtonPreviousGroups = go.GetCompItem<Button>("button_previous_stages");
            m_ButtonNextGroups     = go.GetCompItem<Button>("button_next_stages");
            m_ButtonClose          = go.GetCompItem<Button>("button_close");
            for (int i = 1; i <= LevelGroupItemsCount; i++)
            {
                var levelGroupItem = go.GetCompItem<LevelsPanelGroupItem>($"stage_item_{i}");
                levelGroupItem.Init(
                    LevelsLoader,
                    RawLevelInfoGetter,
                    Model,
                    Ticker,
                    Managers.AudioManager,
                    Managers.LocalizationManager,
                    FontProvider,
                    m_SpriteLevelGroupItemEnabled, 
                    m_SpriteLevelGroupItemDisabled,
                    m_SpriteLevelGroupItemSelected,
                    m_SpriteLevelGroupItemStarEnabled,
                    m_SpriteLevelGroupItemStarDisabled);
                m_LevelPanelGroupItems.Add(i, levelGroupItem);
            }
        }

        private void LocalizeTexts()
        {
            var locMan = Managers.LocalizationManager;
            locMan.AddTextObject(new LocalizableTextObjectInfo(
                m_TitleText,
                ETextType.MenuUI, 
                "stages",
                _Text => _Text.ToUpper(CultureInfo.CurrentUICulture)));
        }

        private void LoadLevelGroupButtonSprites()
        {
            var psm = Managers.PrefabSetManager;
            m_SpriteLevelGroupItemEnabled = psm.GetObject<Sprite>(
                "views", 
                "level_stage_item_enabled_sprite");
            m_SpriteLevelGroupItemDisabled = psm.GetObject<Sprite>(
                "views", 
                "level_stage_item_disabled_sprite");
            m_SpriteLevelGroupItemSelected = psm.GetObject<Sprite>(
                "views", 
                "level_stage_item_selected_sprite");
            m_SpriteLevelGroupItemStarEnabled = psm.GetObject<Sprite>(
                "views", 
                "level_stage_item_star_enabled_sprite");
            m_SpriteLevelGroupItemStarDisabled = psm.GetObject<Sprite>(
                "views", 
                "level_stage_item_star_disabled_sprite");
        }

        private void SubscribeButtonEvents()
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
                CommonInputCommandArg.KeyCurrentLevelType, out _);
            bool isCurrentLevelBonus = currentLevelType == CommonInputCommandArg.ParameterLevelTypeBonus;
            int currentLevelGroupIdx = isCurrentLevelBonus ? 
                (int)Model.LevelStaging.LevelIndex + 1 
                : RmazorUtils.GetLevelsGroupIndex(Model.LevelStaging.LevelIndex);
            int currentLevelsGroupGroupIndex = GetCurrentLevelGroupGroupIndex();
            int startLevelGroupIndexInGroup = m_SelectedLevelGroupGroupIndex * LevelGroupItemsCount;
            bool isUnknownStage = m_SelectedLevelGroupGroupIndex > currentLevelsGroupGroupIndex + 1;
            for (int i = 1; i <= LevelGroupItemsCount; i++)
            {
                bool? stageEnabled = null;
                int iLevelsGroup = startLevelGroupIndexInGroup + i;
                if (!isUnknownStage)
                    stageEnabled = iLevelsGroup <= maxLevelGroup;
                m_LevelPanelGroupItems[i].UpdateItem(
                    iLevelsGroup,
                    currentLevelGroupIdx,
                    stageEnabled, 
                    _LevelInGroupIndex =>
                    {
                        long currentLevelIdxInGroup = RmazorUtils.GetIndexInGroup(Model.LevelStaging.LevelIndex);   
                        if (RmazorUtils.WasLevelGroupFinishedBefore(currentLevelGroupIdx) || currentLevelIdxInGroup == 0 || isCurrentLevelBonus)
                        {
                            LoadLevel(iLevelsGroup, _LevelInGroupIndex);
                        }
                        else
                        {
                            ConfirmLoadLevelDialogPanel.SetLevelGroupAndIndex(iLevelsGroup, _LevelInGroupIndex);
                            var dv = DialogViewersController.GetViewer(ConfirmLoadLevelDialogPanel.DialogViewerType);
                            dv.Show(ConfirmLoadLevelDialogPanel);
                        }
                    });
            }
        }

        private void LoadLevel(int _LevelGroupIndex, int _LevelInGroupIndex)
        {
            Managers.AudioManager.PlayClip(CommonAudioClipArgs.UiButtonClick);
            long levelIndex = RmazorUtils.GetFirstLevelInGroupIndex(_LevelGroupIndex) + _LevelInGroupIndex;
            bool isLevelExist = levelIndex < LevelsLoader.GetLevelsCount(
                CommonData.GameId, _LevelGroupIndex == -1);
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
                args.SetSafe(CommonInputCommandArg.KeyNextLevelType, CommonInputCommandArg.ParameterLevelTypeBonus);
                levelIndex = _LevelGroupIndex - 1;
            }
            else
            {
                args.SetSafe(CommonInputCommandArg.KeyNextLevelType, CommonInputCommandArg.ParameterLevelTypeMain);
                levelIndex = RmazorUtils.GetFirstLevelInGroupIndex(_LevelGroupIndex) + _LevelInGroupIndex;
            }
            args.SetSafe(CommonInputCommandArg.KeyLevelIndex, levelIndex);
            args.SetSafe(CommonInputCommandArg.KeySource, CommonInputCommandArg.ParameterLevelsPanel);
            SwitchLevelStageCommandInvoker.SwitchLevelStage(EInputCommand.StartUnloadingLevel, args);
        }

        private int GetCurrentLevelGroupGroupIndex()
        {
            int levelGroupIndex = RmazorUtils.GetLevelsGroupIndex(Model.LevelStaging.LevelIndex);
            return Mathf.FloorToInt(levelGroupIndex / (float)LevelGroupItemsCount);
        }

        #endregion
    }
}