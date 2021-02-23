using System.Collections.Generic;
using System.Linq;
using Constants;
using DialogViewers;
using Entities;
using Extensions;
using GameHelpers;
using Managers;
using TMPro;
using UI.Factories;
using UI.Managers;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Panels
{
    public class PlusLifesPanel : DialogPanelBase, IMenuUiCategory
    {
        #region notify messages

        public const string NotifyMessagePlus1LifeButtonClick = nameof(NotifyMessagePlus1LifeButtonClick);
        public const string NotifyMessagePlus10LifesButtonClick = nameof(NotifyMessagePlus10LifesButtonClick);
        public const string NotifyMessagePlus100LifesButtonClick = nameof(NotifyMessagePlus100LifesButtonClick);
        public const string NotifyMessageExchangeButtonClick = nameof(NotifyMessageExchangeButtonClick);
        public const string NotifyMessageResetButtonClick = nameof(NotifyMessageResetButtonClick);
        public const string NotifyMessageShopButtonClick = nameof(NotifyMessageShopButtonClick);
        
        #endregion
        
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
        private Image m_GoldWarningIcon;
        private Image m_DiamondsWarningIcon;
        private int m_LifesCount = 1;
        
        public static readonly Dictionary<BankItemType, long> OneLifePrice = new Dictionary<BankItemType, long>
        {
            {BankItemType.Gold, 1000}, {BankItemType.Diamonds, 10}
        };

        private Dictionary<BankItemType, long> m_Money;
        
        #endregion

        #region api
        
        public MenuUiCategory Category => MenuUiCategory.PlusLifes;
        public PlusLifesPanel(IMenuDialogViewer _DialogViewer)
        {
            m_DialogViewer = _DialogViewer;
        }
        
        public override void Init()
        {
            GameObject go = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.Container,
                    RtrLites.FullFill),
                CommenPrefabSetNames.MainMenuDialogPanels,
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
            m_GoldWarningIcon = go.GetCompItem<Image>("gold_warning_icon");
            m_DiamondsWarningIcon = go.GetCompItem<Image>("diamonds_warning_icon");
            
            m_Plus1Button.SetOnClick(OnPlus1ButtonClick);
            m_Plus10Button.SetOnClick(OnPlus10ButtonClick);
            m_Plus100Button.SetOnClick(OnPlus100ButtonClick);
            m_ExchangeButton.SetOnClick(OnExchangeButtonClick);
            m_ResetButton.SetOnClick(OnResetButtonClick);
            shopButton.SetOnClick(OnShopButtonClick);
            
            Panel = go.RTransform();
        }

        public override void OnDialogEnable()
        {
            GetBank();
        }

        #endregion

        #region nonpublic methods

        private void OnPlus1ButtonClick()
        {
            Notify(this, NotifyMessagePlus1LifeButtonClick);
            m_LifesCount++;
            UpdatePanelState();
        }

        private void OnPlus10ButtonClick()
        {
            Notify(this, NotifyMessagePlus10LifesButtonClick);
            m_LifesCount += 10;
            UpdatePanelState();
        }

        private void OnPlus100ButtonClick()
        {
            Notify(this, NotifyMessagePlus100LifesButtonClick);
            m_LifesCount += 100;
            UpdatePanelState();
        }
        
        private void OnExchangeButtonClick()
        {
            Notify(this, NotifyMessageExchangeButtonClick);
            var pricesTemp = OneLifePrice
                .CloneAlt()
                .ToDictionary(_P => _P.Key,
                    _P => _P.Value * m_LifesCount);
            bool result = BankManager.Instance.TryMinusBankItems(pricesTemp);
            if (result)
                BankManager.Instance.PlusBankItems(BankItemType.Lifes, m_LifesCount);
            else
                Debug.LogError("Not enough money to buy lifes");

            m_LifesCount = 0;
            GetBank();
        }
        
        private void OnResetButtonClick()
        {
            Notify(this, NotifyMessageResetButtonClick);
            m_LifesCount = 0;
            UpdatePanelState();
        }
        
        private void OnShopButtonClick()
        {
            Notify(this, NotifyMessageShopButtonClick);
            var shopPanel = new ShopPanel(m_DialogViewer.Container);
            shopPanel.AddObservers(GetObservers());
            shopPanel.Init();
            m_DialogViewer.Show(shopPanel);
        }
        
        private void UpdatePanelState()
        {
            m_Plus100Button.interactable = IsMoneyEnough(m_LifesCount + 100);
            m_Plus10Button.interactable = IsMoneyEnough(m_LifesCount + 10);
            m_Plus1Button.interactable = m_LifesCount == 0 || IsMoneyEnough(m_LifesCount + 1);
            m_ExchangeButton.interactable = m_LifesCount > 0 && IsMoneyEnough(1);
            m_ResetButton.interactable = m_LifesCount > 0;
            m_GoldWarningIcon.SetGoActive(m_LifesCount > 0 && !IsMoneyEnoughForOneLife(BankItemType.Gold));
            m_DiamondsWarningIcon.SetGoActive(m_LifesCount > 0 && !IsMoneyEnoughForOneLife(BankItemType.Diamonds));
            SetTexts();
        }

        private bool IsMoneyEnough(long _LifesCount)
        {
            var moneyTemp = m_Money.CloneAlt();
            var pricesTemp = OneLifePrice
                .CloneAlt()
                .ToDictionary(_Price => _Price.Key,
                    _Price => _Price.Value * _LifesCount);
            return pricesTemp.All(_Kvp => _Kvp.Value <= moneyTemp[_Kvp.Key]);
        }
        
        private bool IsMoneyEnoughForOneLife(BankItemType _BankItemType)
        {
            return OneLifePrice[_BankItemType] <= m_Money[_BankItemType];
        }

        private void SetTexts()
        {
            m_GoldCountText.text = (OneLifePrice[BankItemType.Gold] * m_LifesCount).ToNumeric();
            m_DiamondsCountText.text = (OneLifePrice[BankItemType.Diamonds] * m_LifesCount).ToNumeric();
            m_LifesCountText.text = m_LifesCount.ToNumeric();
        }

        private void GetBank()
        {
            var bank = BankManager.Instance.GetBank();
            Coroutines.Run(Coroutines.WaitWhile(
                () => !bank.Loaded,
                () =>
            {
                m_Money = bank.BankItems;
                UpdatePanelState();
            }));
        }

        #endregion
    }
}