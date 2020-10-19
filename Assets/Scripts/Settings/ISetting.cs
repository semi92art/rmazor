namespace Settings
{
    public interface ISetting
    {
        SettingType Type { get; }
        void Set(object _Parameter);
    }

    public enum SettingType
    {
        OnOff,
        DropSelection,
        Slider
    }
}