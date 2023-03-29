using System;
using System.Collections;
using Common.Managers.Advertising;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Managers.IAP;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Shop_Panel_Items
{
    public class ViewShopItemInfo
    {
        public IAP_ProductInfo ShopItemArgs     { get; set; }
        public int          PurchaseKey      { get; set; }
        public bool         BuyForWatchingAd { get; set; }
        public Sprite       Icon             { get; set; }
        public Sprite       Background       { get; set; }
        public int          Reward           { get; set; }
    }
    
    public abstract class ShopItemBase : SimpleUiItem
    {
        #region serialized fields
        
        public                     TextMeshProUGUI price;
        public                     TextMeshProUGUI title;
        public                     Image           itemIcon;
        [SerializeField] protected Button          buyButton;
        [SerializeField] protected Image           watchAdImage;
        [SerializeField] protected Animator        loadingAnim;

        #endregion

        #region nonpublic members
        
        private ViewShopItemInfo m_Info;
        private IEnumerator      m_StopIndicateLoadingCoroutine;
        
        private IAdsManager AdsManager { get; set; }

        #endregion

        #region api
        
        public virtual void Init(
            IUITicker            _UITicker,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager,
            IAdsManager          _AdsManager,
            UnityAction          _Click,
            ViewShopItemInfo     _Info)
        {
            AdsManager = _AdsManager;
            base.Init(_UITicker, _AudioManager, _LocalizationManager);
            m_Info = _Info;
            LocalizationManager.AddLocalization(new LocTextInfo(title, ETextType.MenuUI_H1));
            LocalizationManager.AddLocalization(new LocTextInfo(price, ETextType.MenuUI_H1));
            watchAdImage.SetGoActive(true);
            loadingAnim.SetGoActive(true);
            name = "Shop Item";
            buyButton.onClick.AddListener(PlayButtonClickSound);
            buyButton.onClick.AddListener(_Click);
            itemIcon.sprite = _Info.Icon;
            if (_Info.Background.IsNotNull())
                background.sprite = _Info.Background;
            Cor.Run(IndicateLoadingCoroutine());
        }
        
        public override void Init(
            IUITicker            _UITicker, 
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region nonpublic methods

        private IEnumerator IndicateLoadingCoroutine()
        {
            IndicateLoading(true);
            yield return Cor.WaitWhile(() => m_Info == null 
                ? AdsManager.RewardedAdReady
                : m_Info.ShopItemArgs != null && m_Info.ShopItemArgs.Result() == EShopProductResult.Pending);
            IndicateLoading(false);
            FinishAction();
        }
        
        private void IndicateLoading(bool _Indicate)
        {
            watchAdImage.SetGoActive(!_Indicate && m_Info.BuyForWatchingAd);
            price       .SetGoActive(!_Indicate && !m_Info.BuyForWatchingAd);
            loadingAnim .SetGoActive(_Indicate);
            loadingAnim .enabled = _Indicate;
        }

        private void FinishAction()
        {
            price.SetGoActive(!m_Info.BuyForWatchingAd);
            if (m_Info.BuyForWatchingAd) 
                return;
            price.text = $"{m_Info.ShopItemArgs.LocalizedPriceString}";
            if (!string.IsNullOrEmpty(price.text))
                return;
            price.text = LocalizationManager.GetTranslation("buy");
        }

        private void OnDestroy()
        {
            Cor.Stop(m_StopIndicateLoadingCoroutine);
        }

        #endregion
    }
}