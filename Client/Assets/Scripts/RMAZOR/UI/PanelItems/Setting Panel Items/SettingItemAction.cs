using System;
using Common.Extensions;
using Common.Managers;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Setting_Panel_Items
{
    public class SettingItemAction : SimpleUiItem
    {
        [SerializeField] private Image           icon;
        [SerializeField] private Button          button;
        public                   TextMeshProUGUI title;

        private bool m_IsTitleNotNull;

        public void Init(
            IUITicker            _UITicker,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager,
            UnityAction          _Select,
            Sprite               _Icon       = null,
            Sprite               _Background = null)
        {
            base.Init(_UITicker, _AudioManager, _LocalizationManager);
            name = "Setting";
            button.onClick.AddListener(PlayButtonClickSound);
            button.onClick.AddListener(_Select);
            var titleRectTr = title.rectTransform;
            if (_Icon.IsNotNull())
            {
                icon.enabled = true;
                icon.sprite = _Icon;
                titleRectTr.anchoredPosition  = new Vector2(44f, 0f);
                titleRectTr.sizeDelta = new Vector2(-88f, 0f);
            }
            else
            {
                icon.enabled = false;
                titleRectTr.anchoredPosition = Vector2.zero;
                titleRectTr.sizeDelta = Vector2.zero;
            }
            if (_Background.IsNotNull())
                background.sprite = _Background;
        }

        public override void Init(
            IUITicker            _UITicker, 
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager)
        {
            throw new NotSupportedException();
        }

        protected override void CheckIfSerializedItemsNotNull()
        {
            base.CheckIfSerializedItemsNotNull();
            m_IsTitleNotNull = title.IsNotNull();
        }
    }
}