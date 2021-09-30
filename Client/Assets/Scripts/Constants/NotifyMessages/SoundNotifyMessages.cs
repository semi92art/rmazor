namespace Constants.NotifyMessages
{
    public static class SoundNotifyMessages
    {
        /// <summary>
        /// When calling in OnNotify, args are:
        /// 1) name of audio clip (string),
        /// 2) cycling on/off (bool),
        /// 3) volume from 0 to 1 (float).
        /// </summary>
        public const string PlayAudioClip           = nameof(CommonNotifyMessages) + nameof(PlayAudioClip);
        public const string SwitchSoundSetting      = nameof(CommonNotifyMessages) + nameof(SwitchSoundSetting);
    }
}