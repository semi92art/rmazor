﻿using System;
using System.Collections.Generic;
using System.Linq;
using Common.Enums;
using Common.Managers;
using Common.Ticker;
using Common.UI;
using Common.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Setting_Panel_Items
{
    public class SettingLanguageItem : SimpleUiItemBase
    {
        #region serialized fields

        [SerializeField] private Image languageIcon;
        [SerializeField] private Image iconCheck;
        
        #endregion
        
        private ELanguage                        m_Language;
        private bool                             m_Initialized;
        private IEnumerable<SettingLanguageItem> m_Items;
        private UnityAction<ELanguage>           m_Select;
        private Func<bool>                       m_ReadyToSelect;
        private bool                             m_IsTitleNotNull;
        
        public void Init(
            IUITicker               _UITicker,
            IAudioManager           _AudioManager,
            ILocalizationManager    _LocalizationManager,
            ELanguage               _Language,
            UnityAction<ELanguage>  _Select,
            bool                    _IsOn,
            Func<ELanguage, Sprite> _GetIconFunc,
            Func<bool>              _ReadyToSelect)
        {
            base.Init(_UITicker, _AudioManager, _LocalizationManager);
            m_Language = _Language;
            m_Select = _Select;
            m_ReadyToSelect = _ReadyToSelect;
            m_Initialized = true;
            languageIcon.sprite = _GetIconFunc(m_Language);
            if (!_IsOn)
            {
                SetNormalState();
                return;
            }
            SetSelectedState();
            Cor.Run(Cor.WaitWhile(
                () => m_Items == null, () => Select(null)));
        }

        public override void Init(
            IUITicker            _UITicker, 
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager)
        {
            throw new NotSupportedException();
        }

        public void SetItems(IEnumerable<SettingLanguageItem> _Items)
        {
            m_Items = _Items;
        }

        public void Select(BaseEventData _BaseEventData)
        {
            if (!m_Initialized || !m_ReadyToSelect()) 
                return;
            SoundOnClick();
            m_Select?.Invoke(m_Language);

            foreach (var item in m_Items.ToArray())
            {
                if (item == this)
                    continue;
                item.SetNormalState();
            }
            SetSelectedState();
        }

        private void SetNormalState()
        {
            iconCheck.enabled = false;
        }
        
        private void SetSelectedState()
        {
            iconCheck.enabled = true;
        }
    }
}