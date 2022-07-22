using Common.Extensions;
using UnityEngine;
using UnityEngine.Audio;

namespace Common.Entities
{
    public class AudioClipInfo : AudioClipArgs
    {
        private readonly AudioSource m_Source;

        private bool m_OnPause;
        
        public AudioClipInfo(
            AudioSource   _Source,
            AudioClipArgs _Args)
            : base(
                _Args.ClipName,
                _Args.Type, 
                _Args.StartVolume,
                _Args.Loop, 
                _Args.Id,
                _Args.AttenuationSecondsOnPlay, 
                _Args.AttenuationSecondsOnStop)
        {
            m_Source = _Source;
            Loop = _Args.Loop;
        }
        
        public bool OnPause
        {
            get => m_OnPause;
            set
            {
                if (value)
                    m_Source.Pause();
                else m_Source.UnPause();
                m_OnPause = value;
            }
        }

        public float SourceVolume
        {
            get => m_Source.volume;
            set => m_Source.volume = value;
        }

        public new bool Loop
        {
            get => m_Source.loop;
            set => m_Source.loop = value;
        }

        public bool Playing
        {
            get => m_Source.isPlaying;
            set
            {
                if (value)
                    m_Source.Play();
                else m_Source.Stop();
            }
        }

        public AudioMixerGroup MixerGroup
        {
            set => m_Source.outputAudioMixerGroup = value;
        }
        
        public void DestroySource()
        {
            m_Source.gameObject.DestroySafe();
        }
    }

}