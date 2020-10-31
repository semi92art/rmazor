using System;
using System.Collections.Generic;
using System.Linq;
using Extentions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UICreationSystem.Factories;
using UnityEngine.Events;
using Utils;
using Object = UnityEngine.Object;

namespace UICreationSystem.Panels
{
    public interface IMiniPanel
    {
        void Show();
        void Hide();
    }

    public interface IActionExecuter
    { 
        Action Action { get; set; }
    }
    
    public class BankMiniPanel : IMiniPanel, IActionExecuter
    {
        #region types

        private class CoinAnimObject
        {
            public RectTransform Item { get; set; }
            public bool IsBusy { get; set; }
        }
        
        #endregion
        
        #region private members

        private const float IncomeAnimTime = 2f;
        private const float IncomeAnimDeltaTime = 0.1f;
        private const int IncomeCoinsAnimOnScreen = 3;
        private const int PoolSize = 10;
        private List<CoinAnimObject> m_CoinsPool = new List<CoinAnimObject>(PoolSize);
        private Image m_GoldIcon;
        private Image m_DiamondIcon;
        private TextMeshProUGUI m_GoldCount;
        private TextMeshProUGUI m_DiamondsCount;
        private Button m_PlusButton;
        private Animator m_Animator;
        private bool m_IsShowing;

        private int AkShowInMm => AnimKeys.Anim2;
        private int AkShowInDlg => AnimKeys.Anim;
        private int AkFromMmToDlg => AnimKeys.Anim4;
        private int AkFromDlgToMm => AnimKeys.Anim3;
        private int AkHideInMm => AnimKeys.Stop2;
        private int AkHideInDlg => AnimKeys.Stop;
        
        #endregion

        #region api
        
        public Action Action { get; set; }
        
        public BankMiniPanel(RectTransform _Parent, IDialogViewer _DialogViewer)
        {
            var go = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    _Parent,
                    UiAnchor.Create(1, 1, 1, 1),
                    new Vector2(-137f, -49.2f),
                    Vector2.one * 0.5f,
                    new Vector2(237.9f, 100f)),
                "main_menu", "bank_mini_panel");
            m_GoldIcon = go.GetComponentItem<Image>("gold_icon");
            m_DiamondIcon = go.GetComponentItem<Image>("diamond_icon");
            m_GoldCount = go.GetComponentItem<TextMeshProUGUI>("gold_count_text");
            m_DiamondsCount = go.GetComponentItem<TextMeshProUGUI>("diamonds_count_text");
            m_PlusButton = go.GetComponentItem<Button>("plus_money_button");
            m_Animator = go.GetComponentItem<Animator>("animator");
            
            m_PlusButton.SetOnClick(() =>
            {
                IDialogPanel shopPanel = new ShopPanel(_DialogViewer);
                shopPanel.Show();
            });

            MoneyManager.Instance.OnMoneyCountChanged += MoneyCountChanged;
            MoneyManager.Instance.OnIncome += Income;
            UiManager.Instance.OnCurrentCategoryChanged += CurrentCategoryChanged;
        }

        public void Show()
        {
            SetMoneyText(MoneyManager.Instance.GetMoney());
            int trigger = UiManager.Instance.CurrentCategory == UiCategory.MainMenu ?
                AnimKeys.Anim2 : AnimKeys.Anim;
            m_Animator.SetTrigger(trigger);
            m_IsShowing = true;
        }

        public void Hide()
        {
            throw new NotImplementedException();
        }
        
        #endregion
        
        #region private methods and destructor

        private void AnimateIncome(Dictionary<MoneyType, int> _Income, RectTransform _From)
        {
            string iconName = "gold_coin";
            Vector3 to = m_GoldIcon.transform.position;
            Image icon = m_GoldIcon;
            MoneyType moneyType = MoneyType.Gold;
            if (_Income.ContainsKey(MoneyType.Diamonds) && _Income[MoneyType.Diamonds] > 0)
            {
                iconName = "diamond_coin";
                icon = m_DiamondIcon;
                to = m_DiamondIcon.transform.position;
                moneyType = MoneyType.Diamonds;
            }
            Sprite sprite = PrefabInitializer.GetObject<Sprite>("icons", iconName);

            for (int i = 0; i < PoolSize; i++)
            {
                GameObject item = new GameObject($"coin_{i}");
                Image coinIcon = item.AddComponent<Image>();
                coinIcon.raycastTarget = false;
                coinIcon.sprite = sprite;
                item.RTransform().sizeDelta = icon.RTransform().sizeDelta;
                item.transform.SetParent(m_Animator.transform);
                item.transform.localScale = Vector3.one;
                item.SetActive(false);
                
                m_CoinsPool.Add(new CoinAnimObject
                {
                    Item = item.RTransform(),
                    IsBusy = false
                });
            }

            Coroutines.StartCoroutine(Coroutines.Repeat(() =>
                {
                    if (!m_CoinsPool.Any())
                        return;
                    var coin = m_CoinsPool.First(_Coin => !_Coin.IsBusy);
                    if (coin == null)
                        return;
                    coin.IsBusy = true;
                    coin.Item.gameObject.SetActive(true);
                    Coroutines.StartCoroutine(Coroutines.LerpPosition(
                        coin.Item,
                        _From.position,
                        to,
                        IncomeAnimDeltaTime * IncomeCoinsAnimOnScreen,
                        () =>
                        {
                            coin.IsBusy = false;
                            if (coin.Item != null)
                                coin.Item.gameObject.SetActive(false);
                        }));
                },
                IncomeAnimDeltaTime,
                IncomeAnimTime,
                () =>
                {
                    foreach (var item in m_CoinsPool)
                        Object.Destroy(item.Item.gameObject);
                    m_CoinsPool.Clear();
                    Coroutines.StartCoroutine(Coroutines.Delay(Action, 0.3f));
                }));

            var currMoney = MoneyManager.Instance.GetMoney();
            if (_Income.ContainsKey(moneyType))
                Coroutines.StartCoroutine(Coroutines.LerpValue(
                    currMoney[moneyType],
                    currMoney[moneyType] + _Income[moneyType],
                    IncomeAnimTime,
                    _Value =>
                    {
                        if (moneyType == MoneyType.Gold)
                            m_GoldCount.text = $"{_Value:n0}";
                        else if (moneyType == MoneyType.Diamonds)
                            m_DiamondsCount.text = $"{_Value:n0}";
                    }));
        }
        
        private void MoneyCountChanged(MoneyEventArgs _Args)
        {
            SetMoneyText(_Args.Money);
        }
        
        private void Income(IncomeEventArgs _Args)
        {
            if (m_IsShowing)
                AnimateIncome(_Args.Money, _Args.From);
        }

        private void SetMoneyText(Dictionary<MoneyType, int> _Money)
        {
            m_GoldCount.text = $"{_Money[MoneyType.Gold]:n0}";
            m_DiamondsCount.text = $"{_Money[MoneyType.Diamonds]:n0}";
        }

        private void CurrentCategoryChanged(UiCategory _Prev, UiCategory _New)
        {
            if (_Prev == _New || _Prev == UiCategory.Nothing || _New == UiCategory.Nothing)
                return;
            m_PlusButton.interactable = true;
            m_IsShowing = true;
            int trigger = -1;
            switch (_New)
            {
                case UiCategory.Profile:
                case UiCategory.Settings:
                case UiCategory.Loading:
                case UiCategory.SelectGame:
                case UiCategory.LoginOrRegistration:
                case UiCategory.WheelOfFortune:
                    m_IsShowing = false;
                    trigger = _Prev == UiCategory.MainMenu ? AkHideInMm : AkHideInDlg;
                    break;
                case UiCategory.Shop:
                    m_PlusButton.interactable = false;
                    switch (_Prev)
                    {
                        case UiCategory.MainMenu:
                            trigger = AkFromMmToDlg;
                            break;
                        case UiCategory.Profile:
                        case UiCategory.Settings:
                        case UiCategory.Loading:
                        case UiCategory.SelectGame:
                        case UiCategory.LoginOrRegistration:
                        case UiCategory.WheelOfFortune:
                            trigger = AkShowInDlg;
                            break;
                        case UiCategory.DailyBonus:
                            // Do nothing
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    break;
                case UiCategory.DailyBonus:
                    switch (_Prev)
                    {
                        case UiCategory.MainMenu:
                            trigger = AkFromMmToDlg;
                            break;
                        case UiCategory.Profile:
                        case UiCategory.Settings:
                        case UiCategory.Loading:
                        case UiCategory.SelectGame:
                        case UiCategory.LoginOrRegistration:
                        case UiCategory.WheelOfFortune:
                            trigger = AkShowInDlg;
                            break;
                        case UiCategory.Shop:
                            // Do nothing
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    break;
                case UiCategory.MainMenu:
                    switch (_Prev)
                    {
                        case UiCategory.Profile:
                        case UiCategory.Settings:
                        case UiCategory.Loading:
                        case UiCategory.SelectGame:
                        case UiCategory.LoginOrRegistration:
                        case UiCategory.WheelOfFortune:
                            trigger = AkShowInMm;
                            break;
                        case UiCategory.Shop:
                        case UiCategory.DailyBonus:
                            trigger = AkFromDlgToMm;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
            
            if (trigger != -1)
                m_Animator.SetTrigger(trigger);
        }

        ~BankMiniPanel()
        {
            MoneyManager.Instance.OnMoneyCountChanged -= MoneyCountChanged;
            MoneyManager.Instance.OnIncome -= Income;
            UiManager.Instance.OnCurrentCategoryChanged -= CurrentCategoryChanged;
        }
        
        #endregion

        
    }
}