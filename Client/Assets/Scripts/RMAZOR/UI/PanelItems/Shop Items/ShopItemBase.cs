using System;
using System.Collections;
using Common;
using Common.Extensions;
using Common.Managers;
using Common.Providers;
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
        public string Currency         { get; set; }
        public int    Reward           { get; set; }
    }
    
    public abstract class ShopItemBase : SimpleUiDialogItemView
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
        private ILocalizationManager m_LocalizationManager;

        public virtual void Init(
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager,
            IUITicker            _UITicker,
            IColorProvider       _ColorProvider,
            UnityAction          _Click,
            ViewShopItemInfo     _Info)
        {
            base.Init(_AudioManager, _UITicker, _ColorProvider);
            m_LocalizationManager = _LocalizationManager;
            m_Info = _Info;
            watchAdImage.SetGoActive(true);
            loadingAnim.SetGoActive(true);
            name = "Shop Item";
            buyButton.onClick.AddListener(SoundOnClick);
            buyButton.onClick.AddListener(_Click);
            itemIcon.sprite = _Info.Icon;
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
            IAudioManager _AudioManager,
            IUITicker _UITicker, 
            IColorProvider _ColorProvider)
        {
            throw new NotSupportedException();
        }

        protected override void CheckIfSerializedItemsNotNull()
        {
            base.CheckIfSerializedItemsNotNull();
            m_IsBuyButtonNotNull = buyButton.IsNotNull();
            m_IsWatchAdImageNotNull = watchAdImage.IsNotNull();
        }

        protected override void SetColorsOnInit()
        {
            base.SetColorsOnInit();
            if (m_IsBuyButtonNotNull)
                buyButton.targetGraphic.color = ColorProvider.GetColor(ColorIdsCommon.UiDialogBackground);
            if (m_IsWatchAdImageNotNull)
                watchAdImage.color = ColorProvider.GetColor(ColorIdsCommon.UI);
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            base.OnColorChanged(_ColorId, _Color);
            if (_ColorId == ColorIdsCommon.UiDialogBackground)
            {
                if (m_IsBuyButtonNotNull)
                    buyButton.targetGraphic.color = _Color;
            }
            else if (_ColorId == ColorIdsCommon.UI)
            {
                if (m_IsWatchAdImageNotNull)
                    watchAdImage.color = _Color;
            }
        }

        private void IndicateLoading(bool _Indicate, bool _BuyForWatchingAd)
        {
            watchAdImage.SetGoActive(!_Indicate && _BuyForWatchingAd);
            price.SetGoActive(!_Indicate && !_BuyForWatchingAd);
            loadingAnim.SetGoActive(_Indicate);
            loadingAnim.enabled = _Indicate;
        }

        protected void FinishAction()
        {
            price.SetGoActive(!m_Info.BuyForWatchingAd);
            if (m_Info.BuyForWatchingAd) 
                return;
            // price.text = $"{m_Info.Price} {m_Info.Currency}";
            price.text = $"{m_Info.Price}";
            if (string.IsNullOrEmpty(price.text) || string.IsNullOrWhiteSpace(price.text))
                price.text = m_LocalizationManager.GetTranslation("buy");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Cor.Stop(m_StopIndicateLoadingCoroutine);
        }
    }
}