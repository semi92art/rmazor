using System;
using System.Collections.Generic;
using Common;
using Common.Entities;
using Common.Helpers;
using Common.Managers.Advertising;
using Unity.RemoteConfig;
using Common.Utils;
using Newtonsoft.Json;
using RMAZOR.Views.Common.ViewMazeBackgroundPropertySets;
using UnityEngine;
using System.Linq;

namespace RMAZOR.Managers
{
    public interface IRemoteConfigManager : IInit
    {
        T GetConfig<T>(string _Key);
    }
    
    public class RemoteConfigManager : InitBase, IRemoteConfigManager
    {
        #region nonpublic members

        private static bool _fetchCompletedActionDone;
        private static bool _failedToInit;

        #endregion
        
        #region types

        private struct UserAttributes { }
        private struct AppAttributes { }

        #endregion

        #region inject

        private static CommonGameSettings  CommonGameSettings { get; set; }
        private static ModelSettings       ModelSettings      { get; set; }
        private static ViewSettings        ViewSettings       { get; set; }
        private static RemoteProperties    RemoteProperties   { get; set; }
        private static RemoteConfigManager Manager            { get; set; }

        public RemoteConfigManager(
            CommonGameSettings _CommonGameSettings,
            ModelSettings      _ModelSettings,
            ViewSettings       _ViewSettings,
            RemoteProperties   _RemoteProperties)
        {
            CommonGameSettings = _CommonGameSettings;
            ModelSettings      = _ModelSettings;
            ViewSettings       = _ViewSettings;
            RemoteProperties   = _RemoteProperties;
            Manager = this;
        }

        #endregion

        #region api
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void ResetState()
        {
            ConfigManager.FetchCompleted -= OnInitialized;
        }

        public override void Init()
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

        private void InitBase()
        {
            base.Init();
        }
        
        private static void FetchConfigs()
        {
            ConfigManager.FetchCompleted -= OnInitialized;
            ConfigManager.FetchCompleted += OnInitialized;
            if (Application.platform != RuntimePlatform.WindowsEditor
                || CommonGameSettings.rewriteSettingsByRemoteConfigInEditor)
            {
                ConfigManager.FetchCompleted -= OnFetchCompleted;
                ConfigManager.FetchCompleted += OnFetchCompleted;
            }
            
            Cor.Run(Cor.Delay(3f,
                () =>
                {
                    if (_fetchCompletedActionDone)
                        return;
                    _failedToInit = true;
                    Dbg.Log("Failed to initialize remote config");
                    Manager.InitBase();
                }));
            
            ConfigManager.FetchConfigs(new UserAttributes(), new AppAttributes());
        }

        private static void OnFetchCompleted(ConfigResponse _Response)
        {
            Dbg.Log("OnFetchCompleted");
            EAdsProvider provider = default;
            bool adsAdMob = CommonGameSettings.adsProvider.HasFlag(EAdsProvider.AdMob);
            GetConfig(ref adsAdMob, "ads.admob");
            if (adsAdMob) provider |= EAdsProvider.AdMob;
            bool adsUnity = CommonGameSettings.adsProvider.HasFlag(EAdsProvider.UnityAds);
            GetConfig(ref adsAdMob, "ads.unityads");
            if (adsUnity) provider |= EAdsProvider.UnityAds;
            CommonGameSettings.adsProvider = provider;
            GetConfig(ref CommonGameSettings.admobRate,                    "ads.admob.rate");
            GetConfig(ref CommonGameSettings.unityAdsRate,                 "ads.unityads.rate");
            GetConfig(ref CommonGameSettings.showAdsEveryLevel,            "ads.show_ad_every_level");
            GetConfig(ref CommonGameSettings.firstLevelToShowAds,          "ads.first_level_to_show_ads");
            GetConfig(ref ModelSettings.characterSpeed,                    "character.speed");
            GetConfig(ref ModelSettings.gravityBlockSpeed,                 "mazeitems.gravityblock.speed");
            GetConfig(ref ModelSettings.movingItemsSpeed,                  "mazeitems.movingtrap.speed");
            GetConfig(ref ViewSettings.rateRequestsFrequency,              "common.raterequestsfrequency");
            GetConfig(ref ViewSettings.adsRequestsFrequency,               "ads.adsrequestsfrequency");
            GetConfig(ref ViewSettings.firstLevelToRateGame,               "common.first_level_to_rate_game");
            GetConfig(ref ViewSettings.mazeItemTransitionTime,             "common.maze_item_transition_time");
            GetConfig(ref ViewSettings.mazeItemTransitionDelayCoefficient, "common.maze_item_transition_coefficient");
            
            string mainColorSetsRaw = string.Empty;
            GetConfig(ref mainColorSetsRaw, "common.main_color_sets", true);
            var converter1 = new ColorJsonConverter();
            RemoteProperties.MainColorsSet = JsonConvert.DeserializeObject<IList<MainColorsSetItem>>(
                mainColorSetsRaw, converter1);
            string backAndFrontColrSetsRaw = string.Empty;
            GetConfig(ref backAndFrontColrSetsRaw, "common.back_and_front_color_sets", true);
            var converter2 = new ColorJsonConverter();
            RemoteProperties.BackAndFrontColorsSet = JsonConvert.DeserializeObject<IList<BackAndFrontColorsSetItem>>(
                backAndFrontColrSetsRaw, converter2);
            string linesTexturePropsSetRaw = string.Empty;
            GetConfig(ref linesTexturePropsSetRaw, "common.background_texture_lines_props_set", true);
            RemoteProperties.LinesTextureSet = JsonConvert.DeserializeObject<IList<LinesTextureSetItem>>(
                linesTexturePropsSetRaw);
            string circlesTexturePropsSetRaw = string.Empty;
            GetConfig(ref linesTexturePropsSetRaw, "common.background_texture_circles_props_set", true);
            RemoteProperties.CirclesTextureSet = JsonConvert.DeserializeObject<IList<CirclesTextureSetItem>>(
                circlesTexturePropsSetRaw);
            string circles2TexturePropsSetRaw = string.Empty;
            GetConfig(ref linesTexturePropsSetRaw, "common.background_texture_circles2_props_set", true);
            RemoteProperties.Circles2TextureSet = JsonConvert.DeserializeObject<IList<Circles2TextureSetItem>>(
                circles2TexturePropsSetRaw);
            string trianglesTexturePropsSetRaw = string.Empty;
            GetConfig(ref linesTexturePropsSetRaw, "common.background_texture_triangles_props_set", true);
            RemoteProperties.TrianglesTextureSet = JsonConvert.DeserializeObject<IList<TrianglesTextureSetItem>>(
                trianglesTexturePropsSetRaw);
            
            if (Application.platform == RuntimePlatform.WindowsEditor
                && !CommonGameSettings.rewriteSettingsByRemoteConfigInEditor)
            {
                _fetchCompletedActionDone = true;
                return;
            }
            string testDeviceIdfasJson = string.Empty;
            GetConfig(ref testDeviceIdfasJson, "common.test_device_ids", true);
            string[] deviceIds;
            if (testDeviceIdfasJson == null 
                || (deviceIds = JsonConvert.DeserializeObject<string[]>(testDeviceIdfasJson)) == null)
            {
                _fetchCompletedActionDone = true;
                return;
            }
            var idfaEntity = CommonUtils.GetIdfa();
            Cor.Run(Cor.WaitWhile(() => idfaEntity.Result == EEntityResult.Pending,
                () =>
                {
                    bool isThisDeviceForTesting = deviceIds.Any(
                        _Idfa => _Idfa.Equals(
                            idfaEntity.Value,
                            StringComparison.InvariantCultureIgnoreCase));
                    CommonGameSettings.debugEnabled = isThisDeviceForTesting;
                    CommonGameSettings.testAds = isThisDeviceForTesting;
                    _fetchCompletedActionDone = true;
                },
                _Seconds: 3f));
        }

        private static void OnInitialized(ConfigResponse _Response)
        {
            Cor.Run(Cor.WaitWhile(() => !_fetchCompletedActionDone,
                () =>
                {
                    if (_failedToInit)
                        return;
                    Dbg.Log("Remote Config Initialized with status: " + _Response.status);
                    Manager.InitBase();
                }));
            if (Application.platform == RuntimePlatform.WindowsEditor
                && !CommonGameSettings.rewriteSettingsByRemoteConfigInEditor)
            {
                _fetchCompletedActionDone = true;
            }
        }
        
        public static void GetConfig<T>(ref T _Parameter, string _Key, bool _IsJson = false)
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