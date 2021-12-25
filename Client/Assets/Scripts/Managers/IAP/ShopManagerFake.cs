using System.Collections.Generic;
using Constants;
using DI.Extensions;
using Entities;
using UnityEditor;
using UnityEngine.Events;
using Utils;

namespace Managers.IAP
{
    public class ShopManagerFake : ShopManagerBase, IShopManager
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
            }}
        };
        
        private readonly   Dictionary<int, UnityAction> m_PurchaseActions = new Dictionary<int, UnityAction>();
        protected override List<ProductInfo>            Products    => null;
        public             bool                         Initialized { get; private set; }
        public event UnityAction                        Initialize;
        
        public void Init()
        {
            Initialize?.Invoke();
            Initialized = true;
        }

        public void RestorePurchases()
        {
            SaveUtils.PutValue(SaveKeys.DisableAds, null);
            Dbg.Log("Purchases restored.");
        }

        public void Purchase(int _Key)
        {
            m_PurchaseActions[_Key]?.Invoke();
        }

        public void RateGame()
        {
#if UNITY_EDITOR
            EditorUtility.DisplayDialog("Dialog", "Available only on device", "OK");
#endif
        }

        public ShopItemArgs GetItemInfo(int _Key)
        {
            var args = m_ShopItems[_Key];
            return args;
        }

        public void SetPurchaseAction(int _Key, UnityAction _Action)
        {
            m_PurchaseActions.SetSafe(_Key, _Action);
        }

        public void SetDeferredAction(int _Key, UnityAction _Action)
        {
            // do nothing
        }
    }
}