using System.Collections.Generic;
using System.Linq;
using Entities;
using GameHelpers;
using UnityEngine;
using Utils;

namespace Controllers
{
    public interface ISoundManager
    {
        void PlayClip(string _Name, bool _Cycling = false, float? _Volume = null);
        void EnableSound(bool _Enable);
        void StopPlayingClips();
    }
    
    public class SoundManager : ISoundManager
    {
        #region nonpublic members
    
        private readonly Dictionary<GameObject,AudioSource> m_Clips
            = new Dictionary<GameObject, AudioSource>();

        #endregion

        #region api
        
        public void PlayClip(string _Name, bool _Cycling = false, float? _Volume = null)
        {
            var clip = PrefabUtilsEx.GetObject<AudioClip>("sounds", _Name);
            PlayClipCore(clip, _Cycling, _Volume);
        }
        
        public void EnableSound(bool _Enable)
        {
            SaveUtils.PutValue(SaveKey.SettingSoundOn, _Enable);
            foreach (var clip in m_Clips
                .Where(_Clip => _Clip.Key != null))
                clip.Value.volume = _Enable ? 1 : 0;
        }
        
        public void StopPlayingClips()
        {
            foreach (var clip in m_Clips.Values.ToArray())
                clip.Stop();
        }

        #endregion
        
        #region nonpublic methods

        private void PlayClipCore(AudioClip _Clip, bool _Cycling, float? _Volume = null)
        {
            var go = new GameObject($"AudioClip_{_Clip.name}");
            var audioSource = go.AddComponent<AudioSource>();
            audioSource.clip = _Clip;
            audioSource.volume = (_Volume ?? 1f) * (SaveUtils.GetValue<bool>(SaveKey.SettingSoundOn) ? 1 : 0);
            audioSource.loop = _Cycling;
            m_Clips.Add(go, audioSource);

            Coroutines.Run(Coroutines.WaitEndOfFrame(() =>
            {
                Coroutines.Run(Coroutines.WaitWhile(
                () => audioSource.isPlaying,
                () =>
                {
                    m_Clips.Remove(go);
                    Object.Destroy(go);
                }));
            }));
            audioSource.Play();
        }
        
        #endregion
    }
}
