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
using Network;
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

        private const float IncomeAnimTime = 2f;
        private const float IncomeAnimDeltaTime = 0.1f;
        private const int IncomeCoinsAnimOnScreen = 3;
        private const int PoolSize = 8;

        private readonly IMenuDialogViewer m_DialogViewer;
        private readonly INotificationViewer m_NotificationViewer;
        private readonly RectTransform m_Parent;
        private readonly List<CoinAnimObject> m_CoinsPool = 
            new List<CoinAnimObject>(PoolSize);
        private Image m_GoldIcon;
        private Image m_DiamondIcon;
        private Image m_LifesIcon;
        private TextMeshProUGUI m_GoldCount;
        private TextMeshProUGUI m_DiamondsCount;
        private TextMeshProUGUI m_LifesCount;
        private Button m_PlusMoneyButton;
        private Button m_PlusLifesButton;
        private Animator m_Animator;
        private RectTransform m_BankMiniPanel;
        private RectTransform m_MoneyPanel;
        private RectTransform m_LifesPanel;
        private Animator m_PlusMoneyButtonAnim;
        private Animator m_PlusLifesButtonAnim;
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
            m_GoldIcon = go.GetCompItem<Image>("gold_icon");
            m_DiamondIcon = go.GetCompItem<Image>("diamond_icon");
            m_LifesIcon = go.GetCompItem<Image>("lifes_icon");
            m_GoldCount = go.GetCompItem<TextMeshProUGUI>("gold_count_text");
            m_DiamondsCount = go.GetCompItem<TextMeshProUGUI>("diamonds_count_text");
            m_LifesCount = go.GetCompItem<TextMeshProUGUI>("lifes_count_text");
            m_PlusMoneyButton = go.GetCompItem<Button>("plus_money_button");
            m_PlusLifesButton = go.GetCompItem<Button>("plus_lifes_button");
            m_Animator = go.GetCompItem<Animator>("animator");
            m_BankMiniPanel = go.RTransform();
            m_MoneyPanel = go.GetCompItem<RectTransform>("money_panel");
            m_LifesPanel = go.GetCompItem<RectTransform>("lifes_panel");
            m_PlusMoneyButtonAnim = m_PlusMoneyButton.GetComponent<Animator>();
            m_PlusLifesButtonAnim = m_PlusLifesButton.GetComponent<Animator>();

            m_GoldCount.text = string.Empty;
            m_DiamondsCount.text = string.Empty;
            m_LifesCount.text = string.Empty;
            
            m_PlusMoneyButton.SetOnClick(() =>
            {
                Notify(this, CommonNotifyMessages.UiButtonClick);
                var plusMoneyPanel = new PlusMoneyPanel(m_DialogViewer, m_NotificationViewer, this);
                plusMoneyPanel.AddObservers(GetObservers());
                plusMoneyPanel.Init();
                m_DialogViewer.Show(plusMoneyPanel);
            });
            
            m_PlusLifesButton.SetOnClick(() =>
            {
                Notify(this, CommonNotifyMessages.UiButtonClick);
                var plusLifesPanel = new PlusLifesPanel(m_DialogViewer);
                plusLifesPanel.AddObservers(GetObservers());
                plusLifesPanel.Init();
                m_DialogViewer.Show(plusLifesPanel);
            });

            BankManager.Instance.OnMoneyCountChanged += MoneyCountChanged;
            BankManager.Instance.OnIncome += Income;
            UiManager.Instance.OnCurrentMenuCategoryChanged += CurrentMenuCategoryChanged;
        }

        public void Show()
        {
            Coroutines.Run(Coroutines.WaitWhile(
                () => GameClient.Instance.AccountId == default,
                () =>
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
                    m_LifesPanel.sizeDelta = m_LifesPanel.sizeDelta.SetX(100);

                    int maxMoneyTextLength = bank.BankItems
                        .Max(_Kvp => _Kvp.Value).ToNumeric().Length;

                    Coroutines.Run(Coroutines.Lerp(
                        m_MoneyPanel.sizeDelta.x,
                        GetMoneyPanelWidth(maxMoneyTextLength),
                        0.3f,
                        SetMoneyPanelWidth,
                        UiTimeProvider.Instance));

                    Coroutines.Run(Coroutines.Lerp(
                        m_LifesPanel.sizeDelta.x,
                        GetLifesPanelWidth(bank.BankItems[BankItemType.Lifes].ToNumeric().Length),
                        0.3f,
                        SetLifesPanelWidth,
                        UiTimeProvider.Instance));
                }));
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
            Image icon = m_GoldIcon;
            if (_Income.ContainsKey(BankItemType.Diamonds) && _Income[BankItemType.Diamonds] > 0)
            {
                iconName = "diamond_coin";
                icon = m_DiamondIcon;
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
            Vector3 to = m_GoldIcon.transform.position;
            if (_Income.ContainsKey(BankItemType.Diamonds) && _Income[BankItemType.Diamonds] > 0)
                to = m_DiamondIcon.transform.position;
            else if (_Income.ContainsKey(BankItemType.Lifes) && _Income[BankItemType.Lifes] > 0)
                to = m_LifesIcon.transform.position;
            
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
                        () =>
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
                bool isMoney;
                switch (rewardType)
                {
                    case BankItemType.Gold:
                    case BankItemType.Diamonds:
                        isMoney = true;
                        break;
                    case BankItemType.Lifes:
                        isMoney = false;
                        break;
                    default:
                        throw new SwitchCaseNotImplementedException(rewardType);
                }
                
                Coroutines.Run(Coroutines.WaitWhile(
                    () => !currMoney.Loaded,
                    () =>
                {
                    int maxTextLength = isMoney ? Mathf.Max(m_GoldCount.text.Length,
                        m_DiamondsCount.text.Length) : m_LifesCount.text.Length;

                    int newMaxCountTextLength =
                        Mathf.Max((currMoney.BankItems[rewardType] + _Income[rewardType]).ToNumeric().Length, maxTextLength);
                           
                    if (Mathf.Abs(newMaxCountTextLength - maxTextLength) > float.Epsilon)
                    {
                        float currentPanelWidth = isMoney ? m_MoneyPanel.sizeDelta.x : m_LifesPanel.sizeDelta.x;
                        float newPanelWidth = isMoney
                            ? GetMoneyPanelWidth(newMaxCountTextLength)
                            : GetLifesPanelWidth(newMaxCountTextLength);

                        if (isMoney)
                        {
                            m_GoldCount.rectTransform.sizeDelta = m_DiamondsCount.rectTransform.sizeDelta =
                                m_GoldCount.rectTransform.sizeDelta.SetX(GetTextWidth(newMaxCountTextLength));    
                        }
                        else
                        {
                            m_LifesCount.rectTransform.sizeDelta =
                                m_LifesCount.rectTransform.sizeDelta.SetX(GetTextWidth(newMaxCountTextLength));    
                        }
                        
                        Coroutines.Run(Coroutines.Lerp(
                            currentPanelWidth, 
                            newPanelWidth,
                            0.3f, 
                            _Width =>
                            {
                                if (isMoney)
                                    m_MoneyPanel.sizeDelta = m_MoneyPanel.sizeDelta.SetX(_Width);
                                else
                                    m_LifesPanel.sizeDelta = m_LifesPanel.sizeDelta.SetX(_Width);
                            }, UiTimeProvider.Instance));
                    }
                    
                    Coroutines.Run(Coroutines.Lerp(
                        currMoney.BankItems[rewardType],
                        currMoney.BankItems[rewardType] + _Income[rewardType],
                        IncomeAnimTime,
                        _Value =>
                        {
                            if (rewardType == BankItemType.Gold)
                                m_GoldCount.text = _Value.ToNumeric();
                            else if (rewardType == BankItemType.Diamonds)
                                m_DiamondsCount.text = _Value.ToNumeric();
                            else if (rewardType == BankItemType.Lifes)
                                m_LifesCount.text = _Value.ToNumeric();
                        }, UiTimeProvider.Instance));
                }));
            }
        }
        
        private void MoneyCountChanged(BankEventArgs _Args)
        {
            SetMoney(_Args.BankEntity.BankItems);
        }

        private void SetMoney(Dictionary<BankItemType, long> _Money)
        {
            SetMoneyText(_Money);
            SetLifesText(_Money);
            bool moneyNotEnough = _Money[BankItemType.Gold] < PlusLifesPanel.OneLifePrice[BankItemType.Gold] || 
                                  _Money[BankItemType.Diamonds] < PlusLifesPanel.OneLifePrice[BankItemType.Diamonds];
            bool lifesNotEnough = _Money[BankItemType.Lifes] < 10;
            m_PlusMoneyButtonAnim.SetTrigger(moneyNotEnough ? AkPlusButtonAnim : AkPlusButtonStop);
            Coroutines.Run(Coroutines.Delay(
                () => m_PlusLifesButtonAnim.SetTrigger(lifesNotEnough ? 
                    AkPlusButtonAnim : AkPlusButtonStop),
                2.5f));

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
            m_GoldCount.text = _Money[BankItemType.Gold].ToNumeric();
            SetTextWidth(m_GoldCount);
            m_DiamondsCount.text = _Money[BankItemType.Diamonds].ToNumeric();
            SetTextWidth(m_DiamondsCount);
            SetMoneyPanelWidth();
        }

        private void SetLifesText(Dictionary<BankItemType, long> _Money)
        {
            m_LifesCount.text = _Money[BankItemType.Lifes].ToNumeric();
            SetTextWidth(m_LifesCount);
            SetLifesPanelWidth();
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
            return m_GoldIcon.RTransform().rect.width + 
                                  GetTextWidth(textLength) +
                                  m_PlusMoneyButton.RTransform().rect.width + 40;
        }

        private float GetLifesPanelWidth(int? _TextLength)
        {
            int textLength = _TextLength ?? m_LifesCount.text.Length;
            return m_LifesIcon.RTransform().rect.width +
                   GetTextWidth(textLength) +
                   m_PlusLifesButton.RTransform().rect.width + 70;
        }

        private void SetMoneyPanelWidth(float _Width)
        {
            m_BankMiniPanel.sizeDelta = m_MoneyPanel.sizeDelta.SetX(_Width);
            m_MoneyPanel.sizeDelta = m_MoneyPanel.sizeDelta.SetX(_Width);
            m_MoneyPanel.anchoredPosition = m_MoneyPanel.anchoredPosition.SetX(0);
        }

        private void SetLifesPanelWidth(float _Width)
        {
            m_LifesPanel.sizeDelta = m_LifesPanel.sizeDelta.SetX(_Width);
            m_LifesPanel.anchoredPosition = m_LifesPanel.anchoredPosition.SetX(0);
        }
        
        private void SetMoneyPanelWidth(int? _MaxTextLength = null)
        {
            SetMoneyPanelWidth(GetMoneyPanelWidth(_MaxTextLength));
        }

        private void SetLifesPanelWidth(int? _TextLength = null)
        {
            SetLifesPanelWidth(GetLifesPanelWidth(_TextLength));
        }
        
        private void CurrentMenuCategoryChanged(MenuUiCategory _Prev, MenuUiCategory _New)
        {
            if (_Prev == _New || _Prev == MenuUiCategory.Nothing || _New == MenuUiCategory.Nothing)
                return;

            m_PlusMoneyButton.interactable = _New != MenuUiCategory.PlusMoney && _New != MenuUiCategory.Shop;
            m_PlusLifesButton.interactable = _New != MenuUiCategory.PlusLifes && _New != MenuUiCategory.Shop;
            
            m_IsShowing = true;
            int trigger = -1;
            switch (_New)
            {
                case MenuUiCategory.Settings:
                case MenuUiCategory.Loading:
                case MenuUiCategory.SelectGame:
                case MenuUiCategory.Login:
                    m_IsShowing = false;
                    trigger = _Prev == MenuUiCategory.MainMenu ? AkHideInMm : AkHideInDlg;
                    break;
                case MenuUiCategory.Shop:
                case MenuUiCategory.PlusMoney:
                case MenuUiCategory.PlusLifes:
                    switch (_Prev)
                    {
                        case MenuUiCategory.MainMenu:
                            trigger = AkFromMmToDlg;
                            break;
                        case MenuUiCategory.Settings:
                        case MenuUiCategory.Loading:
                        case MenuUiCategory.SelectGame:
                        case MenuUiCategory.Login:
                            trigger = AkShowInDlg;
                            break;
                        case MenuUiCategory.Profile:
                        case MenuUiCategory.WheelOfFortune:
                        case MenuUiCategory.DailyBonus:
                        case MenuUiCategory.Shop:
                        case MenuUiCategory.PlusMoney:
                        case MenuUiCategory.PlusLifes:
                            // Do nothing
                            break;
                        default:
                            throw new SwitchCaseNotImplementedException(_Prev);
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
                            trigger = AkShowInDlg;
                            break;
                        case MenuUiCategory.Profile:
                        case MenuUiCategory.WheelOfFortune:
                        case MenuUiCategory.Shop:
                        case MenuUiCategory.DailyBonus:
                        case MenuUiCategory.PlusMoney:
                        case MenuUiCategory.PlusLifes:
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
                        case MenuUiCategory.Login:
                            trigger = AkShowInMm;
                            break;
                        case MenuUiCategory.Profile:
                        case MenuUiCategory.Shop:
                        case MenuUiCategory.DailyBonus:
                        case MenuUiCategory.WheelOfFortune:
                        case MenuUiCategory.PlusMoney:
                        case MenuUiCategory.PlusLifes:
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