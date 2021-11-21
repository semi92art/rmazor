using System.Collections.Generic;
using System.Linq;
using DI.Extensions;
using Games.RazorMaze.Views.Common;
using Ticker;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public abstract class SimpleUiItemBase : MonoBehaviour
    {
        [SerializeField] protected Image                 background;
        [SerializeField] protected Image                 border;
        [SerializeField] protected List<TextMeshProUGUI> texts;
        [SerializeField] protected List<Image>           images;
        protected const            float                 TransitionTime = 0.2f;
        protected                  int                   m_TransitionCount;
        protected                  ITicker               m_Ticker;
        protected                  IColorProvider        m_ColorProvider;
        protected                  bool                  m_Initialized;

        protected void InitCore(IUITicker _Ticker, IColorProvider _ColorProvider)
        {
            m_Ticker = _Ticker;
            m_ColorProvider = _ColorProvider;
            m_Initialized = true;
            m_ColorProvider.ColorChanged += OnColorChanged;
            SetColorsOnInit();
        }
        
        private void SetColorsOnInit()
        {
            if (border.IsNotNull())
                border.color = m_ColorProvider.GetColor(ColorIds.UI);
            var textColor = m_ColorProvider.GetColor(ColorIds.UiText);
            foreach (var text in texts.Where(_Text => _Text.IsNotNull()))
                text.color = textColor;
            var uiColor = m_ColorProvider.GetColor(ColorIds.UI); 
            foreach (var image in images.Where(_Image => _Image.IsNotNull()))
                image.color = uiColor;
        }

        protected virtual void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId == ColorIds.UI)
            {
                foreach (var image in images)
                {
                    if (image.IsNotNull())
                        image.color = _Color;
                }
            }
            if (_ColorId == ColorIds.UiBorder)
            {
                if (border.IsNotNull())
                    border.color = _Color;
            }
            else if (_ColorId == ColorIds.UiText)
            {
                foreach (var text in texts)
                {
                    if (text.IsNotNull())
                        text.color = _Color;
                }
            }
            else if (_ColorId == ColorIds.UiBackground)
            {
                if (background.IsNotNull())
                    background.color = _Color;
            }
        }

        
        protected ColorBlock GetColorBlock()
        {
            return new ColorBlock
            {
                fadeDuration = 0.1f,
                colorMultiplier = 1f,
                normalColor = m_ColorProvider.GetColor(ColorIds.UiDialogItemNormal),
                highlightedColor = m_ColorProvider.GetColor(ColorIds.UiDialogItemNormal),
                pressedColor = m_ColorProvider.GetColor(ColorIds.UiDialogItemPressed),
                selectedColor = m_ColorProvider.GetColor(ColorIds.UiDialogItemSelected),
                disabledColor = m_ColorProvider.GetColor(ColorIds.UiDialogItemDisabled)
            };
        }
        
        private void OnDestroy()
        {
            if (m_ColorProvider != null)
                m_ColorProvider.ColorChanged -= OnColorChanged;
            m_TransitionCount = 0;
        }
        
        protected void MakeTransition(Color _Background)
        {
            if (background.IsNull() || !m_Initialized)
                return;
            m_TransitionCount++;
            Color from = background.color;
            Color to = _Background;
            int transCount = m_TransitionCount;
            Coroutines.Run(Coroutines.Lerp(
                from,
                to,
                TransitionTime,
                _Color =>
                {
                    if (transCount != m_TransitionCount) return;
                    background.color = _Color;
                },
                m_Ticker));
        }

    }
}