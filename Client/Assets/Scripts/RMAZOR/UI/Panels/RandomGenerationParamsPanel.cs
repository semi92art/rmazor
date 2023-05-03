using System.Collections.Generic;
using System.Globalization;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.Utils;
using RMAZOR.Managers;
using RMAZOR.UI.Utils;
using RMAZOR.Views.Common;
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.UI.Panels
{
    public interface IRandomGenerationParamsPanel : IDialogPanel
    {
        UnityAction OnReadyToLoadLevelAction { get; set; }
    }
    
    public class RandomGenerationParamsPanelFake : DialogPanelFake, IRandomGenerationParamsPanel
    {
        public UnityAction OnReadyToLoadLevelAction { get; set; }
    }
    
    public class RandomGenerationParamsPanel : DialogPanelBase, IRandomGenerationParamsPanel
    {
        #region nonpublic members
        
        private Button m_ButtonPlay, m_ButtonClose;

        private TextMeshProUGUI
            m_ButtonPlayText,
            m_LevelSizeText,
            m_SlowGenWarningText;

        private Toggle
            m_ToggleLevelSizeSmall,
            m_ToggleLevelSizeMedium,
            m_ToggleLevelSizeBig,
            m_ToggleLevelSizeVeryBig,
            m_ToggleLevelSizeVeryVeryBig;
        
        private TextMeshProUGUI
            m_ToggleLevelSizeSmallText,
            m_ToggleLevelSizeMediumText,
            m_ToggleLevelSizeBigText,
            m_ToggleLevelSizeVeryBigText,
            m_ToggleLevelSizeVeryVeryBigText;

        private int m_LevelSizeValue;

        protected override string PrefabName => "random_generation_params_panel";
        
        #endregion

        #region inject

        private ViewSettings                    ViewSettings           { get; }
        private IViewFullscreenTransitioner     FullscreenTransitioner { get; }
        private IViewLevelStageSwitcher         LevelStageSwitcher     { get; }
        private IViewGameUiCreatingLevelMessage CreatingLevelMessage   { get; }

        private RandomGenerationParamsPanel(
            ViewSettings                    _ViewSettings,
            IViewFullscreenTransitioner     _FullscreenTransitioner,
            IManagersGetter                 _Managers,
            IUITicker                       _Ticker,
            ICameraProvider                 _CameraProvider,
            IColorProvider                  _ColorProvider,
            IViewTimePauser                 _TimePauser,
            IViewInputCommandsProceeder     _CommandsProceeder,
            IViewLevelStageSwitcher         _LevelStageSwitcher,
            IViewGameUiCreatingLevelMessage _CreatingLevelMessage)
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
            LevelStageSwitcher     = _LevelStageSwitcher;
            CreatingLevelMessage   = _CreatingLevelMessage;
        }

        #endregion

        #region api

        public UnityAction OnReadyToLoadLevelAction { get; set; }

        public override int DialogViewerId => DialogViewerIdsCommon.FullscreenCommon;

        #endregion

        #region nonpublic methods
        
        protected override void LoadPanelCore(RectTransform _Container, ClosePanelAction _OnClose)
        {
            base.LoadPanelCore(_Container, _OnClose);
            m_ToggleLevelSizeMedium.isOn = true;
        }

        protected override void GetPrefabContentObjects(GameObject _Go)
        {
            m_ButtonPlay                     = _Go.GetCompItem<Button>("button_play");
            m_ButtonClose                    = _Go.GetCompItem<Button>("button_close");
            m_ButtonPlayText                 = _Go.GetCompItem<TextMeshProUGUI>("button_play_text");
            m_LevelSizeText                  = _Go.GetCompItem<TextMeshProUGUI>("level_size_text");
            m_ToggleLevelSizeSmall           = _Go.GetCompItem<Toggle>("toggle_level_size_small");
            m_ToggleLevelSizeMedium          = _Go.GetCompItem<Toggle>("toggle_level_size_medium");
            m_ToggleLevelSizeBig             = _Go.GetCompItem<Toggle>("toggle_level_size_big");
            m_ToggleLevelSizeVeryBig         = _Go.GetCompItem<Toggle>("toggle_level_size_very_big");
            m_ToggleLevelSizeVeryVeryBig     = _Go.GetCompItem<Toggle>("toggle_level_size_very_very_big");
            m_ToggleLevelSizeSmallText       = _Go.GetCompItem<TextMeshProUGUI>("toggle_level_size_small_text");
            m_ToggleLevelSizeMediumText      = _Go.GetCompItem<TextMeshProUGUI>("toggle_level_size_medium_text");
            m_ToggleLevelSizeBigText         = _Go.GetCompItem<TextMeshProUGUI>("toggle_level_size_big_text");
            m_ToggleLevelSizeVeryBigText     = _Go.GetCompItem<TextMeshProUGUI>("toggle_level_size_very_big_text");
            m_ToggleLevelSizeVeryVeryBigText = _Go.GetCompItem<TextMeshProUGUI>("toggle_level_size_very_very_big_text");
            m_SlowGenWarningText             = _Go.GetCompItem<TextMeshProUGUI>("slow_gen_warning_text");
        }

        protected override void LocalizeTextObjectsOnLoad()
        {
            static string TextFormula(string _Text) => _Text.ToUpper(CultureInfo.CurrentUICulture);
            var locTextInfos = new[]
            {
                new LocTextInfo(m_ButtonPlayText,                 ETextType.MenuUI_H1, "Play",             TextFormula),
                new LocTextInfo(m_LevelSizeText,                  ETextType.MenuUI_H1, "level_size",       TextFormula),
                new LocTextInfo(m_ToggleLevelSizeSmallText,       ETextType.MenuUI_H1, "small",            TextFormula), 
                new LocTextInfo(m_ToggleLevelSizeMediumText,      ETextType.MenuUI_H1, "medium",           TextFormula), 
                new LocTextInfo(m_ToggleLevelSizeBigText,         ETextType.MenuUI_H1, "big",              TextFormula), 
                new LocTextInfo(m_ToggleLevelSizeVeryBigText,     ETextType.MenuUI_H1, "very_big",         TextFormula), 
                new LocTextInfo(m_ToggleLevelSizeVeryVeryBigText, ETextType.MenuUI_H1, "very_very_big",    TextFormula), 
                new LocTextInfo(m_SlowGenWarningText,             ETextType.MenuUI_H1, "slow_gen_warning", TextFormula),
            };
            foreach (var locTextInfo in locTextInfos)
                Managers.LocalizationManager.AddLocalization(locTextInfo);
        }

        protected override void SubscribeButtonEvents()
        {
            m_ButtonPlay                .onClick       .AddListener(OnButtonPlayClick);
            m_ButtonClose               .onClick       .AddListener(OnButtonCloseClick);
            m_ToggleLevelSizeSmall      .onValueChanged.AddListener(OnToggleLevelSizeSmallValueChanged);
            m_ToggleLevelSizeMedium     .onValueChanged.AddListener(OnToggleLevelSizeMediumValueChanged);
            m_ToggleLevelSizeBig        .onValueChanged.AddListener(OnToggleLevelSizeBigValueChanged);
            m_ToggleLevelSizeVeryBig    .onValueChanged.AddListener(OnToggleLevelSizeVeryBigValueChanged);
            m_ToggleLevelSizeVeryVeryBig.onValueChanged.AddListener(OnToggleLevelSizeVeryVeryBigValueChanged);
        }

        private void OnButtonPlayClick()
        {
            PlayButtonClickSound();
            LoadRandomLevel();
        }

        private void OnButtonCloseClick()
        {
            PlayButtonClickSound();
            OnClose();
        }

        private void OnToggleLevelSizeSmallValueChanged(bool _Value)
        {
            if (_Value)
                m_LevelSizeValue = 0;
        }
        
        private void OnToggleLevelSizeMediumValueChanged(bool _Value)
        {
            if (_Value)
                m_LevelSizeValue = 1;
        }
        
        private void OnToggleLevelSizeBigValueChanged(bool _Value)
        {
            if (_Value)
                m_LevelSizeValue = 2;
        }
        
        private void OnToggleLevelSizeVeryBigValueChanged(bool _Value)
        {
            if (_Value)
                m_LevelSizeValue = 3;
        }
        
        private void OnToggleLevelSizeVeryVeryBigValueChanged(bool _Value)
        {
            if (_Value)
                m_LevelSizeValue = 4;
        }

        private void LoadRandomLevel()
        {
            var args = new Dictionary<string, object>
            {
                {KeyGameMode,                       ParameterGameModeRandom},
                {KeyLevelIndex,                     0L},
                {KeyRandomLevelSize,                m_LevelSizeValue},
                {KeyOnReadyToLoadLevelFinishAction, OnReadyToLoadLevelAction},
                {KeyAiSimulation,                   false}
            };
            OnClose();
            var loadLevelCoroutine = MainMenuUtils.LoadLevelCoroutine(
                args, ViewSettings, FullscreenTransitioner, Ticker, LevelStageSwitcher);
            Cor.Run(Cor.Delay(
                ViewSettings.betweenLevelTransitionTime * 0.8f, Ticker, CreatingLevelMessage.ShowMessage));
            Cor.Run(loadLevelCoroutine);
        }

        #endregion
    }
}