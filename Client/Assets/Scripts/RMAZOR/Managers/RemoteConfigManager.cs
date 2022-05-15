using System;
using System.Collections.Generic;
using Common;
using Common.Entities;
using Common.Helpers;
using Unity.RemoteConfig;
using Common.Utils;
using Newtonsoft.Json;
using RMAZOR.Views.Common.ViewMazeBackgroundPropertySets;
using UnityEngine;
using System.Linq;
using System.Runtime.Serialization;
using Common.Extensions;
using Common.Managers.Advertising;
using Common.Network;
using Common.Network.DataFieldFilters;
using UnityEngine.Events;

namespace RMAZOR.Managers
{
    public interface IRemoteConfigManager : IInit { }
    
    public class RemoteConfigManager : InitBase, IRemoteConfigManager
    {
        #region nonpublic members

        private static bool           _fetchCompletedActionDone;
        private static ConfigResponse? _configResponse;

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
        private static RemoteConfigManager Instance            { get; set; }

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
            Instance = this;
        }

        #endregion

        #region api
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void ResetState()
        {
            ConfigManager.FetchCompleted -= OnFetchCompleted;
        }

        public override void Init()
        {
            if (Initialized)
                return;
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
            // Common
            if ((value = GetField(_Fields, nameof(CommonGameSettings.adsProvider))?.GetValue()) != null)
                CommonGameSettings.adsProvider = (EAdsProvider)value;
            if ((value = GetField(_Fields, nameof(CommonGameSettings.admobRate))?.GetValue()) != null)
                CommonGameSettings.admobRate = Convert.ToSingle(value);
            if ((value = GetField(_Fields, nameof(CommonGameSettings.unityAdsRate))?.GetValue()) != null)
                CommonGameSettings.unityAdsRate = Convert.ToSingle(value);
            if ((value = GetField(_Fields, nameof(CommonGameSettings.showAdsEveryLevel))?.GetValue()) != null)
                CommonGameSettings.showAdsEveryLevel = Convert.ToInt32(value);
            if ((value = GetField(_Fields, nameof(CommonGameSettings.firstLevelToShowAds))?.GetValue()) != null)
                CommonGameSettings.firstLevelToShowAds = Convert.ToInt32(value);
            if ((value = GetField(_Fields, nameof(CommonGameSettings.ironSourceAppKeyAndroid))?.GetValue()) != null)
                CommonGameSettings.ironSourceAppKeyAndroid = Convert.ToString(value);
            if ((value = GetField(_Fields, nameof(CommonGameSettings.ironSourceAppKeyIos))?.GetValue()) != null)
                CommonGameSettings.ironSourceAppKeyIos = Convert.ToString(value);
            if ((value = GetField(_Fields, nameof(CommonGameSettings.showRewardedInsteadOfInterstitialOnUnpause))?.GetValue()) != null)
                CommonGameSettings.showRewardedInsteadOfInterstitialOnUnpause = Convert.ToBoolean(value);
            if ((value = GetField(_Fields, nameof(CommonGameSettings.moneyItemCoast))?.GetValue()) != null)
                CommonGameSettings.moneyItemCoast = Convert.ToInt32(value);
            // Model
            if ((value = GetField(_Fields, nameof(ModelSettings.characterSpeed))?.GetValue()) != null)
                ModelSettings.characterSpeed = Convert.ToSingle(value);
            if ((value = GetField(_Fields, nameof(ModelSettings.gravityBlockSpeed))?.GetValue()) != null)
                ModelSettings.gravityBlockSpeed = Convert.ToSingle(value);
            if ((value = GetField(_Fields, nameof(ModelSettings.movingItemsSpeed))?.GetValue()) != null)
                ModelSettings.movingItemsSpeed = Convert.ToSingle(value);
            // View
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
            // Color sets
            if ((value = GetField(_Fields, nameof(RemoteProperties.MainColorsSet))?.GetValue()) != null)
                RemoteProperties.MainColorsSet = value as IList<MainColorsProps>;
            if ((value = GetField(_Fields, nameof(RemoteProperties.BackAndFrontColorsSet))?.GetValue()) != null)
                RemoteProperties.BackAndFrontColorsSet = value as IList<BackAndFrontColorsProps>;
            // Texture sets
            if ((value = GetField(_Fields, nameof(RemoteProperties.LinesTextureSet))?.GetValue()) != null)
                RemoteProperties.LinesTextureSet = value as IList<TexturePropsBase>;
            if ((value = GetField(_Fields, nameof(RemoteProperties.TrianglesTextureSet))?.GetValue()) != null)
                RemoteProperties.TrianglesTextureSet = (IList<TrianglesTextureProps>) value;
            if ((value = GetField(_Fields, nameof(RemoteProperties.Tria2TextureSet))?.GetValue()) != null)
                RemoteProperties.Tria2TextureSet = (IList<Triangles2TextureProps>) value;
        }
        
        private void FetchConfigs()
        {
            Cor.Run(Cor.Delay(3f, () => FinishFetching(false)));
            if (!Application.isEditor || CommonGameSettings.rewriteSettingsByRemoteConfigInEditor)
                ConfigManager.FetchCompleted += OnFetchCompleted;
            ConfigManager.FetchConfigs(new UserAttributes(), new AppAttributes());
        }

        private static void FinishFetching(bool _OnFetchCompleted)
        {
            if (!_configResponse.HasValue)
            {
                if (!_OnFetchCompleted)
                    Instance.InitBase();
                return;
            }
            Dbg.Log("Remote Config Initialized with status: " + _configResponse.Value.status);
            if (_OnFetchCompleted)
                Instance.InitBase();
        }

        private static void OnFetchCompleted(ConfigResponse _Response)
        {
            Cor.Run(Cor.WaitWhile(
                () => !_fetchCompletedActionDone,
                () => FinishFetching(true)));
            if (Application.isEditor && !Instance.CommonGameSettings.rewriteSettingsByRemoteConfigInEditor)
            {
                _fetchCompletedActionDone = true;
                return;
            }
            
            if (_Response.status != ConfigRequestStatus.Success)
            {
                Dbg.Log("Remote Config Initialized with status: " + _Response.status);
                _fetchCompletedActionDone = true;
                return;
            }
            FetchCommonSettings();
            FetchModelSettings();
            FetchViewSettings();
            FetchColorSets();
            FetchBackgroundTextureSets();
            FetchTestDevices();
            Dbg.Log(nameof(RemoteConfigManager) + ": " + nameof(OnFetchCompleted));
        }

        private static void FetchCommonSettings()
        {
            EAdsProvider adsProviders = 0;
            string adsProvidersRaw = string.Empty;
            GetConfig(ref adsProvidersRaw, "ads.providers");
            if (adsProvidersRaw.Contains("admob"))
                adsProviders |= EAdsProvider.AdMob;
            if (adsProvidersRaw.Contains("unity"))
                adsProviders |= EAdsProvider.UnityAds;
            if (adsProvidersRaw.Contains("iron_source"))
                adsProviders |= EAdsProvider.IronSource;
            Instance.CommonGameSettings.adsProvider = adsProviders;
            
            GetConfig(ref Instance.CommonGameSettings.admobRate,                          "ads.admob.rate");
            GetConfig(ref Instance.CommonGameSettings.unityAdsRate,                       "ads.unityads.rate");
            GetConfig(ref Instance.CommonGameSettings.ironSourceRate,                     "ads.iron_source.rate");
            GetConfig(ref Instance.CommonGameSettings.showAdsEveryLevel,                  "ads.show_ad_every_level");
            GetConfig(ref Instance.CommonGameSettings.firstLevelToShowAds,                "ads.first_level_to_show_ads");
            GetConfig(ref Instance.CommonGameSettings.ironSourceAppKeyAndroid, "ads.iron_source.app_key.android");
            GetConfig(ref Instance.CommonGameSettings.ironSourceAppKeyIos,     "ads.iron_source.app_key.ios");
            GetConfig(ref Instance.CommonGameSettings.payToContinueMoneyCount,           "common.pay_to_continue_money_count");
            GetConfig(ref Instance.CommonGameSettings.showRewardedInsteadOfInterstitialOnUnpause,
                "ads.show_rewarded_instead_of_interstitial_on_unpause");
            GetConfig(ref Instance.CommonGameSettings.moneyItemCoast,"common.money_item_coast");
            var filter = GetDataFieldFilterForRemoteFieldIds();
            filter.Filter(_Fields =>
            {
                GetField(_Fields, nameof(Instance.CommonGameSettings.adsProvider))
                    ?.SetValue(Instance.CommonGameSettings.adsProvider);
                GetField(_Fields, nameof(Instance.CommonGameSettings.unityAdsRate))
                    ?.SetValue(Instance.CommonGameSettings.unityAdsRate);
                GetField(_Fields, nameof(Instance.CommonGameSettings.showAdsEveryLevel))
                    ?.SetValue(Instance.CommonGameSettings.showAdsEveryLevel);
                GetField(_Fields, nameof(Instance.CommonGameSettings.firstLevelToShowAds))
                    ?.SetValue(Instance.CommonGameSettings.payToContinueMoneyCount);
                GetField(_Fields, nameof(Instance.CommonGameSettings.payToContinueMoneyCount))
                    ?.SetValue(Instance.CommonGameSettings.firstLevelToShowAds);
                GetField(_Fields, nameof(Instance.CommonGameSettings.showRewardedInsteadOfInterstitialOnUnpause))
                    ?.SetValue(Instance.CommonGameSettings.showRewardedInsteadOfInterstitialOnUnpause);
                GetField(_Fields, nameof(Instance.CommonGameSettings.moneyItemCoast))
                    ?.SetValue(Instance.CommonGameSettings.moneyItemCoast);
            });
        }

        private static void FetchModelSettings()
        {
            GetConfig(ref Instance.ModelSettings.characterSpeed,   "character.speed");
            GetConfig(ref Instance.ModelSettings.gravityBlockSpeed,"mazeitems.gravityblock.speed");
            GetConfig(ref Instance.ModelSettings.movingItemsSpeed, "mazeitems.movingtrap.speed");
            var filter = GetDataFieldFilterForRemoteFieldIds();
            filter.Filter(_Fields =>
            {
                GetField(_Fields, nameof(Instance.ModelSettings.characterSpeed))
                    ?.SetValue(Instance.ModelSettings.characterSpeed);
                GetField(_Fields, nameof(Instance.ModelSettings.gravityBlockSpeed))
                    ?.SetValue(Instance.ModelSettings.gravityBlockSpeed);
                GetField(_Fields, nameof(Instance.ModelSettings.movingItemsSpeed))
                    ?.SetValue(Instance.ModelSettings.movingItemsSpeed);
            });
        }

        private static void FetchViewSettings()
        {
            GetConfig(ref Instance.ViewSettings.rateRequestsFrequency,              "common.raterequestsfrequency");
            GetConfig(ref Instance.ViewSettings.adsRequestsFrequency,               "ads.adsrequestsfrequency");
            GetConfig(ref Instance.ViewSettings.firstLevelToRateGame,               "common.first_level_to_rate_game");
            GetConfig(ref Instance.ViewSettings.mazeItemTransitionTime,             "common.maze_item_transition_time");
            GetConfig(ref Instance.ViewSettings.mazeItemTransitionDelayCoefficient, "common.maze_item_transition_coefficient");
            var filter = GetDataFieldFilterForRemoteFieldIds();
            filter.Filter(_Fields =>
            {
                GetField(_Fields, nameof(Instance.ViewSettings.rateRequestsFrequency))
                    ?.SetValue(Instance.ViewSettings.rateRequestsFrequency);
                GetField(_Fields, nameof(Instance.ViewSettings.adsRequestsFrequency))
                    ?.SetValue(Instance.ViewSettings.adsRequestsFrequency);
                GetField(_Fields, nameof(Instance.ViewSettings.firstLevelToRateGame))
                    ?.SetValue(Instance.ViewSettings.firstLevelToRateGame);
                GetField(_Fields, nameof(Instance.ViewSettings.mazeItemTransitionTime))
                    ?.SetValue(Instance.ViewSettings.mazeItemTransitionTime);
                GetField(_Fields, nameof(Instance.ViewSettings.mazeItemTransitionDelayCoefficient))
                    ?.SetValue(Instance.ViewSettings.mazeItemTransitionDelayCoefficient);
            });
        }

        private static void FetchColorSets()
        {
            string mainColorSetsRaw = string.Empty;
            GetConfig(ref mainColorSetsRaw, "common.main_color_sets", true);
            var converter1 = new ColorJsonConverter();
            Instance.RemoteProperties.MainColorsSet = JsonConvert.DeserializeObject<IList<MainColorsProps>>(
                mainColorSetsRaw, converter1);
            string backAndFrontColrSetsRaw = string.Empty;
            GetConfig(ref backAndFrontColrSetsRaw, "common.back_and_front_color_sets", true);
            var converter2 = new ColorJsonConverter();
            Instance.RemoteProperties.BackAndFrontColorsSet = JsonConvert.DeserializeObject<IList<BackAndFrontColorsProps>>(
                backAndFrontColrSetsRaw, converter2);
            var filter = GetDataFieldFilterForRemoteFieldIds();
            filter.Filter(_Fields =>
            {
                GetField(_Fields, nameof(Instance.RemoteProperties.MainColorsSet))
                    ?.SetValue(Instance.RemoteProperties.MainColorsSet);
                    GetField(_Fields, nameof(Instance.RemoteProperties.BackAndFrontColorsSet))
                    ?.SetValue(Instance.RemoteProperties.BackAndFrontColorsSet);
            });
        }

        private static void FetchBackgroundTextureSets()
        {
            string setRaw = string.Empty;
            GetConfig(ref setRaw, "common.background_texture_lines_props_set", true);
            Instance.RemoteProperties.LinesTextureSet = 
                JsonConvert.DeserializeObject<IList<TexturePropsBase>>(setRaw);
            GetConfig(ref setRaw, "common.background_texture_triangles_props_set", true);
            Instance.RemoteProperties.TrianglesTextureSet = 
                JsonConvert.DeserializeObject<IList<TrianglesTextureProps>>(setRaw);
            GetConfig(ref setRaw, "common.background_texture_triangles2_props_set", true);
            Instance.RemoteProperties.Tria2TextureSet = 
                JsonConvert.DeserializeObject<IList<Triangles2TextureProps>>(setRaw);
            var filter = GetDataFieldFilterForRemoteFieldIds();
            filter.Filter(_Fields =>
            {
                GetField(_Fields, nameof(Instance.RemoteProperties.LinesTextureSet))
                    ?.SetValue(Instance.RemoteProperties.LinesTextureSet);
                GetField(_Fields, nameof(Instance.RemoteProperties.TrianglesTextureSet))
                    ?.SetValue(Instance.RemoteProperties.TrianglesTextureSet);
                GetField(_Fields, nameof(Instance.RemoteProperties.Tria2TextureSet))
                    ?.SetValue(Instance.RemoteProperties.Tria2TextureSet);
            });
        }

        private static void FetchTestDevices()
        {
            if (Application.isEditor && !Instance.CommonGameSettings.rewriteSettingsByRemoteConfigInEditor)
            {
                _fetchCompletedActionDone = true;
                return;
            }
            string testDeviceIdfasJson = string.Empty;
            GetConfig(ref testDeviceIdfasJson, "common.test_device_ids", true);
            string[] deviceIds;
            try
            {
                deviceIds = JsonConvert.DeserializeObject<string[]>(testDeviceIdfasJson);
                if (deviceIds.NullOrEmpty())
                {
                    _fetchCompletedActionDone = true;
                    return;
                }
            }
            catch (SerializationException ex)
            {
                Dbg.LogException(ex);
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
                    if (isThisDeviceForTesting)
                        Dbg.Log("This device is test");
                    Instance.CommonGameSettings.debugEnabled = isThisDeviceForTesting;
                    Instance.CommonGameSettings.testAds = isThisDeviceForTesting;
                    _fetchCompletedActionDone = true;
                }));
        }
        
        private static void GetConfig<T>(ref T _Parameter, string _Key, bool _IsJson = false)
        {
            var config = ConfigManager.appConfig;
            object result = _Parameter;
            var result1 = result;
            var @switch = new Dictionary<Type, Func<object>>
            {
                {typeof(bool),   () => config.GetBool(  _Key, Convert.ToBoolean(result1))},
                {typeof(float),  () => config.GetFloat( _Key, Convert.ToSingle(result1))},
                {typeof(string), () => config.GetString(_Key, Convert.ToString(result1))},
                {typeof(int),    () => config.GetInt(   _Key, Convert.ToInt32(result1))},
                {typeof(long),   () => config.GetLong(  _Key, Convert.ToInt64(result1))}
            };
            result = !_IsJson ? @switch[typeof(T)]() : config.GetJson(_Key);
            _Parameter = (T) result;
        }

        private static GameDataFieldFilter GetDataFieldFilterForRemoteFieldIds()
        {
            var commonFieldIds = new[]
            {
                CommonUtils.StringToHash(nameof(Instance.CommonGameSettings.adsProvider)),
                CommonUtils.StringToHash(nameof(Instance.CommonGameSettings.admobRate)),
                CommonUtils.StringToHash(nameof(Instance.CommonGameSettings.unityAdsRate)),
                CommonUtils.StringToHash(nameof(Instance.CommonGameSettings.showAdsEveryLevel)),
                CommonUtils.StringToHash(nameof(Instance.CommonGameSettings.firstLevelToShowAds)),
                CommonUtils.StringToHash(nameof(Instance.CommonGameSettings.ironSourceAppKeyAndroid)),
                CommonUtils.StringToHash(nameof(Instance.CommonGameSettings.ironSourceAppKeyIos)),
                CommonUtils.StringToHash(nameof(Instance.CommonGameSettings.showRewardedInsteadOfInterstitialOnUnpause)),
                CommonUtils.StringToHash(nameof(Instance.CommonGameSettings.moneyItemCoast)),
            };
            var modelFieldIds = new[]
            {
                CommonUtils.StringToHash(nameof(Instance.ModelSettings.characterSpeed)),
                CommonUtils.StringToHash(nameof(Instance.ModelSettings.gravityBlockSpeed)),
                CommonUtils.StringToHash(nameof(Instance.ModelSettings.movingItemsSpeed)),
            };
            var viewFieldIds = new[]
            {
                CommonUtils.StringToHash(nameof(Instance.ViewSettings.rateRequestsFrequency)),
                CommonUtils.StringToHash(nameof(Instance.ViewSettings.adsRequestsFrequency)),
                CommonUtils.StringToHash(nameof(Instance.ViewSettings.firstLevelToRateGame)),
                CommonUtils.StringToHash(nameof(Instance.ViewSettings.mazeItemTransitionTime)),
                CommonUtils.StringToHash(nameof(Instance.ViewSettings.mazeItemTransitionDelayCoefficient)),
            };
            var colorSetFieldIds = new[]
            {
                CommonUtils.StringToHash(nameof(Instance.RemoteProperties.MainColorsSet)),
                CommonUtils.StringToHash(nameof(Instance.RemoteProperties.BackAndFrontColorsSet)),
            };
            var backTexSetFieldIds = new[]
            {
                CommonUtils.StringToHash(nameof(Instance.RemoteProperties.LinesTextureSet)),
                CommonUtils.StringToHash(nameof(Instance.RemoteProperties.TrianglesTextureSet)),
                CommonUtils.StringToHash(nameof(Instance.RemoteProperties.Tria2TextureSet)),
            };
            var fildIds = commonFieldIds
                .Concat(modelFieldIds)
                .Concat(viewFieldIds)
                .Concat(colorSetFieldIds)
                .Concat(backTexSetFieldIds)
                .Select(_Id => (ushort) _Id)
                .ToArray();
            return new GameDataFieldFilter(
                    Instance.GameClient, 
                    GameClientUtils.AccountId, 
                    Instance.CommonGameSettings.gameId,
                    fildIds) 
                {OnlyLocal = true};
        }
        
        private static GameDataField GetField(IEnumerable<GameDataField> _Fields, string _FieldName)
        {
            ushort id = (ushort)CommonUtils.StringToHash(_FieldName);
            return _Fields.FirstOrDefault(_F => _F.FieldId == id);
        }

        #endregion
    }
}