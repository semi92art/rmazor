using System.Collections.Generic;
using Constants;
using DI.Extensions;
using DialogViewers;
using Entities;
using Exceptions;
using GameHelpers;
using Managers;
using Ticker;
using UI.Factories;
using UI.PanelItems;
using UnityEngine;

namespace UI.Panels
{
    public interface IShopDialogPanel : IDialogPanel
    {
        void PreInit(RectTransform _Container);
    }
    
    public class ShopPanel : DialogPanelBase, IShopDialogPanel
    {
        #region nonpublic members
        
        private RectTransform m_Container;

        #endregion

        #region inject
        
        public ShopPanel(
            IDialogViewer _DialogViewer,
            IManagersGetter _Managers,
            IUITicker _UITicker) 
            : base(_Managers, _UITicker, _DialogViewer) { }
        
        #endregion

        #region api

        
        public override EUiCategory Category => EUiCategory.Shop;
        
        public void PreInit(RectTransform _Container)
        {
            m_Container = _Container;
        }

        public override void Init()
        {
            base.Init();
            GameObject go = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_Container,
                    RtrLites.FullFill),
                CommonPrefabSetNames.MainMenuDialogPanels,
                "shop_panel");
            RectTransform content = go.GetCompItem<RectTransform>("content");

            foreach (var shopItemProps in GetShopItemPropsList())
            {
                IShopItem item;
                switch (shopItemProps.Type)
                {
                    case ShopItemType.NoAds:
                        if (!AdsManager.Instance.ShowAds)
                            continue;
                        item = ShopItemDefault.Create(content); 
                        break;
                    case ShopItemType.Money:
                        item = ShopItemMoney.Create(content);
                        break;
                    default:
                        throw new SwitchCaseNotImplementedException(shopItemProps.Type);
                }
                item.Init(shopItemProps, Managers);
            }

            content.anchoredPosition = content.anchoredPosition.SetY(0);
            Panel = go.RTransform();
        }

        #endregion

        #region nonpublic methdos

        private List<ShopItemProps> GetShopItemPropsList()
        {
            return new List<ShopItemProps>
            {
                new ShopItemProps(ShopItemType.NoAds, "No Ads", 9.99f,_Description: "No advertising forever"),
                new ShopItemProps(ShopItemType.Money, "Rockie money set", 9.99f,
                    new Dictionary<BankItemType, long>
                    {
                        {BankItemType.FirstCurrency, 100000L},
                        {BankItemType.SecondCurrency, 1000L}
                    }, _Size: ShopItemSize.Small),
                new ShopItemProps(ShopItemType.Money, "Advanced money set", 19.99f,
                    new Dictionary<BankItemType, long>
                    {
                        {BankItemType.FirstCurrency, 5000000L},
                        {BankItemType.SecondCurrency, 5000L}
                    }, _Size: ShopItemSize.Medium),
                new ShopItemProps(ShopItemType.Money,"Pro money set", 39.99f,
                    new Dictionary<BankItemType, long>
                    {
                        {BankItemType.FirstCurrency, 20000000L},
                        {BankItemType.SecondCurrency, 20000L}
                    }, _Size: ShopItemSize.Big)
            };
        }

        #endregion
    }
}