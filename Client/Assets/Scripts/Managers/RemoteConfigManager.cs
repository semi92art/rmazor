using System;
using System.Collections.Generic;
using GameHelpers;
using Games.RazorMaze;
using Unity.RemoteConfig;
using UnityEngine.Events;

namespace Managers
{
    public interface IRemoteConfigManager : IInit
    {
        T GetConfig<T>(string _Key);
    }
    
    public class RemoteConfigManager : IRemoteConfigManager
    {
        #region types

        private struct UserAttributes { }
        private struct AppAttributes { }

        #endregion

        #region inject

        private CommonGameSettings CommonGameSettings { get; }
        private ModelSettings      ModelSettings      { get; }
        private ViewSettings       ViewSettings       { get; }
        
        public RemoteConfigManager(
            CommonGameSettings _CommonGameSettings,
            ModelSettings _ModelSettings,
            ViewSettings _ViewSettings)
        {
            CommonGameSettings = _CommonGameSettings;
            ModelSettings = _ModelSettings;
            ViewSettings = _ViewSettings;
        }

        #endregion

        #region api

        public bool              Initialized { get; private set; }
        public event UnityAction Initialize;

        public void Init()
        {
            FetchConfigs();
        }

        public T GetConfig<T>(string _Key)
        {
            T result = default;
            GetConfig(ref result, _Key);
            return result;
        }

        #endregion

        #region nonpblic methods

        private void FetchConfigs()
        {
            ConfigManager.FetchCompleted += OnFetchCompleted;
            ConfigManager.FetchCompleted += OnInitialized;
            ConfigManager.FetchConfigs(new UserAttributes(), new AppAttributes());
        }

        private void OnFetchCompleted(ConfigResponse _Response)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            //return;
#endif
            EAdsProvider provider = default;
            bool adsAdMob = CommonGameSettings.AdsProvider.HasFlag(EAdsProvider.GoogleAds);
            GetConfig(ref adsAdMob, "ads.admob");
            if (adsAdMob) provider |= EAdsProvider.GoogleAds;
            
            bool adsUnity = CommonGameSettings.AdsProvider.HasFlag(EAdsProvider.UnityAds);
            GetConfig(ref adsAdMob, "ads.unityads");
            if (adsUnity) provider |= EAdsProvider.UnityAds;
            
            bool adsFacebook = CommonGameSettings.AdsProvider.HasFlag(EAdsProvider.Facebook);
            GetConfig(ref adsFacebook, "ads.facebook");
            if (adsFacebook) provider |= EAdsProvider.Facebook;

            CommonGameSettings.AdsProvider = provider;
            
            GetConfig(ref ModelSettings.characterSpeed, "character.speed");
            GetConfig(ref ModelSettings.gravityBlockSpeed, "mazeitems.gravityblock.speed");
            GetConfig(ref ModelSettings.movingItemsSpeed, "mazeitems.movingtrap.speed");
            GetConfig(ref ViewSettings.rateRequestsFrequency, "common.raterequestsfrequency");
            GetConfig(ref ViewSettings.adsRequestsFrequency, "ads.adsrequestsfrequency");
        }

        private void OnInitialized(ConfigResponse _Response)
        {
            Dbg.Log("Remote Config Initialized with status: " + _Response.status);
            Initialize?.Invoke();
            Initialized = true;
        }
        
        private static void GetConfig<T>(ref T _Parameter, string _Key)
        {
            var config = ConfigManager.appConfig;
            var @switch = new Dictionary<Type, object>
            {
                {typeof(bool),   config.GetBool(  _Key, Convert.ToBoolean(_Parameter)) },
                {typeof(float),  config.GetFloat( _Key, Convert.ToSingle(_Parameter)) },
                {typeof(string), config.GetString(_Key, Convert.ToString(_Parameter))},
                {typeof(int),    config.GetInt(   _Key, Convert.ToInt32(_Parameter)) },
                {typeof(long),   config.GetLong(  _Key, Convert.ToInt64(_Parameter)) }
            };
            var result = @switch[typeof(T)];
            _Parameter = (T) result;
        }

        ~RemoteConfigManager()
        {
            ConfigManager.FetchCompleted -= OnFetchCompleted;
            ConfigManager.FetchCompleted -= OnInitialized;
        }
    
        #endregion
    }
}