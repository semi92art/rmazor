using System;
using System.Collections.Generic;
using DI.Extensions;
using DialogViewers;
using Entities;
using Games.RazorMaze.Views.Common;
using Ticker;
using UI.Entities;
using UI.PanelItems.Shop_Items;
using UnityEngine;
using UnityEngine.Events;

namespace UI.Panels.ShopPanels
{
    public interface IShopDialogPanel : IDialogPanel { }
    
    public class ShopPanel : ShopPanelBase<ShopMainItem>, IShopDialogPanel
    {
        #region nonpublic members

        protected override Vector2 StartContentPos => m_Content.anchoredPosition.SetY(m_Content.rect.height * 0.5f);
        protected override string ItemSetName => null;
        protected override string PanelPrefabName => "shop_panel";
        protected override string PanelItemPrefabName => "shop_main_item";

        protected override RectTransformLite ShopItemRectLite => new RectTransformLite
        {
            Anchor = UiAnchor.Create(0, 0, 0, 0),
            AnchoredPosition = Vector2.zero,
            Pivot = Vector2.one * 0.5f,
            SizeDelta = new Vector2(300f, 110)
        };

        #endregion

        #region inject
        
        private IShopMoneyDialogPanel ShopMoneyPanel { get; }
        private IShopHeadsDialogPanel ShopHeadsPanel { get; }
        private IShopTailsDialogPanel ShopTailsPanel { get; }

        public ShopPanel(
            IManagersGetter _Managers,
            IUITicker _UITicker,
            IBigDialogViewer _DialogViewer,
            ICameraProvider _CameraProvider,
            IColorProvider _ColorProvider,
            IShopMoneyDialogPanel _ShopMoneyPanel,
            IShopHeadsDialogPanel _ShopHeadsPanel,
            IShopTailsDialogPanel _ShopTailsPanel)
            : base(_Managers, _UITicker, _DialogViewer, _CameraProvider, _ColorProvider)
        {
            ShopMoneyPanel = _ShopMoneyPanel;
            ShopHeadsPanel = _ShopHeadsPanel;
            ShopTailsPanel = _ShopTailsPanel;
        }
        
        #endregion

        #region api

        public override EUiCategory Category      => EUiCategory.Shop;
        public override bool        AllowMultiple => false;

        #endregion

        #region nonpublic methods

        protected override void InitItems()
        {
            var dict = new Dictionary<string, Tuple<string, UnityAction>>
            {
                {"shop_money_icon", new Tuple<string, UnityAction>("coins", OpenShopMoneyPanel)},
                {"shop_heads_icon", new Tuple<string, UnityAction>("heads", OpenShopHeadsPanel)},
                {"shop_tails_icon", new Tuple<string, UnityAction>("tails", OpenShopTailsPanel)}
            };
            
            foreach (var kvp in dict)
            {
                var item = CreateItem();
                Managers.LocalizationManager.AddTextObject(item.title, kvp.Value.Item1);
                var args = new ViewShopItemInfo
                {
                    Icon = Managers.PrefabSetManager.GetObject<Sprite>(PrefabSetName, kvp.Key)
                };
                item.Init(
                    Managers,
                    Ticker,
                    ColorProvider,
                    kvp.Value.Item2,
                    args);
            }
        }

        private void OpenShopMoneyPanel()
        {
            ShopMoneyPanel.LoadPanel();
            DialogViewer.Show(ShopMoneyPanel);
        }

        private void OpenShopHeadsPanel()
        {
            ShopHeadsPanel.LoadPanel();
            DialogViewer.Show(ShopHeadsPanel);
        }

        private void OpenShopTailsPanel()
        {
            ShopTailsPanel.LoadPanel();   
            DialogViewer.Show(ShopTailsPanel);
        }

        #endregion
    }
}