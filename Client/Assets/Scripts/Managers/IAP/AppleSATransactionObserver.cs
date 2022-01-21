using System.Collections.Generic;
using System.Text;
using Common;
using Common.Extensions;
using Newtonsoft.Json;
using SA.Foundation.Templates;
using SA.iOS.StoreKit;
using UnityEngine.Events;

namespace Managers.IAP
{
    public interface ISN_iSKPaymentTransactionObserverFacade : ISN_iSKPaymentTransactionObserver
    {
        void SetPurchaseAction(string _Id, UnityAction _Action);
        void SetDeferredAction(string _Id, UnityAction _Action);
    }
    
    public class AppleSATransactionObserver : ISN_iSKPaymentTransactionObserverFacade
    {
        #region nonpublic members

        private readonly Dictionary<string, UnityAction> m_PurchaseActions = new Dictionary<string, UnityAction>();
        private readonly Dictionary<string, UnityAction> m_DeferredActions = new Dictionary<string, UnityAction>();

        #endregion
        
        #region api

        public void OnTransactionUpdated(ISN_iSKPaymentTransaction _Transaction)
        {
            string tRaw = _Transaction == null ? "null" : JsonConvert.SerializeObject(_Transaction);
            if (_Transaction == null)
            {
                Dbg.LogError("Transaction is null");
                return;
            }
            var sb = new StringBuilder();
            sb.AppendLine($"transaction JSON: {tRaw}");
            sb.AppendLine($"OnTransactionComplete: {_Transaction.ProductIdentifier}");
            sb.AppendLine($"OnTransactionComplete: state: {_Transaction.State}");
            Dbg.Log(sb.ToString());
            string id = _Transaction.ProductIdentifier;
            switch (_Transaction.State) 
            {
                case ISN_SKPaymentTransactionState.Purchasing:
                    break;
                case ISN_SKPaymentTransactionState.Purchased:
                case ISN_SKPaymentTransactionState.Restored:
                    UnlockProducts(_Transaction);
                    break;
                case ISN_SKPaymentTransactionState.Deferred:
                    //iOS 8 introduces Ask to Buy, which lets parents approve any 
                    //purchases initiated by children
                    //You should update your UI to reflect this deferred state, 
                    //and expect another Transaction Complete to be called again 
                    //with a new transaction state 
                    //reflecting the parent’s decision or after the transaction times out. 
                    //Avoid blocking your UI or gameplay while waiting 
                    //for the transaction to be updated.
                    var deferredAction = m_DeferredActions.GetSafe(id, out bool containsKey);
                    if (!containsKey)
                        Dbg.LogError($"Product with key \"{id}\" does not have deferred action");
                    else
                        deferredAction?.Invoke();
                    break;
                case ISN_SKPaymentTransactionState.Failed:
                    //Our purchase flow is failed.
                    //We can unlock intrefase and report user that the purchase is failed. 
                    Dbg.Log("Transaction failed, code: " + _Transaction.Error.Code + "\n" +
                            "Transaction failed, description: " + _Transaction.Error.Message);
                    ISN_SKPaymentQueue.FinishTransaction(_Transaction);
                    break;
            }
            if (_Transaction.State == ISN_SKPaymentTransactionState.Failed) 
                Dbg.LogWarning("Error code: " + _Transaction.Error.Code + "\n" + "Error description:" + _Transaction.Error.Message);
            else 
                Dbg.Log("product " + _Transaction.ProductIdentifier + " state: " + _Transaction.State);
        }
        
        public bool OnShouldAddStorePayment(ISN_SKProduct _Result)
        {
            // Return true to continue the transaction in your app.
            // Return false to defer or cancel the transaction.
            // If you return false, you can continue the transaction later using requetsed <see cref="ISN_SKProduct"/>
            // 
            // we are okay, to continue trsansaction, so let's return true
            Dbg.Log(nameof(OnShouldAddStorePayment) + ": " + _Result.ProductIdentifier);
            return true;
        }

        public void OnRestoreTransactionsComplete(SA_Result _Result)
        {
            if (_Result.IsSucceeded) {
                Dbg.Log("Restore Compleated");
            } else {
                Dbg.Log("Error: " + _Result.Error.Code + " message: " + _Result.Error.Message);
            }
        }
        
        public void SetPurchaseAction(string _Id, UnityAction _Action)
        {
            m_PurchaseActions.SetSafe(_Id, _Action);
        }

        public void SetDeferredAction(string _Id, UnityAction _Action)
        {
            m_DeferredActions.SetSafe(_Id, _Action);
        }

        public void OnTransactionRemoved(ISN_iSKPaymentTransaction _Result) 
        {
            // do nothing
        }

        public void DidChangeStorefront()
        {
            // do nothing
        }

        #endregion
        
        #region nonpublic methods
        
        private void UnlockProducts(ISN_iSKPaymentTransaction _Transaction)
        {
            Dbg.Log("Receipt: " + ISN_SKPaymentQueue.AppStoreReceipt.AsBase64String);
            string id = _Transaction.ProductIdentifier;
            if (m_PurchaseActions.ContainsKey(id))
            {
                m_PurchaseActions[id]?.Invoke();
                m_PurchaseActions.Remove(id);
            }
            else
            {
                Dbg.LogError($"Purchase action for id {id} was not set");
            }
            ISN_SKPaymentQueue.FinishTransaction(_Transaction);
        }
        
        #endregion
    }
}