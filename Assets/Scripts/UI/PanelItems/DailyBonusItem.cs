using System.Collections.Generic;
using Constants;
using Entities;
using Extensions;
using Managers;
using TMPro;
using UI.Panels;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.PanelItems
{
    public class DailyBonusItem : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Animator iconAnimator;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI price;
        [SerializeField] private TextMeshProUGUI day;
        [SerializeField] private TextMeshProUGUI tomorrow;
    
        private DailyBonusProps m_Props;
    
        public void Init(DailyBonusProps _Props, IActionExecuter _ActionExecuter)
        {
            icon.sprite = _Props.Icon;
            day.text = $"Day {_Props.Day}";
            price.text = _Props.Gold != 0 ? $"{_Props.Gold}" : $"{_Props.Diamonds}";
            button.interactable = _Props.IsActive;
            if (_Props.IsActive)
                iconAnimator.SetTrigger(AnimKeys.Anim);
            
            tomorrow.enabled = _Props.IsTomorrow;
        
            button.onClick.AddListener(() =>
            {
                var money = new Dictionary<MoneyType, int>();
                if (_Props.Gold > 0)
                    money.Add(MoneyType.Gold, _Props.Gold);
                if (_Props.Diamonds > 0)
                    money.Add(MoneyType.Diamonds, _Props.Diamonds);
                MoneyManager.Instance.SetIncome(money, icon.RTransform());
                MoneyManager.Instance.PlusMoney(money);
            
                SaveUtils.PutValue(SaveKey.DailyBonusLastDate, System.DateTime.Today);
                SaveUtils.PutValue(SaveKey.DailyBonusLastItemClickedDay, _Props.Day);
                iconAnimator.SetTrigger(AnimKeys.Stop);
            });
        
        
            button.onClick.AddListener(() =>
            {
                _ActionExecuter.Action = _Props.Click;
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
        public System.Action Click { get; set; }

        public DailyBonusProps(
            int _Day,
            int _Gold = 0,
            int _Diamonds = 0,
            Sprite _Icon = null,
            System.Action _Click = null)
        {
            Day = _Day;
            Gold = _Gold;
            Diamonds = _Diamonds;
            Icon = _Icon;
            Click = _Click;
        }
    }
}