using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Entities;
using Common.Extensions;
using Common.Utils;
using GameHelpers;
using Managers;
using Managers.Scores;
using RMAZOR;
using RMAZOR.UI.Panels.ShopPanels;
using ScriptableObjects;
using TMPro;

namespace Utils
{
    public static class ShopUtils
    {
        public static void OnScoreChanged(ScoresEventArgs _Args, TMP_Text _Text)
        {
            Cor.Run(Cor.WaitWhile(
                () => _Args.ScoresEntity.Result == EEntityResult.Pending,
                () =>
                {
                    if (_Args.ScoresEntity.Result == EEntityResult.Fail)
                    {
                        Dbg.LogWarning("Failed to load score");
                        return;
                    }
                    var score = _Args.ScoresEntity.GetFirstScore();
                    if (!score.HasValue || _Text.IsNull())
                        return;
                    int moneySpentOnPurchases = GetMoneySpentOnPurchases();
                    _Text.text = (score.Value - moneySpentOnPurchases).ToString();
                }));
        }

        private static int GetMoneySpentOnPurchases()
        {
            var boughtPurchaseIds = SaveUtils.GetValue(SaveKeysCommon.BoughtPurchaseIds);
            if (boughtPurchaseIds == null)
                return 0;
            static ShopItemsScriptableObject.ShopItemSet GetSet(string _Key)
            {
                return new PrefabSetManager(new AssetBundleManagerFake())
                    .GetObject<ShopItemsScriptableObject>("shop_items", _Key).set;
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