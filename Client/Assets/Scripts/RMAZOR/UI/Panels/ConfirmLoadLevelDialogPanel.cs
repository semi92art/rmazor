using System.Collections.Generic;
using System.Globalization;
using Common.Constants;
using Common.Entities;
using Common.Extensions;
using Common.UI;
using Common.Utils;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Entities.UI;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Views.Common;
using RMAZOR.Views.InputConfigurators;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RMAZOR.UI.Panels
{
    public interface IConfirmLoadLevelDialogPanel : IDialogPanel
    {
        void SetLevelGroupAndIndex(
            int         _LevelGroupIndex, 
            int         _LevelIndexInGroup);
    }
    
    public class ConfirmLoadLevelDialogPanelFake : IConfirmLoadLevelDialogPanel
    {
        public EDialogViewerType DialogViewerType   => default;
        public EAppearingState   AppearingState     { get; set; }
        public RectTransform     PanelRectTransform => null;
        public Animator          Animator           => null;
        
        public void LoadPanel(RectTransform _Container, ClosePanelAction _OnClose) { }

        public void SetLevelGroupAndIndex(
            int         _LevelGroupIndex,
            int         _LevelIndexInGroup) { }
    }
     
    public class ConfirmLoadLevelDialogPanel : DialogPanelBase, IConfirmLoadLevelDialogPanel
    {
        #region nonpublic members
        
        private TextMeshProUGUI 
            m_TitleText, 
            m_ButtonYesText, 
            m_ButtonNoText;
        private Button
            m_ButtonYes,
            m_ButtonNo;

        private int  m_LevelGroupIndex, m_LevelIndexInGroup;
        
        #endregion
        
        #region inject

        private IViewSwitchLevelStageCommandInvoker SwitchLevelStageCommandInvoker { get; }

        private ConfirmLoadLevelDialogPanel(
            IManagersGetter                     _Managers,
            IUITicker                           _Ticker,
            ICameraProvider                     _CameraProvider,
            IColorProvider                      _ColorProvider,
            IViewTimePauser                     _TimePauser,
            IViewInputCommandsProceeder         _CommandsProceeder,
            IViewSwitchLevelStageCommandInvoker _SwitchLevelStageCommandInvoker)
            : base(
                _Managers,
                _Ticker,
                _CameraProvider, 
                _ColorProvider,
                _TimePauser,
                _CommandsProceeder)
        {
            SwitchLevelStageCommandInvoker = _SwitchLevelStageCommandInvoker;
        }

        #endregion

        #region api
        
        public override EDialogViewerType DialogViewerType => EDialogViewerType.Medium1;

        public override void LoadPanel(RectTransform _Container, ClosePanelAction _OnClose)
        {
            base.LoadPanel(_Container, _OnClose);
            var go  = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(_Container, RectTransformLite.FullFill),
                CommonPrefabSetNames.DialogPanels, "confirm_load_level_panel");
            PanelRectTransform = go.RTransform();
            PanelRectTransform.SetGoActive(false);
            GetPrefabContentObjects(go);
            LocalizeTexts();
            SubscribeButtonEvents();
        }

        public void SetLevelGroupAndIndex(
            int         _LevelGroupIndex,
            int         _LevelIndexInGroup)
        {
            m_LevelGroupIndex   = _LevelGroupIndex;
            m_LevelIndexInGroup = _LevelIndexInGroup;
        }

        #endregion

        #region nonpublic methods

        private void GetPrefabContentObjects(GameObject _GameObject)
        {
            var go = _GameObject;
            m_ButtonYes     = go.GetCompItem<Button>("button_yes");
            m_ButtonNo      = go.GetCompItem<Button>("button_no");
            m_TitleText     = go.GetCompItem<TextMeshProUGUI>("title_text");
            m_ButtonYesText = go.GetCompItem<TextMeshProUGUI>("button_yes_text");
            m_ButtonNoText  = go.GetCompItem<TextMeshProUGUI>("button_no_text");
        }

        private void LocalizeTexts()
        {
            var locMan = Managers.LocalizationManager;
            var locInfo = new LocalizableTextObjectInfo(
                m_TitleText, ETextType.MenuUI, "load_level_warning");
            locMan.AddTextObject(locInfo);
            locInfo = new LocalizableTextObjectInfo(
                m_ButtonNoText, ETextType.MenuUI, "back", 
                _T => _T.ToUpper(CultureInfo.CurrentUICulture));
            locMan.AddTextObject(locInfo);
            locInfo = new LocalizableTextObjectInfo(
                m_ButtonYesText, ETextType.MenuUI, "Play",
                _T => _T.ToUpper(CultureInfo.CurrentUICulture));
            locMan.AddTextObject(locInfo);
        }

        private void SubscribeButtonEvents()
        {
            m_ButtonYes.onClick.AddListener(OnButtonYesClick);
            m_ButtonNo.onClick.AddListener(OnButtonNoClick);
        }

        private void OnButtonYesClick()
        {
            base.OnClose(LoadLevel);
        }

        private void OnButtonNoClick()
        {
            base.OnClose(null);
        }

        private void LoadLevel()
        {
            base.OnClose(() =>
            {
                SwitchLevelStageCommandInvoker.SwitchLevelStage(EInputCommand.UnPauseLevel);
            });
            Managers.AudioManager.PlayClip(CommonAudioClipArgs.UiButtonClick);
            Managers.AudioManager.PlayClip(CommonAudioClipArgs.UiButtonClick);
            InvokeStartUnloadingLevel(m_LevelGroupIndex, m_LevelIndexInGroup);
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

        #endregion
    }
}