using System.Collections.Generic;
using Entities;
using Extensions;
using GameHelpers;
using UI.Entities;
using UI.Factories;
using UI.Managers;
using UI.PanelItems;
using UnityEngine;
using Utils;
using Constants;

namespace UI.Panels
{
    public class ShopPanel : DialogPanelBase, IMenuUiCategory
    {
        #region nonpublic members
        
        private readonly List<ShopItemProps> m_ShopItemPropsList = new List<ShopItemProps>
        {
            //new ShopItemProps("No Ads", "9.99$", "20$", PrefabInitializer.GetObject<Sprite>("shop_items", "item_0_icon")),
            new ShopItemProps(0, "30,000", "9.99$", "20$", 
                PrefabInitializer.GetObject<Sprite>("shop_items", "item_1_icon")),
            new ShopItemProps(1, "30", "9.99$", "20$", 
                PrefabInitializer.GetObject<Sprite>("shop_items", "item_2_icon")),
            new ShopItemProps(2, "80,000", "19.99$", "40$",
                PrefabInitializer.GetObject<Sprite>("shop_items", "item_3_icon")),
            new ShopItemProps(3, "80", "19.99$", "40$",
                PrefabInitializer.GetObject<Sprite>("shop_items", "item_4_icon")),
            new ShopItemProps(4, "200,000", "39.99$",
                "80$", PrefabInitializer.GetObject<Sprite>("shop_items", "item_5_icon")),
            new ShopItemProps(5, "200", "39.99$",
                "80$", PrefabInitializer.GetObject<Sprite>("shop_items", "item_6_icon")),
            new ShopItemProps(6, "500,000", "79.99$",
                "180$", PrefabInitializer.GetObject<Sprite>("shop_items", "item_7_icon")),
            new ShopItemProps(7, "500", "79.99$", 
                "180$", PrefabInitializer.GetObject<Sprite>("shop_items", "item_8_icon"))
        };

        private readonly RectTransform m_Container;
        
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
            
            GameObject shopItem = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    content,
                    UiAnchor.Create(0, 1, 0, 1),
                    new Vector2(218f, -60f),
                    Vector2.one * 0.5f,
                    new Vector2(416f, 100f)),
                CommonStyleNames.MainMenu,
                "shop_item");
            
            foreach (var shopItemProps in m_ShopItemPropsList)
            {
                if (shopItemProps.Amount == "No Ads" && !SaveUtils.GetValue<bool>(SaveKey.ShowAds))
                    continue;
                var shopItemClone = shopItem.Clone();
                ShopItem si = shopItemClone.GetComponent<ShopItem>();
                si.Init(shopItemProps, GetObservers());
            }
            
            Object.Destroy(shopItem);

            content.anchoredPosition = content.anchoredPosition.SetY(0);
            Panel = go.RTransform();
        }

        #endregion
    }
}