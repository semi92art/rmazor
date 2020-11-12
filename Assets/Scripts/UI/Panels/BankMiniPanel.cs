using System;
using System.Collections.Generic;
using System.Linq;
using Constants;
using Extensions;
using Helpers;
using Managers;
using TMPro;
using UI.Entities;
using UI.Factories;
using UI.Managers;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Object = UnityEngine.Object;

namespace UI.Panels
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
        private const int PoolSize = 8;
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
        private int AkHideInMm => AnimKeys.Stop;
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
            m_GoldIcon = go.GetCompItem<Image>("gold_icon");
            m_DiamondIcon = go.GetCompItem<Image>("diamond_icon");
            m_GoldCount = go.GetCompItem<TextMeshProUGUI>("gold_count_text");
            m_DiamondsCount = go.GetCompItem<TextMeshProUGUI>("diamonds_count_text");
            m_PlusButton = go.GetCompItem<Button>("plus_money_button");
            m_Animator = go.GetCompItem<Animator>("animator");

            m_PlusButton.SetOnClick(() =>
            {
                SoundManager.Instance.PlayMenuButtonClick();
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
            
            List<Sprite> sprites = new List<Sprite>();
            int spriteCount = 8;
            for (int i = 0; i < spriteCount; i++)
                sprites.Add(PrefabInitializer.GetObject<Sprite>("coins", $"{iconName}_{i}"));
            
            for (int i = 0; i < PoolSize; i++)
            {
                GameObject item = new GameObject($"coin_{i}");
                Image coinIcon = item.AddComponent<Image>();
                coinIcon.raycastTarget = false;
                coinIcon.sprite = sprites[i % spriteCount];
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
            Dictionary<int, bool> finishedDict = new Dictionary<int, bool>();
            int coroutineIndex = 0;
            Coroutines.Run(Coroutines.Repeat(() =>
                {
                    if (!m_CoinsPool.Any())
                        return;
                    var coin = m_CoinsPool.First(_Coin => !_Coin.IsBusy);
                    if (coin == null)
                        return;
                    coin.IsBusy = true;
                    coin.Item.gameObject.SetActive(true);
                    finishedDict.Add(coroutineIndex, false);
                    int cI = coroutineIndex++;
                    Coroutines.Run(Coroutines.LerpPosition(
                        coin.Item,
                        _From.position,
                        to,
                        IncomeAnimDeltaTime * IncomeCoinsAnimOnScreen,
                        () =>
                        {
                            coin.IsBusy = false;
                            if (coin.Item != null)
                                coin.Item.gameObject.SetActive(false);
                            finishedDict[cI] = true;
                        }));
                },
                IncomeAnimDeltaTime,
                IncomeAnimTime,
                () =>
                {
                    Coroutines.Run(Coroutines.WaitWhile(() =>
                    {
                        foreach (var item in m_CoinsPool)
                            Object.Destroy(item.Item.gameObject);
                        m_CoinsPool.Clear();
                        Coroutines.Run(Coroutines.Delay(Action, 0.3f));
                    }, () => finishedDict.Any(_Kvp => !_Kvp.Value)));
                }));

            var currMoney = MoneyManager.Instance.GetMoney();
            if (_Income.ContainsKey(moneyType))
                Coroutines.Run(Coroutines.Lerp(
                    currMoney[moneyType],
                    currMoney[moneyType] + _Income[moneyType],
                    IncomeAnimTime,
                    _Value =>
                    {
                        if (moneyType == MoneyType.Gold)
                            m_GoldCount.text = _Value.ToNumeric();
                        else if (moneyType == MoneyType.Diamonds)
                            m_DiamondsCount.text = _Value.ToNumeric();
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
            m_GoldCount.text = _Money[MoneyType.Gold].ToNumeric();
            m_DiamondsCount.text = _Money[MoneyType.Diamonds].ToNumeric();
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
                case UiCategory.Settings:
                case UiCategory.Loading:
                case UiCategory.SelectGame:
                    m_IsShowing = false;
                    trigger = _Prev == UiCategory.MainMenu ? AkHideInMm : AkHideInDlg;
                    break;
                case UiCategory.Login:
                case UiCategory.Countries:
                    m_IsShowing = false;
                    trigger = _Prev == UiCategory.MainMenu ? AkHideInMm : AkHideInDlg;
                    if (_Prev == UiCategory.Countries || _Prev == UiCategory.Login)
                        trigger = -1;
                    break;
                case UiCategory.Shop:
                    m_PlusButton.interactable = false;
                    switch (_Prev)
                    {
                        case UiCategory.MainMenu:
                            trigger = AkFromMmToDlg;
                            break;
                        case UiCategory.Settings:
                        case UiCategory.Loading:
                        case UiCategory.SelectGame:
                        case UiCategory.Login:
                        case UiCategory.Countries:
                            trigger = AkShowInDlg;
                            break;
                        case UiCategory.Profile:
                        case UiCategory.WheelOfFortune:
                        case UiCategory.DailyBonus:
                            // Do nothing
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    break;
                case UiCategory.Profile:
                case UiCategory.WheelOfFortune:
                case UiCategory.DailyBonus:
                    switch (_Prev)
                    {
                        case UiCategory.MainMenu:
                            trigger = AkFromMmToDlg;
                            break;
                        case UiCategory.Settings:
                        case UiCategory.Loading:
                        case UiCategory.SelectGame:
                        case UiCategory.Login:
                        case UiCategory.Countries:
                            trigger = AkShowInDlg;
                            break;
                        case UiCategory.Profile:
                        case UiCategory.WheelOfFortune:
                        case UiCategory.Shop:
                        case UiCategory.DailyBonus:
                            // Do nothing
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    break;
                case UiCategory.MainMenu:
                    switch (_Prev)
                    {
                        case UiCategory.Settings:
                        case UiCategory.Loading:
                        case UiCategory.SelectGame:
                        case UiCategory.Login:
                        case UiCategory.Countries:
                            trigger = AkShowInMm;
                            break;
                        case UiCategory.Profile:
                        case UiCategory.Shop:
                        case UiCategory.DailyBonus:
                        case UiCategory.WheelOfFortune:
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
            {
                if (prevTrigger != -1)
                    m_Animator.ResetTrigger(prevTrigger);
                m_Animator.SetTrigger(trigger);
                prevTrigger = trigger;
            }
        }

        private int prevTrigger = -1;

        ~BankMiniPanel()
        {
            MoneyManager.Instance.OnMoneyCountChanged -= MoneyCountChanged;
            MoneyManager.Instance.OnIncome -= Income;
            UiManager.Instance.OnCurrentCategoryChanged -= CurrentCategoryChanged;
        }
        
        #endregion

        
    }
}