using System.Collections.Generic;
using System.Linq;
using Constants;
using DI.Extensions;
using Entities;
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
        #region serialized fields

        [SerializeField] private Image                 background;
        [SerializeField] private Image                 border;
        [SerializeField] private List<TextMeshProUGUI> texts;
        [SerializeField] private List<Image>           images;

        #endregion
        
        #region nonpublic members

        protected IManagersGetter Managers      { get; private set; }
        protected IUITicker       UITicker      { get; private set; }
        protected IColorProvider  ColorProvider { get; private set; }

        private bool   m_IsBackgroundNotNull;
        private bool   m_IsBorderNotNull;
        private bool[] m_AreTextsNotNull;
        private bool[] m_AreImagesNotNull;
        private Color  m_BorderDefaultColor;
        private Color  m_BorderHighlightedColor;

        #endregion
        
        #region api
        
        public bool Highlighted { get; set; }
        
        #endregion
        
        #region nonpublic methods

        public virtual void Init(
            IManagersGetter _Managers,
            IUITicker _UITicker,
            IColorProvider _ColorProvider)
        {
            Managers = _Managers;
            UITicker = _UITicker;
            ColorProvider = _ColorProvider;
            ColorProvider.ColorChanged += OnColorChanged;
            CheckIfSerializedItemsNotNull();
            SetColorsOnInit();
        }

        protected virtual void CheckIfSerializedItemsNotNull()
        {
            m_IsBackgroundNotNull = background.IsNotNull();
            m_IsBorderNotNull = border.IsNotNull();
            m_AreTextsNotNull = texts.Select(_T => _T.IsNotNull()).ToArray();
            m_AreImagesNotNull = images.Select(_I => _I.IsNotNull()).ToArray();
        }
        
        protected virtual void SetColorsOnInit()
        {
            if (border.IsNotNull())
                border.color = ColorProvider.GetColor(ColorIds.UiBorder);
            var textColor = ColorProvider.GetColor(ColorIds.UiText);
            foreach (var text in texts.Where(_Text => _Text.IsNotNull()))
                text.color = textColor;
            var uiColor = ColorProvider.GetColor(ColorIds.UI); 
            foreach (var image in images.Where(_Image => _Image.IsNotNull()))
                image.color = uiColor;
            m_BorderDefaultColor = ColorProvider.GetColor(ColorIds.UiBorder);
            m_BorderHighlightedColor = ColorProvider.GetColor(ColorIds.UiItemHighlighted);
        }

        protected virtual void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId == ColorIds.UI)
            {
                for (int i = 0; i < images.Count; i++)
                {
                    if (m_AreImagesNotNull[i])
                        images[i].color = _Color;
                }
            }
            if (_ColorId == ColorIds.UiBorder)
            {
                if (m_IsBorderNotNull)
                    border.color = _Color;
            }
            else if (_ColorId == ColorIds.UiText)
            {
                for (int i = 0; i < texts.Count; i++)
                {
                    if (m_AreTextsNotNull[i])
                        texts[i].color = _Color;
                }
            }
            else if (_ColorId == ColorIds.UiBackground)
            {
                if (m_IsBackgroundNotNull)
                    background.color = _Color;
            }
        }
        
        protected void SoundOnClick()
        {
            Managers.AudioManager.PlayClip(CommonAudioClipArgs.UiButtonClick);
        }
        
        #endregion

        #region engine methods

        private void Update()
        {
            if (UITicker.Pause)
                return;
            if (!Highlighted)
                return;
            const float amplitude = 1f;
            const float period = 1f;
            float lerpVal = MathUtils.TriangleWave(UITicker.Time, period, amplitude) + amplitude;
            border.color = Color.Lerp(m_BorderDefaultColor, m_BorderHighlightedColor, lerpVal);
        }

        protected virtual void OnDestroy()
        {
            if (ColorProvider != null)
                ColorProvider.ColorChanged -= OnColorChanged;
        }

        #endregion
    }
}