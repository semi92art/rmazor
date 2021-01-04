using Constants;
using DialogViewers;
using Exceptions;
using Extensions;
using GameHelpers;
using Managers;
using TMPro;
using UI.Factories;
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
        private readonly MoneyType m_MoneyType;
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
            MoneyType _MoneyType,
            long _Reward,
            UnityAction _OnClose)
        {
            m_NotificationViewer = _NotificationViewer;
            m_MoneyType = _MoneyType;
            m_Reward = _Reward;
            m_OnClose = _OnClose;
        }
        
        public override void Init()
        {
            var go = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_NotificationViewer.Container,
                    RtrLites.FullFill),
                CommonStyleNames.MainMenuDialogPanels, "wof_reward_panel");
            Panel = go.RTransform();
            go.SetActive(false);
            
            m_RewardIcon = go.GetCompItem<Image>("reward_icon");
            m_RewardCount = go.GetCompItem<TextMeshProUGUI>("reward_count");
            m_Animator = go.GetCompItem<Animator>("animator");
            m_GetButton = go.GetCompItem<Button>("get_button");
            
            m_GetButton.SetOnClick(() => m_NotificationViewer.Back());
            
            string iconName;
            string styleName;
            switch (m_MoneyType)
            {
                case MoneyType.Gold:
                    iconName = "gold_coin_0";
                    styleName = "coins";
                    break;
                case MoneyType.Diamonds:
                    iconName = "diamond_coin_0";
                    styleName = "coins";
                    break;
                case MoneyType.Lifes:
                    iconName = "icon_life";
                    styleName = "icons";
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(m_MoneyType);
            }

            Sprite iconSprite = PrefabInitializer.GetObject<Sprite>(styleName, iconName);
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