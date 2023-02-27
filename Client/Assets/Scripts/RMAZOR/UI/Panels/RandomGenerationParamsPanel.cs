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
    
    public class RandomGenerationParamsPanelFake : FakeDialogPanel, IRandomGenerationParamsPanel
    {
        public UnityAction OnReadyToLoadLevelAction { get; set; }
    }
    
    public class RandomGenerationParamsPanel : DialogPanelBase, IRandomGenerationParamsPanel
    {
        #region constants

        private const float SlowGenerationWarningLeveSizeThreshold = 0.7f;

        #endregion
        
        #region nonpublic members
        
        private Slider     m_SliderLevelSize;
        private Button     m_ButtonPlay, m_ButtonClose, m_ButtonAiSim;
        private Image      m_SlowGenWarningIcon;
        private GameObject m_AiSimToggleOnObj, m_AiSimToggleOffObj;

        private TextMeshProUGUI
            m_ButtonPlayText,
            m_SlowGenWarningText,
            m_SliderStartText,
            m_SliderEndText,
            m_LevelSizeText,
            m_AiSimText;

        private float m_LevelSizeValue;
        private bool  m_AiSimulationOn;

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

        protected override void OnDialogStartAppearing()
        {
            base.OnDialogStartAppearing();
            SetAiSimToggleOnOff();
            m_LevelSizeValue = m_SliderLevelSize.value;
            CheckForSlowGenerationWarning();
        }

        protected override void GetPrefabContentObjects(GameObject _Go)
        {
            m_ButtonPlay         = _Go.GetCompItem<Button>("button_play");
            m_ButtonClose        = _Go.GetCompItem<Button>("button_close");
            m_ButtonPlayText     = _Go.GetCompItem<TextMeshProUGUI>("button_play_text");
            m_SliderLevelSize    = _Go.GetCompItem<Slider>("slider_level_size");
            m_SliderStartText    = _Go.GetCompItem<TextMeshProUGUI>("slider_start_text");
            m_SliderEndText      = _Go.GetCompItem<TextMeshProUGUI>("slider_end_text");
            m_LevelSizeText      = _Go.GetCompItem<TextMeshProUGUI>("level_size_text");
            m_SlowGenWarningText = _Go.GetCompItem<TextMeshProUGUI>("slow_gen_warning_text");
            m_SlowGenWarningIcon = _Go.GetCompItem<Image>("slow_gen_warning_icon");
            m_AiSimText          = _Go.GetCompItem<TextMeshProUGUI>("ai_simulation_text");
            m_AiSimToggleOnObj   = _Go.GetContentItem("ai_simulation_toggle_on");
            m_AiSimToggleOffObj  = _Go.GetContentItem("ai_simulation_toggle_off");
            m_ButtonAiSim        = _Go.GetCompItem<Button>("ai_simulation_button");
        }

        protected override void LocalizeTextObjectsOnLoad()
        {
            static string TextFormula(string _Text) => _Text.ToUpper(CultureInfo.CurrentUICulture);
            var locTextInfos = new[]
            {
                new LocTextInfo(m_ButtonPlayText,     ETextType.MenuUI, "Play",             TextFormula),
                new LocTextInfo(m_SlowGenWarningText, ETextType.MenuUI, "slow_gen_warning", TextFormula),
                new LocTextInfo(m_LevelSizeText,      ETextType.MenuUI, "level_size",       TextFormula),
                new LocTextInfo(m_SliderStartText,    ETextType.MenuUI, "small",            TextFormula),
                new LocTextInfo(m_SliderEndText,      ETextType.MenuUI, "big",              TextFormula), 
                new LocTextInfo(m_AiSimText,          ETextType.MenuUI, "ai_simulation",    TextFormula), 
            };
            foreach (var locTextInfo in locTextInfos)
                Managers.LocalizationManager.AddLocalization(locTextInfo);
        }

        protected override void SubscribeButtonEvents()
        {
            m_ButtonPlay     .onClick.AddListener(OnButtonPlayClick);
            m_ButtonClose    .onClick.AddListener(OnButtonCloseClick);
            m_SliderLevelSize.onValueChanged.AddListener(OnSliderLevelSizeValueChanged);
            m_ButtonAiSim    .onClick.AddListener(OnAiSimButtonClick);
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

        private void OnSliderLevelSizeValueChanged(float _Value)
        {
            m_LevelSizeValue = _Value;
            CheckForSlowGenerationWarning();
        }

        private void CheckForSlowGenerationWarning()
        {
            bool enableWarning = m_LevelSizeValue > SlowGenerationWarningLeveSizeThreshold;
            m_SlowGenWarningText.enabled = enableWarning;
            m_SlowGenWarningIcon.enabled = enableWarning;
        }

        private void OnAiSimButtonClick()
        {
            PlayButtonClickSound();
            m_AiSimulationOn = !m_AiSimulationOn;
            SetAiSimToggleOnOff();
        }
        
        private void SetAiSimToggleOnOff()
        {
            m_AiSimToggleOnObj.SetActive(m_AiSimulationOn);
            m_AiSimToggleOffObj.SetActive(!m_AiSimulationOn);
        }
        
        private void LoadRandomLevel()
        {
            var args = new Dictionary<string, object>
            {
                {KeyGameMode,                       ParameterGameModeRandom},
                {KeyLevelIndex,                     0L},
                {KeyRandomLevelSize,                m_LevelSizeValue},
                {KeyOnReadyToLoadLevelFinishAction, OnReadyToLoadLevelAction},
                {KeyAiSimulation,                   m_AiSimulationOn}
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