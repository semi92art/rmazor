using System.Collections.Generic;
using Constants;
using Constants.NotifyMessages;
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
            IGameObservable _GameObservable)
        {
            base.Init(_GameObservable);
            icon.sprite = GetLogo(_Props.GameId);
            button.SetOnClick(() =>
            {
                GameObservable.Notify(SoundNotifyMessages.PlayAudioClip, AudioClipNames.UIButtonClick);
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