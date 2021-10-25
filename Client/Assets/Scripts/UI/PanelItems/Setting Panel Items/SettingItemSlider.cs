using TMPro;
using UnityEngine.UI;

namespace UI.PanelItems.Setting_Panel_Items
{
    public class SettingItemSlider : SimpleUiDialogItemView
    {
        public TextMeshProUGUI title;
        public Slider slider;
        public TextMeshProUGUI value;

        public void Init(string _Name, float _Min, float _Max, float _Value, bool _WholeNumbers)
        {
            InitBase(_Name, _Min, _Max, _Value);
            slider.wholeNumbers = _WholeNumbers;
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
