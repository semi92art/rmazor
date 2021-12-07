using DI.Extensions;
using Entities;
using Games.RazorMaze.Views.Common;
using Ticker;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.PanelItems.Shop_Items
{
    public class ShopMoneyItem : ShopItemBase
    {
        private Image m_BuyButtonImage;
        
        public override void Init(
            IManagersGetter _Managers,
            IUITicker _UITicker,
            IColorProvider _ColorProvider,
            UnityAction _Click,
            ViewShopItemInfo _Info)
        {
            void FinishAction()
            {
                price.SetGoActive(!_Info.BuyForWatchingAd);
                if (!_Info.BuyForWatchingAd)
                    price.text = $"{_Info.Price} {_Info.Currency}";
            }
            InitCore(_Managers, _UITicker, _ColorProvider, _Click, _Info, FinishAction);
            m_BuyButtonImage = buyButton.GetComponent<Image>();
            m_BuyButtonImage.color = _ColorProvider.GetColor(ColorIds.UiDialogBackground);
            itemIcon.sprite = _Info.Icon;
            if (_Info.Reward > 0)
                title.text = _Info.Reward.ToString();
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            base.OnColorChanged(_ColorId, _Color);
            if (_ColorId == ColorIds.UiDialogBackground)
            {
                m_BuyButtonImage.color = _Color;
            }
        }
    }
}