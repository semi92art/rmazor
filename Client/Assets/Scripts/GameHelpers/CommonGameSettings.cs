using System;
using UnityEngine;

namespace GameHelpers
{
    [Flags]
    public enum EAdsProvider
    {
        GoogleAds = 1,
        UnityAds = 2,
        Facebook = 4
    }
    
    [CreateAssetMenu(fileName = "game_settings", menuName = "Configs and Sets/Game Settings", order = 1)]
    public class CommonGameSettings : ScriptableObject
    {
        [SerializeField] private EAdsProvider adsProvider;
        [SerializeField] private float        adsLoadDelay;
        [SerializeField] private bool         srDebuggerOn;
        [SerializeField] private bool         testAds;
        
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

        public bool SrDebuggerOn
        {
            get => srDebuggerOn;
            set => srDebuggerOn = value;
        }

        public bool TestAds
        {
            get => testAds;
            set => testAds = value;
        }
    }
}