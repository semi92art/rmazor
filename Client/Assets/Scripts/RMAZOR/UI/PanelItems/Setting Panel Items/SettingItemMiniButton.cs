using System;
using Common.Extensions;
using Common.Managers;
using Common.Ticker;
using Common.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Setting_Panel_Items
{
    public class SettingItemMiniButton : SimpleUiItemBase
    {
        #region serialized fields
        
        [SerializeField] private Image  icon;
        [SerializeField] private Toggle toggle;
        [SerializeField] private Sprite backSpriteOn, backSpriteOff;

        #endregion

        #region nonpublic members
        
        private Sprite m_IconOn, m_IconOff;
        private bool   m_IsIconNotNull;

        #endregion

        #region constructor
        
        public void Init(
            IUITicker            _UITicker,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager,
            bool                 _IsOn,
            UnityAction<bool>    _Action,
            Sprite               _IconOn,
            Sprite               _IconOff)
        {
            base.Init(_UITicker, _AudioManager, _LocalizationManager);
            m_IconOn = _IconOn;
            m_IconOff = _IconOff;
            SetBackgroundAndIcon(_IsOn);
            toggle.isOn = _IsOn;
            toggle.onValueChanged.AddListener(_On =>
            {
                _Action?.Invoke(_On);
                SoundOnClick();
                SetBackgroundAndIcon(_On);
            });
        }

        public override void Init(
            IUITicker            _UITicker, 
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region nonpublic methods
        
        protected override void CheckIfSerializedItemsNotNull()
        {
            base.CheckIfSerializedItemsNotNull();
            m_IsIconNotNull = icon.IsNotNull();
        }
        
        private void SetBackgroundAndIcon(bool _On)
        {
            icon.sprite = _On ? m_IconOn : m_IconOff;
            background.sprite = _On ? backSpriteOn : backSpriteOff;
        }

        #endregion
    }
}