using Constants;
using DI.Extensions;
using DialogViewers;
using Entities;
using GameHelpers;
using ScriptableObjects;
using Ticker;
using UI.Entities;
using UI.PanelItems.Shop_Items;
using UnityEngine;
using Utils;

namespace UI.Panels.ShopPanels
{
    public interface IShopMoneyDialogPanel : IDialogPanel { }
    
    public class ShopMoneyPanel : ShopPanelBase<ShopMoneyItem>, IShopMoneyDialogPanel
    {
        #region nonpublic members

        protected override Vector2 StartContentPos => m_Content.anchoredPosition.SetY(m_Content.rect.height * 0.5f);
        protected override string ItemSetName => "shop_money_items_set";
        protected override string PanelPrefabName => "shop_money_panel";
        protected override string PanelItemPrefabName => "shop_money_item";

        protected override RectTransformLite ShopItemRectLite => new RectTransformLite
        {
            Anchor = UiAnchor.Create(0, 0, 0, 0),
            AnchoredPosition = Vector2.zero,
            Pivot = Vector2.one * 0.5f,
            SizeDelta = new Vector2(300f, 110)
        };

        #endregion
        
        #region inject
        
        public ShopMoneyPanel(
            IManagersGetter _Managers,
            IUITicker _UITicker,
            IDialogViewer _DialogViewer,
            ICameraProvider _CameraProvider)
            : base(_Managers, _UITicker, _DialogViewer, _CameraProvider)
        { }
        
        #endregion

        #region api

        public override EUiCategory Category => EUiCategory.Shop;
        
        #endregion

        #region nonpublic methods


        protected override void InitItems()
        {
            var set = PrefabUtilsEx.GetObject<ShopMoneyItemsScriptableObject>(
                PrefabSetName, ItemSetName).set;
            foreach (var itemInSet in set)
            {
                var info = itemInSet.watchingAds ? null : Managers.ShopManager.GetItemInfo(itemInSet.purchaseKey);
                var item = CreateItem();
                var args = new ShopItemArgs
                {
                    BuyForWatchingAd = itemInSet.watchingAds,
                    Reward = itemInSet.reward,
                    Icon = itemInSet.icon
                };
                item.Init(Managers,
                    Ticker,
                    () =>
                    {
                        if (itemInSet.watchingAds)
                            Managers.AdsManager.ShowRewardedAd(() => OnPaid(args.Reward));
                        else
                            Managers.ShopManager.Purchase(itemInSet.purchaseKey, () => OnPaid(args.Reward));
                    },
                    args);
                Coroutines.Run(Coroutines.WaitWhile(
                    () => !info?.Ready() ?? !Managers.AdsManager.RewardedAdReady,
                    () =>
                    {
                        if (info != null)
                        {
                            args.Currency = info.Currency;
                            args.Price = info.Price;
                        }
                        args.Ready = true;
                    }));
            }
        }

        private void OnPaid(int _Reward)
        {
            var score = Managers.ScoreManager.GetScore(DataFieldIds.FirstCurrency);
            Coroutines.Run(Coroutines.WaitWhile(
                () => score.Loaded,
                () =>
                {
                    Managers.ScoreManager
                        .SetScore(DataFieldIds.FirstCurrency, score.GetFirstScore().Value + _Reward);
                }));
        }

        #endregion
    }
}