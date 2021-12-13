using System;
using UnityEngine;

namespace GameHelpers
{
    [Flags]
    public enum EAdsProvider
    {
        GoogleAds = 1,
        UnityAds = 2,
        UnityMediation = 4
    }
    
    [CreateAssetMenu(fileName = "game_settings", menuName = "Configs and Sets/Game Settings", order = 1)]
    public class CommonGameSettings : ScriptableObject
    {
        [SerializeField] private EAdsProvider adsProvider;
        [SerializeField] private bool         adsTestMode;
        [SerializeField] private float        adsLoadDelay;
        
        public EAdsProvider AdsProvider
        {
            get => adsProvider;
            set => adsProvider = value;
        }
        
        public bool AdsTestMode
        {
            get => adsTestMode;
            set => adsTestMode = value;
        }

        public float AdsLoadDelay
        {
            get => adsLoadDelay;
            set => adsLoadDelay = value;
        }
    }
}