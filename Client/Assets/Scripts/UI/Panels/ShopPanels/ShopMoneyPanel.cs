using Constants;
using DI.Extensions;
using DialogViewers;
using Entities;
using GameHelpers;
using Games.RazorMaze.Views.Common;
using Managers.Advertising;
using ScriptableObjects;
using Ticker;
using UI.Entities;
using UI.PanelItems.Shop_Items;
using UnityEngine;
using UnityEngine.Events;
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
            IBigDialogViewer _DialogViewer,
            ICameraProvider _CameraProvider,
            IColorProvider _ColorProvider)
            : base(_Managers, _UITicker, _DialogViewer, _CameraProvider, _ColorProvider)
        { }
        
        #endregion

        #region api

        public override EUiCategory Category => EUiCategory.Shop;
        
        #endregion

        #region nonpublic methods

        protected override void InitItems()
        {
            var showAdsEntity = Managers.AdsManager.ShowAds;
            Coroutines.Run(Coroutines.WaitWhile(
                () => showAdsEntity.Result == EEntityResult.Pending,
                () =>
                {
                    if (showAdsEntity.Result == EEntityResult.Fail)
                    {
                        Dbg.LogError("Failed to load ShowAds entity");
                        return;
                    }
                    var set = PrefabUtilsEx.GetObject<ShopMoneyItemsScriptableObject>(
                        PrefabSetName, ItemSetName).set;
                    var moneyIcon = PrefabUtilsEx.GetObject<Sprite>("shop_items", "shop_money_icon");
                    foreach (var itemInSet in set)
                    {
                        var args = itemInSet.watchingAds ? null : Managers.ShopManager.GetItemInfo(itemInSet.purchaseKey);
                        var info = new ViewShopItemInfo
                        {
                            PurchaseKey = itemInSet.purchaseKey,
                            BuyForWatchingAd = itemInSet.watchingAds,
                            Reward = itemInSet.reward,
                            Icon = moneyIcon
                        };
                        InitItem(args, info);
                    }

                    if (!showAdsEntity.Value)
                        return;
                    
                    var argsDisableAds = Managers.ShopManager.GetItemInfo(PurchaseKeys.NoAds);
                    var infoDisableAds = new ViewShopItemInfo
                    {
                        PurchaseKey = PurchaseKeys.NoAds,
                        Icon = PrefabUtilsEx.GetObject<Sprite>(PrefabSetName, "shop_no_ads_icon"),
                        BuyForWatchingAd = false,
                        Reward = 0
                    };
                    var itemDisableAds = InitItem(argsDisableAds, infoDisableAds, BuyHideAdsItem);
                    Managers.LocalizationManager.AddTextObject(itemDisableAds.title, "no_ads");
                }));
        }

        private ShopMoneyItem InitItem(ShopItemArgs _Args, ViewShopItemInfo _Info, UnityAction _OnPaid = null)
        {
            void OnPaidReal()
            {
                if (_OnPaid != null)
                    _OnPaid.Invoke();
                else
                    OnPaid(_Info.Reward);
            }
            var item = CreateItem();
            item.Init(Managers,
                Ticker,
                ColorProvider,
                () =>
                {
                    if (_Info.BuyForWatchingAd)
                        Managers.AdsManager.ShowRewardedAd(OnPaidReal);
                    else
                        Managers.ShopManager.Purchase(_Info.PurchaseKey, OnPaidReal);
                },
                _Info);
            Coroutines.Run(Coroutines.WaitWhile(
                () =>
                {
                    if (_Args != null)
                        return _Args.Result() == EShopProductResult.Pending;
                    return !Managers.AdsManager.RewardedAdReady;
                },
                () =>
                {
                    if (_Args?.Result() == EShopProductResult.Success)
                    {
                        _Info.Currency = _Args.Currency;
                        _Info.Price = _Args.Price;
                    }
                    _Info.Ready = true;
                }));
            return item;
        }
        
        private void BuyHideAdsItem()
        {
            Managers.AdsManager.ShowAds = new BoolEntity
            {
                Result = EEntityResult.Success,
                Value = false
            };
        }

        private void OnPaid(int _Reward)
        {
            var scoreEntity = Managers.ScoreManager.GetScore(DataFieldIds.Money);
            Coroutines.Run(Coroutines.WaitWhile(
                () => scoreEntity.Result == EEntityResult.Pending,
                () =>
                {
                    if (scoreEntity.Result == EEntityResult.Fail)
                    {
                        Dbg.LogError("Failed to load score entity");
                        return;
                    }
                    var firstVal = scoreEntity.GetFirstScore();
                    if (!firstVal.HasValue)
                    {
                        Dbg.LogError("Money score entity does not contain first value");
                        return;
                    }
                    Managers.ScoreManager
                        .SetScore(DataFieldIds.Money, firstVal.Value + _Reward);
                }));
        }

        #endregion
    }
}