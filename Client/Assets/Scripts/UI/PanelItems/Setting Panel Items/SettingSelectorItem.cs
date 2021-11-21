using System;
using System.Collections.Generic;
using System.Linq;
using Constants;
using Entities;
using Games.RazorMaze.Views.Common;
using Ticker;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;

namespace UI.PanelItems.Setting_Panel_Items
{
    public class SettingSelectorItem : SimpleUiDialogItemView
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
            IManagersGetter _Managers,
            IUITicker _UITicker,
            IColorProvider _ColorProvider,
            string _Text,
            Action<string> _Select,
            bool _IsOn)
        {
            InitCore(_Managers, _UITicker, _ColorProvider);
            title.text = _Text;
            name = $"{_Text} Setting";
            m_OnSelect = _Select;

            m_IsInitialized = true;

            if (_IsOn)
                Coroutines.Run(Coroutines.WaitWhile(
                    () => m_Items == null, () => Select(null)));
        }

        public void Select(BaseEventData _BaseEventData)
        {
            if (!m_IsInitialized) 
                return;
            Managers.AudioManager.PlayClip(CommonAudioClipArgs.UiButtonClick);
            m_OnSelect?.Invoke(title.text);

            foreach (var item in m_Items.ToArray())
            {
                if (item == this)
                    continue;
                item.DeSelect();
            }
            animator.SetTrigger(AnimKeys.Selected);
        }

        public void DeSelect()
        {
            animator.SetTrigger(AnimKeys.Normal);
        }
    }
}