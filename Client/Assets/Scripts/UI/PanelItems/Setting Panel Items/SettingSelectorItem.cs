using System.Collections.Generic;
using System.Linq;
using Entities;
using Games.RazorMaze.Views.Common;
using Ticker;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Utils;
using DI.Extensions;

namespace UI.PanelItems.Setting_Panel_Items
{
    public class SettingSelectorItem : SimpleUiDialogItemView
    {
        #region serialized fields
        
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private Animator animator;
        
        #endregion
        
        private bool                             m_IsInitialized;
        private IEnumerable<SettingSelectorItem> m_Items;
        private UnityAction<string>              m_OnSelect;
        private bool                             m_IsTitleNotNull;
        private float                            m_BackgroundColorAlphaNormal;
        private float                            m_BackgroundColorAlphaSelected;
        
        public void Init(
            IManagersGetter _Managers,
            IUITicker _UITicker,
            IColorProvider _ColorProvider,
            string _Text,
            UnityAction<string> _Select,
            bool _IsOn)
        {
            base.Init(_Managers, _UITicker, _ColorProvider);
            m_BackgroundColorAlphaNormal = dialogBackground.color.a;
            m_BackgroundColorAlphaSelected = m_BackgroundColorAlphaNormal + 0.1f;
            title.text = _Text;
            name = $"{_Text} Setting";
            m_OnSelect = _Select;

            m_IsInitialized = true;

            if (_IsOn)
            {
                Coroutines.Run(Coroutines.WaitWhile(
                    () => m_Items == null, () => Select(null)));
            }
        }

        public override void Init(IManagersGetter _Managers, IUITicker _UITicker, IColorProvider _ColorProvider)
        {
            throw new System.NotSupportedException();
        }

        public void SetItems(IEnumerable<SettingSelectorItem> _Items)
        {
            m_Items = _Items;
        }

        public void Select(BaseEventData _BaseEventData)
        {
            if (!m_IsInitialized) 
                return;
            SoundOnClick();
            m_OnSelect?.Invoke(title.text);

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
            if (m_IsDialogBackgroundNotNull)
                dialogBackground.color = dialogBackground.color.SetA(m_BackgroundColorAlphaNormal);
        }

        private void SetSelectedState()
        {
            if (m_IsDialogBackgroundNotNull)
                dialogBackground.color = dialogBackground.color.SetA(m_BackgroundColorAlphaSelected);
        }

        protected override void CheckIfSerializedItemsNotNull()
        {
            base.CheckIfSerializedItemsNotNull();
            m_IsTitleNotNull = title.IsNotNull();
        }

        protected override void SetColorsOnInit()
        {
            base.SetColorsOnInit();
            title.color = ColorProvider.GetColor(ColorIds.UiText);
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            base.OnColorChanged(_ColorId, _Color);
            if (_ColorId == ColorIds.UiText)
            {
                if (m_IsTitleNotNull)
                    title.color = _Color;
            }
        }
    }
}