using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Extensions;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using Common.UI;
using Common.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace RMAZOR.UI.PanelItems.Setting_Panel_Items
{
    public class SettingSelectorItem : SimpleUiDialogItemView
    {
        #region serialized fields
        
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private Animator animator;

        private object        m_Value;
        
        #endregion
        
        private bool                             m_IsInitialized;
        private IEnumerable<SettingSelectorItem> m_Items;
        private UnityAction<object>              m_Select;
        private bool                             m_IsTitleNotNull;
        private float                            m_BackgroundColorAlphaNormal;
        private float                            m_BackgroundColorAlphaSelected;
        
        public void Init(
            IUITicker            _UITicker,
            IColorProvider       _ColorProvider,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager,
            IPrefabSetManager    _PrefabSetManager,
            string               _Text,
            object               _Value,
            UnityAction<object>  _Select,
            bool                 _IsOn,
            Func<TMP_FontAsset>  _Font = null)
        {
            base.Init(_UITicker, _ColorProvider, _AudioManager, _LocalizationManager, _PrefabSetManager);
            m_BackgroundColorAlphaNormal = dialogBackground.color.a;
            m_BackgroundColorAlphaSelected = m_BackgroundColorAlphaNormal + 0.1f;
            title.text = _Text;
            m_Value = _Value;
            if (_Font != null)
                title.font = _Font();
            else LocalizationManager.AddTextObject(
                new LocalizableTextObjectInfo(title, ETextType.MenuUI));
            name = $"{_Text} Setting";
            m_Select = _Select;
            m_IsInitialized = true;
            if (_IsOn)
            {
                Cor.Run(Cor.WaitWhile(
                    () => m_Items == null, () => Select(null)));
            }
        }

        public override void Init(
            IUITicker            _UITicker, 
            IColorProvider       _ColorProvider,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager,
            IPrefabSetManager    _PrefabSetManager,
            bool                 _AutoFont = true)
        {
            throw new NotSupportedException();
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
            m_Select?.Invoke(m_Value);

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
            if (_ColorId != ColorIds.UiText)
                return;
            if (m_IsTitleNotNull)
                title.color = _Color;
        }
    }
}