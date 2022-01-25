namespace Settings
{
    public interface ISettingsGetter
    {
        ISoundSetting SoundSetting { get; }
        IMusicSetting MusicSetting { get; }
        INotificationSetting NotificationSetting { get; }
        IHapticsSetting HapticsSetting { get; }
        ILanguageSetting LanguageSetting { get; }
        IDebugSetting DebugSetting { get; }
    }
    
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SettingsGetter : ISettingsGetter
    {
        public ISoundSetting SoundSetting { get; }
        public IMusicSetting MusicSetting { get; }
        public INotificationSetting NotificationSetting { get; }
        public IHapticsSetting HapticsSetting { get; }
        public ILanguageSetting LanguageSetting { get; }
        public IDebugSetting DebugSetting { get; }
        
        public SettingsGetter(
            ISoundSetting        _SoundSetting,
            IMusicSetting        _MusicSetting,
            INotificationSetting _NotificationSetting, 
            IHapticsSetting      _HapticsSetting,
            ILanguageSetting     _LanguageSetting,
            IDebugSetting        _DebugSetting)
        {
            SoundSetting = _SoundSetting;
            MusicSetting = _MusicSetting;
            NotificationSetting = _NotificationSetting;
            HapticsSetting = _HapticsSetting;
            LanguageSetting = _LanguageSetting;
            DebugSetting = _DebugSetting;
        }
    }
}