using Entities;
using Ticker;
using UnityEngine.Events;
using Utils;

namespace UI.PanelItems.Shop_Items
{
    public class ShopMoneyItem : ShopItemBase
    {
        public override void Init(
            IManagersGetter _Managers,
            IUITicker _UITicker,
            UnityAction _Click,
            ViewShopItemInfo _Info)
        {
            void FinishAction()
            {
                price.text = $"{_Info.Price} {_Info.Currency}";
            }
            InitCore(_Managers, _UITicker, _Click, _Info, FinishAction);
            itemIcon.sprite = _Info.Icon;
            title.text = _Info.Reward.ToString();
            Coroutines.Run(Coroutines.WaitWhile(
                () => !_Info.Ready,
                () =>
                {
                    IndicateLoading(false, _Info.BuyForWatchingAd);
                    buyButton.SetGoActive(!_Info.BuyForWatchingAd);
                    price.text = $"{_Info.Price} {_Info.Currency}";
                }));
        }
    }
}