using System.Collections.Generic;
using System.Linq;
using Constants;
using DialogViewers;
using Entities;
using Helpers;
using Settings;
using UI;
using UI.PanelItems;
using UI.Panels;
using UnityEngine;
using Utils;

namespace Managers
{
    public abstract class SoundController : IGameObserver
    {
        #region nonpublic members
    
        private readonly Dictionary<GameObject,AudioSource> m_Clips
            = new Dictionary<GameObject, AudioSource>();

        #endregion

        #region api

        public abstract void OnNotify(object _Sender, int _NotifyId, params object[] _Args);
        
        public void SwitchSound(bool _IsOn)
        {
            SaveUtils.PutValue(SaveKey.SettingSoundOn, _IsOn);
            foreach (var clip in m_Clips
                .Where(_Clip => _Clip.Key != null))
                clip.Value.volume = _IsOn ? 1 : 0;
        }

        public void PlayClip(string _Name, bool _Cycling, float? _Volume = null)
        {
            AudioClip clip = PrefabInitializer.GetObject<AudioClip>("sounds", _Name);
            PlayClipCore(clip, _Cycling, _Volume);
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
            GameObject go = new GameObject($"AudioClip_{_Clip.name}");
            AudioSource audioSource = go.AddComponent<AudioSource>();
            audioSource.clip = _Clip;
            audioSource.volume = (_Volume ?? 1f) * (SaveUtils.GetValue<bool>(SaveKey.SettingSoundOn) ? 1 : 0);
            audioSource.loop = _Cycling;
            m_Clips.Add(go, audioSource);

            Coroutines.Run(Coroutines.WaitEndOfFrame(() =>
            {
                Coroutines.Run(Coroutines.WaitWhile(() =>
                {
                    m_Clips.Remove(go);
                    Object.Destroy(go);
                }, () => audioSource.isPlaying));
            }));
            audioSource.Play();
        }
        
        #endregion
    }

    public class UiSoundController : SoundController
    {
        public override void OnNotify(object _Sender, int _NotifyId, params object[] _Args)
        {
            switch (_Sender)
            {
                case MainMenuUi _:
                    OnNotifyInMainMenuUi(_NotifyId);
                    break;
                case SoundSetting _:
                    SwitchSound((bool)_Args[0]);
                    break;
                case MainMenuDialogViewer _:
                case LoginPanel _:
                case RegistrationPanel _:
                case PlusLifesPanel _:
                case PlusMoneyPanel _:
                    PlayUiButtonClick();
                    break;
                default:
                    if (_NotifyId == CommonNotifyIds.UiButtonClick)
                        PlayUiButtonClick();
                    break;
            }
        }

        private void OnNotifyInMainMenuUi(int _NotifyId)
        {
            switch (_NotifyId)
            {
                case MainMenuUi.NotifyIdMainMenuLoaded:
                    PlayClip("main_menu_theme", true, 0.1f);
                    break;
                case MainMenuUi.NotifyIdSelectGamePanelButtonClick:
                case MainMenuUi.NotifyIdProfileButtonClick:
                case MainMenuUi.NotifyIdSettingsButtonClick: 
                case MainMenuUi.NotifyIdLoginButtonClick:
                case MainMenuUi.NotifyIdShopButtonClick:
                case MainMenuUi.NotifyIdRatingsButtonClick: 
                case MainMenuUi.NotifyIdDailyBonusButtonClick: 
                case MainMenuUi.NotifyIdWheelOfFortuneButtonClick:
                    PlayUiButtonClick();
                    break;
                case MainMenuUi.NotifyIdPlayButtonClick:
                    StopPlayingClips();
                    break;
            }
        }
        
        private void PlayUiButtonClick()
        {
            PlayClip("ui_button_click", false);
        }
    }
}
