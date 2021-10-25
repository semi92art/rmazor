using UnityEngine.Events;

namespace Managers
{
    public interface IShopManager
    {
        void Purchase(string _PurchaseCode, UnityAction _OnPurchase);
        void GoToRatePage();
    }
    
    public class ShopManager : IShopManager
    {
        #region singleton
    
        private static ShopManager _instance;
        public static ShopManager Instance => _instance ?? (_instance = new ShopManager());
    
        #endregion
        
        #region api
        
        public void Purchase(string _PurchaseCode, UnityAction _OnPurchase)
        {
            throw new System.NotImplementedException();
        }

        public void GoToRatePage()
        {
            throw new System.NotImplementedException();
        }

        #endregion


    }
}