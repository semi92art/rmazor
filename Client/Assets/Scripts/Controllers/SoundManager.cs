using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DI.Extensions;
using Entities;
using GameHelpers;
using Games.RazorMaze.Views.ContainerGetters;
using Ticker;
using UnityEngine;
using Utils;

namespace Controllers
{
    public class AudioClipInfo
    {
        public string ClipName { get; }
        public AudioClip AudioClip { get; }
        public AudioSource Source { get; }
        public float Volume { get; set; }
        public bool OnPause { get; set; }
        public IEnumerable<string> Tags { get; }

        public AudioClipInfo(
            string _ClipName, 
            AudioClip _AudioClip, 
            AudioSource _Source, 
            float _Volume, 
            IEnumerable<string> _Tags)
        {
            ClipName = _ClipName;
            AudioClip = _AudioClip;
            Source = _Source;
            Volume = _Volume;
            Tags = _Tags;
        }
    }
    
    public interface ISoundManager
    {
        void PlayClip(
            string _ClipName,
            bool _Loop = false,
            float _Volume = 1f,
            float _AttenuationSecs = 0f,
            params string[] _Tags);
        void PauseClip(string _ClipName, params string[] _Tags);
        void UnPauseClip(string _ClipName, params string[] _Tags);
        void StopClip(string _ClipName, float _AttenuationSecs = 0f, params string[] _Tags);
        void EnableSound(bool _Enable, params string[] _Tags);
    }
    
    public class SoundManager : ISoundManager, IUpdateTick
    {
        #region nonpublic members

        private readonly List<AudioClipInfo> m_ClipInfos = new List<AudioClipInfo>();
        
        #endregion
        
        #region inject
        
        private IContainersGetter ContainersGetter { get; }
        private IGameTicker GameTicker { get; }
        private IUITicker UITicker { get; }

        public SoundManager(
            IContainersGetter _ContainersGetter,
            IGameTicker _GameTicker,
            IUITicker _UITicker)
        {
            ContainersGetter = _ContainersGetter;
            GameTicker = _GameTicker;
            UITicker = _UITicker;
            
            GameTicker.Register(this);
        }

        #endregion

        #region api

        public void PlayClip(
            string _ClipName, 
            bool _Loop = false,
            float _Volume = 1f,
            float _AttenuationSecs = 0f,
            params string[] _Tags)
        {
            var clipInfo = GetInfo(_ClipName, _Tags);
            if (clipInfo == null)
            {
                var clip = PrefabUtilsEx.GetObject<AudioClip>("sounds", _ClipName);
                if (clip == null)
                    return;
                var go = new GameObject($"AudioClip_{_ClipName}");
                go.SetParent(ContainersGetter.GetContainer(ContainerNames.AudioSources));
                var audioSource = go.AddComponent<AudioSource>();
                audioSource.clip = clip;
                audioSource.volume = _Volume * (SaveUtils.GetValue<bool>(SaveKey.SettingSoundOn) ? 1 : 0);
                audioSource.loop = _Loop;
                clipInfo = new AudioClipInfo(_ClipName, clip, audioSource, _Volume, _Tags);
                m_ClipInfos.Add(clipInfo);
            }

            clipInfo.Source.Play();
            if (_AttenuationSecs > float.Epsilon)
                Coroutines.Run(AttenuateCoroutine(clipInfo, _AttenuationSecs, true));
            else
                clipInfo.Source.volume = _Volume;
        }

        public void PauseClip(string _ClipName, params string[] _Tags)
        {
            PauseClipCore(_ClipName, _Tags, true);
        }
        
        public void UnPauseClip(string _ClipName, params string[] _Tags)
        {
            PauseClipCore(_ClipName, _Tags, false);
        }

        public void StopClip(string _ClipName, float _AttenuationSecs = 0, params string[] _Tags)
        {
            var clipInfo = GetInfo(_ClipName, _Tags);
            if (clipInfo == null)
                return;
            clipInfo.OnPause = false;
            if (_AttenuationSecs > float.Epsilon)
                Coroutines.Run(AttenuateCoroutine(clipInfo, _AttenuationSecs, false));
            else
                clipInfo.Source.Stop();
        }

        public void EnableSound(bool _Enable, params string[] _Tags)
        {
            foreach (var info in m_ClipInfos.Where(_Info => !_Info.Source.isPlaying))
            {
                if (_Tags != null && _Tags.Any() && info.Tags.Any(_Tags.Contains))
                    info.Source.volume = _Enable ? info.Volume : 0f;
            }
            if (_Tags == null || !_Tags.Any())
                SaveUtils.PutValue(SaveKey.SettingSoundOn, _Enable);
        }
        
        #endregion
        
        #region nonpublic methods

        private IEnumerator AttenuateCoroutine(AudioClipInfo _Info, float _Seconds, bool _AttenuateUp)
        {
            float startVolume = _AttenuateUp ? 0f : _Info.Volume;
            float endVolume = !_AttenuateUp ? 0f : _Info.Volume;
            yield return Coroutines.Lerp(
                startVolume,
                endVolume,
                _Seconds,
                _Volume => _Info.Source.volume = _Volume,
                _Info.Tags.Contains("ui") ? (ITicker)UITicker : GameTicker,
                (_, __) =>
                {
                    if (!_AttenuateUp) 
                        _Info.Source.Stop();
                });
        }

        private AudioClipInfo GetInfo(string _ClipName, string[] _Tags, bool _FullMatch = false)
        {
            if (_FullMatch)
                return m_ClipInfos.FirstOrDefault(
                    _Info => _Info.ClipName == _ClipName && _Info.Tags.All(_Tags.Contains));
            return m_ClipInfos.FirstOrDefault(
                _Info => _Info.ClipName == _ClipName && _Info.Tags.Any(_Tags.Contains));
        }
        
        private void PauseClipCore(string _ClipName, string[] _Tags, bool _Pause)
        {
            var clipInfo = GetInfo(_ClipName, _Tags);
            if (clipInfo == null)
                return;
            if (_Pause)
                clipInfo.Source.Pause();
            else 
                clipInfo.Source.UnPause();
            clipInfo.OnPause = _Pause;
        }

        #endregion

        public void UpdateTick()
        {
            // TODO
        }
    }
}
