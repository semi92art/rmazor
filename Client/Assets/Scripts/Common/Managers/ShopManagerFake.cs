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
            {PurchaseKeys.Money1,    new ShopItemArgs(100m, "RUB 100", "RUB", false, () => Result)},
            {PurchaseKeys.Money2,    new ShopItemArgs(200m, "RUB 200", "RUB", false, () => Result)},
            {PurchaseKeys.Money3,    new ShopItemArgs(300m, "RUB 300", "RUB", false, () => Result)},
            {PurchaseKeys.NoAds,     new ShopItemArgs(400m, "RUB 400", "RUB", false, () => Result)},
            {PurchaseKeys.DarkTheme, new ShopItemArgs(400m, "RUB 400", "RUB", true, () => Result)},
            
            {PurchaseKeys.Character01, new ShopItemArgs(400m, "RUB 400", "RUB", true, () => Result)},
            {PurchaseKeys.Character02, new ShopItemArgs(400m, "RUB 400", "RUB", false, () => Result)},
            {PurchaseKeys.Character03, new ShopItemArgs(400m, "RUB 400", "RUB", false, () => Result)},
            {PurchaseKeys.Character04, new ShopItemArgs(400m, "RUB 400", "RUB", false, () => Result)},
            {PurchaseKeys.Character05, new ShopItemArgs(400m, "RUB 400", "RUB", false, () => Result)},
            {PurchaseKeys.Character06, new ShopItemArgs(400m, "RUB 400", "RUB", false, () => Result)},
            {PurchaseKeys.Character07, new ShopItemArgs(400m, "RUB 400", "RUB", false, () => Result)},
            {PurchaseKeys.Character08, new ShopItemArgs(400m, "RUB 400", "RUB", false, () => Result)},
            {PurchaseKeys.Character09, new ShopItemArgs(400m, "RUB 400", "RUB", false, () => Result)},
            {PurchaseKeys.Character10, new ShopItemArgs(400m, "RUB 400", "RUB", false, () => Result)},
            
            {PurchaseKeys.CharacterColorSet01, new ShopItemArgs(400m, "RUB 400", "RUB", false, () => Result)},
            {PurchaseKeys.CharacterColorSet02, new ShopItemArgs(400m, "RUB 400", "RUB", false, () => Result)},
            {PurchaseKeys.CharacterColorSet03, new ShopItemArgs(400m, "RUB 400", "RUB", false, () => Result)},
            {PurchaseKeys.CharacterColorSet04, new ShopItemArgs(400m, "RUB 400", "RUB", false, () => Result)},
            {PurchaseKeys.CharacterColorSet05, new ShopItemArgs(400m, "RUB 400", "RUB", false, () => Result)},
            {PurchaseKeys.CharacterColorSet06, new ShopItemArgs(400m, "RUB 400", "RUB", false, () => Result)},
            {PurchaseKeys.CharacterColorSet07, new ShopItemArgs(400m, "RUB 400", "RUB", false, () => Result)},
            {PurchaseKeys.CharacterColorSet08, new ShopItemArgs(400m, "RUB 400", "RUB", false, () => Result)},
            {PurchaseKeys.CharacterColorSet09, new ShopItemArgs(400m, "RUB 400", "RUB", false, () => Result)},
            {PurchaseKeys.CharacterColorSet10, new ShopItemArgs(400m, "RUB 400", "RUB", false, () => Result)},
        };
        
        private readonly Dictionary<int, UnityAction> m_PurchaseActions = new Dictionary<int, UnityAction>();

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