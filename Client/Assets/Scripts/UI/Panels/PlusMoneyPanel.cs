using Constants;
using DI.Extensions;
using DialogViewers;
using Entities;
using GameHelpers;
using Ticker;
using UI.Factories;
using UI.Managers;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Panels
{
    public class PlusMoneyPanel : DialogPanelBase, IMenuUiCategory
    {
        #region notify ids

        public const string NotifyMessageShopButtonClick = nameof(NotifyMessageShopButtonClick);
        public const string NotifyMessageDailyBonusButtonClick = nameof(NotifyMessageDailyBonusButtonClick);
        public const string NotifyMessageWheelOfFortuneButtonClick = nameof(NotifyMessageWheelOfFortuneButtonClick);
        
        #endregion
        
        #region nonpublic members

        private readonly IMenuDialogViewer m_DialogViewer;
        private readonly INotificationViewer m_NotificationViewer;
        private readonly IActionExecutor m_ActionExecutor;
        
        #endregion

        #region api
        
        public MenuUiCategory Category => MenuUiCategory.PlusMoney;
        
        public PlusMoneyPanel(
            IMenuDialogViewer _DialogViewer,
            INotificationViewer _NotificationViewer,
            IActionExecutor _ActionExecutor,
            IManagersGetter _Managers,
            IUITicker _UITicker) : base(_Managers, _UITicker)
        {
            m_DialogViewer = _DialogViewer;
            m_NotificationViewer = _NotificationViewer;
            m_ActionExecutor = _ActionExecutor;
        }
        
        public override void Init()
        {
            base.Init();
            GameObject go = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.Container,
                    RtrLites.FullFill),
                CommonPrefabSetNames.MainMenuDialogPanels,
                "plus_money_panel");
            
            go.GetCompItem<Button>("shop_button").SetOnClick(OnShopButtonClick);
            go.GetCompItem<Button>("daily_bonus_button").SetOnClick(OnDailyBonusButtonClick);
            go.GetCompItem<Button>("wheel_of_fortune_button").SetOnClick(OnWheelOfFortuneButtonClick);
            Panel = go.RTransform();
        }

        #endregion

        #region nonpublic methods

        private void OnShopButtonClick()
        {
            UIUtils.OnButtonClick(Managers, NotifyMessageShopButtonClick);
            var panel = new ShopPanel(m_DialogViewer.Container, Managers, (IUITicker)Ticker);
            panel.Init();
            m_DialogViewer.Show(panel);
        }

        private void OnDailyBonusButtonClick()
        {
            UIUtils.OnButtonClick(Managers, NotifyMessageDailyBonusButtonClick);
            var panel = new DailyBonusPanel(m_DialogViewer, m_ActionExecutor, Managers, (IUITicker)Ticker);
            panel.Init();
            m_DialogViewer.Show(panel);
        }

        private void OnWheelOfFortuneButtonClick()
        {
            UIUtils.OnButtonClick(Managers, NotifyMessageWheelOfFortuneButtonClick);
            var panel = new WheelOfFortunePanel(m_DialogViewer, m_NotificationViewer, Managers, (IUITicker)Ticker);
            panel.Init();
            m_DialogViewer.Show(panel);
        }

        #endregion
    }
}