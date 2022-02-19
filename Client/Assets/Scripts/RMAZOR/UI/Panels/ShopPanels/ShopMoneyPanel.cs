using System.Collections.Generic;
using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Entities;
using Common.Entities.UI;
using Common.Extensions;
using Common.Managers.IAP;
using Common.Providers;
using Common.ScriptableObjects;
using Common.Ticker;
using Common.UI;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.UI.PanelItems.Shop_Items;
using UnityEngine;
using UnityEngine.Events;

namespace RMAZOR.UI.Panels.ShopPanels
{
    public interface IShopMoneyDialogPanel : IDialogPanel, IInit { }
    
    public class ShopMoneyPanel : ShopPanelBase<ShopMoneyItem>, IShopMoneyDialogPanel
    {
        #region nonpublic members

        protected override Vector2 StartContentPos => Content.anchoredPosition.SetY(Content.rect.height * 0.5f);
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
        private readonly Dictionary<int, ShopMoneyItem> m_Items = new Dictionary<int, ShopMoneyItem>();

        #endregion
        
        #region inject
        
        private IModelGame Model { get; }

        public ShopMoneyPanel(
            IModelGame       _Model,
            IManagersGetter  _Managers,
            IUITicker        _UITicker,
            IBigDialogViewer _DialogViewer,
            ICameraProvider  _CameraProvider,
            IColorProvider   _ColorProvider)
            : base(_Managers, _UITicker, _DialogViewer, _CameraProvider, _ColorProvider)
        {
            Model = _Model;
        }
        
        #endregion

        #region api

        public override EUiCategory Category      => EUiCategory.Shop;
        public override bool        AllowMultiple => false;
        public          bool        Initialized   { get; private set; }
        public event UnityAction    Initialize;
        
        public void Init()
        {
            Cor.Run(Cor.WaitWhile(() => !Managers.ShopManager.Initialized,
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
            Managers.ShopManager.SetPurchaseAction(PurchaseKeys.DarkTheme, BuyDarkTheme);
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
            AddItemArgsToDict(PurchaseKeys.NoAds);
            AddItemArgsToDict(PurchaseKeys.DarkTheme);
        }

        private void AddItemArgsToDict(int _PurchaseKey)
        {
            if (m_ShopItemArgsDict.ContainsKey(_PurchaseKey))
                return;
            var args = Managers.ShopManager.GetItemInfo(_PurchaseKey);
            m_ShopItemArgsDict.Add(_PurchaseKey, args);
        }

        protected override void InitItems()
        {
            LoadItemInfos();
            m_Items.Clear();
            var showAdsEntity = Managers.AdsManager.ShowAds;
            Cor.Run(Cor.WaitWhile(
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
                        m_Items.Add(info.PurchaseKey, item);
                    }
                    if (showAdsEntity.Value)
                        InitBuyNoAdsItem();
                    if (!ColorProvider.DarkThemeAvailable)
                        InitBuyDarkThemeItem();
                }));
        }

        private void InitBuyNoAdsItem()
        {
            var argsDisableAds = m_ShopItemArgsDict[PurchaseKeys.NoAds];
            var infoDisableAds = new ViewShopItemInfo
            {
                PurchaseKey = PurchaseKeys.NoAds,
                Icon = Managers.PrefabSetManager.GetObject<Sprite>(
                    PrefabSetName, 
                    "shop_no_ads_icon"),
                BuyForWatchingAd = false,
                Reward = 0
            };
            var itemDisableAds = InitItem(argsDisableAds, infoDisableAds, BuyHideAdsItem);
            m_Items.Add(PurchaseKeys.NoAds, itemDisableAds);
            Managers.LocalizationManager.AddTextObject(itemDisableAds.title, "no_ads");
        }

        private void InitBuyDarkThemeItem()
        {
            var argsBuyDarkTheme = m_ShopItemArgsDict[PurchaseKeys.DarkTheme];
            var infoBuyDarkTheme = new ViewShopItemInfo
            {
                PurchaseKey = PurchaseKeys.DarkTheme,
                Icon = Managers.PrefabSetManager.GetObject<Sprite>(
                    PrefabSetName, 
                    "dark_theme_on"),
                BuyForWatchingAd = false,
                Reward = 0
            };
            var itemBuyDarkTheme = InitItem(argsBuyDarkTheme, infoBuyDarkTheme, BuyHideAdsItem);
            m_Items.Add(PurchaseKeys.DarkTheme, itemBuyDarkTheme);
            Managers.LocalizationManager.AddTextObject(itemBuyDarkTheme.title, "dark_theme");
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
            item.Init(
                Managers.AudioManager,
                Managers.LocalizationManager,
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
            Cor.Run(Cor.WaitWhile(
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
            string dialogTitle = Managers.LocalizationManager.GetTranslation("purchase") + ":";
            string dialogText = Managers.LocalizationManager.GetTranslation("mandatory_ads_disabled");
            CommonUtils.ShowAlertDialog(dialogTitle, dialogText);
            RemoveItem(PurchaseKeys.NoAds);
        }

        private void BuyDarkTheme()
        {
            ColorProvider.DarkThemeAvailable = true;
            string dialogTitle = Managers.LocalizationManager.GetTranslation("purchase") + ":";
            string dialogText = Managers.LocalizationManager.GetTranslation("dark_theme_available");
            CommonUtils.ShowAlertDialog(dialogTitle, dialogText);
            RemoveItem(PurchaseKeys.DarkTheme);
        }

        private void OnPaid(long _Reward)
        {
            var savedGameEntity = Managers.ScoreManager.GetSavedGameProgress(
                CommonData.SavedGameFileName,
                true);
            Cor.Run(Cor.WaitWhile(
                () => savedGameEntity.Result == EEntityResult.Pending,
                () =>
                {
                    bool castSuccess = savedGameEntity.Value.CastTo(out SavedGame savedGame);
                    if (savedGameEntity.Result == EEntityResult.Fail || !castSuccess)
                    {
                        Dbg.LogWarning("Failed to save new game data");
                        return;
                    }
                    var newSavedGame = new SavedGame
                    {
                        FileName = CommonData.SavedGameFileName,
                        Money = savedGame.Money + _Reward,
                        Level = Model.LevelStaging.LevelIndex
                    };
                    Managers.ScoreManager.SaveGameProgress(newSavedGame, false);
                    string dialogTitle = Managers.LocalizationManager.GetTranslation("purchase") + ":";
                    string dialogText = _Reward + " " +
                                        Managers.LocalizationManager
                                            .GetTranslation("coins_alt")
                                            .ToLowerInvariant();
                    CommonUtils.ShowAlertDialog(dialogTitle, dialogText);
                }));
        }

        private void RemoveItem(int _Key)
        {
            if (!m_Items.ContainsKey(_Key))
                return;
            if (m_Items[_Key].IsNotNull())
                m_Items[_Key].gameObject.DestroySafe();
            m_Items.Remove(_Key);
        }

        #endregion
    }
}