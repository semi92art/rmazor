using DI.Extensions;
using Entities;
using Games.RazorMaze.Views.Common;
using Ticker;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils;

namespace UI.PanelItems.Shop_Items
{
    public class ViewShopItemInfo
    {
        public int    PurchaseKey      { get; set; }
        public bool   BuyForWatchingAd { get; set; }
        public int    UnlockingLevel   { get; set; }
        public string Price            { get; set; }
        public bool   Ready            { get; set; }
        public Sprite Icon             { get; set; }
        public string Currency         { get; set; }
        public int    Reward           { get; set; }
    }
    
    public abstract class ShopItemBase : SimpleUiDialogItemView
    {
        public TextMeshProUGUI price;
        public TextMeshProUGUI title;
        public Image itemIcon;
        [SerializeField] protected Button buyButton;
        [SerializeField] protected Image watchAdImage;
        [SerializeField] protected Animator loadingAnim;

        protected ViewShopItemInfo m_Info;
        private   bool             m_IsBuyButtonNotNull;
        private   bool             m_IsWatchAdImageNotNull;
        
        public virtual void Init(
            IManagersGetter _Managers,
            IUITicker _UITicker,
            IColorProvider _ColorProvider,
            UnityAction _Click,
            ViewShopItemInfo _Info)
        {
            base.Init(_Managers, _UITicker, _ColorProvider);
            m_Info = _Info;
            watchAdImage.SetGoActive(true);
            loadingAnim.SetGoActive(true);
            name = "Shop Item";
            buyButton.onClick.AddListener(SoundOnClick);
            buyButton.onClick.AddListener(_Click);
            itemIcon.sprite = _Info.Icon;
            IndicateLoading(true, _Info.BuyForWatchingAd);
            Coroutines.Run(Coroutines.WaitWhile(
                () => !_Info.Ready,
                () =>
                {
                    IndicateLoading(false, _Info.BuyForWatchingAd);
                    FinishAction();
                }));
        }
        
        public override void Init(
            IManagersGetter _Managers, 
            IUITicker _UITicker, 
            IColorProvider _ColorProvider)
        {
            throw new System.NotSupportedException();
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
                buyButton.targetGraphic.color = ColorProvider.GetColor(ColorIds.UiDialogBackground);
            if (m_IsWatchAdImageNotNull)
                watchAdImage.color = ColorProvider.GetColor(ColorIds.UI);
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            base.OnColorChanged(_ColorId, _Color);
            if (_ColorId == ColorIds.UiDialogBackground)
            {
                if (m_IsBuyButtonNotNull)
                    buyButton.targetGraphic.color = _Color;
            }
            else if (_ColorId == ColorIds.UI)
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

        protected virtual void FinishAction()
        {
            price.SetGoActive(!m_Info.BuyForWatchingAd);
            if (m_Info.BuyForWatchingAd) 
                return;
            price.text = $"{m_Info.Price} {m_Info.Currency}";
            if (string.IsNullOrEmpty(price.text) || string.IsNullOrWhiteSpace(price.text))
                price.text = Managers.LocalizationManager.GetTranslation("buy");
        }
    }
}