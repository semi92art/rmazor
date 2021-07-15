using System.Collections.Generic;

namespace Settings
{
    public interface ISetting
    {
        string Name { get; }
        SettingType Type { get; }
        List<string> Values { get; }
        object Min { get; }
        object Max { get; }
        void Put(object _Parameter);
        object Get();
    }

    public enum SettingType
    {
        OnOff,
        InPanelSelector,
        Slider
    }
}