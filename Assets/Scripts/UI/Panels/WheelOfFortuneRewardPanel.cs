using Constants;
using DialogViewers;
using Exceptions;
using Extensions;
using GameHelpers;
using Managers;
using TMPro;
using UI.Factories;
using UI.PanelItems;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils;

namespace UI.Panels
{
    public class WheelOfFortuneRewardPanel : DialogPanelBase
    {
        #region nonpublic members
        
        private static int AkShow => AnimKeys.Anim;
        private readonly INotificationViewer m_NotificationViewer;
        private readonly BankItemType m_BankItemType;
        private readonly long m_Reward;
        private readonly UnityAction m_OnClose;
        private Animator m_Animator;
        private Image m_RewardIcon;
        private TextMeshProUGUI m_RewardCount;
        private Button m_GetButton;

        #endregion
        
        #region api

        public WheelOfFortuneRewardPanel(
            INotificationViewer _NotificationViewer,
            BankItemType _BankItemType,
            long _Reward,
            UnityAction _OnClose)
        {
            m_NotificationViewer = _NotificationViewer;
            m_BankItemType = _BankItemType;
            m_Reward = _Reward;
            m_OnClose = _OnClose;
        }
        
        public override void Init()
        {
            var go = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_NotificationViewer.Container,
                    RtrLites.FullFill),
                CommenPrefabSetNames.MainMenuDialogPanels, "wof_reward_panel");
            Panel = go.RTransform();
            go.SetActive(false);
            
            m_RewardIcon = go.GetCompItem<Image>("reward_icon");
            m_RewardCount = go.GetCompItem<TextMeshProUGUI>("reward_count");
            m_Animator = go.GetCompItem<Animator>("animator");
            m_GetButton = go.GetCompItem<Button>("get_button");
            
            m_GetButton.SetOnClick(() => m_NotificationViewer.Back());
            
            string iconName;
            string prefabSet;
            switch (m_BankItemType)
            {
                case BankItemType.Gold:
                    iconName = "gold_coin_0";
                    prefabSet = "coins";
                    break;
                case BankItemType.Diamonds:
                    iconName = "diamond_coin_0";
                    prefabSet = "coins";
                    break;
                case BankItemType.Lifes:
                    iconName = "icon_life";
                    prefabSet = "icons";
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(m_BankItemType);
            }

            Sprite iconSprite = PrefabUtilsEx.GetObject<Sprite>(prefabSet, iconName);
            m_RewardIcon.sprite = iconSprite;
            m_RewardCount.text = m_Reward.ToNumeric();
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
        

    }
}