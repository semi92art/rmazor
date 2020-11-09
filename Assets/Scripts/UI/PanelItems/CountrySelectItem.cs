using Extensions;
using LeTai.TrueShadow;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.PanelItems
{
    public class CountrySelectItem : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private Toggle toggle;

        private Image m_Background;
        private Image m_Container;
        private TrueShadow m_Shadow;
        
        private bool m_IsInitialized;

        private void Start()
        {
            m_Background = gameObject.GetCompItem<Image>("background");
            m_Container = gameObject.GetCompItem<Image>("container");
            m_Shadow = gameObject.GetCompItem<TrueShadow>("container");
        }
        
        public void Init(
            bool _Selected,
            string _Key, 
            System.Action<string> _Select, 
            ToggleGroup _ToggleGroup)
        {
            toggle.isOn = _Selected;
            title.text = _Key;
            icon.sprite = Countries.GetFlag(_Key);
            
            toggle.onValueChanged.AddListener(_IsOn =>
            {
                if (_IsOn && m_IsInitialized)
                    _Select?.Invoke(_Key);
            });

            toggle.group = _ToggleGroup;
            m_IsInitialized = true;
        }

        public void SetVisible(bool _Visible)
        {
            icon.gameObject.SetActive(_Visible);
            title.gameObject.SetActive(_Visible);
            m_Background.gameObject.SetActive(_Visible);
            toggle.enabled = _Visible;
            m_Container.enabled = _Visible;
            m_Shadow.enabled = _Visible;
        }
    }
}