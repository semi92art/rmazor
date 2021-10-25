using Zenject;

namespace Settings
{
    public interface ISettingsGetter
    {
        ISoundSetting SoundSetting { get; }
        IMusicSetting MusicSetting { get; }
        INotificationSetting NotificationSetting { get; }
        IVibrationSetting VibrationSetting { get; }
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
        public IVibrationSetting VibrationSetting { get; }
        public ILanguageSetting LanguageSetting { get; }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [Inject] public IDebugSetting DebugSetting { get; }
#endif
        
        public SettingsGetter(
            ISoundSetting _SoundSetting,
            IMusicSetting _MusicSetting,
            INotificationSetting _NotificationSetting, 
            IVibrationSetting _VibrationSetting,
            ILanguageSetting _LanguageSetting)
        {
            SoundSetting = _SoundSetting;
            MusicSetting = _MusicSetting;
            NotificationSetting = _NotificationSetting;
            VibrationSetting = _VibrationSetting;
            LanguageSetting = _LanguageSetting;
        }
    }
}