using System.Collections.Generic;
using Constants;
using DI.Extensions;
using Entities;
using Exceptions;
using GameHelpers;
using Managers;
using Ticker;
using UI.Factories;
using UI.Managers;
using UI.PanelItems;
using UnityEngine;

namespace UI.Panels
{
    public class ShopPanel : DialogPanelBase, IMenuUiCategory
    {
        #region nonpublic members
        
        private readonly RectTransform m_Container;
        private readonly List<ShopItemProps> m_ShopItemPropsList = new List<ShopItemProps>
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
        
        #endregion
        
        #region api

        public MenuUiCategory Category => MenuUiCategory.Shop;
        
        public ShopPanel(
            RectTransform _Container,
            IManagersGetter _Managers,
            IUITicker _UITicker) : base(_Managers, _UITicker)
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

            foreach (var shopItemProps in m_ShopItemPropsList)
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
    }
}