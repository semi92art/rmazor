using DI.Extensions;
using Entities;
using Ticker;
using UI.Entities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.PanelItems.Shop_Items
{
    public class ShopMainItem : ShopItemBase
    {
        [SerializeField] private Button button;
        
        public override void Init(
            IManagersGetter _Managers,
            IUITicker _UITicker,
            UnityAction _Click,
            ShopItemArgs _Args)
        {
            InitCore(_Managers, _UITicker);
            name = "Shop Item";
            button.onClick.AddListener(SoundOnClick);
            button.onClick.AddListener(_Click);

            itemIcon.sprite = _Args.Icon;
            // title.rectTransform.Set(
            //     UiAnchor.Create(0, 0, 1, 1),
            //     new Vector2(-0.5f, 0f),
            //     new Vector2(0.5f, 0.5f),
            //     new Vector2(-12.2f, -22.3f));
        }
    }
}