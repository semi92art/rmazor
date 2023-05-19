using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common;
using Common.Constants;
using Common.Managers.PlatformGameServices;
using Common.Utils;
using mazing.common.Runtime;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Utils;
using UnityEngine.Events;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.UI.Panels.ShopPanels
{
    public interface IShopBuyActionsProvider
    {
        UnityAction GetAction(string _Item);
    }
    
    public class ShopBuyActionsProvider : IShopBuyActionsProvider
    {
        #region inject
        
        private IScoreManager        ScoreManager        { get; }
        private ILocalizationManager LocalizationManager { get; }
        private IAnalyticsManager    AnalyticsManager    { get; }

        private ShopBuyActionsProvider(
            IScoreManager        _ScoreManager,
            ILocalizationManager _LocalizationManager,
            IAnalyticsManager    _AnalyticsManager)
        {
            ScoreManager        = _ScoreManager;
            LocalizationManager = _LocalizationManager;
            AnalyticsManager    = _AnalyticsManager;
        }

        #endregion
        

        #region api

        public UnityAction GetAction(string _Item)
        {
            return _Item switch
            {
                "special_offer"      => BuySpecialOfferItemAction,
                "coins_1"            => BuyCoinItem1Action,
                "coins_2"            => BuyCoinItem2Action,
                "coins_3"            => BuyCoinItem3Action,
                "retro_mode"         => BuyRetroModeItemAction,
                "full_customization" => BuyFullCustomizationItemAction,
                "no_ads"             => BuyNoAdsItemAction,
                "x2_new_coins"       => BuyX2NewCoinsItemAction,
                _                    => throw new SwitchExpressionException(_Item)
            };
        }

        #endregion
        
        #region nonpublic methods

        private void BuySpecialOfferItemAction()
        {
            BuyNoAdsItemAction();
            BuyCoinItemAction(PurchaseKeys.SpecialOffer, 50000);
            BuyX3NewCoinsItemAction();
            BuyFullCustomizationItemAction();
            BuyRetroModeItemAction();
        }

        private void BuyCoinItem1Action()
        {
            BuyCoinItemAction(PurchaseKeys.Money1, 3000);
        }

        private void BuyCoinItem2Action()
        {
            BuyCoinItemAction(PurchaseKeys.Money2, 10000);
        }

        private void BuyCoinItem3Action()
        {
            BuyCoinItemAction(PurchaseKeys.Money3, 30000);
        }

        private static void BuyRetroModeItemAction()
        {
            SaveUtils.PutValue(SaveKeysRmazor.RetroModeUnlocked, true);
        }

        private static void BuyFullCustomizationItemAction()
        {
            SaveUtils.PutValue(SaveKeysRmazor.FullCustomizationUnlocked, true);
        }

        private static void BuyNoAdsItemAction()
        {
            SaveUtils.PutValue(SaveKeysMazor.DisableAds, true);
        }

        private static void BuyX2NewCoinsItemAction()
        {
            float multCoeff = SaveUtils.GetValue(SaveKeysRmazor.MultiplyNewCoinsCoefficient);
            if (multCoeff < 2f)
                SaveUtils.PutValue(SaveKeysRmazor.MultiplyNewCoinsCoefficient, 2f);
        }
        
        private static void BuyX3NewCoinsItemAction()
        {
            float multCoeff = SaveUtils.GetValue(SaveKeysRmazor.MultiplyNewCoinsCoefficient);
            if (multCoeff < 3f)
                SaveUtils.PutValue(SaveKeysRmazor.MultiplyNewCoinsCoefficient, 3f);
        }

        private void BuyCoinItemAction(int _PurchaseKey, long _Reward)
        {
            var savedGame = ScoreManager.GetSavedGame(CommonDataMazor.SavedGameFileName);
            var bankMoneyCountArg = savedGame.Arguments.GetSafe(KeyMoneyCount, out _);
            long money = Convert.ToInt64(bankMoneyCountArg);
            money += _Reward;
            savedGame.Arguments.SetSafe(KeyMoneyCount, money);
            ScoreManager.SaveGame(savedGame);
            string dialogTitle = LocalizationManager.GetTranslation("purchase") + ":";
            string dialogText = _Reward + " " +
                                LocalizationManager
                                    .GetTranslation("coins_alt")
                                    .ToLowerInvariant();
            MazorCommonUtils.ShowAlertDialog(dialogTitle, dialogText);
            string productId = _PurchaseKey switch
            {
                PurchaseKeys.Money1  => "coins_pack_small",
                PurchaseKeys.Money2  => "coins_pack_medium",
                PurchaseKeys.Money3  => "coins_pack_large",
                _  => null
            };
            if (productId == null)
            {
                Dbg.LogError("Analytic Id was not found by Purchase Key");
                return;
            }
        }

        #endregion
    }
}