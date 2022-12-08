using Common.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Levels_Panel_Items
{
    public class LevelPanelGroupItemLevelItem : MonoBehInitBase
    {
        public                   TextMeshProUGUI title;
        public                   Button          button;
        [SerializeField] private Image           star1, star2, star3;

        private Sprite m_StartDisabled, m_StarEnabled;

        public void Init(Sprite _StarEnabled, Sprite _StarDisabled)
        {
            m_StartDisabled = _StarDisabled;
            m_StarEnabled   = _StarEnabled;
        }
        
        public void SetStarsCount(int _Count)
        {
            star1.sprite = _Count > 0 ? m_StarEnabled : m_StartDisabled;
            star2.sprite = _Count > 1 ? m_StarEnabled : m_StartDisabled;
            star3.sprite = _Count > 2 ? m_StarEnabled : m_StartDisabled;
        }
    }
}