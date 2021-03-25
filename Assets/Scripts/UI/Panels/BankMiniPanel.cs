using System;
using System.Collections.Generic;
using System.Linq;
using Constants;
using DialogViewers;
using Entities;
using Exceptions;
using Extensions;
using GameHelpers;
using Managers;
using TMPro;
using UI.Entities;
using UI.Factories;
using UI.Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils;
using Object = UnityEngine.Object;

namespace UI.Panels
{
    public interface IMiniPanel
    {
        void Show();
        void Hide();
        void Init();
    }

    public interface IActionExecutor
    { 
        UnityAction Action { get; set; }
    }
    
    public class BankMiniPanel : GameObservable, IMiniPanel, IActionExecutor
    {
        #region types

        private class CoinAnimObject
        {
            public RectTransform Item { get; set; }
            public bool IsBusy { get; set; }
        }
        
        #endregion
        
        #region nonpublic members
        
        private static readonly Dictionary<BankItemType, long> OneLifePrice = new Dictionary<BankItemType, long>
        {
            {BankItemType.FirstCurrency, 1000}, {BankItemType.SecondCurrency, 10}
        };

        private const float IncomeAnimTime = 2f;
        private const float IncomeAnimDeltaTime = 0.1f;
        private const int IncomeCoinsAnimOnScreen = 3;
        private const int PoolSize = 8;

        private readonly IMenuDialogViewer m_DialogViewer;
        private readonly INotificationViewer m_NotificationViewer;
        private readonly RectTransform m_Parent;
        private readonly List<CoinAnimObject> m_CoinsPool = 
            new List<CoinAnimObject>(PoolSize);
        private Image m_FirstCurrencyIcon;
        private Image m_SecondCurrencyIcon;
        private TextMeshProUGUI m_GoldCount;
        private TextMeshProUGUI m_DiamondsCount;
        private Button m_PlusMoneyButton;
        private Animator m_Animator;
        private RectTransform m_BankMiniPanel;
        private RectTransform m_MoneyPanel;
        private Animator m_PlusMoneyButtonAnim;
        private bool m_IsShowing;

        private static int AkShowInMm => AnimKeys.Anim2;
        private static int AkShowInDlg => AnimKeys.Anim;
        private static int AkFromMmToDlg => AnimKeys.Anim4;
        private static int AkFromDlgToMm => AnimKeys.Anim3;
        private static int AkHideInMm => AnimKeys.Stop;
        private static int AkHideInDlg => AnimKeys.Stop;
        private static int AkPlusButtonAnim => AnimKeys.Anim;
        private static int AkPlusButtonStop => AnimKeys.Stop;
        
        #endregion

        #region api
        
        public UnityAction Action { get; set; }
        
        public BankMiniPanel(
            RectTransform _Parent,
            IMenuDialogViewer _DialogViewer,
            INotificationViewer _NotificationViewer)
        {
            m_Parent = _Parent;
            m_DialogViewer = _DialogViewer;
            m_NotificationViewer = _NotificationViewer;
        }

        public void Init()
        {
            var go = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_Parent,
                    UiAnchor.Create(1, 1, 1, 1),
                    Vector2.up  * -206f,
                    new Vector2(1f, 0.5f), 
                    new Vector2(103f, 120f)),
                "main_menu", "bank_mini_panel");
            m_FirstCurrencyIcon = go.GetCompItem<Image>("gold_icon");
            m_SecondCurrencyIcon = go.GetCompItem<Image>("diamond_icon");
            m_GoldCount = go.GetCompItem<TextMeshProUGUI>("gold_count_text");
            m_DiamondsCount = go.GetCompItem<TextMeshProUGUI>("diamonds_count_text");
            m_PlusMoneyButton = go.GetCompItem<Button>("plus_money_button");
            m_Animator = go.GetCompItem<Animator>("animator");
            m_BankMiniPanel = go.RTransform();
            m_MoneyPanel = go.GetCompItem<RectTransform>("money_panel");
            m_PlusMoneyButtonAnim = m_PlusMoneyButton.GetComponent<Animator>();

            m_GoldCount.text = string.Empty;
            m_DiamondsCount.text = string.Empty;

            m_PlusMoneyButton.SetOnClick(() =>
            {
                Notify(this, CommonNotifyMessages.UiButtonClick);
                var plusMoneyPanel = new PlusMoneyPanel(m_DialogViewer, m_NotificationViewer, this);
                plusMoneyPanel.AddObservers(GetObservers());
                plusMoneyPanel.Init();
                m_DialogViewer.Show(plusMoneyPanel);
            });

            Dbg.Log("Init bank minipanel");
            BankManager.Instance.OnMoneyCountChanged += MoneyCountChanged;
            BankManager.Instance.OnIncome += Income;
            UiManager.Instance.OnCurrentMenuCategoryChanged += CurrentMenuCategoryChanged;
        }

        public void Show()
        {
            var bank = BankManager.Instance.GetBank();
            Coroutines.Run(Coroutines.WaitWhile(
                () => !bank.Loaded,
                () =>
                {
                    SetMoney(bank.BankItems);
                    m_Animator.SetTrigger(UiManager.Instance.CurrentMenuCategory == MenuUiCategory.MainMenu
                        ? AkShowInMm
                        : AkShowInDlg);
                    m_IsShowing = true;
                    m_BankMiniPanel.sizeDelta = m_BankMiniPanel.sizeDelta.SetX(100);
                    m_MoneyPanel.sizeDelta = m_MoneyPanel.sizeDelta.SetX(100);

                    int maxMoneyTextLength = bank.BankItems
                        .Max(_Kvp => _Kvp.Value).ToNumeric().Length;

                    Coroutines.Run(Coroutines.Lerp(
                        m_MoneyPanel.sizeDelta.x,
                        GetMoneyPanelWidth(maxMoneyTextLength),
                        0.3f,
                        SetMoneyPanelWidth,
                        UiTimeProvider.Instance));
                }));
        }

        public void Hide()
        {
            throw new NotImplementedException();
        }
        
        public void UnregisterFromEvents()
        {
            BankManager.Instance.OnMoneyCountChanged -= MoneyCountChanged;
            BankManager.Instance.OnIncome -= Income;
            UiManager.Instance.OnCurrentMenuCategoryChanged -= CurrentMenuCategoryChanged;
        }
        
        #endregion
        
        #region nonpublic methods and destructor

        private void AnimateIncome(Dictionary<BankItemType, long> _Income, RectTransform _From)
        {
            CreateCoinsPool(_Income);
            AnimateCoinsTransfer(_Income, _From);
            AnimateTextIncome(_Income);
        }

        private void CreateCoinsPool(Dictionary<BankItemType, long> _Income)
        {
            string iconName = "gold_coin";
            Image icon = m_FirstCurrencyIcon;
            if (_Income.ContainsKey(BankItemType.SecondCurrency) && _Income[BankItemType.SecondCurrency] > 0)
            {
                iconName = "diamond_coin";
                icon = m_SecondCurrencyIcon;
            }
            
            List<Sprite> sprites = new List<Sprite>();
            int spriteCount = 8;
            for (int i = 0; i < spriteCount; i++)
                sprites.Add(PrefabUtilsEx.GetObject<Sprite>("coins", $"{iconName}_{i}"));
            
            for (int i = 0; i < PoolSize; i++)
            {
                var item = new GameObject($"coin_{i}");
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

        private void AnimateCoinsTransfer(Dictionary<BankItemType, long> _Income, RectTransform _From)
        {
            Vector3 to = m_FirstCurrencyIcon.transform.position;
            if (_Income.ContainsKey(BankItemType.SecondCurrency) && _Income[BankItemType.SecondCurrency] > 0)
                to = m_SecondCurrencyIcon.transform.position;

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
                    coin.Item.SetGoActive(true);
                    finishedDict.Add(coroutineIndex, false);
                    int cI = coroutineIndex++;
                    Coroutines.Run(Coroutines.Lerp(
                        _From.position,
                        to,
                        IncomeAnimDeltaTime * IncomeCoinsAnimOnScreen,
                        _Pos => coin.Item.position = _Pos,
                        UiTimeProvider.Instance,
                        (_Breaked, _Progress) =>
                        {
                            coin.IsBusy = false;
                            if (coin.Item != null)
                                coin.Item.SetGoActive(false);
                            finishedDict[cI] = true;
                        }));
                },
                IncomeAnimDeltaTime,
                IncomeAnimTime,
                UiTimeProvider.Instance,
                _OnFinish:() =>
                {
                    Coroutines.Run(Coroutines.WaitWhile(
                        () => finishedDict.Any(_Kvp => !_Kvp.Value),
                        () =>
                    {
                        foreach (var item in m_CoinsPool)
                            Object.Destroy(item.Item.gameObject);
                        m_CoinsPool.Clear();
                        Coroutines.Run(Coroutines.Delay(Action, 0.3f));
                    }));
                }));
        }

        private void AnimateTextIncome(IReadOnlyDictionary<BankItemType, long> _Income)
        {
            BankItemType rewardType = _Income.ToList().First(_Kvp => _Kvp.Value > 0).Key;
            var currMoney = BankManager.Instance.GetBank();
            if (_Income.ContainsKey(rewardType))
            {
                Coroutines.Run(Coroutines.WaitWhile(
                    () => !currMoney.Loaded,
                    () =>
                {
                    int maxTextLength = Mathf.Max(m_GoldCount.text.Length,
                        m_DiamondsCount.text.Length);

                    int newMaxCountTextLength =
                        Mathf.Max((currMoney.BankItems[rewardType] + _Income[rewardType]).ToNumeric().Length, maxTextLength);
                           
                    if (Mathf.Abs(newMaxCountTextLength - maxTextLength) > float.Epsilon)
                    {
                        float currentPanelWidth = m_MoneyPanel.sizeDelta.x;
                        float newPanelWidth = GetMoneyPanelWidth(newMaxCountTextLength);

                        
                        m_GoldCount.rectTransform.sizeDelta = m_DiamondsCount.rectTransform.sizeDelta =
                            m_GoldCount.rectTransform.sizeDelta.SetX(GetTextWidth(newMaxCountTextLength));

                        Coroutines.Run(Coroutines.Lerp(
                            currentPanelWidth, 
                            newPanelWidth,
                            0.3f, 
                            _Width => m_MoneyPanel.sizeDelta = m_MoneyPanel.sizeDelta.SetX(_Width), 
                            UiTimeProvider.Instance));
                    }
                    
                    Coroutines.Run(Coroutines.Lerp(
                        currMoney.BankItems[rewardType],
                        currMoney.BankItems[rewardType] + _Income[rewardType],
                        IncomeAnimTime,
                        _Value =>
                        {
                            if (rewardType == BankItemType.FirstCurrency)
                                m_GoldCount.text = _Value.ToNumeric();
                            else if (rewardType == BankItemType.SecondCurrency)
                                m_DiamondsCount.text = _Value.ToNumeric();
                        }, UiTimeProvider.Instance));
                }));
            }
        }
        
        private void MoneyCountChanged(BankEventArgs _Args)
        {
            Debug.Log("Money count changed");
            var bank = _Args.BankEntity;
            Coroutines.Run(Coroutines.WaitWhile(() => !bank.Loaded, () =>
            {
                SetMoney(bank.BankItems);
            }));
        }

        private void SetMoney(Dictionary<BankItemType, long> _Money)
        {
            SetMoneyText(_Money);
            bool moneyNotEnough = _Money[BankItemType.FirstCurrency] < OneLifePrice[BankItemType.FirstCurrency] || 
                                  _Money[BankItemType.SecondCurrency] < OneLifePrice[BankItemType.SecondCurrency];
            m_PlusMoneyButtonAnim.SetTrigger(moneyNotEnough ? AkPlusButtonAnim : AkPlusButtonStop);
        }
        
        private void Income(IncomeEventArgs _Args)
        {
            if (m_IsShowing)
                AnimateIncome(_Args.BankEntity.BankItems, _Args.From);
        }

        private float GetTextWidth(int _TextLength)
        {
            return _TextLength * CommonUtils.SymbolWidth;
        }
        
        private void SetMoneyText(Dictionary<BankItemType, long> _Money)
        {
            m_GoldCount.text = _Money[BankItemType.FirstCurrency].ToNumeric();
            SetTextWidth(m_GoldCount);
            m_DiamondsCount.text = _Money[BankItemType.SecondCurrency].ToNumeric();
            SetTextWidth(m_DiamondsCount);
            SetMoneyPanelWidth();
        }

        private void SetTextWidth(TextMeshProUGUI _Text)
        {
            _Text.rectTransform.sizeDelta = 
                _Text.rectTransform.sizeDelta.SetX(GetTextWidth(_Text.text.Length));
        }
        
        private float GetMoneyPanelWidth(int? _MaxTextLength)
        {
            int textLength = _MaxTextLength ?? MathUtils.Max(
                                 m_GoldCount.text.Length, 
                                 m_DiamondsCount.text.Length);
            return m_FirstCurrencyIcon.RTransform().rect.width + 
                                  GetTextWidth(textLength) +
                                  m_PlusMoneyButton.RTransform().rect.width + 40;
        }

        private void SetMoneyPanelWidth(float _Width)
        {
            m_BankMiniPanel.sizeDelta = m_MoneyPanel.sizeDelta.SetX(_Width);
            m_MoneyPanel.sizeDelta = m_MoneyPanel.sizeDelta.SetX(_Width);
            m_MoneyPanel.anchoredPosition = m_MoneyPanel.anchoredPosition.SetX(0);
        }

        private void SetMoneyPanelWidth(int? _MaxTextLength = null)
        {
            SetMoneyPanelWidth(GetMoneyPanelWidth(_MaxTextLength));
        }

        
        private void CurrentMenuCategoryChanged(MenuUiCategory _Prev, MenuUiCategory _New)
        {
            if (_Prev == _New || _Prev == MenuUiCategory.Nothing || _New == MenuUiCategory.Nothing)
                return;

            m_PlusMoneyButton.interactable = _New != MenuUiCategory.PlusMoney && _New != MenuUiCategory.Shop;

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
                case MenuUiCategory.Shop:
                case MenuUiCategory.PlusMoney:
                    switch (_Prev)
                    {
                        case MenuUiCategory.MainMenu:
                            trigger = AkFromMmToDlg;
                            break;
                        case MenuUiCategory.Settings:
                        case MenuUiCategory.Loading:
                        case MenuUiCategory.SelectGame:
                            trigger = AkShowInDlg;
                            break;
                        case MenuUiCategory.WheelOfFortune:
                        case MenuUiCategory.DailyBonus:
                        case MenuUiCategory.Shop:
                        case MenuUiCategory.PlusMoney:
                            // Do nothing
                            break;
                        default:
                            throw new SwitchCaseNotImplementedException(_Prev);
                    }
                    break;
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
                            trigger = AkShowInDlg;
                            break;
                        case MenuUiCategory.WheelOfFortune:
                        case MenuUiCategory.Shop:
                        case MenuUiCategory.DailyBonus:
                        case MenuUiCategory.PlusMoney:
                            // Do nothing
                            break;
                        default:
                            throw new SwitchCaseNotImplementedException(_Prev);
                    }
                    break;
                case MenuUiCategory.MainMenu:
                    switch (_Prev)
                    {
                        case MenuUiCategory.Settings:
                        case MenuUiCategory.Loading:
                        case MenuUiCategory.SelectGame:
                            trigger = AkShowInMm;
                            break;
                        case MenuUiCategory.Shop:
                        case MenuUiCategory.DailyBonus:
                        case MenuUiCategory.WheelOfFortune:
                        case MenuUiCategory.PlusMoney:
                            trigger = AkFromDlgToMm;
                            break;
                        default:
                            throw new SwitchCaseNotImplementedException(_Prev);
                    }
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_New);
            }

            if (trigger != -1)
                m_Animator.SetTrigger(trigger);
        }

        #endregion
    }
}