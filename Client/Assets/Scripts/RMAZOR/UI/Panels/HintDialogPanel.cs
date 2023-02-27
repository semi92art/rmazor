using System.Collections.Generic;
using System.Globalization;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.Utils;
using RMAZOR.Constants;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.UI.Utils;
using RMAZOR.Views.Common;
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using RMAZOR.Views.InputConfigurators;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using LocTextInfo = mazing.common.Runtime.Entities.LocTextInfo;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.UI.Panels
{
    public interface IHintDialogPanel : IDialogPanel
    {
        UnityAction OnClosePanelAction { get; set; }
    }
    
    public class HintDialogPanelFake : DialogPanelFake, IHintDialogPanel
    {
        public UnityAction OnClosePanelAction { get; set; }
    }
    
    public class HintDialogPanel : DialogPanelBase, IHintDialogPanel
    {
        #region nonpublic members

        private Animator
            m_PanelAnimator,
            m_AnimLoadingAds;

        private Image m_IconWatchAds;
        
        private TextMeshProUGUI
            m_TitleText,
            m_ButtonShowHintText;

        private Button
            m_ButtonClose,
            m_ButtonShowHint;

        private bool m_PanelShowing;
        
        protected override string PrefabName => "hint_panel";

        #endregion

        #region inject

        private ViewSettings                ViewSettings           { get; }
        private IViewFullscreenTransitioner FullscreenTransitioner { get; }
        private IViewLevelStageSwitcher     LevelStageSwitcher     { get; }
        private IModelGame                  Model                  { get; }

        private HintDialogPanel(
            ViewSettings                _ViewSettings,
            IViewFullscreenTransitioner _FullscreenTransitioner,
            IManagersGetter             _Managers,
            IUITicker                   _Ticker,
            ICameraProvider             _CameraProvider,
            IColorProvider              _ColorProvider,
            IViewTimePauser             _TimePauser,
            IViewInputCommandsProceeder _CommandsProceeder,
            IViewLevelStageSwitcher     _LevelStageSwitcher,
            IModelGame                  _Model) 
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
            Model                  = _Model;
        }

        #endregion

        #region api
        
        public override    int    DialogViewerId => DialogViewerIdsCommon.MediumCommon;

        public override Animator Animator => m_PanelAnimator;
        
        public UnityAction OnClosePanelAction { get; set; }

        #endregion

        #region nonpublic methods
        
        protected override void OnDialogStartAppearing()
        {
            base.OnDialogStartAppearing();
            m_PanelShowing = true;
            IndicateAdsLoading(true);
            Cor.Run(Cor.WaitWhile(
                () => !Managers.AdsManager.RewardedAdReady,
                () => IndicateAdsLoading(false),
                () => !m_PanelShowing));
        }

        protected override void OnDialogDisappeared()
        {
            base.OnDialogDisappeared();
            m_PanelShowing = false;
        }

        protected override void GetPrefabContentObjects(GameObject _Go)
        {
            m_PanelAnimator      = _Go.GetCompItem<Animator>("animator");
            m_AnimLoadingAds     = _Go.GetCompItem<Animator>("loading_ads_anim");
            m_IconWatchAds       = _Go.GetCompItem<Image>("watch_ads_icon");
            m_ButtonClose        = _Go.GetCompItem<Button>("button_close");
            m_ButtonShowHint     = _Go.GetCompItem<Button>("button_show_hint");
            m_TitleText          = _Go.GetCompItem<TextMeshProUGUI>("title_text");
            m_ButtonShowHintText = _Go.GetCompItem<TextMeshProUGUI>("button_show_hint_text");
        }

        protected override void LocalizeTextObjectsOnLoad()
        {
            static string TextFormula(string _Text) => _Text.ToUpper(CultureInfo.CurrentUICulture);
            var locMan = Managers.LocalizationManager;
            var locInfos = new[]
            {
                new LocTextInfo(m_ButtonShowHintText, ETextType.MenuUI, "show_hint", TextFormula),
                new LocTextInfo(m_TitleText, ETextType.MenuUI, "hint_panel_title", TextFormula)
            };
            foreach (var locInfo in locInfos)
                locMan.AddLocalization(locInfo);
        }

        protected override void SubscribeButtonEvents()
        {
            m_ButtonClose   .onClick.AddListener(OnCloseButtonPressed);
            m_ButtonShowHint.onClick.AddListener(OnWatchAdsButtonPressed);
        }

        private void OnCloseButtonPressed()
        {
            OnClose(OnClosePanelAction);
        }

        private void OnWatchAdsButtonPressed()
        {
            if (AppearingState != EAppearingState.Appeared)
                return;
            void OnBeforeShown()
            {
                TimePauser.PauseTimeInUi();
            }
            void OnReward()
            {
                ShowHint();
            }
            void OnClosed()
            {
                TimePauser.UnpauseTimeInUi();
            }
            Managers.AdsManager.ShowRewardedAd(
                OnBeforeShown,
                _OnReward: OnReward,
                _OnClosed: OnClosed);
            Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.WatchAdInHintPanelClick);
        }

        private void ShowHint()
        {
            var args = new Dictionary<string, object>
            {
                {KeyGameMode,             ParameterGameModePuzzles},
                {KeyNextLevelType,        ParameterLevelTypeDefault},
                {KeyShowPuzzleLevelHint,  true},
                {KeyPassCommandsRecord,   Model.Data.Info.AdditionalInfo.PassCommandsRecord},
                {KeyLevelIndex,           Model.LevelStaging.LevelIndex},
            };
            OnClose(OnClosePanelAction);
            FullscreenTransitioner.SendMessage("puzzle_hint");
            var loadLevelCoroutine = MainMenuUtils.LoadLevelCoroutine(
                args, ViewSettings, FullscreenTransitioner, Ticker, LevelStageSwitcher);
            Cor.Run(loadLevelCoroutine);
        }
        
        private void IndicateAdsLoading(bool _Indicate)
        {
            if (!m_PanelShowing)
                return;
            m_AnimLoadingAds.SetGoActive(_Indicate);
            m_IconWatchAds.enabled        = !_Indicate;
            m_ButtonShowHint.interactable = !_Indicate;
        }

        #endregion
    }
}