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
using Common.Network;
using Common.Network.DataFieldFilters;
using UnityEngine.Events;

namespace RMAZOR.Managers
{
    public interface IRemoteConfigManager : IInit { }
    
    public class RemoteConfigManager : InitBase, IRemoteConfigManager
    {
        #region nonpublic members

        private bool m_FetchCompletedActionDone;
        private bool m_FailedToInit;

        #endregion
        
        #region types

        private struct UserAttributes { }
        private struct AppAttributes { }

        #endregion

        #region inject

        private        IGameClient         GameClient         { get; }
        private        CommonGameSettings  CommonGameSettings { get; }
        private        ModelSettings       ModelSettings      { get; }
        private        ViewSettings        ViewSettings       { get; }
        private        RemoteProperties    RemoteProperties   { get; }
        private static RemoteConfigManager Manager            { get; set; }

        public RemoteConfigManager(
            IGameClient        _GameClient,
            CommonGameSettings _CommonGameSettings,
            ModelSettings      _ModelSettings,
            ViewSettings       _ViewSettings,
            RemoteProperties   _RemoteProperties)
        {
            GameClient         = _GameClient;
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
            LoadPropertiesFromCache(FetchConfigs);
        }

        #endregion

        #region nonpblic methods

        private void InitBase()
        {
            base.Init();
        }

        private void LoadPropertiesFromCache(UnityAction _OnFinish)
        {
            var filter = GetDataFieldFilterForRemoteFieldIds();
            filter.Filter(_Fields =>
            {
                GetPropertiesFromCachedDataFields(_Fields);
                _OnFinish?.Invoke();
            });
        }

        private void GetPropertiesFromCachedDataFields(IReadOnlyList<GameDataField> _Fields)
        {
            object value;
            if ((value = GetField(_Fields, nameof(CommonGameSettings.adsProvider))?.GetValue()) != null)
                CommonGameSettings.adsProvider = (EAdsProvider) value;
            if ((value = GetField(_Fields, nameof(CommonGameSettings.unityAdsRate))?.GetValue()) != null)
                CommonGameSettings.unityAdsRate = Convert.ToSingle(value);
            if ((value = GetField(_Fields, nameof(CommonGameSettings.showAdsEveryLevel))?.GetValue()) != null)
                CommonGameSettings.showAdsEveryLevel = Convert.ToInt32(value);
            if ((value = GetField(_Fields, nameof(CommonGameSettings.firstLevelToShowAds))?.GetValue()) != null)
                CommonGameSettings.firstLevelToShowAds = Convert.ToInt32(value);
            if ((value = GetField(_Fields, nameof(ModelSettings.characterSpeed))?.GetValue()) != null)
                ModelSettings.characterSpeed = Convert.ToSingle(value);
            if ((value = GetField(_Fields, nameof(ModelSettings.gravityBlockSpeed))?.GetValue()) != null)
                ModelSettings.gravityBlockSpeed = Convert.ToSingle(value);
            if ((value = GetField(_Fields, nameof(ModelSettings.movingItemsSpeed))?.GetValue()) != null)
                ModelSettings.movingItemsSpeed = Convert.ToSingle(value);
            if ((value = GetField(_Fields, nameof(ViewSettings.rateRequestsFrequency))?.GetValue()) != null)
                ViewSettings.rateRequestsFrequency = Convert.ToInt32(value);
            if ((value = GetField(_Fields, nameof(ViewSettings.adsRequestsFrequency))?.GetValue()) != null)
                ViewSettings.adsRequestsFrequency = Convert.ToInt32(value);
            if ((value = GetField(_Fields, nameof(ViewSettings.firstLevelToRateGame))?.GetValue()) != null)
                ViewSettings.firstLevelToRateGame = Convert.ToInt32(value);
            if ((value = GetField(_Fields, nameof(ViewSettings.mazeItemTransitionTime))?.GetValue()) != null)
                ViewSettings.mazeItemTransitionTime = Convert.ToSingle(value);
            if ((value = GetField(_Fields, nameof(ViewSettings.mazeItemTransitionDelayCoefficient))?.GetValue()) != null)
                ViewSettings.mazeItemTransitionDelayCoefficient = Convert.ToSingle(value);
        }
        
        private void FetchConfigs()
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
                    if (m_FetchCompletedActionDone)
                        return;
                    m_FailedToInit = true;
                    Dbg.Log("Failed to initialize remote config");
                    Manager.InitBase();
                }));
            
            ConfigManager.FetchConfigs(new UserAttributes(), new AppAttributes());
        }

        private void OnFetchCompleted(ConfigResponse _Response)
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
            var filter = GetDataFieldFilterForRemoteFieldIds();
            filter.Filter(_Fields =>
            {
                GetField(_Fields, nameof(CommonGameSettings.adsProvider))
                    ?.SetValue(CommonGameSettings.adsProvider);
                GetField(_Fields, nameof(CommonGameSettings.unityAdsRate))
                    ?.SetValue(CommonGameSettings.unityAdsRate);
                GetField(_Fields, nameof(CommonGameSettings.showAdsEveryLevel))
                    ?.SetValue(CommonGameSettings.showAdsEveryLevel);
                GetField(_Fields, nameof(CommonGameSettings.firstLevelToShowAds))
                    ?.SetValue(CommonGameSettings.firstLevelToShowAds);
                GetField(_Fields, nameof(ModelSettings.characterSpeed))
                    ?.SetValue(ModelSettings.characterSpeed);
                GetField(_Fields, nameof(ModelSettings.gravityBlockSpeed))
                    ?.SetValue(ModelSettings.gravityBlockSpeed);
                GetField(_Fields, nameof(ModelSettings.movingItemsSpeed))
                    ?.SetValue(ModelSettings.movingItemsSpeed);
                GetField(_Fields, nameof(ViewSettings.rateRequestsFrequency))
                    ?.SetValue(ViewSettings.rateRequestsFrequency);
                GetField(_Fields, nameof(ViewSettings.adsRequestsFrequency))
                    ?.SetValue(ViewSettings.adsRequestsFrequency);
                GetField(_Fields, nameof(ViewSettings.firstLevelToRateGame))
                    ?.SetValue(ViewSettings.firstLevelToRateGame);
                GetField(_Fields, nameof(ViewSettings.mazeItemTransitionTime))
                    ?.SetValue(ViewSettings.mazeItemTransitionTime);
                GetField(_Fields, nameof(ViewSettings.mazeItemTransitionDelayCoefficient))
                    ?.SetValue(ViewSettings.mazeItemTransitionDelayCoefficient);
            });
            if (Application.platform == RuntimePlatform.WindowsEditor
                && !CommonGameSettings.rewriteSettingsByRemoteConfigInEditor)
            {
                m_FetchCompletedActionDone = true;
                return;
            }
            string testDeviceIdfasJson = string.Empty;
            GetConfig(ref testDeviceIdfasJson, "common.test_device_ids", true);
            string[] deviceIds;
            if (testDeviceIdfasJson == null 
                || (deviceIds = JsonConvert.DeserializeObject<string[]>(testDeviceIdfasJson)) == null)
            {
                m_FetchCompletedActionDone = true;
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
                    m_FetchCompletedActionDone = true;
                },
                _Seconds: 3f));
        }

        private static void OnInitialized(ConfigResponse _Response)
        {
            Cor.Run(Cor.WaitWhile(() => !Manager.m_FetchCompletedActionDone,
                () =>
                {
                    if (Manager.m_FailedToInit)
                        return;
                    Dbg.Log("Remote Config Initialized with status: " + _Response.status);
                    Manager.InitBase();
                }));
            if (Application.platform == RuntimePlatform.WindowsEditor
                && !Manager.CommonGameSettings.rewriteSettingsByRemoteConfigInEditor)
            {
                Manager.m_FetchCompletedActionDone = true;
            }
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

        private GameDataFieldFilter GetDataFieldFilterForRemoteFieldIds()
        {
            var fildIds = new []
                {
                    CommonUtils.StringToHash(nameof(CommonGameSettings.adsProvider)),
                    CommonUtils.StringToHash(nameof(CommonGameSettings.admobRate)),
                    CommonUtils.StringToHash(nameof(CommonGameSettings.unityAdsRate)),
                    CommonUtils.StringToHash(nameof(CommonGameSettings.showAdsEveryLevel)),
                    CommonUtils.StringToHash(nameof(CommonGameSettings.firstLevelToShowAds)),
                    CommonUtils.StringToHash(nameof(ModelSettings.characterSpeed)),
                    CommonUtils.StringToHash(nameof(ModelSettings.gravityBlockSpeed)),
                    CommonUtils.StringToHash(nameof(ModelSettings.movingItemsSpeed)),
                    CommonUtils.StringToHash(nameof(ViewSettings.rateRequestsFrequency)),
                    CommonUtils.StringToHash(nameof(ViewSettings.adsRequestsFrequency)),
                    CommonUtils.StringToHash(nameof(ViewSettings.firstLevelToRateGame)),
                    CommonUtils.StringToHash(nameof(ViewSettings.mazeItemTransitionTime)),
                    CommonUtils.StringToHash(nameof(ViewSettings.mazeItemTransitionDelayCoefficient))
                }.Select(_Id => (ushort)_Id)
                .ToArray();
            return new GameDataFieldFilter(
                    GameClient, 
                    GameClientUtils.AccountId, 
                    GameClientUtils.GameId,
                    fildIds) 
                {OnlyLocal = true};
        }
        
        private static GameDataField GetField(IEnumerable<GameDataField> _Fields, string _FieldName)
        {
            ushort id = (ushort)CommonUtils.StringToHash(_FieldName);
            return _Fields.FirstOrDefault(_F => _F.FieldId == id);
        }

        ~RemoteConfigManager()
        {
            ConfigManager.FetchCompleted -= OnFetchCompleted;
            ConfigManager.FetchCompleted -= OnInitialized;
        }
    
        #endregion
    }
}