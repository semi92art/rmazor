using System.Collections.Generic;
using Constants;
using Entities;
using Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.PanelItems
{
    public class ShopItem : MenuItemBase
    {
        public Button button;
        public Image icon;
        public TextMeshProUGUI price;
        public TextMeshProUGUI amount;

        private ShopItemProps m_Props;

        public void Init(ShopItemProps _Props, IEnumerable<IGameObserver> _Observers)
        {
            icon.sprite = _Props.Icon;
            price.text = $"{_Props.DiscountPrice} <color=#F33B3B><s>{_Props.Price}</s>";
            amount.text = _Props.Amount;
            base.Init(_Observers);
            if (_Props.Click != null)
                button.SetOnClick(() =>
                {
                    Notifyer.RaiseNotify(this, CommonNotifyIds.UiButtonClick, _Props.Id);
                    _Props.Click?.Invoke();
                });
        }
    }

    public class ShopItemProps
    {
        public int Id { get; }
        public string Amount { get; }
        public string DiscountPrice { get; }
        public string Price { get; }
        public Sprite Icon { get; }
        public UnityAction Click { get; set; }

        public ShopItemProps(
            int _Id,
            string _Amount,
            string _DiscountPrice,
            string _Price,
            Sprite _Icon)
        {
            Id = _Id;
            Amount = _Amount;
            DiscountPrice = _DiscountPrice;
            Price = _Price;
            Icon = _Icon;
        }
    }
}