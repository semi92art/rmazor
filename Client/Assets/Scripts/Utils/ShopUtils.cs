using System.Collections.Generic;
using System.Linq;
using DI.Extensions;
using Entities;
using GameHelpers;
using Managers;
using ScriptableObjects;
using TMPro;
using UI.Panels.ShopPanels;

namespace Utils
{
    public static class ShopUtils
    {
        public static void OnScoreChanged(ScoresEventArgs _Args, TMP_Text _Text)
        {
            Coroutines.Run(Coroutines.WaitWhile(
                () => !_Args.ScoresEntity.Loaded,
                () =>
                {
                    var score = _Args.ScoresEntity.GetFirstScore();
                    if (!score.HasValue || _Text.IsNull())
                        return;
                    int moneySpentOnPurchases = GetMoneySpentOnPurchases();
                    _Text.text = (score.Value - moneySpentOnPurchases).ToString();
                }));
        }

        private static int GetMoneySpentOnPurchases()
        {
            var boughtPurchaseIds = SaveUtils.GetValue<List<int>>(SaveKey.BoughtPurchaseIds);
            ShopItemsScriptableObject.ShopItemSet GetSet(string _Key) => PrefabUtilsEx
                .GetObject<ShopItemsScriptableObject>("shop_items", _Key).set;
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