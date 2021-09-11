using System.Collections.Generic;
using Constants;
using Entities;
using Extensions;
using GameHelpers;
using Ticker;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.PanelItems
{
    public class ChooseGameItem : MenuItemBase
    {
        public Button button;
        public Image icon;

        public void Init(ChooseGameItemProps _Props, IEnumerable<GameObserver> _Observers, ITicker _Ticker)
        {
            base.Init(_Observers, _Ticker);
            icon.sprite = GetLogo(_Props.GameId);
            button.SetOnClick(() =>
            {
                Notifyer.RaiseNotify(this, CommonNotifyMessages.UiButtonClick);
                _Props.Click?.Invoke();
            });
        }
        
        private Sprite GetLogo(int _GameId)
        {
            return PrefabUtilsEx.GetObject<Sprite>("game_logos", $"game_logo_{_GameId}");
        }
    }

    public class ChooseGameItemProps
    {
        public int GameId { get; }
        public bool IsVisible { get; }
        public UnityEngine.Events.UnityAction Click { get; set; }

        public ChooseGameItemProps(
            int _GameId,
            bool _IsVisible)
        {
            GameId = _GameId;
            IsVisible = _IsVisible;
        }
    }
}