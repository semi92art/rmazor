using Zenject;

namespace Settings
{
    public interface ISettingsGetter
    {
        ISoundSetting SoundSetting { get; }
        IMusicSetting MusicSetting { get; }
        INotificationSetting NotificationSetting { get; }
        IHapticsSetting HapticsSetting { get; }
        ILanguageSetting LanguageSetting { get; }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        IDebugSetting DebugSetting { get; }
#endif
    }
    
    public class SettingsGetter : ISettingsGetter
    {
        public ISoundSetting SoundSetting { get; }
        public IMusicSetting MusicSetting { get; }
        public INotificationSetting NotificationSetting { get; }
        public IHapticsSetting HapticsSetting { get; }
        public ILanguageSetting LanguageSetting { get; }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [Inject] public IDebugSetting DebugSetting { get; }
#endif
        
        public SettingsGetter(
            ISoundSetting _SoundSetting,
            IMusicSetting _MusicSetting,
            INotificationSetting _NotificationSetting, 
            IHapticsSetting _HapticsSetting,
            ILanguageSetting _LanguageSetting)
        {
            SoundSetting = _SoundSetting;
            MusicSetting = _MusicSetting;
            NotificationSetting = _NotificationSetting;
            HapticsSetting = _HapticsSetting;
            LanguageSetting = _LanguageSetting;
        }
    }
}