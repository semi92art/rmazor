using System;
using System.Linq;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Managers.IAP;
using mazing.common.Runtime.Ticker;
using RMAZOR.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Shop_Panel_Items
{
    public class ViewShopItemInfoSpecialOffer : ViewShopItemInfoRealCurrencyV2
    {
        public string[] ContentLocalizationKeys { get; set; }
    }
    
    public class ShopItemSpecialOffer : ShopItemRealCurrencyV2
    {
        #region constants

        private const decimal DiscountValue = 0.9m;

        #endregion
        
        #region serialized fields

        [SerializeField] private Image             timerIcon;
        [SerializeField] private Image             timerBackground;
        [SerializeField] private TextMeshProUGUI   timerText;
        [SerializeField] private TextMeshProUGUI   priceWithoutDiscountText;
        [SerializeField] private TextMeshProUGUI   discountValueText;
        [SerializeField] private TextMeshProUGUI[] contentTexts;

        #endregion

        #region nonpublic members

        private bool m_SeparatePanel;

        private ViewShopItemInfoSpecialOffer m_ViewShopItemInfoSpecialOffer;
        
        private ISpecialOfferTimerController SpecialOfferTimerController { get; set; }

        #endregion

        #region api

        public void Init(
            IUITicker                    _UITicker,
            IAudioManager                _AudioManager,
            ILocalizationManager         _LocalizationManager,
            IAnalyticsManager            _AnalyticsManager,
            IShopManager                 _ShopManager,
            ISpecialOfferTimerController _SpecialOfferTimerController,
            ViewShopItemInfoSpecialOffer _ShopItemInfoInfo,
            bool                         _SeparatePanel)
        {
            m_SeparatePanel                = _SeparatePanel;
            m_ViewShopItemInfoSpecialOffer = _ShopItemInfoInfo;
            SpecialOfferTimerController    = _SpecialOfferTimerController;
            InitTimer();
            base.Init(_UITicker, _AudioManager, _LocalizationManager,
                _AnalyticsManager, _ShopManager, _ShopItemInfoInfo);
        }

#pragma warning disable 0809
        [Obsolete]
        public override void Init(
#pragma warning restore 0809
            IUITicker                      _UITicker,
            IAudioManager                  _AudioManager,
            ILocalizationManager           _LocalizationManager,
            IAnalyticsManager              _AnalyticsManager,
            IShopManager                   _ShopManager,
            ViewShopItemInfoRealCurrencyV2 _ShopItemInfo)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region nonpublic methods

        private void OnTimeValueChanged(TimeSpan _Span)
        {
            timerText.text = _Span.ToString(@"hh\:mm\:ss");
        }
        
        private void OnTimeIsGone()
        {
            HideTimer();
            SpecialOfferTimerController.TimerValueChanged -= OnTimeValueChanged;
            SpecialOfferTimerController.TimeIsGone        -= OnTimeIsGone;
        }

        private void InitTimer()
        {
            SpecialOfferTimerController.TimerValueChanged += OnTimeValueChanged;
            SpecialOfferTimerController.TimeIsGone        += OnTimeIsGone;
            if (SpecialOfferTimerController.IsTimeGone)
                HideTimer();
        }

        private void HideTimer()
        {
            timerBackground.enabled = false;
            timerIcon      .enabled = false;
            timerText      .enabled = false;
        }
        
        protected override void LocalizeTextObjectsOnInit()
        {
            var contentLocKeys = m_ViewShopItemInfoSpecialOffer.ContentLocalizationKeys;
            var textType = m_SeparatePanel ? ETextType.MenuUI_H1 : ETextType.MenuUI_H3;
            var locTextInfos = contentTexts
                .TakeWhile((_T, _I) => _I < contentLocKeys.Length)
                .Select((_T, _I) => new LocTextInfo(_T, textType, contentLocKeys[_I], 
                    _T1 => (m_SeparatePanel ? "" : "- ") + _T1))
                .ToList();
            const string locKey = "empty_key";
            const ETextLocalizationType textLocType = ETextLocalizationType.OnlyFont;
            locTextInfos.AddRange(new []
            {
                new LocTextInfo(priceWithoutDiscountText, ETextType.MenuUI_H3, locKey, _TextLocalizationType: textLocType),
                new LocTextInfo(discountValueText,        ETextType.MenuUI_H3, locKey, _TextLocalizationType: textLocType),
                new LocTextInfo(timerText,                ETextType.MenuUI_H3, locKey, _TextLocalizationType: textLocType)
            });
            foreach (var locTextInfo in locTextInfos)
                LocalizationManager.AddLocalization(locTextInfo);   
            base.LocalizeTextObjectsOnInit();
        }

        protected override void IndicateLoadingCoroutineFinishAction()
        {
            var iapProductInfo = m_ViewShopItemInfoSpecialOffer.IapProductInfo;
            decimal priceWithoutDiscount = iapProductInfo.LocalizedPrice / (1m - DiscountValue);
            priceWithoutDiscountText.text = iapProductInfo.LocalizedPriceString.Substring(0, 3) 
                                            + " " + priceWithoutDiscount;
            base.IndicateLoadingCoroutineFinishAction();
        }

        #endregion
    }
}