using System;
using System.Collections;
using System.Collections.Generic;
using Shapes;
using UICreationSystem;
using UnityEngine;
using Utils;

public class SoundManager : MonoBehaviour, ISingleton
{
    // Managing sound settings
    // Singleton
    
    #region private members
    private static SoundManager _instance;
    private Dictionary<GameObject,AudioSource> m_clipDictionary = new Dictionary<GameObject, AudioSource>();
    private Dictionary<GameObject, AudioSource> m_tempClipDictionary= new Dictionary<GameObject, AudioSource>();

    void Update()
    {
        //TODO gameobject comparation not in update
        CheckPlayingClips();
    }

    private void CheckPlayingClips()
    {
        foreach (var clip in m_tempClipDictionary)
        {
            if (clip.Key != null)
            {
                if (!clip.Value.isPlaying)
                {
                    Destroy(clip.Key);
                    m_clipDictionary.Remove(clip.Key);
                }
            }
            else
            {
                m_clipDictionary.Remove(clip.Key);
            }
        }
        
        m_tempClipDictionary.Clear();
        foreach (var clip in m_clipDictionary)
        {
            m_tempClipDictionary.Add(clip.Key,clip.Value);
        }
    }
    #endregion

    #region public methods
    //instance method
    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = new GameObject("Sound Manager");
                _instance = obj.AddComponent<SoundManager>();
            }
            return _instance;
        }
    }
    
    //functional methods

    public void SwitchSound(bool _IsOn)
    {
        SaveUtils.PutValue(SaveKey.SettingSoundOn, _IsOn);
    }

    public void PlayClip(String _ClipName, bool _Cycling)
    {
        AudioClip clip = PrefabInitializer.GetObject<AudioClip>("sounds",_ClipName);
        GameObject soundClip = new GameObject("Clip_"+clip.name);
        AudioSource soundClipAudioSource = soundClip.AddComponent<AudioSource>();
        soundClipAudioSource.clip = clip;
        if (SaveUtils.GetValue<bool>(SaveKey.SettingSoundOn))
        {
            soundClipAudioSource.volume = 1f;
        }
        else
        {
            soundClipAudioSource.volume = 0f;
        }
        soundClipAudioSource.loop = _Cycling;
        m_clipDictionary.Add(soundClip,soundClipAudioSource);
        soundClipAudioSource.Play();
        if (!_Cycling && !soundClipAudioSource.isPlaying)
        {
            Destroy(soundClip);
        }
    }

    public void SwitchSoundInActualClips(bool _IsOn)
    {
        foreach (var clip in m_clipDictionary)
        {
            if (clip.Key != null)
            {
                clip.Value.volume = _IsOn ? 1 : 0;
            }
        }
    } 

    #endregion
    
    
    
}
