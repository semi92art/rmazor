using System.Collections.Generic;
using System.Globalization;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Views.Common;
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using RMAZOR.Views.InputConfigurators;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.UI.Panels
{
    public interface IConfirmLoadLevelDialogPanel : IDialogPanel
    {
        void SetLevelGroupAndIndex(
            int _LevelGroupIndex,
            int _LevelIndexInGroup);
    }
    
    public class ConfirmLoadLevelDialogPanelFake : FakeDialogPanel, IConfirmLoadLevelDialogPanel
    {
        public void SetLevelGroupAndIndex(
            int _LevelGroupIndex,
            int _LevelIndexInGroup) { }
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
        
        protected override string PrefabName => "confirm_load_level_panel";
        
        #endregion
        
        #region inject

        private IViewLevelStageSwitcher LevelStageSwitcher { get; }

        private ConfirmLoadLevelDialogPanel(
            IManagersGetter                     _Managers,
            IUITicker                           _Ticker,
            ICameraProvider                     _CameraProvider,
            IColorProvider                      _ColorProvider,
            IViewTimePauser                     _TimePauser,
            IViewInputCommandsProceeder         _CommandsProceeder,
            IViewLevelStageSwitcher _LevelStageSwitcher)
            : base(
                _Managers,
                _Ticker,
                _CameraProvider, 
                _ColorProvider,
                _TimePauser,
                _CommandsProceeder)
        {
            LevelStageSwitcher = _LevelStageSwitcher;
        }

        #endregion

        #region api

        public override int DialogViewerId => DialogViewerIdsCommon.MediumCommon;
        
        public void SetLevelGroupAndIndex(
            int         _LevelGroupIndex,
            int         _LevelIndexInGroup)
        {
            m_LevelGroupIndex   = _LevelGroupIndex;
            m_LevelIndexInGroup = _LevelIndexInGroup;
        }

        #endregion

        #region nonpublic methods

        protected override void GetPrefabContentObjects(GameObject _Go)
        {
            m_ButtonYes     = _Go.GetCompItem<Button>("button_yes");
            m_ButtonNo      = _Go.GetCompItem<Button>("button_no");
            m_TitleText     = _Go.GetCompItem<TextMeshProUGUI>("title_text");
            m_ButtonYesText = _Go.GetCompItem<TextMeshProUGUI>("button_yes_text");
            m_ButtonNoText  = _Go.GetCompItem<TextMeshProUGUI>("button_no_text");
        }

        protected override void LocalizeTextObjectsOnLoad()
        {
            static string TextFormula(string _Text) => _Text.ToUpper(CultureInfo.CurrentUICulture);
            var locTextInfos = new[]
            {
                new LocTextInfo(m_TitleText, ETextType.MenuUI, "load_level_warning"),
                new LocTextInfo(m_ButtonNoText, ETextType.MenuUI, "back", TextFormula),
                new LocTextInfo(m_ButtonYesText, ETextType.MenuUI, "Play", TextFormula)
            };
            foreach (var locTextInfo in locTextInfos)
                Managers.LocalizationManager.AddLocalization(locTextInfo);
        }

        protected override void SubscribeButtonEvents()
        {
            m_ButtonYes.onClick.AddListener(OnButtonYesClick);
            m_ButtonNo.onClick.AddListener(OnButtonNoClick);
        }

        private void OnButtonYesClick()
        {
            OnClose(LoadLevel);
        }

        private void OnButtonNoClick()
        {
            OnClose(() =>
            {
                LevelStageSwitcher.SwitchLevelStage(EInputCommand.UnPauseLevel);
            });
        }

        private void LoadLevel()
        {
            OnClose(() =>
            {
                LevelStageSwitcher.SwitchLevelStage(EInputCommand.UnPauseLevel);
            });
            PlayButtonClickSound();
            PlayButtonClickSound();
            InvokeStartUnloadingLevel(m_LevelGroupIndex, m_LevelIndexInGroup);
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

        #endregion
    }
}