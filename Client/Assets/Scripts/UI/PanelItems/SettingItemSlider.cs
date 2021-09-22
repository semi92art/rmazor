using TMPro;
using UnityEngine.UI;

namespace UI.PanelItems
{
    public class SettingItemSlider : MenuItemBase
    {
        public TextMeshProUGUI title;
        public Slider slider;
        public TextMeshProUGUI value;

        public void Init(string _Name, float _Min, float _Max, float _Value)
        {
            InitBase(_Name, _Min, _Max, _Value);
        }

        public void Init(string _Name, int _Min, int _Max, int _Value)
        {
            InitBase(_Name, _Min, _Max, _Value);
            slider.wholeNumbers = true;
        }

        private void InitBase(string _Name, float _Min, float _Max, float _Value)
        {
            title.name = _Name;
            name = $"{_Name} Setting";
            slider.minValue = _Min;
            slider.maxValue = _Max;
            slider.value = _Value;
            value.text = $"{_Value:F2}";
            slider.onValueChanged.AddListener(_Val =>
            {
                value.text = $"{_Val:F2}";
            });
        }
    }
}
