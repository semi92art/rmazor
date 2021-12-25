using System.Collections.Generic;
using Constants;
using DI.Extensions;
using DialogViewers;
using Entities;
using Games.RazorMaze.Views.Common;
using Managers.IAP;
using ScriptableObjects;
using Ticker;
using UI.Entities;
using UI.PanelItems.Shop_Items;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace UI.Panels.ShopPanels
{
    public interface IShopMoneyDialogPanel : IDialogPanel, IInit { }
    
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
        
        private readonly Dictionary<int, ShopItemArgs> m_ShopItemArgsDict = new Dictionary<int, ShopItemArgs>();
        private readonly Dictionary<int, ShopMoneyItem> m_MoneyItems = new Dictionary<int, ShopMoneyItem>();

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

        public override EUiCategory Category    => EUiCategory.Shop;
        public          bool        Initialized { get; private set; }
        public event UnityAction    Initialize;
        
        public void Init()
        {
            Coroutines.Run(Coroutines.WaitWhile(() => !Managers.ShopManager.Initialized,
                () =>
                {
                    InitPurchaseActions();
                    LoadItemInfos();
                    Initialize?.Invoke();
                    Initialized = true;
                }));
        }

        #endregion

        #region nonpublic methods

        private void InitPurchaseActions()
        {
            var set = Managers.PrefabSetManager.GetObject<ShopMoneyItemsScriptableObject>(
                PrefabSetName, ItemSetName).set;
            foreach (var itemInSet in set)
            {
                if (itemInSet.watchingAds)
                    continue;
                Managers.ShopManager.SetPurchaseAction(
                    itemInSet.purchaseKey, 
                    () => OnPaid(itemInSet.reward));
            }
            Managers.ShopManager.SetPurchaseAction(PurchaseKeys.NoAds, BuyHideAdsItem);
        }

        private void LoadItemInfos()
        {
            var set = Managers.PrefabSetManager.GetObject<ShopMoneyItemsScriptableObject>(
                PrefabSetName, ItemSetName).set;
            foreach (var itemInSet in set)
            {
                bool doAddShopArgs = false;
                if (m_ShopItemArgsDict.ContainsKey(itemInSet.purchaseKey))
                {
                    var args = m_ShopItemArgsDict[itemInSet.purchaseKey];
                    if (args == null || args.Result() != EShopProductResult.Success)
                    {
                        m_ShopItemArgsDict.Remove(itemInSet.purchaseKey);
                        doAddShopArgs = true;
                    }
                }
                else doAddShopArgs = true;
                if (!doAddShopArgs)
                    continue;
                var newArgs = itemInSet.watchingAds ? 
                    null : Managers.ShopManager.GetItemInfo(itemInSet.purchaseKey);
                m_ShopItemArgsDict.Add(itemInSet.purchaseKey, newArgs);
            }
            if (m_ShopItemArgsDict.ContainsKey(PurchaseKeys.NoAds))
                return;
            var argsNoAds = Managers.ShopManager.GetItemInfo(PurchaseKeys.NoAds);
            m_ShopItemArgsDict.Add(PurchaseKeys.NoAds, argsNoAds);
        }

        protected override void InitItems()
        {
            LoadItemInfos();
            m_MoneyItems.Clear();
            var showAdsEntity = Managers.AdsManager.ShowAds;
            Coroutines.Run(Coroutines.WaitWhile(
                () => showAdsEntity.Result == EEntityResult.Pending,
                () =>
                {
                    if (showAdsEntity.Result == EEntityResult.Fail)
                    {
                        Dbg.LogError("showAdsEntity.Result Fail");
                        return;
                    }
                    var set = Managers.PrefabSetManager.GetObject<ShopMoneyItemsScriptableObject>(
                        PrefabSetName, ItemSetName).set;
                    var moneyIcon = Managers.PrefabSetManager.GetObject<Sprite>(
                        "shop_items", "shop_money_icon");
                    foreach (var itemInSet in set)
                    {
                        var args = m_ShopItemArgsDict[itemInSet.purchaseKey];
                        var info = new ViewShopItemInfo
                        {
                            PurchaseKey = itemInSet.purchaseKey,
                            BuyForWatchingAd = itemInSet.watchingAds,
                            Reward = itemInSet.reward,
                            Icon = moneyIcon
                        };
                        var item = InitItem(args, info);
                        if (info.BuyForWatchingAd)
                            item.Highlighted = true;
                        m_MoneyItems.Add(info.PurchaseKey, item);
                    }

                    if (!showAdsEntity.Value)
                        return;

                    var argsDisableAds = m_ShopItemArgsDict[PurchaseKeys.NoAds];
                    var infoDisableAds = new ViewShopItemInfo
                    {
                        PurchaseKey = PurchaseKeys.NoAds,
                        Icon = Managers.PrefabSetManager.GetObject<Sprite>(PrefabSetName, "shop_no_ads_icon"),
                        BuyForWatchingAd = false,
                        Reward = 0
                    };
                    var itemDisableAds = InitItem(argsDisableAds, infoDisableAds, BuyHideAdsItem);
                    m_MoneyItems.Add(PurchaseKeys.NoAds, itemDisableAds);
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
                        Managers.ShopManager.Purchase(_Info.PurchaseKey);
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
            const int key = PurchaseKeys.NoAds;
            if (!m_MoneyItems.ContainsKey(key))
                return;
            m_MoneyItems[key].gameObject.DestroySafe();
            m_MoneyItems.Remove(key);
        }

        private void OnPaid(long _Reward)
        {
            var scoreEntity = Managers.ScoreManager.GetScore(DataFieldIds.Money, true);
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
                        .SetScore(DataFieldIds.Money, firstVal.Value + _Reward, false);
                }));
        }

        #endregion
    }
}