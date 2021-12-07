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
        
        public EAdsProvider AdsProvider
        {
            get => adsProvider;
            set => adsProvider = value;
        }
    }
}