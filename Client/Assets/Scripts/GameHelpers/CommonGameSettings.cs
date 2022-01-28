using System;
using System.Collections.Generic;
using Common;
using Common.Entities;
using UnityEngine;

namespace GameHelpers
{
    [Flags]
    public enum EAdsProvider
    {
        AdMob = 1,
        UnityAds = 2
    }
    
    [CreateAssetMenu(fileName = "game_settings", menuName = "Configs and Sets/Game Settings", order = 1)]
    public class CommonGameSettings : ScriptableObject
    {
        [SerializeField] private EAdsProvider adsProvider;
        [SerializeField] private float        adsLoadDelay;
        [SerializeField] private bool         debugEnabled;
        [SerializeField] private bool         testAds;
        [SerializeField] private ELogLevel    logLevel;
        public                   float        admobRate;
        public                   float        unityAdsRate;
        
        public EAdsProvider AdsProvider
        {
            get => adsProvider;
            set => adsProvider = value;
        }

        public float AdsLoadDelay
        {
            get => adsLoadDelay;
            set => adsLoadDelay = value;
        }

        public bool      DebugEnabled
        {
            get => debugEnabled;
            set => debugEnabled = value;
        }

        public bool      TestAds      => testAds;
        public ELogLevel LogLevel     => logLevel;
    }
}