// ReSharper disable ClassNeverInstantiated.Global

using mazing.common.Runtime.Settings;

namespace RMAZOR.Settings
{
    public interface ISettingsGetter
    {
        ISoundSetting        SoundSetting        { get; }
        IMusicSetting        MusicSetting        { get; }
        INotificationSetting NotificationSetting { get; }
        IHapticsSetting      HapticsSetting      { get; }
        ILanguageSetting     LanguageSetting     { get; }
        IRetroModeSetting    RetroModeSetting    { get; }
        IDebugSetting        DebugSetting        { get; }
    }
    
    public class SettingsGetter : ISettingsGetter
    {
        public ISoundSetting        SoundSetting        { get; }
        public IMusicSetting        MusicSetting        { get; }
        public INotificationSetting NotificationSetting { get; }
        public IHapticsSetting      HapticsSetting      { get; }
        public ILanguageSetting     LanguageSetting     { get; }
        public IRetroModeSetting    RetroModeSetting    { get; }
        public IDebugSetting        DebugSetting        { get; }

        public SettingsGetter(
            ISoundSetting        _SoundSetting,
            IMusicSetting        _MusicSetting,
            INotificationSetting _NotificationSetting, 
            IHapticsSetting      _HapticsSetting,
            ILanguageSetting     _LanguageSetting,
            IRetroModeSetting    _RetroModeSetting,
            IDebugSetting        _DebugSetting)
        {
            SoundSetting        = _SoundSetting;
            MusicSetting        = _MusicSetting;
            NotificationSetting = _NotificationSetting;
            HapticsSetting      = _HapticsSetting;
            LanguageSetting     = _LanguageSetting;
            RetroModeSetting    = _RetroModeSetting;
            DebugSetting        = _DebugSetting;
        }
    }
}