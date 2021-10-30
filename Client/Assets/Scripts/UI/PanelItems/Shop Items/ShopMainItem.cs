using DI.Extensions;
using Entities;
using Games.RazorMaze.Views.Common;
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
            IColorProvider _ColorProvider,
            UnityAction _Click,
            ViewShopItemInfo _Info)
        {
            InitCore(_Managers, _UITicker, _ColorProvider);
            name = "Shop Item";
            button.onClick.AddListener(SoundOnClick);
            button.onClick.AddListener(_Click);

            itemIcon.sprite = _Info.Icon;
            // title.rectTransform.Set(
            //     UiAnchor.Create(0, 0, 1, 1),
            //     new Vector2(-0.5f, 0f),
            //     new Vector2(0.5f, 0.5f),
            //     new Vector2(-12.2f, -22.3f));
        }
    }
}