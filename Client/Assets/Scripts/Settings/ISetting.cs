using System.Collections.Generic;
using Entities;

namespace Settings
{
    public interface ISetting<T>
    {
        SaveKey Key { get; }
        string TitleKey { get; }
        ESettingLocation Location { get; }
        ESettingType Type { get; }
        List<T> Values { get; }
        object Min { get; }
        object Max { get; }
        string SpriteOffKey { get; }
        string SpriteOnKey { get; }
        T Get();
        void Put(T _VolumeOn);
    }

    public enum ESettingType
    {
        OnOff,
        InPanelSelector,
        Slider
    }

    public enum ESettingLocation
    {
        Main,
        MiniButtons
    }
}