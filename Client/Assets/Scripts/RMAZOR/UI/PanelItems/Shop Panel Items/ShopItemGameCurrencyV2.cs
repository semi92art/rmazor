using System;
using System.Collections.Generic;
using Common;
using Common.Managers.PlatformGameServices;
using mazing.common.Runtime;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Shop_Panel_Items
{
    public class ViewShopItemInfoGameCurrencyV2 : ViewShopItemInfoBaseV2
    {
        public string      Id                             { get; set; }
        public int         Price                          { get; set; }
        public bool        Consumable                     { get; set; }
        public UnityAction BuyButtonClickIfNotEnoughMoney { get; set; }
    }
    
    public class ShopItemGameCurrencyV2 : ShopItemCurrencyBaseV2
    {
        #region serialized fields

        [SerializeField] private Image coinIcon;

        #endregion
        
        #region nonpublic members

        private ViewShopItemInfoGameCurrencyV2 m_ShopItemInfo;

        private IScoreManager ScoreManager { get; set; }
        
        #endregion
        
        #region api
        
        public event UnityAction Purchased;

        public void Init(
            IUITicker                      _UITicker,
            IAudioManager                  _AudioManager,
            ILocalizationManager           _LocalizationManager,
            IAnalyticsManager              _AnalyticsManager,
            IScoreManager                  _ScoreManager,
            ViewShopItemInfoGameCurrencyV2 _ShopItemInfo)
        {
            m_ShopItemInfo = _ShopItemInfo;
            ScoreManager   = _ScoreManager;
            base.Init(_UITicker, _AudioManager, _LocalizationManager, _AnalyticsManager, _ShopItemInfo);
            priceText.text = _ShopItemInfo.Price.ToString();
            UpdateState();
        }

        public override void UpdateState() { }
        
        #endregion

        #region nonpublic methods
        
        protected override void OnBuyButtonClick()
        {
            PlayButtonClickSound();
            if (IsEnoughGameMoney())
            {
                m_ShopItemInfo.BuyAction?.Invoke();
                SetItemAsPurchased();
                SpentGameCurrency();    
            }
            else
            {
                m_ShopItemInfo.BuyButtonClickIfNotEnoughMoney?.Invoke();
            }
        }

        protected override void SetItemAsPurchased()
        {
            int idHash = CommonUtils.GetHash(m_ShopItemInfo.Id);
            var idsOfPurchasedItems = SaveUtils.GetValue(SaveKeysMazor.BoughtPurchaseIds);
            if (!idsOfPurchasedItems.Contains(idHash) && !m_ShopItemInfo.Consumable)
            {
                idsOfPurchasedItems.Add(idHash);
                SaveUtils.PutValue(SaveKeysMazor.BoughtPurchaseIds, idsOfPurchasedItems);
            }
            coinIcon.enabled = false;
            base.SetItemAsPurchased();
            Purchased?.Invoke();
        }

        protected override bool HasReceipt
        {
            get
            {
                try
                {
                    var boughtPurchaseIds = SaveUtils.GetValue(SaveKeysMazor.BoughtPurchaseIds) 
                                            ?? new List<int>();
                    return !m_ShopItemInfo.Consumable
                           && boughtPurchaseIds.Contains(CommonUtils.GetHash(m_ShopItemInfo.Id));
                }
                catch (Exception ex)
                {
                    Dbg.LogError(ex);
                    return false;
                }
            }
        }

        private void SpentGameCurrency()
        {
            var savedGame = ScoreManager.GetSavedGame(CommonDataMazor.SavedGameFileName);
            object bankMoneyCountArg = savedGame.Arguments.GetSafe(ComInComArg.KeyMoneyCount, out _);
            long money = Convert.ToInt64(bankMoneyCountArg);
            money -= m_ShopItemInfo.Price;
            savedGame.Arguments.SetSafe(ComInComArg.KeyMoneyCount, money);
            ScoreManager.SaveGame(savedGame);
            m_ShopItemInfo.BuyAction?.Invoke();
            Purchased?.Invoke();
        }

        private bool IsEnoughGameMoney()
        {
            var savedGame = ScoreManager.GetSavedGame(CommonDataMazor.SavedGameFileName);
            object bankMoneyCountArg = savedGame.Arguments.GetSafe(ComInComArg.KeyMoneyCount, out _);
            long money = Convert.ToInt64(bankMoneyCountArg);
            return money >= m_ShopItemInfo.Price;
        }

        #endregion
    }
}