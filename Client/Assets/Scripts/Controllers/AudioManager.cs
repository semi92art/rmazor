using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DI.Extensions;
using Entities;
using Exceptions;
using GameHelpers;
using Games.RazorMaze.Views.ContainerGetters;
using Settings;
using Ticker;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Controllers
{
    public enum EAudioClipType
    {
        Sound,
        Music
    }
    
    public class AudioClipArgs
    {
        public         string         ClipName                 { get; }
        public         EAudioClipType Type                     { get; }
        public         string         Id                       { get; }
        public         bool           IsUiSound                { get; }
        public         bool           Loop                     { get; set; }
        public         float          StartVolume              { get; set; }
        public         float          AttenuationSecondsOnPlay { get; set; }
        public         float          AttenuationSecondsOnStop { get; set; }

        public AudioClipArgs(
            string _ClipName,
            EAudioClipType _Type,
            float _Volume = 1f, 
            bool _Loop = false, 
            string _Id = null,
            bool _IsUiSound = false,
            float _AttenuationSecondsOnPlay = 0f,
            float _AttenuationSecondsOnStop = 0f)
        {
            ClipName = _ClipName;
            Type = _Type;
            StartVolume = _Volume;
            Loop = _Loop;
            Id = _Id;
            IsUiSound = _IsUiSound;
            AttenuationSecondsOnPlay = _AttenuationSecondsOnPlay;
            AttenuationSecondsOnStop = _AttenuationSecondsOnStop;
        }
    }

    public class AudioClipInfo : AudioClipArgs
    {
        private AudioSource m_Source    { get; }

        private bool m_OnPause;
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

        public float Volume
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

        public AudioClipInfo(
            AudioSource _Source,
            AudioClipArgs _Args)
            : base(
                _Args.ClipName,
                _Args.Type, 
                _Args.StartVolume,
                _Args.Loop, 
                _Args.Id,
                _Args.IsUiSound,
                _Args.AttenuationSecondsOnPlay, 
                _Args.AttenuationSecondsOnStop)
        {
            m_Source = _Source;
            Loop = _Args.Loop;
        }
    }

    public interface IAudioManager : IInit
    {
        void PlayClip(AudioClipArgs _Args);
        void PauseClip(AudioClipArgs _Args);
        void UnPauseClip(AudioClipArgs _Args);
        void StopClip(AudioClipArgs _Args);
        void EnableAudio(bool _Enable, params EAudioClipType[] _Types);
    }
    
    public class AudioManager : IAudioManager
    {
        #region nonpublic members

        private readonly List<AudioClipInfo> m_ClipInfos = new List<AudioClipInfo>();
        
        #endregion
        
        #region inject
        
        private IContainersGetter ContainersGetter { get; }
        private IViewGameTicker       GameTicker       { get; }
        private IUITicker         UITicker         { get; }
        private IMusicSetting     MusicSetting     { get; }
        private ISoundSetting     SoundSetting     { get; }

        public AudioManager(
            IContainersGetter _ContainersGetter,
            IViewGameTicker _GameTicker,
            IUITicker _UITicker,
            IMusicSetting _MusicSetting,
            ISoundSetting _SoundSetting)
        {
            ContainersGetter = _ContainersGetter;
            GameTicker = _GameTicker;
            UITicker = _UITicker;
            MusicSetting = _MusicSetting;
            SoundSetting = _SoundSetting;

        }

        #endregion

        #region api
        
        public event UnityAction Initialized;
        public void Init()
        {
            GameTicker.Register(this);
            MusicSetting.OnValueSet = _MusicOn => EnableAudio(_MusicOn, EAudioClipType.Music);
            SoundSetting.OnValueSet = _MusicOn => EnableAudio(_MusicOn, EAudioClipType.Sound);
            EnableAudio(MusicSetting.Get(), EAudioClipType.Music);
            EnableAudio(SoundSetting.Get(), EAudioClipType.Sound);
            Initialized?.Invoke();
        }

        public void PlayClip(AudioClipArgs _Args)
        {
            var info = FindClipInfo(_Args);
            if (info == null)
            {
                var clip = PrefabUtilsEx.GetObject<AudioClip>("sounds", _Args.ClipName);
                if (clip == null)
                    return;
                var go = new GameObject($"AudioClip_{_Args.ClipName}");
                go.SetParent(ContainersGetter.GetContainer(ContainerNames.AudioSources));
                var audioSource = go.AddComponent<AudioSource>();
                audioSource.clip = clip;
                info = new AudioClipInfo(audioSource, _Args);
                m_ClipInfos.Add(info);
            }

            info.StartVolume = _Args.StartVolume;
            switch (info.Type)
            {
                case EAudioClipType.Sound:
                    info.Volume = SaveUtils.GetValue<bool>(SaveKey.SettingSoundOn) ? info.StartVolume : 0f;
                    break;
                case EAudioClipType.Music:
                    info.Volume = SaveUtils.GetValue<bool>(SaveKey.SettingMusicOn) ? info.StartVolume : 0f;
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(info.Type);
            }
            
            if (info.OnPause)
                info.OnPause = false;
            else
                info.Playing = true;
            if (info.AttenuationSecondsOnPlay > float.Epsilon)
                Coroutines.Run(AttenuateCoroutine(info, true));
            else
                info.Volume = info.StartVolume;
        }

        public void PauseClip(AudioClipArgs _Args)
        {
            var info = FindClipInfo(_Args);
            if (info != null)
                info.OnPause = true;
        }
        
        public void UnPauseClip(AudioClipArgs _Args)
        {
            var info = FindClipInfo(_Args);
            if (info != null)
                info.OnPause = false;
        }

        public void StopClip(AudioClipArgs _Args)
        {
            var info = FindClipInfo(_Args);
            if (info == null)
                return;
            info.OnPause = false;
            if (info.AttenuationSecondsOnStop > float.Epsilon)
                Coroutines.Run(AttenuateCoroutine(info, false));
            else
                info.Playing = false;
        }

        public void EnableAudio(bool _Enable, params EAudioClipType[] _Types)
        {
            if (_Types == null)
                return;
            var infos = !_Types.Any()
                ? m_ClipInfos : m_ClipInfos.Where(_Info => _Types.Contains(_Info.Type));
            foreach (var info in infos)
                info.Volume = _Enable ? info.StartVolume : 0f;
            if (_Types.Contains(EAudioClipType.Sound))
                SaveUtils.PutValue(SaveKey.SettingSoundOn, _Enable);
            if (_Types.Contains(EAudioClipType.Music))
                SaveUtils.PutValue(SaveKey.SettingMusicOn, _Enable);
        }
        
        #endregion
        
        #region nonpublic methods

        private IEnumerator AttenuateCoroutine(AudioClipInfo _Info, bool _AttenuateUp)
        {
            float startVolume = _AttenuateUp ? 0f : _Info.Volume;
            float endVolume = !_AttenuateUp ? 0f : _Info.Volume;
            yield return Coroutines.Lerp(
                startVolume,
                endVolume,
                _AttenuateUp ? _Info.AttenuationSecondsOnPlay : _Info.AttenuationSecondsOnStop,
                _Volume => _Info.Volume = _Volume,
                _Info.IsUiSound ? (ITicker)UITicker : GameTicker,
                (_, __) =>
                {
                    if (!_AttenuateUp)
                        _Info.Playing = false;
                });
        }

        private AudioClipInfo FindClipInfo(AudioClipArgs _Args)
        {
            if (_Args == null)
                return null;
            return m_ClipInfos.FirstOrDefault(
                _Info =>
                {
                    if (_Info.ClipName != _Args.ClipName)
                        return false;
                    if (_Info.Type != _Args.Type)
                        return false;
                    if (_Info.Id != _Args.Id)
                        return false;
                    return true;
                });
        }

        #endregion
    }
}
