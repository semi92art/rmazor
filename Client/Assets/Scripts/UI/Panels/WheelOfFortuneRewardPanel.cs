using Constants;
using DI.Extensions;
using DialogViewers;
using Entities;
using Exceptions;
using GameHelpers;
using Games.RazorMaze.Views.Common;
using Managers;
using Ticker;
using TMPro;
using UI.Factories;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Panels
{
    public interface IWheelOfFortuneRewardPanel : IDialogPanel
    {
        void PreInit(BankItemType BankItemType, long _Reward, UnityAction _OnClose);
    }
    
    public class WheelOfFortuneRewardPanel : DialogPanelBase, IWheelOfFortuneRewardPanel
    {
        #region nonpublic members
        
        private static int AkShow => AnimKeys.Anim;
        private BankItemType m_BankItemType;
        private long m_Reward;
        private UnityAction m_OnClose;
        private Animator m_Animator;
        private Image m_RewardIcon;
        private TextMeshProUGUI m_RewardCount;
        private Button m_GetButton;

        #endregion

        #region inject

        private INotificationViewer NotificationViewer { get; }
        
        public WheelOfFortuneRewardPanel(
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



        public override EUiCategory Category => EUiCategory.WheelOfFortune;
        
        public void PreInit(BankItemType _BankItemType, long _Reward, UnityAction _OnClose)
        {
            m_BankItemType = _BankItemType;
            m_Reward = _Reward;
            m_OnClose = _OnClose;
        }

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
            
            m_RewardIcon = go.GetCompItem<Image>("reward_icon");
            m_RewardCount = go.GetCompItem<TextMeshProUGUI>("reward_count");
            m_Animator = go.GetCompItem<Animator>("animator");
            m_GetButton = go.GetCompItem<Button>("get_button");
            
            m_GetButton.SetOnClick(() => NotificationViewer.Back());
            
            string iconName;
            string prefabSet;
            switch (m_BankItemType)
            {
                case BankItemType.FirstCurrency:
                    iconName = "gold_coin_0";
                    prefabSet = "coins";
                    break;
                case BankItemType.SecondCurrency:
                    iconName = "diamond_coin_0";
                    prefabSet = "coins";
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