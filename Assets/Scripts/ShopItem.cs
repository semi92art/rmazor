using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UICreationSystem;
using UnityEngine.Events;

public class ShopItem : MonoBehaviour
{
    public Button button;
    public Image icon;
    public TextMeshProUGUI price;
    public TextMeshProUGUI amount;

    private ShopItemProps m_Props;

    public void Init(ShopItemProps _Props)
    {
        icon.sprite = _Props.Icon;
        price.text = $"{_Props.DiscountPrice} <color=#F33B3B><s>{_Props.Price}</s>";
        amount.text = _Props.Amount;
        if (_Props.Click != null)
            button.SetOnClick(_Props.Click);
    }
}

public class ShopItemProps
{
    public string Amount { get; }
    public string DiscountPrice { get; }
    public string Price { get; }
    public Sprite Icon { get; }
    public UnityAction Click { get; set; }

    public ShopItemProps(
        string _Amount,
        string _DiscountPrice,
        string _Price,
        Sprite _Icon)
    {
        Amount = _Amount;
        DiscountPrice = _DiscountPrice;
        Price = _Price;
        Icon = _Icon;
    }
}