using Common.Constants;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.Utils;
using RMAZOR.UI.Panels;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Main_Menu_Panel_Items
{
    public class MainMenuPanelButtonDailyGift : SimpleUiItem
    {
        #region serialized fields

        [SerializeField] private Button         dailyGiftButton;
        [SerializeField] private Image          dailyGiftIcon;
        [SerializeField] private ParticleSystem dailyGiftParticleSystem;
        [SerializeField] private Image          dailyGiftStamp;
        [SerializeField] private Animator       dailyGiftAnimator;

        #endregion

        #region nonpublic members
        
        private Sprite m_DailyGiftDisabledSprite;

        private IDailyGiftPanel DailyGiftPanel { get; set; }

        #endregion

        #region api

        public void Init(
            IUITicker            _UITicker,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager,
            IPrefabSetManager    _PrefabSetManager,
            IDailyGiftPanel      _DailyGiftPanel,
            UnityAction          _OnClick)
        {
            base.Init(_UITicker, _AudioManager, _LocalizationManager);
            DailyGiftPanel = _DailyGiftPanel;
            m_DailyGiftDisabledSprite = _PrefabSetManager.GetObject<Sprite>(
                CommonPrefabSetNames.Views, "daily_gift_icon_disabled");
            if (!DailyGiftPanel.IsDailyGiftAvailableToday)
                Cor.Run(Cor.WaitNextFrame(DisableDailyGiftButton));
            DailyGiftPanel.OnClose += DisableDailyGiftButton;
            dailyGiftButton.SetOnClick(_OnClick);
        }

        public void UpdateState()
        {
            dailyGiftStamp.enabled = false;
            if (!DailyGiftPanel.IsDailyGiftAvailableToday)
                Cor.Run(Cor.WaitNextFrame(DisableDailyGiftButton));
        }

        #endregion

        #region nonpublic methods

        private void DisableDailyGiftButton()
        {
            dailyGiftAnimator.enabled    = false;
            dailyGiftIcon.sprite         = m_DailyGiftDisabledSprite;
            dailyGiftButton.interactable = false;
            dailyGiftStamp.enabled       = true;
            dailyGiftParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        #endregion
    }
}