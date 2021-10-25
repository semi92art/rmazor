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
    public class ChooseGameItem : SimpleUiDialogItemView
    {
        public Button button;
        public Image icon;

        public void Init(
            IManagersGetter _Managers,
            IUITicker _UITicker,
            ChooseGameItemProps _Props)
        {
            InitCore(_Managers, _UITicker);
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