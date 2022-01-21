using System.Collections;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Enums;
using Common.Exceptions;
using Common.Extensions;
using Common.Ticker;
using Common.Utils;
using GameHelpers;
using RMAZOR;
using RMAZOR.Models;
using RMAZOR.Views;
using RMAZOR.Views.ContainerGetters;
using Settings;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using Utils;

namespace Managers.Audio
{

    public interface IAudioManager : IInit, IOnLevelStageChanged
    {
        void InitClip(AudioClipArgs _Args);
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

        private readonly AudioClipInfo[] m_ClipInfos = new AudioClipInfo[500];
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
            IViewGameTicker   _GameTicker,
            IUITicker         _UITicker,
            IMusicSetting     _MusicSetting,
            ISoundSetting     _SoundSetting,
            IPrefabSetManager _PrefabSetManager)
        {
            ContainersGetter = _ContainersGetter;
            GameTicker       = _GameTicker;
            UITicker         = _UITicker;
            MusicSetting     = _MusicSetting;
            SoundSetting     = _SoundSetting;
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
            EnableAudio(MusicSetting.Get(), EAudioClipType.Music, true);
            EnableAudio(SoundSetting.Get(), EAudioClipType.UiSound, true);
            EnableAudio(SoundSetting.Get(), EAudioClipType.GameSound, true);
            InitAudioMixer();
            Initialize?.Invoke();
            Initialized = true;
        }

        public void InitClip(AudioClipArgs _Args)
        {
            InitClip(_Args, false);
        }

        public void PlayClip(AudioClipArgs _Args)
        {
            InitClip(_Args, true);
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
                Cor.Run(AttenuateCoroutine(info, false));
            else
                info.Playing = false;
        }

        public void EnableAudio(bool _Enable, EAudioClipType _Type)
        {
            EnableAudio(_Enable, _Type, false);
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
            for (int i = 0; i < m_ClipInfos.Length; i++)
            {
                var info = m_ClipInfos[i];
                if (info == null)
                    continue;
                if (info.DestroyOnLevelChange)
                    m_ClipInfos[i] = null;
            }
        }

        #endregion
        
        #region nonpublic methods

        private void InitClip(AudioClipArgs _Args, bool _AndPlay)
        {
            var info = FindClipInfo(_Args);
            if (info != null)
            {
                if (_AndPlay)
                    PlayClipCore(_Args, info);
                return;
            }
            
            var clipEntity = PrefabSetManager.GetObjectEntity<AudioClip>("sounds", _Args.ClipName);
            Cor.Run(Cor.WaitWhile(
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
                    {
                        Dbg.LogWarning($"Audio Clip with name {_Args.ClipName} does not exist");
                        return;
                    }
                    var go = new GameObject($"AudioClip_{_Args.ClipName}");
                    go.SetParent(ContainersGetter.GetContainer(ContainerNames.AudioSources));
                    var audioSource = go.AddComponent<AudioSource>();
                    audioSource.clip = clip;
                    info = new AudioClipInfo(audioSource, _Args);
                    info.MixerGroup = m_MasterGroup;
                    AddInfo(info);
                    if (_AndPlay)
                        PlayClipCore(_Args, info);
                }));
        }
        
        private void InitAudioMixer()
        {
            m_Mixer = PrefabSetManager.GetObject<AudioMixer>("audio_mixers", "default_mixer");
            m_MasterGroup = m_Mixer.FindMatchingGroups("Master")[0];
            m_MutedGroup = m_Mixer.FindMatchingGroups("Master/Muted")[0];
        }
        
        private void EnableAudio(bool _Enable, EAudioClipType _Type, bool _OnStart)
        {
            for (int i = 0; i < m_ClipInfos.Length; i++)
            {
                var info = m_ClipInfos[i];
                if (info == null)
                    continue;
                if (info.Type != _Type)
                    continue;
                info.Volume = _Enable ? info.StartVolume : 0f;
            }
            if (!_OnStart)
                SaveUtils.PutValue(GetSaveKeyByType(_Type), _Enable);
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
                Cor.Run(AttenuateCoroutine(_Info, true));
        }
        
        private void MuteAudio(bool _Mute, EAudioClipType _Type)
        {
            for (int i = 0; i < m_ClipInfos.Length; i++)
            {
                var info = m_ClipInfos[i];
                if (info == null)
                    continue;
                if (info.Type != _Type)
                    continue;
                info.MixerGroup = _Mute ? m_MutedGroup : m_MasterGroup;
            }
        }

        private IEnumerator AttenuateCoroutine(AudioClipInfo _Info, bool _AttenuateUp)
        {
            float startVolume = _AttenuateUp ? 0f : _Info.Volume;
            float endVolume = !_AttenuateUp ? 0f : _Info.Volume;
            yield return Cor.Lerp(
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
            for (int i = 0; i < m_ClipInfos.Length; i++)
            {
                var info = m_ClipInfos[i];
                if (info == null)
                    continue;
                if (info.ClipName != _Args.ClipName)
                    continue;
                if (info.Type != _Args.Type)
                    continue;
                if (info.Id != _Args.Id)
                    continue;
                return info;
            }
            return null;
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

        private void AddInfo(AudioClipInfo _Info)
        {
            int i = 0;
            while (i < m_ClipInfos.Length)
            {
                i++;
                if (m_ClipInfos[i] != null)
                    continue;
                m_ClipInfos[i] = _Info;
                break;
            }
        }

        #endregion
    }
}
