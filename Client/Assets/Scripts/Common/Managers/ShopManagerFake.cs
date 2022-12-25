using System.Collections.Generic;
using Common.Constants;
using Common.Extensions;
using Common.Utils;
using mazing.common.Runtime;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers.IAP;
using mazing.common.Runtime.Utils;
using UnityEngine.Events;

namespace Common.Managers.IAP
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ShopManagerFake : ShopManagerBase
    {
        private const EShopProductResult Result = EShopProductResult.Fail;
        
        private readonly Dictionary<int, ShopItemArgs> m_ShopItems = new Dictionary<int, ShopItemArgs>
        {
            {PurchaseKeys.Money1, new ShopItemArgs
            {
                Currency = "RUB",
                Price = "100",
                Result = () => Result
            }},
            {PurchaseKeys.Money2, new ShopItemArgs
            {
                Currency = "RUB",
                Price = "200",
                Result = () => Result
            }},
            {PurchaseKeys.Money3, new ShopItemArgs
            {
                Currency = "RUB",
                Price = "300",
                Result = () => Result
            }},
            {PurchaseKeys.NoAds, new ShopItemArgs
            {
                Currency = "RUB",
                Price = "100",
                Result = () => Result
            }},
            {PurchaseKeys.DarkTheme, new ShopItemArgs
            {
                Currency = "RUB",
                Price = "100",
                Result = () => Result
            }}
        };
        
        private readonly   Dictionary<int, UnityAction> m_PurchaseActions = new Dictionary<int, UnityAction>();

        public override void RestorePurchases()
        {
            SaveUtils.PutValue(SaveKeysMazor.DisableAds, null);
            Dbg.Log("Purchases restored.");
        }

        public override void Purchase(int _Key)
        {
            m_PurchaseActions[_Key]?.Invoke();
        }

        public override bool RateGame()
        {
            return false;
        }

        public override ShopItemArgs GetItemInfo(int _Key)
        {
            var args = m_ShopItems[_Key];
            return args;
        }

        public override void AddPurchaseAction(int _ProductKey, UnityAction _Action)
        {
            m_PurchaseActions.SetSafe(_ProductKey, _Action);
        }

        public override void AddDeferredAction(int _Key, UnityAction _Action)
        {
            // do nothing
        }
    }
}