using System.Collections.Generic;
using System.Linq;
using Constants.NotifyMessages;
using Entities;
using GameHelpers;
using UnityEngine;
using Utils;

namespace Controllers
{
    public interface ISoundGameObserver : IGameObserver { }
    
    public class SoundGameObserver : ISoundGameObserver
    {
        #region nonpublic members
    
        private readonly Dictionary<GameObject,AudioSource> m_Clips
            = new Dictionary<GameObject, AudioSource>();

        #endregion

        #region api

        public void OnNotify(string _NotifyMessage, params object[] _Args)
        {
            int argsCount = _Args.Length;
            switch (_NotifyMessage)
            {
                // case SoundNotifyMessages.PlayMainMenuTheme:
                //     PlayClip("main_menu_theme", true, 0.1f);
                //     break;
                case SoundNotifyMessages.PlayAudioClip:
                    if (argsCount == 0)
                        break;
                    PlayClip(
                        (string)_Args[0],
                        argsCount > 1 && (bool)_Args[1],
                        argsCount > 2 ? (float?)_Args[2] : null);
                    break;
                case SoundNotifyMessages.SwitchSoundSetting:
                    EnableSound((bool) _Args[0]);
                    break;
            }
        }

        #endregion
        
        #region nonpublic methods
        
        private void PlayClip(string _Name, bool _Cycling = false, float? _Volume = null)
        {
            var clip = PrefabUtilsEx.GetObject<AudioClip>("sounds", _Name);
            PlayClipCore(clip, _Cycling, _Volume);
        }
        
        private void EnableSound(bool _Enable)
        {
            SaveUtils.PutValue(SaveKey.SettingSoundOn, _Enable);
            foreach (var clip in m_Clips
                .Where(_Clip => _Clip.Key != null))
                clip.Value.volume = _Enable ? 1 : 0;
        }
        
        private void StopPlayingClips()
        {
            foreach (var clip in m_Clips.Values.ToArray())
                clip.Stop();
        }

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
