using mazing.common.Runtime.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Levels_Panel_Items
{
    public class LevelsPanelLevelItem : SimpleUiItem
    {
        #region serialized fields

        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private Button          button;
        [SerializeField] private Image           checkMarkIcon;
        [SerializeField] private Sprite 
            backgroundEnabledSprite, 
            backgroundDisabledSprite,
            backgroundSelectedSprite;

        #endregion

        #region api
        
        public void UpdateState(
            long        _LevelIndex,
            bool?       _Completed,
            bool        _IsCurrent,
            UnityAction _OnClick)
        {
            title.text = (_LevelIndex + 1).ToString("D");
            Activate(_Completed.HasValue);
            if (_Completed.HasValue)
                Complete(_Completed.Value, _IsCurrent);
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(_OnClick);
        }
        
        #endregion

        #region nonpublic methods
        
        private void Activate(bool _Active)
        {
            gameObject.SetActive(_Active);
        }
        
        private void Complete(bool _Completed, bool _IsCurrent)
        {
            button.interactable = _Completed || _IsCurrent;
            checkMarkIcon.enabled = _Completed;
            button.image.sprite = _Completed switch
            {
                true  => backgroundEnabledSprite,
                false => _IsCurrent ? backgroundSelectedSprite : backgroundDisabledSprite
            };
        }
        
        #endregion
    }
}