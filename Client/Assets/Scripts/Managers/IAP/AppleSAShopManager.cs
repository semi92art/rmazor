using System.Linq;
using System.Text;
using Common;
using Common.Utils;
using SA.iOS.StoreKit;
using SA.iOS.UIKit;
using UnityEngine.Events;
using Utils;

namespace Managers.IAP
{
    public class AppleSAShopManager : ShopManagerBase
    {

        #region nonpublic members

        private ISN_iSKPaymentTransactionObserverFacade m_Observer;
        
        #endregion

        #region inject
        private IRemoteConfigManager RemoteConfigManager { get; }

        public AppleSAShopManager(IRemoteConfigManager _RemoteConfigManager)
        {
            RemoteConfigManager = _RemoteConfigManager;
        }

        #endregion
        
        #region api
        
        public override void Init()
        {
            InitPurchasing(() =>
            {
                base.Init();
            });
        }

        public override void RestorePurchases()
        {
            ISN_SKPaymentQueue.RestoreCompletedTransactions();
        }

        public override void Purchase(int _Key)
        {
            string id = GetProductId(_Key);
            ISN_SKPaymentQueue.AddPayment(id);
        }

        public override void RateGame(bool _JustSuggest = true)
        {
            if (_JustSuggest)
                ISN_SKStoreReviewController.RequestReview();
            else
            {
                string appId = RemoteConfigManager.GetConfig<string>("common.iosgameid");
                string writeReviewURL = $"https://itunes.apple.com/app/id{appId}?action=write-review";
                ISN_UIApplication.OpenURL(writeReviewURL);
            }
        }

        public override ShopItemArgs GetItemInfo(int _Key)
        {
            var args = new ShopItemArgs {Result = () => EShopProductResult.Pending};
            Cor.Run(Cor.Delay(
                3f,
                () =>
                {
                    if (args.Result() == EShopProductResult.Pending)
                        args.Result = () => EShopProductResult.Fail;
                }));
            Cor.Run(Cor.Action(() =>
            {
                string id = GetProductId(_Key);
                var product = ISN_SKPaymentQueue.Products.FirstOrDefault(_P => _P.ProductIdentifier == id);
                if (product == null)
                {
                    Dbg.LogError("Product was not found in payment queue");
                }
                else
                {         
                    Dbg.Log($"Product was found in paymet queue: {product.ProductIdentifier}");
                    args.Price = product.LocalizedPrice;
                    args.Currency = product.PriceLocale.CurrencyCode;
                    args.Result = () => EShopProductResult.Success;
                }
            }));
            return args;
        }

        public override void SetPurchaseAction(int _Key, UnityAction _Action)
        {
            string id = GetProductId(_Key);
            m_Observer.SetPurchaseAction(id, _Action);
        }

        public override void SetDeferredAction(int _Key, UnityAction _Action)
        {
            string id = GetProductId(_Key);
            m_Observer.SetDeferredAction(id, _Action);
        }

        #endregion

        #region nonpublic methods
        
        private void InitPurchasing(UnityAction _OnFinish)
        {
            foreach (var product in Products)
                ISN_SKPaymentQueue.RegisterProductId(product.Id);
            var sb = new StringBuilder();
            ISN_SKPaymentQueue.Init(_Result =>
            {
                Dbg.Log(_Result.ToJson());
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
                if (_Result.InvalidProductIdentifiers.Any())
                {
                    sb.AppendLine($"result.InvalidProductIdentifiers.Count {_Result.InvalidProductIdentifiers.Count}");
                    foreach (var invalidProductIdentifier in _Result.InvalidProductIdentifiers)
                        sb.AppendLine($"Invalid identifier: {invalidProductIdentifier}");
                    Dbg.LogWarning(sb.ToString());
                }
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

        #endregion
    }
}