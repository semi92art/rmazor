using System;
using System.Collections.Generic;
using System.Linq;
using Constants;
using DialogViewers;
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
        
        private readonly List<CoinAnimObject> m_CoinsPool = new List<CoinAnimObject>(PoolSize);
        private readonly Image m_GoldIcon;
        private readonly Image m_DiamondIcon;
        private readonly TextMeshProUGUI m_GoldCount;
        private readonly TextMeshProUGUI m_DiamondsCount;
        private readonly Button m_PlusButton;
        private readonly Animator m_Animator;
        private readonly RectTransform m_BankMiniPanel;
        private bool m_IsShowing;

        private static int AkShowInMm => AnimKeys.Anim2;
        private static int AkShowInDlg => AnimKeys.Anim;
        private static int AkFromMmToDlg => AnimKeys.Anim4;
        private static int AkFromDlgToMm => AnimKeys.Anim3;
        private static int AkHideInMm => AnimKeys.Stop;
        private static int AkHideInDlg => AnimKeys.Stop;
        
        #endregion

        #region api
        
        public Action Action { get; set; }
        
        public BankMiniPanel(RectTransform _Parent, IMenuDialogViewer _MenuDialogViewer)
        {
            var go = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    _Parent,
                    UiAnchor.Create(1, 1, 1, 1),
                    Vector2.up  * -206f,
                    new Vector2(1f, 0.5f), 
                    new Vector2(349f, 100f)),
                "main_menu", "bank_mini_panel");
            m_GoldIcon = go.GetCompItem<Image>("gold_icon");
            m_DiamondIcon = go.GetCompItem<Image>("diamond_icon");
            m_GoldCount = go.GetCompItem<TextMeshProUGUI>("gold_count_text");
            m_DiamondsCount = go.GetCompItem<TextMeshProUGUI>("diamonds_count_text");
            m_PlusButton = go.GetCompItem<Button>("plus_money_button");
            m_Animator = go.GetCompItem<Animator>("animator");
            m_BankMiniPanel = go.RTransform();

            m_GoldCount.text = string.Empty;
            m_DiamondsCount.text = string.Empty;
            
            m_PlusButton.SetOnClick(() =>
            {
                SoundManager.Instance.PlayMenuButtonClick();
                IMenuDialogPanel shopPanel = new ShopPanel(_MenuDialogViewer);
                shopPanel.Show();
            });

            MoneyManager.Instance.OnMoneyCountChanged += MoneyCountChanged;
            MoneyManager.Instance.OnIncome += Income;
            UiManager.Instance.OnCurrentMenuCategoryChanged += CurrentMenuCategoryChanged;
        }

        public void Show()
        {
            var bank = MoneyManager.Instance.GetBank();
            m_Animator.SetTrigger(UiManager.Instance.CurrentMenuCategory == MenuUiCategory.MainMenu ?
                AkShowInMm : AkShowInDlg);
            m_IsShowing = true;
            m_BankMiniPanel.sizeDelta = m_BankMiniPanel.sizeDelta.SetX(100);

            Coroutines.Run(Coroutines.WaitWhile(() =>
            {
                int maxTextLength = bank.Money.Max(
                    _Kvp => _Kvp.Value).ToNumeric().Length;
                Coroutines.Run(Coroutines.Lerp(
                    m_BankMiniPanel.sizeDelta.x, 
                    GetPanelWidth(maxTextLength),
                    0.3f, 
                    SetPanelWidth,
                    () =>
                    {
                        SetMoneyText(bank.Money);
                    }));
            }, () => !bank.Loaded));
        }

        public void Hide()
        {
            throw new NotImplementedException();
        }
        
        #endregion
        
        #region private methods and destructor

        private void AnimateIncome(Dictionary<MoneyType, long> _Income, RectTransform _From)
        {
            CreateCoinsPool(_Income);
            AnimateCoinsTransfer(_Income, _From);
            AnimateTextIncome(_Income);
        }

        private void CreateCoinsPool(Dictionary<MoneyType, long> _Income)
        {
            string iconName = "gold_coin";
            Image icon = m_GoldIcon;
            if (_Income.ContainsKey(MoneyType.Diamonds) && _Income[MoneyType.Diamonds] > 0)
            {
                iconName = "diamond_coin";
                icon = m_DiamondIcon;
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
        }

        private void AnimateCoinsTransfer(Dictionary<MoneyType, long> _Income, RectTransform _From)
        {
            Vector3 to = m_GoldIcon.transform.position;
            if (_Income.ContainsKey(MoneyType.Diamonds) && _Income[MoneyType.Diamonds] > 0)
                to = m_DiamondIcon.transform.position;
            
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
                    Coroutines.Run(Coroutines.Lerp(
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
                _OnFinish:() =>
                {
                    Coroutines.Run(Coroutines.WaitWhile(() =>
                    {
                        foreach (var item in m_CoinsPool)
                            Object.Destroy(item.Item.gameObject);
                        m_CoinsPool.Clear();
                        Coroutines.Run(Coroutines.Delay(Action, 0.3f));
                    }, () => finishedDict.Any(_Kvp => !_Kvp.Value)));
                }));
        }

        private void AnimateTextIncome(Dictionary<MoneyType, long> _Income)
        {
            MoneyType moneyType = MoneyType.Gold;
            if (_Income.ContainsKey(MoneyType.Diamonds) && _Income[MoneyType.Diamonds] > 0)
                moneyType = MoneyType.Diamonds;
            
            var currMoney = MoneyManager.Instance.GetBank();
            if (_Income.ContainsKey(moneyType))
            {
                Coroutines.Run(Coroutines.WaitWhile(() =>
                {
                    int maxCountTextLength = Mathf.Max(m_GoldCount.text.Length, m_DiamondsCount.text.Length);
                    int newMaxCountTextLength =
                        Mathf.Max((currMoney.Money[moneyType] + _Income[moneyType]).ToNumeric().Length, maxCountTextLength);
                           
                    if (Mathf.Abs(newMaxCountTextLength - maxCountTextLength) > float.Epsilon)
                    {
                        float currentPanelWidth = m_BankMiniPanel.sizeDelta.x;
                        float newPanelWidth = m_GoldIcon.RTransform().rect.width + 
                                              GetTextWidth(newMaxCountTextLength) +
                                              m_PlusButton.RTransform().rect.width + 40;
                        m_GoldCount.rectTransform.sizeDelta =
                            m_GoldCount.rectTransform.sizeDelta.SetX(GetTextWidth(newMaxCountTextLength));
                        m_DiamondsCount.rectTransform.sizeDelta =
                            m_DiamondsCount.rectTransform.sizeDelta.SetX(GetTextWidth(newMaxCountTextLength));
                    
                        Coroutines.Run(Coroutines.Lerp(
                            currentPanelWidth, 
                            newPanelWidth,
                            0.3f, 
                            _Width =>
                            {
                                m_BankMiniPanel.sizeDelta = m_BankMiniPanel.sizeDelta.SetX(_Width);
                            }));
                    }
                    
                    Coroutines.Run(Coroutines.Lerp(
                        currMoney.Money[moneyType],
                        currMoney.Money[moneyType] + _Income[moneyType],
                        IncomeAnimTime,
                        _Value =>
                        {
                            if (moneyType == MoneyType.Gold)
                                m_GoldCount.text = _Value.ToNumeric();
                            else if (moneyType == MoneyType.Diamonds)
                                m_DiamondsCount.text = _Value.ToNumeric();
                        }));
                }, () => !currMoney.Loaded));
            }
        }
        
        private void MoneyCountChanged(BankEventArgs _Args)
        {
            SetMoneyText(_Args.Bank.Money);
        }
        
        private void Income(IncomeEventArgs _Args)
        {
            if (m_IsShowing)
                AnimateIncome(_Args.Bank.Money, _Args.From);
        }

        private float GetTextWidth(int _TextLength)
        {
            return _TextLength * Utility.SymbolWidth;
        }
        
        private void SetMoneyText(Dictionary<MoneyType, long> _Money)
        {
            m_GoldCount.text = _Money[MoneyType.Gold].ToNumeric();
            SetTextWidth(m_GoldCount);
            m_DiamondsCount.text = _Money[MoneyType.Diamonds].ToNumeric();
            SetTextWidth(m_DiamondsCount);
            SetPanelWidth();
        }

        private void SetTextWidth(TextMeshProUGUI _Text)
        {
            _Text.rectTransform.sizeDelta = 
                _Text.rectTransform.sizeDelta.SetX(GetTextWidth(_Text.text.Length));
        }
        private float GetPanelWidth(int? _MaxTextLength)
        {
            int textLength = _MaxTextLength ?? 
                             Mathf.Max(m_GoldCount.text.Length, m_DiamondsCount.text.Length);
            return m_GoldIcon.RTransform().rect.width + 
                                  GetTextWidth(textLength) +
                                  m_PlusButton.RTransform().rect.width + 40;
        }

        private void SetPanelWidth(float _Width)
        {
            m_BankMiniPanel.sizeDelta = m_BankMiniPanel.sizeDelta.SetX(_Width);
        }
        
        private void SetPanelWidth(int? _MaxTextLength = null)
        {
            SetPanelWidth(GetPanelWidth(_MaxTextLength));
        }
        
        private void CurrentMenuCategoryChanged(MenuUiCategory _Prev, MenuUiCategory _New)
        {
            if (m_Animator == null)
            {
                MoneyManager.Instance.OnMoneyCountChanged -= MoneyCountChanged;
                MoneyManager.Instance.OnIncome -= Income;
                UiManager.Instance.OnCurrentMenuCategoryChanged -= CurrentMenuCategoryChanged;
                return;
            }
            
            if (_Prev == _New || _Prev == MenuUiCategory.Nothing || _New == MenuUiCategory.Nothing)
                return;
            m_PlusButton.interactable = true;
            m_IsShowing = true;
            int trigger = -1;
            switch (_New)
            {
                case MenuUiCategory.Settings:
                case MenuUiCategory.Loading:
                case MenuUiCategory.SelectGame:
                    m_IsShowing = false;
                    trigger = _Prev == MenuUiCategory.MainMenu ? AkHideInMm : AkHideInDlg;
                    break;
                case MenuUiCategory.Login:
                case MenuUiCategory.Countries:
                    m_IsShowing = false;
                    trigger = _Prev == MenuUiCategory.MainMenu ? AkHideInMm : AkHideInDlg;
                    if (_Prev == MenuUiCategory.Countries || _Prev == MenuUiCategory.Login)
                        trigger = -1;
                    break;
                case MenuUiCategory.Shop:
                    m_PlusButton.interactable = false;
                    switch (_Prev)
                    {
                        case MenuUiCategory.MainMenu:
                            trigger = AkFromMmToDlg;
                            break;
                        case MenuUiCategory.Settings:
                        case MenuUiCategory.Loading:
                        case MenuUiCategory.SelectGame:
                        case MenuUiCategory.Login:
                        case MenuUiCategory.Countries:
                            trigger = AkShowInDlg;
                            break;
                        case MenuUiCategory.Profile:
                        case MenuUiCategory.WheelOfFortune:
                        case MenuUiCategory.DailyBonus:
                            // Do nothing
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    break;
                case MenuUiCategory.Profile:
                case MenuUiCategory.WheelOfFortune:
                case MenuUiCategory.DailyBonus:
                    switch (_Prev)
                    {
                        case MenuUiCategory.MainMenu:
                            trigger = AkFromMmToDlg;
                            break;
                        case MenuUiCategory.Settings:
                        case MenuUiCategory.Loading:
                        case MenuUiCategory.SelectGame:
                        case MenuUiCategory.Login:
                        case MenuUiCategory.Countries:
                            trigger = AkShowInDlg;
                            break;
                        case MenuUiCategory.Profile:
                        case MenuUiCategory.WheelOfFortune:
                        case MenuUiCategory.Shop:
                        case MenuUiCategory.DailyBonus:
                            // Do nothing
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    break;
                case MenuUiCategory.MainMenu:
                    switch (_Prev)
                    {
                        case MenuUiCategory.Settings:
                        case MenuUiCategory.Loading:
                        case MenuUiCategory.SelectGame:
                        case MenuUiCategory.Login:
                        case MenuUiCategory.Countries:
                            trigger = AkShowInMm;
                            break;
                        case MenuUiCategory.Profile:
                        case MenuUiCategory.Shop:
                        case MenuUiCategory.DailyBonus:
                        case MenuUiCategory.WheelOfFortune:
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

        #endregion
    }
}