using Entities;
using Games.RazorMaze.Views.Common;
using Ticker;
using UnityEngine.Events;

namespace UI.PanelItems.Shop_Items
{
    public class ShopMoneyItem : ShopItemBase
    {
        public override void Init(
            IManagersGetter _Managers,
            IUITicker _UITicker,
            IColorProvider _ColorProvider,
            UnityAction _Click,
            ViewShopItemInfo _Info)
        {
            base.Init(_Managers, _UITicker, _ColorProvider, _Click, _Info);
            itemIcon.sprite = _Info.Icon;
            if (_Info.Reward > 0)
                title.text = _Info.Reward.ToString();
        }
    }
}