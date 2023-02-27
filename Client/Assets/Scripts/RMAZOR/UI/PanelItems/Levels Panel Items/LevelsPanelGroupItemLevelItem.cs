using System.Globalization;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Levels_Panel_Items
{
    public class LevelsPanelGroupItemLevelItem : SimpleUiItem
    {
        #region serialized fields

        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI bestTimeText;
        [SerializeField] private Button          button;
        [SerializeField] private Image           star1, star2, star3;

        #endregion

        #region nonpublic members

        private Sprite m_StartDisabled, m_StarEnabled;
        
        private readonly CultureInfo m_FloatValueCultureInfo = new CultureInfo("en-US");

        #endregion

        #region api

        public void Init(
            IUITicker            _UITicker,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager,
            Sprite               _StarEnabled,
            Sprite               _StarDisabled)
        {
            base.Init(_UITicker, _AudioManager, _LocalizationManager);
            m_StartDisabled = _StarDisabled;
            m_StarEnabled   = _StarEnabled;
        }
        
        public void SetTitle(string _Title)
        {
            title.font = LocalizationManager.GetFont(ETextType.MenuUI);
            title.text = _Title;
        }

        public void SetOnClickEvent(UnityAction _OnClick)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(_OnClick);
        }

        public void SetInteractable(bool _Interactable)
        {
            button.interactable = _Interactable;
        }

        public void SetButtonImageSprite(Sprite _Sprite)
        {
            button.image.sprite = _Sprite;
        }

        public void SetBestTimeText(float _BestTime)
        {
            bestTimeText.font = LocalizationManager.GetFont(ETextType.MenuUI);
            bestTimeText.text = float.IsInfinity(_BestTime) ?
                "-" : _BestTime.ToString("F3", m_FloatValueCultureInfo) + "s";
            bestTimeText.text = _BestTime.ToString("F3");
        }
        
        public void SetStarsCount(int _Count)
        {
            star1.sprite = _Count > 0 ? m_StarEnabled : m_StartDisabled;
            star2.sprite = _Count > 1 ? m_StarEnabled : m_StartDisabled;
            star3.sprite = _Count > 2 ? m_StarEnabled : m_StartDisabled;
        }

        #endregion
    }
}