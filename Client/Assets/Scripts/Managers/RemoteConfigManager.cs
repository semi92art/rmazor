using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Common;
using GameHelpers;
using RMAZOR;
using Unity.RemoteConfig;
using UnityEngine;
using UnityEngine.Events;
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
#endif

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
            Dbg.Log(nameof(FetchConfigs));
#if !UNITY_EDITOR
            ConfigManager.FetchCompleted -= OnFetchCompleted;
            ConfigManager.FetchCompleted += OnFetchCompleted;
#endif
            ConfigManager.FetchCompleted -= OnInitialized;
            ConfigManager.FetchCompleted += OnInitialized;
            ConfigManager.FetchConfigs(new UserAttributes(), new AppAttributes());
        }

        private void OnFetchCompleted(ConfigResponse _Response)
        {
            EAdsProvider provider = default;
            bool adsAdMob = CommonGameSettings.AdsProvider.HasFlag(EAdsProvider.AdMob);
            GetConfig(ref adsAdMob, "ads.admob");
            if (adsAdMob) provider |= EAdsProvider.AdMob;
            bool adsUnity = CommonGameSettings.AdsProvider.HasFlag(EAdsProvider.UnityAds);
            GetConfig(ref adsAdMob, "ads.unityads");
            if (adsUnity) provider |= EAdsProvider.UnityAds;
            CommonGameSettings.AdsProvider = provider;
            GetConfig(ref CommonGameSettings.admobRate,           "ads.admob.rate");
            GetConfig(ref CommonGameSettings.unityAdsRate,        "ads.unityads.rate");
            GetConfig(ref CommonGameSettings.showAdsEveryLevel,   "ads.show_ad_every_level");
            GetConfig(ref CommonGameSettings.firstLevelToShowAds, "ads.first_level_to_show_ads");
            GetConfig(ref ModelSettings.characterSpeed,           "character.speed");
            GetConfig(ref ModelSettings.gravityBlockSpeed,        "mazeitems.gravityblock.speed");
            GetConfig(ref ModelSettings.movingItemsSpeed,         "mazeitems.movingtrap.speed");
            GetConfig(ref ViewSettings.rateRequestsFrequency,     "common.raterequestsfrequency");
            GetConfig(ref ViewSettings.adsRequestsFrequency,      "ads.adsrequestsfrequency");
            GetConfig(ref ViewSettings.levelsCountMain,           "common.levels_count_main");

#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            string testDeviceIdsJson = string.Empty;
            GetConfig(ref testDeviceIdsJson, "common.test_device_ids", true);
            Dbg.Log("testDeviceIdsJson: " + testDeviceIdsJson);
            if (testDeviceIdsJson == null)
                return;
            var deviceIds = JsonConvert.DeserializeObject<string[]>(testDeviceIdsJson);
            if (deviceIds == null)
                return;
            CommonGameSettings.DebugEnabled = deviceIds.Contains(SystemInfo.deviceUniqueIdentifier);
#endif
        }

        private void OnInitialized(ConfigResponse _Response)
        {
            Dbg.Log("Remote Config Initialized with status: " + _Response.status);
            Initialize?.Invoke();
            Initialized = true;
        }
        
        private static void GetConfig<T>(ref T _Parameter, string _Key, bool _IsJson = false)
        {
            var config = ConfigManager.appConfig;
            object result = _Parameter;
            var result1 = result;
            var @switch = new Dictionary<Type, Func<object>>
            {
                {typeof(bool),   () => config.GetBool(  _Key, Convert.ToBoolean(result1)) },
                {typeof(float),  () => config.GetFloat( _Key, Convert.ToSingle(result1)) },
                {typeof(string), () => config.GetString(_Key, Convert.ToString(result1))},
                {typeof(int),    () => config.GetInt(   _Key, Convert.ToInt32(result1)) },
                {typeof(long),   () => config.GetLong(  _Key, Convert.ToInt64(result1)) }
            };
            result = !_IsJson ? @switch[typeof(T)]() : config.GetJson(_Key);
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