using System;
using Common;
using Common.Entities;
using Common.Managers.PlatformGameServices;
using Common.ScriptableObjects;
using mazing.common.Runtime;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Entities.UI;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.Utils;
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
    public interface ISetOnCloseFinishAction
    {
        void SetOnCloseFinishAction(UnityAction _Action);
    }
    
    public interface IShopDialogPanelBase<T> 
        : IDialogPanel,
          ISetOnCloseFinishAction
        where T : ShopItemBase { }
    
    public abstract class ShopDialogPanelBase<T> :
        DialogPanelBase, IShopDialogPanelBase<T> 
        where T : ShopItemBase
    {
        #region constants

        protected const string PrefabSetName = "shop_items";

        #endregion
        
        #region nonpublic members

        protected virtual  Vector2           StartContentPos     =>  Vector2.zero;
        protected abstract string            ItemSetName         { get; }
        protected abstract string            PanelItemPrefabName { get; }
        protected abstract RectTransformLite ShopItemRectLite    { get; }

        protected RectTransform   Content;
        private   TextMeshProUGUI m_MoneyText;
        private   Button          m_ButtonClose;

        private UnityAction m_OnCloseFinishAction;

        #endregion

        #region inject
        

        protected ShopDialogPanelBase(
            IManagersGetter             _Managers,
            IUITicker                   _Ticker,
            ICameraProvider             _CameraProvider,
            IColorProvider              _ColorProvider,
            IViewTimePauser             _TimePauser,
            IViewInputCommandsProceeder _CommandsProceeder) 
            : base(
                _Managers,
                _Ticker,
                _CameraProvider,
                _ColorProvider,
                _TimePauser,
                _CommandsProceeder) { }

        #endregion

        #region api

        public override void LoadPanel(RectTransform _Container, ClosePanelAction _OnClose)
        {
            base.LoadPanel(_Container, _OnClose);
            Content.gameObject.DestroyChildrenSafe();
            InitItems();
        }
        
        public void SetOnCloseFinishAction(UnityAction _Action)
        {
            m_OnCloseFinishAction = _Action;
        }

        #endregion

        #region nonpublic methods
        
        protected override void OnDialogStartAppearing()
        {
            TimePauser.PauseTimeInGame();
            InitMoneyMiniPanel();
            base.OnDialogStartAppearing();
        }
        
        private void OnButtonCloseClick()
        {
            OnClose(m_OnCloseFinishAction);
            PlayButtonClickSound();
        }

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
                };
                item.Init(
                    Ticker,
                    Managers.AudioManager,
                    Managers.LocalizationManager,
                    Managers.AdsManager,
                    null,
                    args);
            }
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

        private void InitMoneyMiniPanel()
        {
            var savedGame = Managers.ScoreManager.GetSavedGame(MazorCommonData.SavedGameFileName);
            object moneyCountArg = savedGame.Arguments.GetSafe(ComInComArg.KeyMoneyCount, out _);
            long money = Convert.ToInt64(moneyCountArg);
            m_MoneyText.text = money.ToString("N0");
            Managers.ScoreManager.GameSaved -= OnGameSaved; 
            Managers.ScoreManager.GameSaved += OnGameSaved;
        }
        
        private void OnGameSaved(SavedGameEventArgs _Args)
        {
            bool castSuccess = _Args.SavedGame.CastTo(out SavedGame result);
            if (!castSuccess)
            {
                Dbg.LogError("OnGameSaved cast is not successful");
                return;
            }
            long score = result.Money;
            m_MoneyText.text = score.ToString("N0");
        }

        protected override void GetPrefabContentObjects(GameObject _Go)
        {
            Content       = _Go.GetCompItem<RectTransform>("content");
            m_ButtonClose = _Go.GetCompItem<Button>("close_button");
            m_MoneyText   = _Go.GetCompItem<TextMeshProUGUI>("mini_panel_money_text");
        }

        protected override void SubscribeButtonEvents()
        {
            m_ButtonClose.SetOnClick(OnButtonCloseClick);
        }

        protected override void LocalizeTextObjectsOnLoad() { }

        #endregion
    }
}