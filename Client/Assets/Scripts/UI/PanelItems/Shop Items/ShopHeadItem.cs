using Entities;
using Games.RazorMaze.Views.Common;
using Ticker;
using UnityEngine.Events;

namespace UI.PanelItems.Shop_Items
{
    public class ShopHeadItem : ShopItemBase
    {
        public override void Init(
            IManagersGetter _Managers,
            IUITicker _UITicker,
            IColorProvider _ColorProvider,
            UnityAction _Click,
            ViewShopItemInfo _Info)
        {
            void FinishAction()
            {
                price.text = $"{_Info.Price}";
            }
            InitCore(_Managers, _UITicker, _ColorProvider, _Click, _Info, FinishAction);
        }
    }
}