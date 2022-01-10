namespace Managers.Audio
{
    public class AudioClipArgs
    {
        public string         ClipName                 { get; }
        public EAudioClipType Type                     { get; }
        public string         Id                       { get; }
        public bool           Loop                     { get; set; }
        public float          StartVolume              { get; set; }
        public float          AttenuationSecondsOnPlay { get; set; }
        public float          AttenuationSecondsOnStop { get; set; }
        public bool           DestroyOnLevelChange     { get; set; }

        public AudioClipArgs(
            string _ClipName,
            EAudioClipType _Type,
            float _Volume = 0.5f, 
            bool _Loop = false, 
            string _Id = null,
            float _AttenuationSecondsOnPlay = 0f,
            float _AttenuationSecondsOnStop = 0f,
            bool _DestroyOnLevelChange = false)
        {
            ClipName = _ClipName;
            Type = _Type;
            StartVolume = _Volume;
            Loop = _Loop;
            Id = _Id;
            AttenuationSecondsOnPlay = _AttenuationSecondsOnPlay;
            AttenuationSecondsOnStop = _AttenuationSecondsOnStop;
            DestroyOnLevelChange = _DestroyOnLevelChange;
        }
    }
}