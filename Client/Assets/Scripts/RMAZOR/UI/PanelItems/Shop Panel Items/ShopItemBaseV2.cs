using System;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Shop_Panel_Items
{
    public abstract class ViewShopItemInfoBaseV2
    {
        public Func<bool?> HasReceiptAdditionalPredicate { get; set; }
        public UnityAction BuyAction                     { get; set; }
        public string      NameLocalizationKey           { get; set; }
    }
    
    public abstract class ShopItemBaseV2 : SimpleUiItem
    {
        #region serialized fields

        [SerializeField] protected TextMeshProUGUI title;
        [SerializeField] protected Image           icon;
        [SerializeField] protected Button          buyButton;
        [SerializeField] protected TextMeshProUGUI buyButtonText;

        #endregion

        #region nonpublic members

        private ViewShopItemInfoBaseV2 m_ShopItemInfo;
        
        protected IAnalyticsManager AnalyticsManager { get; private set; }

        #endregion

        #region api

        protected virtual void Init(
            IUITicker              _UITicker,
            IAudioManager          _AudioManager,
            ILocalizationManager   _LocalizationManager,
            IAnalyticsManager      _AnalyticsManager,
            ViewShopItemInfoBaseV2 _ShopItemInfo)
        {
            AnalyticsManager = _AnalyticsManager;
            m_ShopItemInfo = _ShopItemInfo;
            base.Init(_UITicker, _AudioManager, _LocalizationManager);
            LocalizeTextObjectsOnInit();
            SubscribeButtonEvents();
        }
        
#pragma warning disable 0809
        [Obsolete]
        public override void Init(
#pragma warning restore 0809
            IUITicker            _UITicker, 
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager)
        {
            throw new NotSupportedException();
        }

#pragma warning disable 0809
        [Obsolete]
        public override void Init()
#pragma warning restore 0809
        {
            throw new NotSupportedException();
        }

        public abstract void UpdateState();

        #endregion

        #region nonpublic methods

        protected virtual void LocalizeTextObjectsOnInit()
        {
            var locTextInfos = new[]
            {
                new LocTextInfo(title, ETextType.MenuUI_H1, m_ShopItemInfo.NameLocalizationKey),
                new LocTextInfo(buyButtonText, ETextType.MenuUI_H1, "buy")
            };
            foreach (var locTextInfo in locTextInfos)
                LocalizationManager.AddLocalization(locTextInfo);
        }

        private void SubscribeButtonEvents()
        {
            buyButton.SetOnClick(OnBuyButtonClick);
        }

        protected abstract void OnBuyButtonClick();

        #endregion
    }
}