using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Entities;
using Common.Entities.UI;
using Common.Extensions;
using Common.Managers.PlatformGameServices;
using Common.Providers;
using Common.ScriptableObjects;
using Common.Ticker;
using Common.UI;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.UI.PanelItems.Shop_Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace RMAZOR.UI.Panels.ShopPanels
{
    public abstract class ShopPanelBase<T> : DialogPanelBase where T : ShopItemBase
    {
        #region constants

        protected const string PrefabSetName = "shop_items";

        #endregion
        
        #region nonpublic members

        protected virtual  Vector2           StartContentPos     =>  Vector2.zero;
        protected abstract string            ItemSetName         { get; }
        protected abstract string            PanelPrefabName     { get; }
        protected abstract string            PanelItemPrefabName { get; }
        protected abstract RectTransformLite ShopItemRectLite    { get; }

        protected RectTransform           Content;
        private   RectTransform           m_Panel;
        private   TextMeshProUGUI         m_MoneyText;
        private   Image                   m_MoneyIcon;
        private   SimpleUiDialogPanelView m_PanelView;

        #endregion

        #region inject

        protected ShopPanelBase(
            IManagersGetter  _Managers,
            IUITicker        _Ticker,
            IBigDialogViewer _DialogViewer,
            ICameraProvider  _CameraProvider,
            IColorProvider   _ColorProvider) 
            : base(
                _Managers,
                _Ticker,
                _DialogViewer,
                _CameraProvider,
                _ColorProvider) { }

        #endregion

        #region api

        public override void LoadPanel()
        {
            base.LoadPanel();
            var sp = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(
                    DialogViewer.Container,
                    RectTransformLite.FullFill),
                CommonPrefabSetNames.DialogPanels,
                PanelPrefabName);
            m_Panel = sp.GetCompItem<RectTransform>("panel");
            Content = sp.GetCompItem<RectTransform>("content");
            m_PanelView = sp.GetCompItem<SimpleUiDialogPanelView>("panel");
            m_PanelView.Init(
                Ticker,
                ColorProvider, 
                Managers.AudioManager,
                Managers.LocalizationManager,
                Managers.PrefabSetManager);
            PanelObject = sp.RTransform();
            Content.gameObject.DestroyChildrenSafe();
            InitItems();
            InitMoneyPanel();
            Cor.Run(Cor.WaitNextFrame(
                () => Content.anchoredPosition = StartContentPos));
        }

        #endregion

        #region nonpublic methods

        protected virtual void InitItems()
        {
            var set = Managers.PrefabSetManager.GetObject<ShopItemsScriptableObject>(
                PrefabSetName, ItemSetName).set;
            foreach (var itemInSet in set)
            {
                var item = CreateItem();
                var args = new ViewShopItemInfo
                {
                    BuyForWatchingAd = itemInSet.watchingAds,
                    Icon = itemInSet.icon,
                    Price = itemInSet.price.ToString()
                };
                item.Init(
                    Ticker,
                    ColorProvider,
                    Managers.AudioManager,
                    Managers.LocalizationManager,
                    Managers.PrefabSetManager,
                    () =>
                    {
                        
                    },
                    args);
            }
        }
        
        private void InitMoneyPanel()
        {
            var obj = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(
                    m_Panel,
                    new RectTransformLite
                    {
                        Anchor = UiAnchor.Create(0, 1, 0, 1),
                        AnchoredPosition = new Vector2(180, -84),
                        Pivot = Vector2.one * 0.5f,
                        SizeDelta = new Vector2(306f, 66f)
                    }),
                "ui_game", "money_mini_panel");
            m_MoneyText = obj.GetCompItem<TextMeshProUGUI>("money");
            m_MoneyIcon = obj.GetCompItem<Image>("icon");
            m_MoneyText.color = ColorProvider.GetColor(ColorIds.UiText);
            m_MoneyIcon.color = ColorProvider.GetColor(ColorIds.UI);
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
                        Dbg.LogWarning("Failed to load money entity: " +
                                       $"_Result: {savedGameEntity.Result}," +
                                       $" castSuccess: {castSuccess}," +
                                       $" _Value: {savedGameEntity.Value}");
                        return;
                    }
                    m_MoneyText.text = savedGame.Money.ToString();
                }));
            Managers.ScoreManager.GameSaved -= OnGameSaved; 
            Managers.ScoreManager.GameSaved += OnGameSaved;
        }
        
        private void OnGameSaved(SavedGameEventArgs _Args)
        {
            bool castSuccess = _Args.SavedGame.CastTo(out SavedGame result);
            if (!castSuccess)
                return;
            long score = result.Money;
            m_MoneyText.text = score.ToString();
        }
        
        protected T CreateItem()
        {
            var obj = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(
                    Content,
                    ShopItemRectLite),
                PrefabSetName, PanelItemPrefabName);
            return obj.GetComponent<T>();
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            switch (_ColorId)
            {
                case ColorIds.UI when m_MoneyIcon.IsNotNull():     m_MoneyIcon.color = _Color; break;
                case ColorIds.UiText when m_MoneyText.IsNotNull(): m_MoneyText.color = _Color; break;
            }
        }

        #endregion
    }
}