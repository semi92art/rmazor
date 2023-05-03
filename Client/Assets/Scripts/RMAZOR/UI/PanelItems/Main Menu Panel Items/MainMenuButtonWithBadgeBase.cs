using System;
using System.Globalization;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Main_Menu_Panel_Items
{
    public abstract class MainMenuButtonWithBadgeBase : SimpleUiItem
    {
        [SerializeField] protected Button          button;
        [SerializeField] protected TextMeshProUGUI title;
        [SerializeField] protected Image           badgeIcon;
        [SerializeField] protected TextMeshProUGUI badgeText;

        private Func<int> m_GetBadgeNumber;
        private string    m_TitleLocKey;
        
        public abstract bool CanBeVisible { get; }

        public void Init(
            IUITicker            _UITicker,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager,
            UnityAction          _OnClick,
            Func<int>            _GetBadgeNumber,
            string               _TitleLocKey)
        {
            if (!CanBeVisible)
            {
                gameObject.SetActive(false);
                return;
            }
            base.Init(_UITicker, _AudioManager, _LocalizationManager);
            button.SetOnClick(_OnClick);
            m_TitleLocKey = _TitleLocKey;
            m_GetBadgeNumber = _GetBadgeNumber;
            LocalizeTextObjectsOnInit();
        }

        public virtual void UpdateState()
        {
            if (!CanBeVisible)
                return;
            int badgeNumber = m_GetBadgeNumber();
            badgeIcon.enabled = badgeNumber > 0;
            badgeText.enabled = badgeNumber > 0;
            badgeText.text = badgeNumber.ToString();
        }

        private void LocalizeTextObjectsOnInit()
        {
            var locTextInfos = new[]
            {
                new LocTextInfo(title, ETextType.MenuUI_H1, m_TitleLocKey,
                    _T => _T.ToUpper(CultureInfo.CurrentUICulture)), 
                new LocTextInfo(badgeText, ETextType.MenuUI_H1, "empty_key",
                    _TextLocalizationType: ETextLocalizationType.OnlyFont), 
            };
            foreach (var locTextInfo in locTextInfos)
                LocalizationManager.AddLocalization(locTextInfo);
        }
    }
}