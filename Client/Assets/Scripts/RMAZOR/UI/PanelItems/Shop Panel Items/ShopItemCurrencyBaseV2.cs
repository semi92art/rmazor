using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Shop_Panel_Items
{
    public abstract class ShopItemCurrencyBaseV2 : ShopItemBaseV2
    {
        #region serialized fields
        
        [SerializeField] protected Image           priceBackground;
        [SerializeField] protected TextMeshProUGUI priceText;
        [SerializeField] protected Image           checkMarkIcon; 

        #endregion

        #region api

        protected override void Init(
            IUITicker              _UITicker,
            IAudioManager          _AudioManager,
            ILocalizationManager   _LocalizationManager,
            IAnalyticsManager      _AnalyticsManager,
            ViewShopItemInfoBaseV2 _ShopItemInfo)
        {
            base.Init(_UITicker, _AudioManager, _LocalizationManager, _AnalyticsManager, _ShopItemInfo);
            checkMarkIcon.enabled = false;
            SetItemAsPurchasedIfHasReceiptAndNonConsumable();
        }

        #endregion

        #region nonpublic methods

        protected void SetItemAsPurchasedIfHasReceiptAndNonConsumable()
        {
            if (!HasReceipt)
                return;
            SetItemAsPurchased();
        }

        protected virtual void SetItemAsPurchased()
        {
            priceBackground.SetGoActive(false);
            buyButton      .SetGoActive(false);
            checkMarkIcon.enabled = true;
        }

        protected override void LocalizeTextObjectsOnInit()
        {
            var locTextInfos = new[]
            {
                new LocTextInfo(priceText, ETextType.MenuUI_H3, "empty_key", _TextLocalizationType: ETextLocalizationType.OnlyFont),
            };
            foreach (var locTextInfo in locTextInfos)
                LocalizationManager.AddLocalization(locTextInfo);
            base.LocalizeTextObjectsOnInit();
        }

        protected abstract bool HasReceipt { get; }

        #endregion
    }
}