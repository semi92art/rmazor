using Common.CameraProviders;
using Common.Entities.UI;
using Common.Providers;
using Common.Ticker;
using Common.UI;
using RMAZOR.Managers;
using RMAZOR.UI.PanelItems.Shop_Items;
using RMAZOR.Views.Common;
using UI;
using UnityEngine;

namespace RMAZOR.UI.Panels.ShopPanels
{
    public interface IShopTailsDialogPanel : IDialogPanel { }
    
    public class ShopTailsPanel : ShopPanelBase<ShopTailItem>, IShopTailsDialogPanel
    {
        #region constants

        public const string ItemsSet = "shop_tail_items_set";

        #endregion

        #region nonpublic members

        protected override string ItemSetName => ItemsSet;
        protected override string PanelPrefabName => "shop_tails_panel";
        protected override string PanelItemPrefabName => "shop_tail_item";

        protected override RectTransformLite ShopItemRectLite => new RectTransformLite
        {
            Anchor = UiAnchor.Create(0, 0, 0, 0),
            AnchoredPosition = Vector2.zero,
            Pivot = Vector2.one * 0.5f,
            SizeDelta = new Vector2(300f, 110)
        };

        #endregion

        #region inject

        public ShopTailsPanel(
            IManagersGetter _Managers, 
            IUITicker _Ticker,
            IBigDialogViewer _DialogViewer,
            ICameraProvider _CameraProvider,
            IColorProvider _ColorProvider)
            : base(_Managers, _Ticker, _DialogViewer, _CameraProvider, _ColorProvider)
        { }

        #endregion

        #region api

        public override EUiCategory Category      => EUiCategory.Shop;
        public override bool        AllowMultiple => false;

        #endregion
    }
}