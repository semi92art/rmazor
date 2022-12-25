using Common.Entities;
using Common.Managers;
using Common.UI;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Setting_Panel_Items
{
    public class SettingItemOnOff : SimpleUiItemBase
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private Button          button;
        [SerializeField] private GameObject      toggleOnObj, toggleOffObj;

        private bool m_IsOn;

        public void Init(
            IUITicker            _UITicker,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager,
            bool                 _IsOn,
            string               _TitleLocalizationKey,
            UnityAction<bool>    _Action)
        {
            Init(_UITicker, _AudioManager, _LocalizationManager);
            name = "Setting";
            var locInfo = new LocalizableTextObjectInfo(title, ETextType.MenuUI, _TitleLocalizationKey);
            _LocalizationManager.AddTextObject(locInfo);
            m_IsOn = _IsOn;
            SetToggleObject();
            button.onClick.AddListener(() =>
            {
                m_IsOn = !m_IsOn;
                SetToggleObject();
                _Action?.Invoke(m_IsOn);
                SoundOnClick();
            });
        }

        private void SetToggleObject()
        {
            toggleOnObj.SetActive(m_IsOn);
            toggleOffObj.SetActive(!m_IsOn);
        }
    }
}