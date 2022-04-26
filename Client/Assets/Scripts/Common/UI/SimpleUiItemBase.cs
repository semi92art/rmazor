using System.Collections.Generic;
using System.Linq;
using Common.Constants;
using Common.Extensions;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Common.UI
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

        private   IAudioManager        AudioManager        { get; set; }
        private   IUITicker            Ticker              { get; set; }
        protected IColorProvider       ColorProvider       { get; private set; }
        protected IPrefabSetManager    PrefabSetManager    { get; private set; }
        protected ILocalizationManager LocalizationManager { get; private set; }

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
            IUITicker            _UITicker,
            IColorProvider       _ColorProvider,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager,
            IPrefabSetManager    _PrefabSetManager,
            bool                _AutoFont = true)
        {
            Ticker              = _UITicker;
            ColorProvider       = _ColorProvider;
            AudioManager        = _AudioManager;
            LocalizationManager = _LocalizationManager;
            PrefabSetManager    = _PrefabSetManager;
            ColorProvider.ColorChanged += OnColorChanged;
            CheckIfSerializedItemsNotNull();
            SetColorsOnInit();
            foreach (var text in texts.Where(_Text => _Text.IsNotNull()))
                LocalizationManager.AddTextObject(new LocalizableTextObjectInfo(text, ETextType.MenuUI));
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
            if (m_IsBackgroundNotNull)
                background.color = ColorProvider.GetColor(ColorIds.UiBackground);
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
            switch (_ColorId)
            {
                case ColorIds.UiBorder when m_IsBorderNotNull:
                    border.color = _Color;
                    break;
                case ColorIds.UiBackground when m_IsBackgroundNotNull:
                    background.color = _Color;
                    break;
                case ColorIds.UiItemHighlighted:
                    m_BorderHighlightedColor = _Color;
                    break;
                case ColorIds.UI:
                {
                    for (int i = 0; i < images.Count; i++)
                        if (m_AreImagesNotNull[i])
                            images[i].color = _Color;
                    break;
                }
                case ColorIds.UiText:
                {
                    for (int i = 0; i < texts.Count; i++)
                        if (m_AreTextsNotNull[i])
                            texts[i].color = _Color;
                    break;
                }
            }
        }
        
        protected void SoundOnClick()
        {
            AudioManager.PlayClip(CommonAudioClipArgs.UiButtonClick);
        }
        
        #endregion

        #region engine methods

        private void Update()
        {
            if (Ticker.Pause)
                return;
            if (!Highlighted)
                return;
            const float amplitude = 1f;
            const float period = 1f;
            float lerpVal = MathUtils.TriangleWave(Ticker.Time, period, amplitude) + amplitude;
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