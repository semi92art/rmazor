using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Entities;
using Common.Extensions;
using Common.Managers;
using Common.ScriptableObjects;
using Common.Utils;
using RMAZOR.UI.Panels.ShopPanels;
using TMPro;

namespace RMAZOR.Utils
{
    public static class ShopUtils
    {
        private static int GetMoneySpentOnPurchases()
        {
            var boughtPurchaseIds = SaveUtils.GetValue(SaveKeysCommon.BoughtPurchaseIds);
            if (boughtPurchaseIds == null)
                return 0;
            static ShopItemsScriptableObject.ShopItemSet GetSet(string _Key)
            {
                var abm = new AssetBundleManagerFake();
                var psm = new PrefabSetManager(abm);
                return psm
                    .GetObject<ShopItemsScriptableObject>("shop_items", _Key)?.set;
            }
            var sets = new List<ShopItemsScriptableObject.ShopItemSet>
            {
                GetSet(ShopHeadsPanel.ItemsSet),
                GetSet(ShopTailsPanel.ItemsSet)
            };
            return boughtPurchaseIds.Select(_Id => sets.SelectMany(_Set => _Set)
                    .FirstOrDefault(_Item => _Item.purchaseId == _Id))
                .Where(_Item => _Item != null)
                .Sum(_Item => _Item.price);
        }
    }
}