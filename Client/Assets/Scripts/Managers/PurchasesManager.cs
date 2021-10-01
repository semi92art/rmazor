using UnityEngine.Events;

namespace Managers
{
    public interface IPurchasesManager
    {
        void Purchase(string _PurchaseCode, UnityAction _OnPurchase);
    }
    
    public class PurchasesManager : IPurchasesManager
    {
        #region singleton
    
        private static PurchasesManager _instance;
        public static PurchasesManager Instance => _instance ?? (_instance = new PurchasesManager());
    
        #endregion
        
        #region api
        
        public void Purchase(string _PurchaseCode, UnityAction _OnPurchase)
        {
            throw new System.NotImplementedException();
        }

        #endregion


    }
}