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
            ShopItemArgs _Args)
        {
            void FinishAction()
            {
                price.text = $"{_Args.Price}";
            }
            InitCore(_Managers, _UITicker, _Click, _Args, FinishAction);
        }
    }
}