using System;
using Common;
using Common.Helpers;
using UnityEngine;

public class TestAnythingMonoBeh : MonoBehInitBase
{
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        // Application.wantsToQuit += OnApplicationWantsToQuit;
        // Application.quitting += OnApplicationQuitting;
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        Dbg.Log(nameof(OnApplicationFocus) + " " + hasFocus);
    }
    
    // private void OnApplicationPause(bool pauseStatus)
    // {
    //     Dbg.Log(nameof(OnApplicationPause) + " " + pauseStatus);
    // }
    
    // private void OnApplicationQuit()
    // {
    //     Dbg.Log(nameof(OnApplicationQuit));
    // }
    //
    // private void OnDestroy()
    // {
    //     Dbg.Log(nameof(OnDestroy));
    // }
    //
    // private void OnApplicationQuitting()
    // {
    //     throw new NotImplementedException();
    // }
    //
    // private bool OnApplicationWantsToQuit()
    // {
    //     Dbg.Log(nameof(OnApplicationWantsToQuit));
    //     return true;
    // }
}