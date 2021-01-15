using System.Collections.Generic;
using System.Linq;
using Entities;
using Extensions;
using GameHelpers;
using UI.Factories;
using UI.Managers;
using UI.PanelItems;
using UnityEngine;
using Utils;
using Constants;
using Exceptions;
using Managers;
using Network;

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
                    {BankItemType.Gold, 100000L},
                    {BankItemType.Diamonds, 1000L}
                }, _Size: ShopItemSize.Small),
            new ShopItemProps(ShopItemType.Money, "Advanced money set", 19.99f,
                new Dictionary<BankItemType, long>
                {
                    {BankItemType.Gold, 5000000L},
                    {BankItemType.Diamonds, 5000L}
                }, _Size: ShopItemSize.Medium),
            new ShopItemProps(ShopItemType.Money,"Pro money set", 39.99f,
                new Dictionary<BankItemType, long>
                {
                    {BankItemType.Gold, 20000000L},
                    {BankItemType.Diamonds, 20000L}
                }, _Size: ShopItemSize.Big),
            new ShopItemProps(ShopItemType.Lifes,"Rockie lifes set", 9.99f,
                new Dictionary<BankItemType, long>
                {
                    {BankItemType.Lifes, 100L}
                }, _Size: ShopItemSize.Small),
            new ShopItemProps(ShopItemType.Lifes, "Advanced lifes set", 19.99f,
                new Dictionary<BankItemType, long>
                {
                    {BankItemType.Lifes, 500L}
                }, _Size: ShopItemSize.Medium),
            new ShopItemProps(ShopItemType.Lifes, "Pro lifes set", 39.99f,
                new Dictionary<BankItemType, long>
                {
                    {BankItemType.Lifes, 2000L}
                }, _Size: ShopItemSize.Big)
        };
        
        #endregion
        
        #region api

        public MenuUiCategory Category => MenuUiCategory.Shop;
        
        public ShopPanel(RectTransform _Container)
        {
            m_Container = _Container;
        }

        public override void Init()
        {
            GameObject go = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_Container,
                    RtrLites.FullFill),
                CommonStyleNames.MainMenuDialogPanels,
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
                    case ShopItemType.Lifes:
                        item = ShopItemDefault.Create(content); 
                        break;
                    case ShopItemType.Money:
                        item = ShopItemMoney.Create(content);
                        break;
                    default:
                        throw new SwitchCaseNotImplementedException(shopItemProps.Type);
                }
                item.Init(shopItemProps, GetObservers());
            }

            content.anchoredPosition = content.anchoredPosition.SetY(0);
            Panel = go.RTransform();
        }

        #endregion
    }
}