using System;
using System.Collections;
using Common.Extensions;
using Common.Managers;
using Common.Ticker;
using Common.UI;
using Common.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Shop_Items
{
    public class ViewShopItemInfo
    {
        public int    PurchaseKey      { get; set; }
        public bool   BuyForWatchingAd { get; set; }
        public string Price            { get; set; }
        public bool   Ready            { get; set; }
        public Sprite Icon             { get; set; }
        public Sprite Background       { get; set; }
        public string Currency         { get; set; }
        public int    Reward           { get; set; }
    }
    
    public abstract class ShopItemBase : SimpleUiItemBase
    {
        public                     TextMeshProUGUI price;
        public                     TextMeshProUGUI title;
        public                     Image           itemIcon;
        [SerializeField] protected Button          buyButton;
        [SerializeField] protected Image           watchAdImage;
        [SerializeField] protected Animator        loadingAnim;

        private ViewShopItemInfo     m_Info;
        private bool                 m_IsBuyButtonNotNull;
        private bool                 m_IsWatchAdImageNotNull;
        private IEnumerator          m_StopIndicateLoadingCoroutine;

        public virtual void Init(
            IUITicker            _UITicker,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager,
            UnityAction          _Click,
            ViewShopItemInfo     _Info)
        {
            base.Init(_UITicker, _AudioManager, _LocalizationManager);
            m_Info = _Info;
            LocalizationManager.AddTextObject(new LocalizableTextObjectInfo(title, ETextType.MenuUI));
            LocalizationManager.AddTextObject(new LocalizableTextObjectInfo(price, ETextType.Currency));
            watchAdImage.SetGoActive(true);
            loadingAnim.SetGoActive(true);
            name = "Shop Item";
            buyButton.onClick.AddListener(SoundOnClick);
            buyButton.onClick.AddListener(_Click);
            itemIcon.sprite = _Info.Icon;
            if (_Info.Background.IsNotNull())
                background.sprite = _Info.Background;
            IndicateLoading(true, _Info.BuyForWatchingAd);
            m_StopIndicateLoadingCoroutine = Cor.WaitWhile(
                () => !_Info.Ready,
                () =>
                {
                    IndicateLoading(false, _Info.BuyForWatchingAd);
                    FinishAction();
                });
            Cor.Run(m_StopIndicateLoadingCoroutine);
        }
        
        public override void Init(
            IUITicker            _UITicker, 
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager)
        {
            throw new NotSupportedException();
        }

        protected override void CheckIfSerializedItemsNotNull()
        {
            base.CheckIfSerializedItemsNotNull();
            m_IsBuyButtonNotNull = buyButton.IsNotNull();
            m_IsWatchAdImageNotNull = watchAdImage.IsNotNull();
        }

        private void IndicateLoading(bool _Indicate, bool _BuyForWatchingAd)
        {
            watchAdImage.SetGoActive(!_Indicate && _BuyForWatchingAd);
            price.SetGoActive(!_Indicate && !_BuyForWatchingAd);
            loadingAnim.SetGoActive(_Indicate);
            loadingAnim.enabled = _Indicate;
        }

        private void FinishAction()
        {
            price.SetGoActive(!m_Info.BuyForWatchingAd);
            if (m_Info.BuyForWatchingAd) 
                return;
            price.text = $"{m_Info.Price}";
            if (!string.IsNullOrEmpty(price.text) && !string.IsNullOrWhiteSpace(price.text))
                return;
            price.text = LocalizationManager.GetTranslation("buy");
            price.font = LocalizationManager.GetFont(ETextType.MenuUI, LocalizationManager.GetCurrentLanguage());
        }

        private void OnDestroy()
        {
            Cor.Stop(m_StopIndicateLoadingCoroutine);
        }
    }
}