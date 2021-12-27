using System.Collections.Generic;
using System.Linq;
using System.Text;
using Constants;
using SA.iOS.StoreKit;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using Utils;

namespace Managers.IAP
{
    public class AppleSAShopManager : ShopManagerBase, IShopManager
    {
        protected override List<ProductInfo> Products => new List<ProductInfo>
        {
            new ProductInfo(PurchaseKeys.Money1, "small_pack_of_coins_2",           ProductType.Consumable),
            new ProductInfo(PurchaseKeys.Money2, "medium_pack_of_coins_2",          ProductType.Consumable),
            new ProductInfo(PurchaseKeys.Money3, Application.identifier + ".bigpackofcoins",             ProductType.Consumable),
            new ProductInfo(PurchaseKeys.NoAds,  "disable_mandatory_advertising_2", ProductType.NonConsumable),
        };

        private ISN_iSKPaymentTransactionObserverFacade m_Observer;
        
        public bool              Initialized { get; private set; }
        public event UnityAction Initialize;
        public void Init()
        {
            InitPurchasing(() =>
            {
                Initialize?.Invoke();
                Initialized = true;
            });
        }

        public void RestorePurchases()
        {
            ISN_SKPaymentQueue.RestoreCompletedTransactions();
        }

        public void Purchase(int _Key)
        {
            string id = GetProductId(_Key);
            ISN_SKPaymentQueue.AddPayment(id);
        }

        public void RateGame()
        {
            ISN_SKStoreReviewController.RequestReview();
        }

        public ShopItemArgs GetItemInfo(int _Key)
        {
            var args = new ShopItemArgs {Result = () => EShopProductResult.Pending};
            Coroutines.Run(Coroutines.Delay(
                3f,
                () =>
                {
                    if (args.Result() == EShopProductResult.Pending)
                        args.Result = () => EShopProductResult.Fail;
                }));
            Coroutines.Run(Coroutines.Action(() =>
            {
                string id = GetProductId(_Key);
                var product = ISN_SKPaymentQueue.Products.FirstOrDefault(_P => _P.ProductIdentifier == id);
                if (product == null)
                {
                    Dbg.LogError("Product was not found in payment queue");
                }
                else
                {         
                    Dbg.Log("Product was found in paymet queue");
                    args.Price = product.LocalizedPrice;
                    args.Currency = product.PriceLocale.CurrencyCode;
                    args.Result = () => EShopProductResult.Success;
                }
            }));
            return args;
        }

        public void SetPurchaseAction(int _Key, UnityAction _Action)
        {
            string id = GetProductId(_Key);
            m_Observer.SetPurchaseAction(id, _Action);
        }

        public void SetDeferredAction(int _Key, UnityAction _Action)
        {
            string id = GetProductId(_Key);
            m_Observer.SetDeferredAction(id, _Action);
        }

        private void InitPurchasing(UnityAction _OnFinish)
        {
            foreach (var product in Products)
                ISN_SKPaymentQueue.RegisterProductId(product.Id);
            var sb = new StringBuilder();
            ISN_SKPaymentQueue.Init(_Result =>
            {
                if (_Result.IsFailed || _Result.HasError)
                {
                    Dbg.LogWarning($"Failed to init purchasing: isFailed: {_Result.IsFailed}; {_Result.Error.FullMessage}");
                }
                else
                {
                    Dbg.Log("Purchasing initialized successfully");
                    InitObservers();
                }
                _OnFinish?.Invoke();
                sb.AppendLine($"Can make payments: {CanMakePayments()}");
                sb.AppendLine($"result.Products.Count: {_Result.Products.Count}");
                Dbg.Log(sb.ToString());
                sb.Clear();
                sb.AppendLine($"result.InvalidProductIdentifiers.Count {_Result.InvalidProductIdentifiers.Count}");
                foreach (var invalidProductIdentifier in _Result.InvalidProductIdentifiers)
                    sb.AppendLine($"Invalid identifier: {invalidProductIdentifier}");
                Dbg.LogWarning(sb.ToString());
            });
        }

        private static bool CanMakePayments()
        {
            return ISN_SKPaymentQueue.CanMakePayments;
        }

        private void InitObservers()
        {
            m_Observer = new AppleSATransactionObserver();
            ISN_SKPaymentQueue.AddTransactionObserver(m_Observer);
        }
    }
}