using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DI.Extensions;
using Entities;
using Exceptions;
using GameHelpers;
using Games.RazorMaze.Views;
using Games.RazorMaze.Views.ContainerGetters;
using Settings;
using Ticker;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using Utils;

namespace Controllers
{
    public enum EAudioClipType
    {
        GameSound,
        UiSound,
        Music
    }
    
    public interface IAudioManager : IInit, IOnLevelStageChanged
    {
        void PlayClip(AudioClipArgs _Args);
        void PauseClip(AudioClipArgs _Args);
        void UnPauseClip(AudioClipArgs _Args);
        void StopClip(AudioClipArgs _Args);
        void EnableAudio(bool _Enable, EAudioClipType _Type);
        void MuteAudio(EAudioClipType _Type);
        void UnmuteAudio(EAudioClipType _Type);
    }
    
    public class AudioManager : IAudioManager
    {
        #region nonpublic members

        private readonly List<AudioClipInfo> m_ClipInfos = new List<AudioClipInfo>();
        private          AudioMixer          m_Mixer;
        private          AudioMixerGroup     m_MasterGroup;
        private          AudioMixerGroup     m_MutedGroup;
        
        #endregion
        
        #region inject
        
        private IContainersGetter ContainersGetter { get; }
        private IViewGameTicker   GameTicker       { get; }
        private IUITicker         UITicker         { get; }
        private IMusicSetting     MusicSetting     { get; }
        private ISoundSetting     SoundSetting     { get; }
        private IPrefabSetManager PrefabSetManager { get; }

        public AudioManager(
            IContainersGetter _ContainersGetter,
            IViewGameTicker _GameTicker,
            IUITicker _UITicker,
            IMusicSetting _MusicSetting,
            ISoundSetting _SoundSetting,
            IPrefabSetManager _PrefabSetManager)
        {
            ContainersGetter = _ContainersGetter;
            GameTicker = _GameTicker;
            UITicker = _UITicker;
            MusicSetting = _MusicSetting;
            SoundSetting = _SoundSetting;
            PrefabSetManager = _PrefabSetManager;
        }

        #endregion

        #region api

        public bool              Initialized { get; private set; }
        public event UnityAction Initialize;
        public void Init()
        {
            GameTicker.Register(this);
            MusicSetting.OnValueSet = _MusicOn => EnableAudio(_MusicOn, EAudioClipType.Music);
            SoundSetting.OnValueSet = _MusicOn =>
            {
                EnableAudio(_MusicOn, EAudioClipType.UiSound);
                EnableAudio(_MusicOn, EAudioClipType.GameSound);
            };
            EnableAudio(MusicSetting.Get(), EAudioClipType.Music);
            EnableAudio(SoundSetting.Get(), EAudioClipType.UiSound);
            EnableAudio(SoundSetting.Get(), EAudioClipType.GameSound);
            InitAudioMixer();
            Initialize?.Invoke();
            Initialized = true;
        }

        public void PlayClip(AudioClipArgs _Args)
        {
            var info = FindClipInfo(_Args);
            if (info != null)
            {
                PlayClipCore(_Args, info);
                return;
            }
            
            var clipEntity = PrefabSetManager.GetObjectEntity<AudioClip>("sounds", _Args.ClipName);
            Coroutines.Run(Coroutines.WaitWhile(
                () => clipEntity.Result == EEntityResult.Pending,
                () =>
                {
                    if (clipEntity.Result == EEntityResult.Fail)
                    {
                        Dbg.LogWarning($"Sound clip with name {_Args.ClipName} not found in prefab sets");
                        return;
                    }
                    var clip = clipEntity.Value;
                    if (clip == null)
                        return;
                    var go = new GameObject($"AudioClip_{_Args.ClipName}");
                    go.SetParent(ContainersGetter.GetContainer(ContainerNames.AudioSources));
                    var audioSource = go.AddComponent<AudioSource>();
                    audioSource.clip = clip;
                    info = new AudioClipInfo(audioSource, _Args);
                    m_ClipInfos.Add(info);
                    PlayClipCore(_Args, info);
                }));
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

        public void EnableAudio(bool _Enable, EAudioClipType _Type)
        {
            var infos = m_ClipInfos.Where(_Info => _Type == _Info.Type);
            foreach (var info in infos)
                info.Volume = _Enable ? info.StartVolume : 0f;
            SaveUtils.PutValue(GetSaveKeyByType(_Type), _Enable);
        }

        public void MuteAudio(EAudioClipType _Type)
        {
            MuteAudio(true, _Type);
        }

        public void UnmuteAudio(EAudioClipType _Type)
        {
            MuteAudio(false, _Type);
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage != ELevelStage.Loaded) 
                return;
            m_ClipInfos
                .Where(_Info => !string.IsNullOrEmpty(_Info.Id))
                .ToList()
                .ForEach(_Info =>
                {
                    _Info.DestroySource();
                    m_ClipInfos.Remove(_Info);
                });
        }

        #endregion
        
        #region nonpublic methods

        private void InitAudioMixer()
        {
            m_Mixer = PrefabSetManager.GetObject<AudioMixer>("audio_mixers", "default_mixer");
            m_MasterGroup = m_Mixer.FindMatchingGroups("Master")[0];
            m_MutedGroup = m_Mixer.FindMatchingGroups("Master/Muted")[0];
        }

        private void PlayClipCore(AudioClipArgs _Args, AudioClipInfo _Info)
        {
            _Info.StartVolume = _Args.StartVolume;
            _Info.Volume = SaveUtils.GetValue(GetSaveKeyByType(_Info.Type)) ? _Info.StartVolume : 0f;
            if (_Info.OnPause)
                _Info.OnPause = false;
            else
                _Info.Playing = true;
            if (_Info.AttenuationSecondsOnPlay > float.Epsilon)
                Coroutines.Run(AttenuateCoroutine(_Info, true));
        }
        
        private void MuteAudio(bool _Mute, EAudioClipType _Type)
        {
            var infos = m_ClipInfos.Where(_Info => _Type == _Info.Type);
            foreach (var info in infos)
                info.MixerGroup = _Mute ? m_MutedGroup : m_MasterGroup;
        }

        private IEnumerator AttenuateCoroutine(AudioClipInfo _Info, bool _AttenuateUp)
        {
            float startVolume = _AttenuateUp ? 0f : _Info.Volume;
            float endVolume = !_AttenuateUp ? 0f : _Info.Volume;
            yield return Coroutines.Lerp(
                startVolume,
                endVolume,
                _AttenuateUp ? _Info.AttenuationSecondsOnPlay : _Info.AttenuationSecondsOnStop,
                _Volume => _Info.Volume = _Volume,
                _Info.Type == EAudioClipType.UiSound ? (ITicker)UITicker : GameTicker,
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

        private static SaveKey<bool> GetSaveKeyByType(EAudioClipType _Type)
        {
            switch (_Type)
            {
                case EAudioClipType.GameSound:
                    return SaveKeys.SettingSoundOn;
                case EAudioClipType.UiSound:
                    return SaveKeys.SettingSoundOn;
                case EAudioClipType.Music:
                    return SaveKeys.SettingMusicOn;
                default:
                    throw new SwitchCaseNotImplementedException(_Type);
            }
        }

        #endregion
    }
}
