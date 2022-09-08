using System.Collections.Generic;
using System.Linq;
using Common.Constants;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Ticker;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Common.UI
{
    public abstract class SimpleUiItemBase : MonoBehInitBase
    {
        #region serialized fields

        [SerializeField] protected Image               background;
        [SerializeField] private List<TextMeshProUGUI> texts;

        #endregion
        
        #region nonpublic members

        private   IAudioManager        AudioManager        { get; set; }
        protected IUITicker            Ticker              { get; set; }
        protected ILocalizationManager LocalizationManager { get; private set; }

        private bool   m_IsBackgroundNotNull;
        private bool[] m_AreTextsNotNull;
        private Color  m_BorderDefaultColor;
        private Color  m_BorderHighlightedColor;

        #endregion
        
        #region api
        
        public bool Highlighted { get; set; }
        
        public virtual void Init(
            IUITicker            _UITicker,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager)
        {
            Ticker              = _UITicker;
            AudioManager        = _AudioManager;
            LocalizationManager = _LocalizationManager;
            
            Ticker.Register(this);
            CheckIfSerializedItemsNotNull();
            foreach (var text in texts.Where(_Text => _Text.IsNotNull()))
                LocalizationManager.AddTextObject(new LocalizableTextObjectInfo(text, ETextType.MenuUI));
            base.Init();
        }
        
        #endregion
        
        #region nonpublic methods

        protected virtual void CheckIfSerializedItemsNotNull()
        {
            m_IsBackgroundNotNull = background.IsNotNull();
            m_AreTextsNotNull = texts.Select(_T => _T.IsNotNull()).ToArray();
        }

        
        protected void SoundOnClick()
        {
            AudioManager.PlayClip(CommonAudioClipArgs.UiButtonClick);
        }
        
        #endregion
    }
}