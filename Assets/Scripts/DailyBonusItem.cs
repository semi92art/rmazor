using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UICreationSystem;
using UnityEngine.Events;

public class DailyBonusItem : MonoBehaviour
{
    public Button button;
    public Animator animator;
    public Image icon;
    public TextMeshProUGUI price;
    public TextMeshProUGUI day;

    private DailyBonusProps m_Props;
    
    public void Init(DailyBonusProps _Props)
    {
        icon.sprite = _Props.Icon;
        day.text = $"Day {_Props.Day}";
        price.text = _Props.Gold != 0 ? $"{_Props.Gold}" : $"{_Props.Diamonds}";
        if (_Props.IsActive)
            animator.SetTrigger(AnimKeys.Anim);
        
        button.SetOnClick(_Props.Click);
    }
}

public class DailyBonusProps
{
    public int Day { get; }
    public int Gold { get; }
    public int Diamonds { get; }
    public Sprite Icon { get; }
    public bool IsActive { get; set; }
    public UnityAction Click { get; }

    public DailyBonusProps(
        int _Day,
        int _Gold = 0,
        int _Diamonds = 0,
        Sprite _Icon = null,
        UnityAction _OnClick = null)
    {
        Day = _Day;
        Gold = _Gold;
        Diamonds = _Diamonds;
        Icon = _Icon;
        Click = _OnClick;
    }
}