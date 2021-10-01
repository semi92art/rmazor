using System.Collections.Generic;
using Constants;

using DI.Extensions;
using Entities;
using GameHelpers;
using Ticker;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.PanelItems
{
    public class ChooseGameItem : MenuItemBase
    {
        public Button button;
        public Image icon;

        public void Init(
            ChooseGameItemProps _Props,
            IManagersGetter _Managers)
        {
            base.Init(_Managers);
            icon.sprite = GetLogo(_Props.GameId);
            button.SetOnClick(() =>
            {
                Managers.Notify(_SM => _SM.PlayClip(AudioClipNames.UIButtonClick));
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
        public UnityAction Click { get; set; }

        public ChooseGameItemProps(
            int _GameId,
            bool _IsVisible)
        {
            GameId = _GameId;
            IsVisible = _IsVisible;
        }
    }
}