using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Entities.UI;
using Common.Extensions;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using Common.UI;
using Common.UI.DialogViewers;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
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
        #region nonpublic members

        private Button          m_ButtonRate;
        private Button          m_ButtonLater;
        private Button          m_ButtonNever;
        private TextMeshProUGUI m_TextTitle;
        private TextMeshProUGUI m_TextMessage;
        private Animator        m_StarsAnimator;

        #endregion

        #region inject
        
        private IViewInputCommandsProceeder CommandsProceeder { get; }

        public RateGameDialogPanel(
            IManagersGetter             _Managers,
            IUITicker                   _Ticker,
            ICameraProvider             _CameraProvider,
            IColorProvider              _ColorProvider,
            IViewInputCommandsProceeder _CommandsProceeder)
            : base(
                _Managers, 
                _Ticker, 
                _CameraProvider,
                _ColorProvider)
        {
            CommandsProceeder = _CommandsProceeder;
        }

        #endregion

        #region api

        public override EDialogViewerType DialogViewerType => EDialogViewerType.Fullscreen;
        public override EUiCategory       Category         => EUiCategory.RateGame;
        
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
                new LocalizableTextObjectInfo(rateButtonText, ETextType.MenuUI, "rate_game",
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
            m_StarsAnimator.SetTrigger(AnimKeys.Anim);
            CommandsProceeder.LockCommands(GetCommandsToLock(), nameof(IRateGameDialogPanel));
            base.OnDialogStartAppearing();
        }

        public override void OnDialogDisappeared()
        {
            CommandsProceeder.UnlockCommands(GetCommandsToLock(), nameof(IRateGameDialogPanel));
            base.OnDialogDisappeared();
        }

        #endregion

        #region nonpublic methods

        private void OnRateButtonClick()
        {
            Managers.AnalyticsManager.SendAnalytic(AnalyticIds.RateGameButton2Pressed);
            Managers.ShopManager.RateGame();
            SaveUtils.PutValue(SaveKeysCommon.GameWasRated, true);
            OnClose(null);
        }

        private void OnLaterButtonClick()
        {
            OnClose(null);
        }

        private void OnNeverButtonClick()
        {
            SaveUtils.PutValue(SaveKeysCommon.GameWasRated, true);
            OnClose(null);
        }

        private static IEnumerable<EInputCommand> GetCommandsToLock()
        {
            return RmazorUtils.MoveAndRotateCommands.Concat(new []
            {
                EInputCommand.ShopMenu, EInputCommand.SettingsMenu
            });
        }

        #endregion
    }
}