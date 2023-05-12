using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Common;
using Common.Constants;
using Common.ScriptableObjects;
using Common.Utils;
using mazing.common.Runtime;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Entities.UI;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers.IAP;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Constants;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.UI.PanelItems.Shop_Panel_Items;
using RMAZOR.Views.Common;
using RMAZOR.Views.InputConfigurators;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.UI.Panels.ShopPanels
{
    public interface IShopMoneyDialogPanel : IInit, IShopDialogPanelBase<ShopMoneyItem> { }
    
    public class ShopMoneyDialogPanel : ShopDialogPanelBase<ShopMoneyItem>, IShopMoneyDialogPanel
    {
        #region nonpublic members

        protected override Vector2 StartContentPos     => Content.anchoredPosition.SetY(Content.rect.height * 0.5f);
        protected override string  ItemSetName         => "shop_money_items_set";
        protected override string  PrefabName          => "shop_money_panel";
        protected override string  PanelItemPrefabName => "shop_money_item";

        protected override RectTransformLite ShopItemRectLite => new RectTransformLite
        {
            Anchor           = UiAnchor.Create(0, 0, 0, 0),
            AnchoredPosition = Vector2.zero,
            Pivot            = Vector2.one * 0.5f,
            SizeDelta        = new Vector2(300f, 100)
        };
        
        private readonly Dictionary<int, IAP_ProductInfo> m_ShopItemArgsDict
            = new Dictionary<int, IAP_ProductInfo>();

        private TextMeshProUGUI m_Title;
        private Image           m_BackGlowDark;

        #endregion
        
        #region inject
        
        private IModelGame Model { get; }
        
        private ShopMoneyDialogPanel(
            IModelGame _Model,
            IManagersGetter             _Managers,
            IUITicker                   _UITicker,
            ICameraProvider             _CameraProvider,
            IColorProvider              _ColorProvider,
            IViewTimePauser             _TimePauser,
            IViewInputCommandsProceeder _CommandsProceeder)
            : base(
                _Managers, 
                _UITicker,
                _CameraProvider,
                _ColorProvider,
                _TimePauser,
                _CommandsProceeder)
        {
            Model = _Model;
        }
        
        #endregion

        #region api

        public override int      DialogViewerId => DialogViewerIdsMazor.Medium2;
        public          bool     Initialized    { get; private set; }
        public event UnityAction Initialize;
        
        public void Init()
        {
            Cor.Run(InitCoroutine());
        }

        #endregion

        #region nonpublic methods

        protected override void OnDialogStartAppearing()
        {
            m_BackGlowDark.enabled = Model.LevelStaging.LevelStage == ELevelStage.None;
            base.OnDialogStartAppearing();
        }
        
        protected override void GetPrefabContentObjects(GameObject _Go)
        {
            base.GetPrefabContentObjects(_Go);
            m_Title = PanelRectTransform.GetCompItem<TextMeshProUGUI>("title");
            m_BackGlowDark = PanelRectTransform.GetCompItem<Image>("back_glow_dark");
        }

        protected override void LocalizeTextObjectsOnLoad()
        {
            base.LocalizeTextObjectsOnLoad();
            var locInfo = new LocTextInfo(m_Title, ETextType.MenuUI_H1, "shop",
                _T => _T.ToUpper(CultureInfo.CurrentUICulture));
            Managers.LocalizationManager.AddLocalization(locInfo);
        }

        private IEnumerator InitCoroutine()
        {
            yield return Cor.WaitWhile(() => !Managers.ShopManager.Initialized);
            InitPurchaseActions();
            LoadItemInfos();
            Initialize?.Invoke();
            Initialized = true;
        }

        private void InitPurchaseActions()
        {
            var set = Managers.PrefabSetManager.GetObject<ShopPanelItemsScriptableObject>(
                PrefabSetName, ItemSetName).set;
            foreach (var itemInSet in set)
            {
                if (itemInSet.watchingAds)
                    continue;
                Managers.ShopManager.AddPurchaseAction(
                    itemInSet.purchaseKey,
                    () => OnPaid(itemInSet.purchaseKey, itemInSet.reward));
            }
        }

        private void LoadItemInfos()
        {
            var set = Managers.PrefabSetManager.GetObject<ShopPanelItemsScriptableObject>(
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
            var set = Managers.PrefabSetManager.GetObject<ShopPanelItemsScriptableObject>(
                PrefabSetName, ItemSetName).set;
            foreach (var itemInSet in set)
            {
                var args = m_ShopItemArgsDict[itemInSet.purchaseKey];
                var background = itemInSet.watchingAds
                    ? Managers.PrefabSetManager.GetObject<Sprite>(
                        CommonPrefabSetNames.Views, "shop_item_for_ad_background")
                    : null;
                var info = new ViewShopItemInfo
                {
                    ShopItemArgs     = args,
                    PurchaseKey      = itemInSet.purchaseKey,
                    BuyForWatchingAd = itemInSet.watchingAds,
                    Reward           = itemInSet.reward,
                    Icon             = itemInSet.icon,
                    Background       = background
                };
                var item = InitItem(info);
                if (info.BuyForWatchingAd)
                    item.Highlighted = true;
            }
        }

        private ShopMoneyItem InitItem(
            ViewShopItemInfo _Info,
            UnityAction      _OnPaid = null)
        {
            var item = CreateItem();
            item.Init(
                Ticker,
                Managers.AudioManager,
                Managers.LocalizationManager,
                Managers.AdsManager,
                () =>
                {
                    if (_Info.BuyForWatchingAd)
                        WatchAdAndBuyItem(_Info, _OnPaid);
                    else
                        Managers.ShopManager.Purchase(_Info.PurchaseKey);
                },
                _Info);
            return item;
        }

        private void WatchAdAndBuyItem(
            ViewShopItemInfo _Info,
            UnityAction      _OnPaid = null)
        {
            Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.WatchAdInShopPanelClick);
            void OnPaidReal()
            {
                if (_OnPaid != null)
                    _OnPaid.Invoke();
                else
                    OnPaid(_Info.PurchaseKey, _Info.Reward);
            }
            void OnBeforeAdShown()
            {
                Managers.AudioManager.MuteAudio(EAudioClipType.Music);
                TimePauser.PauseTimeInUi();
            }
            void OnAdClosed()
            {
                Managers.AudioManager.UnmuteAudio(EAudioClipType.Music);
                TimePauser.UnpauseTimeInUi();
            }
            Managers.AdsManager.ShowRewardedAd(
                _OnReward:       OnPaidReal,
                _OnBeforeShown:  OnBeforeAdShown,
                _OnClosed:       OnAdClosed,
                _OnFailedToShow: OnAdClosed);
        }

        private void OnPaid(int _PurchaseKey, long _Reward)
        {
            var savedGame = Managers.ScoreManager.GetSavedGame(CommonDataMazor.SavedGameFileName);
            var bankMoneyCountArg = savedGame.Arguments.GetSafe(ComInComArg.KeyMoneyCount, out _);
            long money = Convert.ToInt64(bankMoneyCountArg);
            money += _Reward;
            savedGame.Arguments.SetSafe(ComInComArg.KeyMoneyCount, money);
            Managers.ScoreManager.SaveGame(savedGame);
            string dialogTitle = Managers.LocalizationManager.GetTranslation("purchase") + ":";
            string dialogText = _Reward + " " +
                                Managers.LocalizationManager
                                    .GetTranslation("coins_alt")
                                    .ToLowerInvariant();
            MazorCommonUtils.ShowAlertDialog(dialogTitle, dialogText);
            string productId = _PurchaseKey switch
            {
                -1 => "coins_pack_micro",
                1  => "coins_pack_small",
                2  => "coins_pack_medium",
                3  => "coins_pack_large",
                _  => null
            };
            if (productId == null)
            {
                Dbg.LogError("Analytic Id was not found by Purchase Key");
                return;
            }
            Managers.AnalyticsManager.SendAnalytic(
                AnalyticIds.Purchase,
                new Dictionary<string, object> { {AnalyticIds.ParameterPurchaseProductId, productId}});
        }

        #endregion
    }
}