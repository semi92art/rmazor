﻿using System;
using System.Collections.Generic;
using System.Linq;
using Constants;

using Entities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;

namespace UI.PanelItems
{
    public class SettingSelectorItem : MenuItemBase
    {
        [SerializeField] private EventTrigger trigger;
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private Animator animator;

        private bool m_IsInitialized;
        private IEnumerable<SettingSelectorItem> m_Items;
        private Action<string> m_OnSelect;

        public void SetItems(IEnumerable<SettingSelectorItem> _Items)
        {
            m_Items = _Items;
        }
        
        public void Init(
            string _Text,
            Action<string> _Select,
            bool _IsOn,
            IManagersGetter _Managers)
        {
            base.Init(_Managers);
            title.text = _Text;
            name = $"{_Text} Setting";
            m_OnSelect = _Select;
            var pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            pointerDown.callback.AddListener(Select);
            trigger.triggers.Add(pointerDown);

            m_IsInitialized = true;

            if (_IsOn)
                Coroutines.Run(Coroutines.WaitWhile(
                    () => m_Items == null, () => Select(null)));
        }

        public void Select(BaseEventData _BaseEventData)
        {
            if (!m_IsInitialized) 
                return;
            Managers.Notify(_SM => _SM.PlayClip(AudioClipNames.UIButtonClick));
            m_OnSelect?.Invoke(title.text);

            foreach (var item in m_Items.ToArray())
            {
                if (item == this)
                    continue;
                item.DeSelect(null);
            }
            animator.SetTrigger(AnimKeys.Selected);
        }

        public void DeSelect(BaseEventData _BaseEventData)
        {
            animator.SetTrigger(AnimKeys.Normal);
        }
    }
}