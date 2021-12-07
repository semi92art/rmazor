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
    public interface IWofRewardPanel : IDialogPanel
    {
        void PreInit(long _Reward, UnityAction _OnClose);
    }
    
    public class WofRewardDialogPanel : DialogPanelBase, IWofRewardPanel
    {
        #region nonpublic members
        
        private static int AkShow => AnimKeys.Anim;
        private long m_Reward;
        private UnityAction m_OnClose;
        private Animator m_Animator;
        private Image m_RewardIcon;
        private TextMeshProUGUI m_RewardCount;
        private Button m_GetButton;

        #endregion

        #region inject

        private IProposalDialogViewer ProposalDialogViewer { get; }
        
        public WofRewardDialogPanel(
            IBigDialogViewer _DialogViewer,
            IProposalDialogViewer _ProposalDialogViewer,
            IManagersGetter _Managers,
            IUITicker _UITicker,
            ICameraProvider _CameraProvider,
            IColorProvider _ColorProvider) 
            : base(_Managers, _UITicker, _DialogViewer, _CameraProvider, _ColorProvider)
        {
            ProposalDialogViewer = _ProposalDialogViewer;
        }
        
        #endregion
        
        #region api



        public override EUiCategory Category => EUiCategory.WheelOfFortune;
        
        public void PreInit(long _Reward, UnityAction _OnClose)
        {
            m_Reward = _Reward;
            m_OnClose = _OnClose;
        }

        public override void LoadPanel()
        {
            base.LoadPanel();
            var go = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    ProposalDialogViewer.Container,
                    RtrLites.FullFill),
                CommonPrefabSetNames.DialogPanels, "wof_reward_panel");
            Panel = go.RTransform();
            go.SetActive(false);
            
            m_RewardIcon = go.GetCompItem<Image>("reward_icon");
            m_RewardCount = go.GetCompItem<TextMeshProUGUI>("reward_count");
            m_Animator = go.GetCompItem<Animator>("animator");
            m_GetButton = go.GetCompItem<Button>("get_button");
            
            m_GetButton.SetOnClick(() => ProposalDialogViewer.Back());
            
            string iconName;
            string prefabSet;
            iconName = "gold_coin_0";
            prefabSet = "coins";
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