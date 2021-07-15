using System;
using System.Collections.Generic;
using Constants;
using Entities;
using Exceptions;
using Extensions;
using GameHelpers;
using Managers;
using Network;
using Network.Packets;
using TMPro;
using UI.Entities;
using UI.Factories;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils;

namespace UI.PanelItems
{
    public interface IShopItem
    {
        void Init(ShopItemProps _Props, IEnumerable<GameObserver> _Observers);
    }

    public abstract class ShopItemBase : MenuItemBase
    {
        [SerializeField] protected Button button;
        [SerializeField] protected TextMeshProUGUI title;
        [SerializeField] protected TextMeshProUGUI description;
        [SerializeField] protected TextMeshProUGUI price;

        protected static IShopItem Create<T>(RectTransform _Parent, string _Prefab) where T : ShopItemBase
        {
            var go = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    _Parent,
                    UiAnchor.Create(0, 1, 0, 1),
                    new Vector2(218f, -60f),
                    Vector2.one * 0.5f,
                    new Vector2(416f, typeof(T) == typeof(ShopItemMoney) ? 125f : 80f)),
                "shop_items",
                _Prefab);
            return (IShopItem)go.GetComponent<T>();
        }

        protected void Init(UnityAction _Action, ShopItemProps _Props, IEnumerable<GameObserver> _Observers)
        {
            base.Init(_Observers);
            button.SetOnClick(_Action);
            title.text = _Props.Title;
            if (!string.IsNullOrEmpty(_Props.Description))
                description.text = _Props.Description;
            price.text = $"{_Props.Price:F2}$";

            switch (_Props.Type)
            {
                case ShopItemType.NoAds:
                    title.color = ColorUtils.GetColorFromCurrentPalette(CommonPaletteColors.UiShopNoAdsTitle);
                    break;
                case ShopItemType.Money:
                    switch (_Props.Size)
                    {
                        case ShopItemSize.Small:
                            title.color = ColorUtils.GetColorFromCurrentPalette(
                                CommonPaletteColors.UiShopSmallSetTitle);
                            break;
                        case ShopItemSize.Medium:
                            title.color = ColorUtils.GetColorFromCurrentPalette(
                                CommonPaletteColors.UiShopMediumSetTitle);
                            break;
                        case ShopItemSize.Big:
                            title.color = ColorUtils.GetColorFromCurrentPalette(
                                CommonPaletteColors.UiShopBigSetTitle);
                            break;
                        default:
                            throw new SwitchCaseNotImplementedException(_Props.Size);
                    }
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Props.Type);
            }
        }

        protected string BagSize(ShopItemSize _Size)
        {
            string bagSize;
            switch (_Size)
            {
                case ShopItemSize.Small:
                    bagSize = "small"; break;
                case ShopItemSize.Medium:
                    bagSize = "medium"; break;
                case ShopItemSize.Big:
                    bagSize = "big"; break;
                default:
                    throw new SwitchCaseNotImplementedException(_Size);
            }
            return bagSize;
        }
    }
    
    [Flags] public enum ShopItemType { Money, NoAds }
    public enum ShopItemSize { Small, Medium, Big }

    public class ShopItemProps
    {
        public ShopItemType Type { get; }
        public Dictionary<BankItemType, long> Rewards { get; }
        public string Title { get; }
        public string Description { get; }
        public float Price { get; }
        public ShopItemSize Size { get; }

        public ShopItemProps(
            ShopItemType _Type,
            string _Title,
            float _Price,
            Dictionary<BankItemType, long> _Rewards = null,
            string _Description = null,
            ShopItemSize? _Size = null)
        {
            Type = _Type;
            Rewards = _Rewards;
            Title = _Title;
            Description = _Description;
            Price = _Price;
            if (_Size.HasValue)
                Size = _Size.Value;
        }
    }
}