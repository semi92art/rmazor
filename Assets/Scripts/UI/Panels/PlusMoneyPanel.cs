using DialogViewers;
using Extensions;
using Helpers;
using Managers;
using UI.Factories;
using UI.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels
{
    public class PlusMoneyPanel : IMenuDialogPanel
    {
        #region nonpublic members

        private readonly IMenuDialogViewer m_DialogViewer;
        private readonly IActionExecuter m_ActionExecutor;
        
        #endregion

        #region api
        
        public MenuUiCategory Category => MenuUiCategory.PlusMoney;
        public RectTransform Panel { get; private set; }

        public PlusMoneyPanel(IMenuDialogViewer _DialogViewer, IActionExecuter _ActionExecutor)
        {
            m_DialogViewer = _DialogViewer;
            m_ActionExecutor = _ActionExecutor;
        }
        
        public void Show()
        {
            Panel = Create();
            m_DialogViewer.Show(this);
        }

        public void OnEnable() { }

        #endregion

        #region nonpublic methods

        private RectTransform Create()
        {
            GameObject go = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.DialogContainer,
                    RtrLites.FullFill),
                "main_menu",
                "plus_money_panel");
            
            go.GetCompItem<Button>("shop_button").SetOnClick(OnShopButtonClick);
            go.GetCompItem<Button>("daily_bonus_button").SetOnClick(OnDailyBonusButtonClick);
            go.GetCompItem<Button>("wheel_of_fortune_button").SetOnClick(OnWheelOfFortuneButtonClick);

            return go.RTransform();
        }

        private void OnShopButtonClick()
        {
            SoundManager.Instance.PlayUiButtonClick();
            IMenuDialogPanel shopPanel = new ShopPanel(m_DialogViewer);
            shopPanel.Show();
        }

        private void OnDailyBonusButtonClick()
        {
            SoundManager.Instance.PlayUiButtonClick();
            IMenuDialogPanel shopPanel = new DailyBonusPanel(m_DialogViewer, m_ActionExecutor);
            shopPanel.Show();
        }

        private void OnWheelOfFortuneButtonClick()
        {
            SoundManager.Instance.PlayUiButtonClick();
            IMenuDialogPanel shopPanel = new WheelOfFortunePanel(m_DialogViewer);
            shopPanel.Show();
        }

        #endregion
    }
}