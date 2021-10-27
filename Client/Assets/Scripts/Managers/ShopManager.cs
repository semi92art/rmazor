using System;
using UnityEngine;
using UnityEngine.Events;

namespace Managers
{
    public class ShopItemInfo
    {
        public string Price { get; set; }
        public string Currency { get; set; }
        public Func<bool> Ready { get; set; }
    }
    
    public interface IShopManager
    {
        void Purchase(int _Key, UnityAction _OnPurchase);
        void GoToRatePage();
        ShopItemInfo GetItemInfo(int _Key);
    }
    
    public class ShopManager : IShopManager
    {
        #region singleton
    
        private static ShopManager _instance;
        public static ShopManager Instance => _instance ?? (_instance = new ShopManager());
    
        #endregion
        
        #region api
        
        public void Purchase(int _Key, UnityAction _OnPurchase)
        {
            throw new System.NotImplementedException();
        }

        public void GoToRatePage()
        {
            throw new System.NotImplementedException();
        }

        public ShopItemInfo GetItemInfo(int _Key)
        {
            var res = new ShopItemInfo();
            string code = GetPurchaseCode(_Key);
            GetPurchaseItemInfo(code, ref res, out var ready);
            res.Ready = ready;
            return res;
        }

        private string GetPurchaseCode(int _Key)
        {
            // TODO здесь получить код товара
            return default;
        }

        private void GetPurchaseItemInfo(string _Code, ref ShopItemInfo _Info, out Func<bool> _Ready)
        {
            // TODO здесь получить инфу о товаре
            _Info.Price = "100";
            _Info.Currency = "USD";
            float time = Time.time;
            _Ready = () => Time.time > time + 2f;
        }

        #endregion


    }
}