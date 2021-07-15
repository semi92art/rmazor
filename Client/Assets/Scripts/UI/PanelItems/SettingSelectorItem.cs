﻿using System.Collections.Generic;
using System.Linq;
using Constants;
using Entities;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;
using Entry = UnityEngine.EventSystems.EventTrigger.Entry;

namespace UI.PanelItems
{
    public class SettingSelectorItem : MenuItemBase
    {
        [SerializeField] private EventTrigger trigger;
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private Animator animator;

        private bool m_IsInitialized;
        private IEnumerable<SettingSelectorItem> m_Items;
        private System.Action<string> m_OnSelect;

        public void SetItems(IEnumerable<SettingSelectorItem> _Items)
        {
            m_Items = _Items;
        }
        
        public void Init(
            string _Text,
            System.Action<string> _Select,
            bool _IsOn,
            IEnumerable<GameObserver> _Observers)
        {
            base.Init(_Observers);
            title.text = _Text;
            name = $"{_Text} Setting";
            m_OnSelect = _Select;
            var pointerDown = new Entry { eventID = EventTriggerType.PointerDown };
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
            Notifyer.RaiseNotify(this, CommonNotifyMessages.UiButtonClick);
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