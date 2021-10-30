using Constants;
using DI.Extensions;
using DialogViewers;
using Entities;
using GameHelpers;
using Games.RazorMaze.Views.Common;
using Ticker;
using TMPro;
using UI.Factories;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Panels
{
    public interface ICharacterDiedDialogPanel : IDialogPanel { }
    
    public class CharacterDiedDialogPanel : DialogPanelBase, ICharacterDiedDialogPanel
    {
        #region nonpublic members
        
        private static int AkShow => AnimKeys.Anim;
        private Animator m_Animator;
        private TextMeshProUGUI m_TextYouHaveMoney;
        private Button m_WatchAdsButton;
        private Button m_PayMoneyButton;
        private UnityAction m_OnClose;

        #endregion

        #region inject

        private INotificationViewer NotificationViewer { get; }
        
        public CharacterDiedDialogPanel(
            IDialogViewer _DialogViewer,
            INotificationViewer _NotificationViewer,
            IManagersGetter _Managers,
            IUITicker _UITicker,
            ICameraProvider _CameraProvider,
            IColorProvider _ColorProvider) 
            : base(_Managers, _UITicker, _DialogViewer, _CameraProvider, _ColorProvider)
        {
            NotificationViewer = _NotificationViewer;
        }
        
        #endregion
        
        #region api
        
        public override EUiCategory Category => EUiCategory.CharacterDied;

        public override void Init()
        {
            base.Init();
            var go = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    NotificationViewer.Container,
                    RtrLites.FullFill),
                CommonPrefabSetNames.DialogPanels, "wof_reward_panel");
            Panel = go.RTransform();
            go.SetActive(false);

            m_Animator = go.GetCompItem<Animator>("animator");
            m_WatchAdsButton = go.GetCompItem<Button>("watch_ad_button");
            m_PayMoneyButton = go.GetCompItem<Button>("pay_money_button");
            m_TextYouHaveMoney = go.GetCompItem<TextMeshProUGUI>("you_have_money_text");
            
            m_WatchAdsButton.onClick.AddListener(OnWatchAdsButtonClick);
            m_PayMoneyButton.onClick.AddListener(OnPayMoneyButtonClick);
        }

        public override void OnDialogShow()
        {
            m_Animator.SetTrigger(AkShow);
        }

        public override void OnDialogHide()
        {
            m_OnClose?.Invoke();
        }

        #endregion
        
        #region nonpublic methods

        private void OnWatchAdsButtonClick()
        {
            
        }

        private void OnPayMoneyButtonClick()
        {
            
        }
        
        #endregion
    }
}