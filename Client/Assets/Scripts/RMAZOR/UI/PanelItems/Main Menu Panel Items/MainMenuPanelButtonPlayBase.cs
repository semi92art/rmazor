using System.Linq;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Main_Menu_Panel_Items
{
    public abstract class MainMenuPanelButtonPlayBase : SimpleUiItem
    {
        #region nonpublic members

        protected abstract SaveKey<bool> BadgeNewGameModeMustBeHidden { get; }

        #endregion
        
        #region serialized fields

        [SerializeField] protected GameObject      badgeNewGameMode;
        [SerializeField] protected Button          button;
        [SerializeField] protected Image           icon;
        [SerializeField] protected TextMeshProUGUI title;
        [SerializeField] protected TextMeshProUGUI body;
        [SerializeField] protected Slider          progressBar;
        [SerializeField] protected TextMeshProUGUI progressBarText;

        #endregion

        #region api

        public override void Init(
            IUITicker            _UITicker,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager)
        {
            HideBadgeNewGameModeIfNeed();
            base.Init(_UITicker, _AudioManager, _LocalizationManager);
        }

        public void SetOnClick(UnityAction _OnClick)
        {
            HideBadgeNewGameModeIfNeed();
            button.SetOnClick(() =>
            {
                _OnClick?.Invoke();
                if (BadgeNewGameModeMustBeHidden != null)
                    SaveUtils.PutValue(BadgeNewGameModeMustBeHidden, true);
                HideBadgeNewGameModeIfNeed();
            });
        }

        public virtual void UpdateState()
        {
            const float fontSize = 26f;
            const bool enableAutoSizing = true;
            foreach (var text in new [] { title, body}
                .Where(_T => _T.IsNotNull()))
            {
                text.fontSize         = fontSize;
                text.enableAutoSizing = enableAutoSizing;
                text.fontSizeMin      = fontSize * 0.5f;
                text.fontSizeMax      = fontSize;
            }
        }

        #endregion

        #region MyRegion

        private void HideBadgeNewGameModeIfNeed()
        {
            if (BadgeNewGameModeMustBeHidden == null)
                return;
            bool mustHideBadge = SaveUtils.GetValue(BadgeNewGameModeMustBeHidden);
            badgeNewGameMode.SetActive(!mustHideBadge);
        }

        #endregion
    }
}