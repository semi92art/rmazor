using System;
using Common;
using Common.Managers.PlatformGameServices;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Setting_Panel_Items
{
    public class SettingItemOnOff : SimpleUiItem
    {
        [SerializeField] protected TextMeshProUGUI title;
        [SerializeField] protected Button          button;
        [SerializeField] protected GameObject      toggleOnObj, toggleOffObj;

        private bool m_IsOn;

        public void Init(
            IUITicker            _UITicker,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager,
            bool                 _IsOn,
            string               _TitleLocalizationKey,
            UnityAction<bool>    _Action)
        {
            base.Init(_UITicker, _AudioManager, _LocalizationManager);
            name = "Setting";
            var locInfo = new LocTextInfo(title, ETextType.MenuUI_H1, _TitleLocalizationKey);
            _LocalizationManager.AddLocalization(locInfo);
            m_IsOn = _IsOn;
            SetToggleObject();
            button.onClick.AddListener(() =>
            {
                m_IsOn = !m_IsOn;
                SetToggleObject();
                _Action?.Invoke(m_IsOn);
                PlayButtonClickSound();
            });
        }

        private void SetToggleObject()
        {
            toggleOnObj.SetActive(m_IsOn);
            toggleOffObj.SetActive(!m_IsOn);
        }

    }
}