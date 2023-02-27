using System.Globalization;
using mazing.common.Runtime;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Entities;
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
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
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

        private Button       
            m_ButtonRate,
            m_ButtonLater,
            m_ButtonNever;
        
        private TextMeshProUGUI
            m_TextTitle,
            m_TextMessage,
            m_ButtonRateText,
            m_ButtonLaterText,
            m_ButtonNeverText;
        
        private Animator m_StarsAnimator;
        private bool     m_Disappeared;
        
        protected override string PrefabName => "rate_game_panel";

        #endregion

        #region inject
        
        private IViewLevelStageSwitcher LevelStageSwitcher { get; }

        public RateGameDialogPanel(
            IManagersGetter             _Managers,
            IUITicker                   _Ticker,
            ICameraProvider             _CameraProvider,
            IColorProvider              _ColorProvider,
            IViewInputCommandsProceeder _CommandsProceeder,
            IViewTimePauser             _TimePauser,
            IViewLevelStageSwitcher     _LevelStageSwitcher)
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

        public override int DialogViewerId => DialogViewerIdsCommon.FullscreenCommon;
        
        #endregion

        #region nonpublic methods
        
        protected override void OnDialogStartAppearing()
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

        protected override void OnDialogDisappeared()
        {
            TimePauser.UnpauseTimeInGame();
            m_Disappeared = true;
            base.OnDialogDisappeared();
        }

        protected override void GetPrefabContentObjects(GameObject _Go)
        {
            m_TextTitle       = _Go.GetCompItem<TextMeshProUGUI>("title");
            m_TextMessage     = _Go.GetCompItem<TextMeshProUGUI>("message");
            m_ButtonRate      = _Go.GetCompItem<Button>("rate_button");
            m_ButtonLater     = _Go.GetCompItem<Button>("later_button");
            m_ButtonNever     = _Go.GetCompItem<Button>("never_button");
            m_StarsAnimator   = _Go.GetCompItem<Animator>("stars_animator");
            m_ButtonRateText  = _Go.GetCompItem<TextMeshProUGUI>("rate_button_text");
            m_ButtonLaterText = _Go.GetCompItem<TextMeshProUGUI>("later_button_text");
            m_ButtonNeverText = _Go.GetCompItem<TextMeshProUGUI>("never_button_text");
        }

        protected override void LocalizeTextObjectsOnLoad()
        {
            static string TextFormula1(string _Text) => _Text.ToUpper(CultureInfo.CurrentUICulture);
            static string TextFormula2(string _Text) => _Text.FirstCharToUpper(CultureInfo.CurrentUICulture);
            var locTextInfos = new []
            {
                new LocTextInfo(m_TextTitle,       ETextType.MenuUI, "rate_game_panel_title", TextFormula1),
                new LocTextInfo(m_TextMessage,     ETextType.MenuUI, "rate_game_panel_text",  TextFormula2),
                new LocTextInfo(m_ButtonRateText,  ETextType.MenuUI, "leave_feedback",        TextFormula2),
                new LocTextInfo(m_ButtonLaterText, ETextType.MenuUI, "later",                 TextFormula2),
                new LocTextInfo(m_ButtonNeverText, ETextType.MenuUI, "never",                 TextFormula2)
            };
            foreach (var locTextInfo in locTextInfos)
                Managers.LocalizationManager.AddLocalization(locTextInfo);
        }

        protected override void SubscribeButtonEvents()
        {
            m_ButtonRate.onClick.AddListener(OnRateButtonClick);
            m_ButtonLater.onClick.AddListener(OnLaterButtonClick);
            m_ButtonNever.onClick.AddListener(OnNeverButtonClick);
        }

        private void OnRateButtonClick()
        {
            Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.RateGameButton2Click);
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
                () => LevelStageSwitcher.SwitchLevelStage(EInputCommand.UnPauseLevel),
                false,
                3U));
        }

        #endregion
    }
}