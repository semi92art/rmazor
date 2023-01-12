using System.Globalization;
using Common.Constants;
using mazing.common.Runtime;
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
using RMAZOR.Constants;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Views.Common;
using RMAZOR.Views.InputConfigurators;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RMAZOR.UI.Panels
{
    public interface IRateGameDialogPanel : IDialogPanel { }
    
    public class RateGameDialogPanelFake : FakeDialogPanel, IRateGameDialogPanel { }
    
    public class RateGameDialogPanel : DialogPanelBase, IRateGameDialogPanel
    {
        private IViewSwitchLevelStageCommandInvoker SwitchLevelStageCommandInvoker { get; }

        #region nonpublic members

        private Button       
            m_ButtonRate,
            m_ButtonLater,
            m_ButtonNever;
        private TextMeshProUGUI
            m_TextTitle,
            m_TextMessage;
        private Animator m_StarsAnimator;
        private bool     m_Disappeared;

        #endregion

        #region inject
        

        public RateGameDialogPanel(
            IManagersGetter                     _Managers,
            IUITicker                           _Ticker,
            ICameraProvider                     _CameraProvider,
            IColorProvider                      _ColorProvider,
            IViewInputCommandsProceeder         _CommandsProceeder,
            IViewTimePauser                     _TimePauser,
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

        public override int DialogViewerId => DialogViewerIdsCommon.FullscreenCommon;
        
        public override void LoadPanel(RectTransform _Container, ClosePanelAction _OnClose)
        {
            base.LoadPanel(_Container, _OnClose);
            var go = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(
                    _Container,
                    RectTransformLite.FullFill),
                CommonPrefabSetNames.DialogPanels, "rate_game_panel");
            PanelRectTransform = go.RTransform();
            go.SetActive(false);
            m_TextTitle   = go.GetCompItem<TextMeshProUGUI>("title");
            m_TextMessage = go.GetCompItem<TextMeshProUGUI>("message");
            m_ButtonRate  = go.GetCompItem<Button>("rate_button");
            m_ButtonLater = go.GetCompItem<Button>("later_button");
            m_ButtonNever = go.GetCompItem<Button>("never_button");
            m_StarsAnimator = go.GetCompItem<Animator>("stars_animator");

            var rateButtonText = go.GetCompItem<TextMeshProUGUI>("rate_button_text");
            var laterButtonText = go.GetCompItem<TextMeshProUGUI>("later_button_text");
            var neverButtonText = go.GetCompItem<TextMeshProUGUI>("never_button_text");
            
            m_ButtonRate.onClick.AddListener(OnRateButtonClick);
            m_ButtonLater.onClick.AddListener(OnLaterButtonClick);
            m_ButtonNever.onClick.AddListener(OnNeverButtonClick);
            
            Managers.LocalizationManager.AddTextObject(
                new LocalizableTextObjectInfo(m_TextTitle, ETextType.MenuUI, "rate_game_panel_title",
                    _T => _T.ToUpper(CultureInfo.CurrentUICulture)));
            Managers.LocalizationManager.AddTextObject(
                new LocalizableTextObjectInfo(m_TextMessage, ETextType.MenuUI, "rate_game_panel_text",
                    _T => _T.FirstCharToUpper(CultureInfo.CurrentUICulture)));
            Managers.LocalizationManager.AddTextObject(
                new LocalizableTextObjectInfo(rateButtonText, ETextType.MenuUI, "leave_feedback",
                    _T => _T.FirstCharToUpper(CultureInfo.CurrentUICulture)));
            Managers.LocalizationManager.AddTextObject(
                new LocalizableTextObjectInfo(laterButtonText, ETextType.MenuUI, "later",
                    _T => _T.FirstCharToUpper(CultureInfo.CurrentUICulture)));
            Managers.LocalizationManager.AddTextObject(
                new LocalizableTextObjectInfo(neverButtonText, ETextType.MenuUI, "never",
                    _T => _T.FirstCharToUpper(CultureInfo.CurrentUICulture)));
        }
        
        public override void OnDialogStartAppearing()
        {
            TimePauser.PauseTimeInGame();
            Cor.Run(Cor.Delay(
                5f, 
                Ticker, 
                _OnStart: () =>
                {
                    m_ButtonLater.interactable = false;
                    m_ButtonNever.interactable = false;
                },
                _OnDelay: () =>
                {
                    if (m_Disappeared)
                        return;
                    m_ButtonLater.interactable = true;
                    m_ButtonNever.interactable = true;
                }));
            m_StarsAnimator.SetTrigger(AnimKeys.Anim);
            base.OnDialogStartAppearing();
        }

        public override void OnDialogDisappeared()
        {
            TimePauser.UnpauseTimeInGame();
            m_Disappeared = true;
            base.OnDialogDisappeared();
        }

        #endregion

        #region nonpublic methods

        private void OnRateButtonClick()
        {
            Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.RateGameButton2Pressed);
            Managers.ShopManager.RateGame();
            SaveUtils.PutValue(SaveKeysCommon.GameWasRated, true);
            OnClose(UnpauseLevel);
        }

        private void OnLaterButtonClick()
        {
            OnClose(UnpauseLevel);
        }

        private void OnNeverButtonClick()
        {
            SaveUtils.PutValue(SaveKeysCommon.GameWasRated, true);
            OnClose(UnpauseLevel);
        }

        private void UnpauseLevel()
        {
            Cor.Run(Cor.WaitNextFrame(
                () => SwitchLevelStageCommandInvoker.SwitchLevelStage(EInputCommand.UnPauseLevel),
                false,
                3U));
        }

        #endregion
    }
}