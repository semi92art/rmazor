using System.Collections.Generic;
using System.Linq;
using DialogViewers;
using Extensions;
using Helpers;
using Managers;
using TMPro;
using UI.Factories;
using UI.Managers;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Panels
{
    public class PlusLifesPanel : IMenuDialogPanel
    {
        #region nonpublic members
        
        private readonly IMenuDialogViewer m_DialogViewer;
        private TextMeshProUGUI m_GoldCountText;
        private TextMeshProUGUI m_DiamondsCountText;
        private TextMeshProUGUI m_LifesCountText;
        private Button m_Plus1Button;
        private Button m_Plus10Button;
        private Button m_Plus100Button;
        private Button m_ExchangeButton;
        private Button m_ResetButton;
        private int m_LifesCount;
        
        private readonly Dictionary<MoneyType, long> m_OneLifePrice = new Dictionary<MoneyType, long>
        {
            {MoneyType.Gold, 1000}, {MoneyType.Diamonds, 10}
        };

        private Dictionary<MoneyType, long> m_Money;
        
        #endregion

        #region api
        
        public MenuUiCategory Category => MenuUiCategory.PlusLifes;
        public RectTransform Panel { get; private set; }

        public PlusLifesPanel(IMenuDialogViewer _DialogViewer)
        {
            m_DialogViewer = _DialogViewer;
        }
        
        public void Show()
        {
            Panel = Create();
            m_DialogViewer.Show(this);
        }

        public void OnEnable()
        {
            var bank = MoneyManager.Instance.GetBank();
            Coroutines.Run(Coroutines.WaitWhile(() =>
            {
                m_Money = bank.Money;
                UpdatePanelState();
            }, () => !bank.Loaded));
        }

        #endregion

        #region nonpublic methods

        private RectTransform Create()
        {
            GameObject go = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.DialogContainer,
                    RtrLites.FullFill),
                "main_menu",
                "plus_lifes_panel");

            m_GoldCountText = go.GetCompItem<TextMeshProUGUI>("gold_count_text");
            m_DiamondsCountText = go.GetCompItem<TextMeshProUGUI>("diamonds_count_text");
            m_LifesCountText = go.GetCompItem<TextMeshProUGUI>("lifes_count_text");
            m_Plus1Button = go.GetCompItem<Button>("plus_1_button");
            m_Plus10Button = go.GetCompItem<Button>("plus_10_button");
            m_Plus100Button = go.GetCompItem<Button>("plus_100_button");
            m_ExchangeButton = go.GetCompItem<Button>("exchange_button");
            m_ResetButton = go.GetCompItem<Button>("reset_button");
            Button shopButton = go.GetCompItem<Button>("shop_button");
            
            m_Plus1Button.SetOnClick(OnPlus1ButtonClick);
            m_Plus10Button.SetOnClick(OnPlus10ButtonClick);
            m_Plus100Button.SetOnClick(OnPlus100ButtonClick);
            m_ExchangeButton.SetOnClick(OnExchangeButtonClick);
            m_ResetButton.SetOnClick(OnResetButtonClick);
            shopButton.SetOnClick(OnShopButtonClick);
            
            return go.RTransform();
        }

        private void OnPlus1ButtonClick()
        {
            m_LifesCount++;
            UpdatePanelState();
        }

        private void OnPlus10ButtonClick()
        {
            m_LifesCount += 10;
            UpdatePanelState();
        }

        private void OnPlus100ButtonClick()
        {
            m_LifesCount += 100;
            UpdatePanelState();
        }
        
        private void OnExchangeButtonClick()
        {
            var pricesTemp = m_OneLifePrice
                .CloneAlt()
                .ToDictionary(_P => _P.Key,
                    _P => _P.Value * m_LifesCount);
            bool result = MoneyManager.Instance.TryMinusMoney(pricesTemp);
            if (result)
                MoneyManager.Instance.PlusMoney(MoneyType.Lifes, m_LifesCount);
            else
                Debug.LogError("Not enough money to buy lifes");

            m_LifesCount = 0;
            UpdatePanelState();
        }
        
        private void OnResetButtonClick()
        {
            m_LifesCount = 0;
            UpdatePanelState();
        }

        private void UpdatePanelState()
        {
            m_Plus100Button.interactable = IsMoneyEnough(m_LifesCount + 100);
            m_Plus10Button.interactable = IsMoneyEnough(m_LifesCount + 10);
            m_Plus1Button.interactable = IsMoneyEnough(m_LifesCount + 1);
            m_ExchangeButton.interactable = m_LifesCount > 0;
            m_ResetButton.interactable = m_LifesCount > 0;
            SetTexts();
        }

        private void OnShopButtonClick()
        {
            SoundManager.Instance.PlayUiButtonClick();
            IMenuDialogPanel shopPanel = new ShopPanel(m_DialogViewer);
            shopPanel.Show();
        }

        private bool IsMoneyEnough(long _LifesCount)
        {
            var moneyTemp = m_Money.CloneAlt();
            var pricesTemp = m_OneLifePrice
                .CloneAlt()
                .ToDictionary(_Price => _Price.Key,
                    _Price => _Price.Value * _LifesCount);
            return pricesTemp.All(_Kvp => _Kvp.Value <= moneyTemp[_Kvp.Key]);
        }

        private void SetTexts()
        {
            m_GoldCountText.text = (m_OneLifePrice[MoneyType.Gold] * m_LifesCount).ToNumeric();
            m_DiamondsCountText.text = (m_OneLifePrice[MoneyType.Diamonds] * m_LifesCount).ToNumeric();
            m_LifesCountText.text = m_LifesCount.ToNumeric();
        }

        #endregion
    }
}