using System.Collections.Generic;
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
                new Dictionary<MoneyType, long>
                {
                    {MoneyType.Gold, 100000L},
                    {MoneyType.Diamonds, 1000L}
                }, _Size: ShopItemSize.Small),
            new ShopItemProps(ShopItemType.Money, "Advanced money set", 19.99f,
                new Dictionary<MoneyType, long>
                {
                    {MoneyType.Gold, 5000000L},
                    {MoneyType.Diamonds, 5000L}
                }, _Size: ShopItemSize.Medium),
            new ShopItemProps(ShopItemType.Money,"Pro money set", 39.99f,
                new Dictionary<MoneyType, long>
                {
                    {MoneyType.Gold, 20000000L},
                    {MoneyType.Diamonds, 20000L}
                }, _Size: ShopItemSize.Big),
            new ShopItemProps(ShopItemType.Lifes,"Rockie lifes set", 9.99f,
                new Dictionary<MoneyType, long>
                {
                    {MoneyType.Lifes, 100L}
                }, _Size: ShopItemSize.Small),
            new ShopItemProps(ShopItemType.Lifes, "Advanced lifes set", 19.99f,
                new Dictionary<MoneyType, long>
                {
                    {MoneyType.Lifes, 500L}
                }, _Size: ShopItemSize.Medium),
            new ShopItemProps(ShopItemType.Lifes, "Pro lifes set", 39.99f,
                new Dictionary<MoneyType, long>
                {
                    {MoneyType.Lifes, 2000L}
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
                if (shopItemProps.Title == "No Ads" && !SaveUtils.GetValue<bool>(SaveKey.ShowAds))
                    continue;

                IShopItem item;
                switch (shopItemProps.Type)
                {
                    case ShopItemType.NoAds:
                    case ShopItemType.Lifes:
                        item = ShopItemDefault.Create(content); break;
                    case ShopItemType.Money:
                        item = ShopItemMoney.Create(content);
                        break;
                    default:
                        throw new InvalidEnumArgumentExceptionEx(shopItemProps.Type);
                }
                item.Init(shopItemProps, GetObservers());
            }

            content.anchoredPosition = content.anchoredPosition.SetY(0);
            Panel = go.RTransform();
        }

        #endregion
    }
}