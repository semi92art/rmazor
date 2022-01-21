using System;
using Common.Constants;
using Common.Utils;
using Lean.Localization;
using RMAZOR;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils;

namespace UI.PanelItems
{
    public class DailyBonusItem : SimpleUiDialogItemView
    {
        [SerializeField] private Button button;
        [SerializeField] private Animator iconAnimator;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI price;
        [SerializeField] private TextMeshProUGUI day;
        [SerializeField] private TextMeshProUGUI tomorrow;
    
        private DailyBonusProps m_Props;
    
        public void Init(
            DailyBonusProps _Props,
            IAction _Action)
        {
            icon.sprite = _Props.Icon;
            day.text = $"{LeanLocalization.GetTranslationText("Day")} {_Props.Day}";
            price.text = _Props.Gold != 0 ? $"{_Props.Gold}" : $"{_Props.Diamonds}";
            button.interactable = _Props.IsActive;
            if (_Props.IsActive)
                iconAnimator.SetTrigger(AnimKeys.Anim);
            
            tomorrow.enabled = _Props.IsTomorrow;
        
            button.onClick.AddListener(() =>
            {
                // var money = new Dictionary<BankItemType, long>();
                // if (_Props.Gold > 0)
                //     money.Add(BankItemType.FirstCurrency, _Props.Gold);
                // if (_Props.Diamonds > 0)
                //     money.Add(BankItemType.SecondCurrency, _Props.Diamonds);
                // BankManager.Instance.SetIncome(money, icon.RTransform());
                // BankManager.Instance.PlusBankItems(money);
            
                SaveUtils.PutValue(SaveKeys.DailyBonusLastDate, DateTime.Today);
                SaveUtils.PutValue(SaveKeys.DailyBonusLastClickedDay, _Props.Day);
                iconAnimator.SetTrigger(AnimKeys.Stop);
            });
        
        
            button.onClick.AddListener(() =>
            {
                _Action.Action = _Props.Click;
            });
        }
    }

    public class DailyBonusProps
    {
        public int Day { get; }
        public int Gold { get; }
        public int Diamonds { get; }
        public Sprite Icon { get; }
        public bool IsTomorrow { get; set; }
        public bool IsActive { get; set; }
        public UnityAction Click { get; set; }

        public DailyBonusProps(
            int _Day,
            int _Gold = 0,
            int _Diamonds = 0,
            Sprite _Icon = null,
            UnityAction _Click = null)
        {
            Day = _Day;
            Gold = _Gold;
            Diamonds = _Diamonds;
            Icon = _Icon;
            Click = _Click;
        }
    }
}