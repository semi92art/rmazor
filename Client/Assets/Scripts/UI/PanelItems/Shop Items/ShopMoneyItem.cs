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
            ShopItemArgs _Args)
        {
            void FinishAction()
            {
                price.text = $"{_Args.Price} {_Args.Currency}";
            }
            InitCore(_Managers, _UITicker, _Click, _Args, FinishAction);
            itemIcon.sprite = _Args.Icon;
            title.text = _Args.Reward.ToString();
            Coroutines.Run(Coroutines.WaitWhile(
                () => !_Args.Ready,
                () =>
                {
                    IndicateLoading(false, _Args.BuyForWatchingAd);
                    buyButton.SetGoActive(!_Args.BuyForWatchingAd);
                    price.text = $"{_Args.Price} {_Args.Currency}";
                }));
        }
    }
}