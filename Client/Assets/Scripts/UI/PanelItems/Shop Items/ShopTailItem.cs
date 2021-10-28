using Entities;
using Ticker;
using UnityEngine.Events;

namespace UI.PanelItems.Shop_Items
{
    public class ShopTailItem : ShopItemBase
    {
        public override void Init(
            IManagersGetter _Managers,
            IUITicker _UITicker,
            UnityAction _Click,
            ViewShopItemInfo _Info)
        {
            void FinishAction()
            {
                price.text = $"{_Info.Price}";
            }
            InitCore(_Managers, _UITicker, _Click, _Info, FinishAction);
        }
    }
}