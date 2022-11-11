using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.CameraProviders.Camera_Effects_Props;
using Common.Entities;
using Common.Helpers;
using Common.Managers.Advertising;
using Common.Managers.Notifications;
using Common.Network;
using Common.Network.DataFieldFilters;
using Common.Utils;
using Newtonsoft.Json;
using RMAZOR.Views.Common.ViewMazeBackgroundPropertySets;
using UnityEngine.Events;

namespace RMAZOR.Managers
{
    public interface IRemotePropertiesInfoProvider : IInit
    {
        List<RemoteConfigPropertyInfo> GetInfos();
    }
    
    public class RemotePropertiesInfoProvider : InitBase, IRemotePropertiesInfoProvider 
    {
        #region constants

        private const string IdAdditionalColorPropsSet              = "additional_color_props_set";
        private const string IdFirstLevelToShowAds                  = "ads_first_level_to_show_ads";
        private const string IdAdsProvidersInfos                    = "ads_providers_infos_v2";
        private const string IdShowAdEveryLevel                     = "ads_show_ad_every_level";
        private const string IdAnimatePathFill                      = "animate_path_fill";
        private const string IdBackgroundTextureTriangles2PropsSet  = "background_texture_triangles2_props_set";
        private const string IdCharacterSpeed                       = "character_speed";
        private const string IdColorGradingProps                    = "color_grading_props_1";
        private const string IdFirstLevelToRateGame                 = "first_level_to_rate_game";
        private const string IdFirstLevelToRateGameThisSession      = "first_level_to_rate_game_this_session";
        private const string IdHammerShotPause                      = "hammer_shot_pause";
        private const string IdInAppNotificationList                = "inapp_notifications_list_v2";
        private const string IdInterstitialAdsRatio                 = "interstitial_ads_ratio";
        private const string IdLevelsInGroup                        = "levels_in_group";
        private const string IdLineThickness                        = "line_thickness";
        private const string IdMainColorPropsSet                    = "main_color_props_set";
        private const string IdMazeItemTransitionTime               = "maze_item_transition_time";
        private const string IdMazeRotationSpeed                    = "maze_rotation_speed";
        private const string IdGravityBlockSpeed                    = "mazeitems_gravityblock_speed";
        private const string IdMoneyItemCoast                       = "money_item_coast";
        private const string IdMovingTrapPause                      = "moving_trap_pause";
        private const string IdMovingTrapSpeed                      = "moving_trap_speed";
        private const string IdPayToContinueMoneyCount              = "pay_to_continue_money_count";
        private const string IdShowFullTutorial                     = "show_full_tutorial";
        private const string IdShredingerBlockTime                  = "shredinger_block_time";
        private const string IdSkipButtonDelay                      = "skip_button_seconds";
        private const string IdSpearProjectileSpeed                 = "spear_projectile_speed";
        private const string IdTestDeviceIds                        = "test_device_ids";
        private const string IdTestDeviceIdsForAdmob                = "test_device_ids_for_admob";
        private const string IdTrapIncreasingIdleTime               = "trap_increasing_idle_time";
        private const string IdTrapIncreasingIncreasedTime          = "trap_increasing_increased_time";
        private const string IdMoneyItemsFillRate                   = "money_items_fill_rate";
        private const string IdFinishLevelGroupPanelGetMoneyTextVar = "fin_lev_g_pan_get_money_button_text_variant";
        private const string IdFinishLevelGroupPanelBackgroundVar   = "fin_lev_g_pan_background_variant";
        private const string IdCharacterDiedPanelBackgroundVar      = "char_died_g_pan_background_variant";
        private const string IdExtraBordersIndices                  = "extra_borders_indices_v2";
        private const string IdBackgroundTextures                   = "background_textures_v2";
        private const string IdAdditionalBackgroundType             = "additional_background_type";
        private const string IdCornerRadius                         = "corner_radius";

        #endregion
        
        #region inject
        
        private IGameClient             GameClient         { get; }
        private GlobalGameSettings      GlobalGameSettings { get; }
        private ModelSettings           ModelSettings      { get; }
        private ViewSettings            ViewSettings       { get; }
        private IRemotePropertiesRmazor RemoteProperties   { get; }

        public RemotePropertiesInfoProvider(
            IGameClient             _GameClient,
            GlobalGameSettings      _GlobalGameSettings,
            ModelSettings           _ModelSettings,
            ViewSettings            _ViewSettings,
            IRemotePropertiesRmazor _RemoteProperties)
        {
            GameClient         = _GameClient;
            GlobalGameSettings = _GlobalGameSettings;
            ModelSettings      = _ModelSettings;
            ViewSettings       = _ViewSettings;
            RemoteProperties   = _RemoteProperties;
        }

        #endregion

        #region api

        public List<RemoteConfigPropertyInfo> GetInfos()
        {
            var filter = GetFilter();
            return GetModelSettingsInfos(filter)
                .Concat(GetViewSettingsInfos(filter))
                .Concat(GetGlobalSettingsInfos(filter))
                .Concat(GetRemotePropertiesInfos(filter))
                .Concat(new[]
                {
                    new RemoteConfigPropertyInfo(filter, typeof(string), IdTestDeviceIds,
                        _Value => Execute(
                            _Value, _V => { }),
                        true),
                }).ToList();
        }

        #endregion

        #region nonpublic methods

        private IEnumerable<RemoteConfigPropertyInfo> GetModelSettingsInfos(GameDataFieldFilter _Filter)
        {
            return new List<RemoteConfigPropertyInfo>
            {
                new RemoteConfigPropertyInfo(_Filter, typeof(float), IdCharacterSpeed,
                    _Value => Execute(
                        _Value, _V =>
                        {
                            Dbg.Log("Character speed: " + ToFloat(_V));
                            ModelSettings.characterSpeed = ToFloat(_V);
                        })),
                new RemoteConfigPropertyInfo(_Filter, typeof(float), IdGravityBlockSpeed,
                    _Value => Execute(
                        _Value, _V => ModelSettings.gravityBlockSpeed = ToFloat(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(float), IdHammerShotPause,
                    _Value => Execute(
                        _Value, _V => ModelSettings.hammerShotPause = ToFloat(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(float), IdShredingerBlockTime,
                    _Value => Execute(
                        _Value, _V => ModelSettings.shredingerBlockProceedTime = ToFloat(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(float), IdTrapIncreasingIdleTime,
                    _Value => Execute(
                        _Value, _V => ModelSettings.trapIncreasingIdleTime = ToFloat(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(float), IdTrapIncreasingIncreasedTime,
                    _Value => Execute(
                        _Value, _V => ModelSettings.trapIncreasingIncreasedTime = ToFloat(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(float), IdMovingTrapSpeed,
                    _Value => Execute(
                        _Value, _V => ModelSettings.movingItemsSpeed = ToFloat(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(float), IdMovingTrapPause,
                    _Value => Execute(
                        _Value, _V => ModelSettings.movingItemsPause = ToFloat(_V))),
            };
        }

        private IEnumerable<RemoteConfigPropertyInfo> GetViewSettingsInfos(GameDataFieldFilter _Filter)
        {
            return new List<RemoteConfigPropertyInfo>
            {
                new RemoteConfigPropertyInfo(_Filter, typeof(bool), IdAnimatePathFill,
                    _Value => Execute(
                        _Value, _V => ViewSettings.animatePathFill = ToBool(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(float), IdSkipButtonDelay,
                    _Value => Execute(
                        _Value, _V => ViewSettings.skipLevelSeconds = ToFloat(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(float), IdLineThickness,
                    _Value => Execute(
                        _Value, _V => ViewSettings.lineThickness = ToFloat(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(float), IdCornerRadius,
                    _Value => Execute(
                        _Value, _V => ViewSettings.cornerRadius = ToFloat(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(float), IdMazeRotationSpeed,
                    _Value => Execute(
                        _Value, _V => ViewSettings.mazeRotationSpeed = ToFloat(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(float), IdSpearProjectileSpeed,
                    _Value => Execute(
                        _Value, _V => ViewSettings.spearProjectileSpeed = ToFloat(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(int), IdFirstLevelToRateGame,
                    _Value => Execute(
                        _Value, _V => ViewSettings.firstLevelToRateGame = ToInt(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(int), IdFirstLevelToRateGameThisSession,
                    _Value => Execute(
                        _Value, _V => ViewSettings.firstLevelToRateGameThisSession = ToInt(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(float), IdMazeItemTransitionTime,
                    _Value => Execute(
                        _Value, _V => ViewSettings.betweenLevelTransitionTime = ToFloat(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(string), IdLevelsInGroup,
                    _Value => Execute(
                        _Value, _V =>
                        {
                            ViewSettings.LevelsInGroup =
                                JsonConvert.DeserializeObject<int[]>(ToString(_V));
                        }),
                    true),
                new RemoteConfigPropertyInfo(_Filter, typeof(bool), IdShowFullTutorial,
                    _Value => Execute(
                        _Value, _V => ViewSettings.showFullTutorial = ToBool(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(int), IdFinishLevelGroupPanelGetMoneyTextVar,
                    _Value => Execute(
                        _Value, _V => ViewSettings.finishLevelGroupPanelGetMoneyButtonTextVariant = ToInt(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(int), IdFinishLevelGroupPanelBackgroundVar,
                    _Value => Execute(
                        _Value, _V => ViewSettings.finishLevelGroupPanelBackgroundVariant = ToInt(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(int), IdCharacterDiedPanelBackgroundVar,
                    _Value => Execute(
                        _Value, _V => ViewSettings.characterDiedPanelBackgroundVariant = ToInt(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(string), IdExtraBordersIndices,
                    _Value => Execute(
                        _Value, _V => ViewSettings.extraBordersIndices = ToString(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(string), IdBackgroundTextures,
                    _Value => Execute(
                        _Value, _V =>
                        {
                            ViewSettings.BackgroundTextures = 
                                JsonConvert.DeserializeObject<List<string>>(ToString(_V));
                        })),
                new RemoteConfigPropertyInfo(_Filter, typeof(int), IdAdditionalBackgroundType,
                    _Value => Execute(
                        _Value, _V => ViewSettings.additionalBackgroundType = ToInt(_V))),
            };
        }

        private IEnumerable<RemoteConfigPropertyInfo> GetGlobalSettingsInfos(GameDataFieldFilter _Filter)
        {
            return new List<RemoteConfigPropertyInfo>
            {
                new RemoteConfigPropertyInfo(_Filter, typeof(int), IdFirstLevelToShowAds,
                    _Value => Execute(
                        _Value, _V => GlobalGameSettings.firstLevelToShowAds = ToInt(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(int), IdShowAdEveryLevel,
                    _Value => Execute(
                        _Value, _V => GlobalGameSettings.showAdsEveryLevel = ToInt(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(long), IdMoneyItemCoast,
                    _Value => Execute(
                        _Value, _V => GlobalGameSettings.moneyItemCoast = ToInt(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(long), IdPayToContinueMoneyCount,
                    _Value => Execute(
                        _Value, _V => GlobalGameSettings.payToContinueMoneyCount = ToInt(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(float), IdInterstitialAdsRatio,
                    _Value => Execute(
                        _Value, _V => GlobalGameSettings.interstitialAdsRatio = ToFloat(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(float), IdMoneyItemsFillRate,
                    _Value => Execute(
                        _Value, _V => GlobalGameSettings.moneyItemsRate = ToFloat(_V))),
            };
        }

        private IEnumerable<RemoteConfigPropertyInfo> GetRemotePropertiesInfos(GameDataFieldFilter _Filter)
        {
            return new List<RemoteConfigPropertyInfo>
            {
                new RemoteConfigPropertyInfo(_Filter, typeof(string), IdAdsProvidersInfos,
                    _Value => Execute(
                        _Value, _V => RemoteProperties.AdsProviders =
                        JsonConvert.DeserializeObject<IList<AdProviderInfo>>(_V.ToString()))),
                new RemoteConfigPropertyInfo(_Filter, typeof(string), IdInAppNotificationList,
                    _Value => Execute(
                        _Value, _V => RemoteProperties.Notifications =
                        JsonConvert.DeserializeObject<IList<NotificationInfoEx>>(_V.ToString()))),
                new RemoteConfigPropertyInfo(_Filter, typeof(string), IdAdditionalColorPropsSet,
                    _Value => Execute(
                        _Value, _V =>
                    {
                        RemoteProperties.BackAndFrontColorsSet =
                            JsonConvert.DeserializeObject<IList<AdditionalColorsProps>>(
                                Convert.ToString(_Value), new ColorJsonConverter());
                    }),
                    true),
                new RemoteConfigPropertyInfo(_Filter, typeof(string), IdBackgroundTextureTriangles2PropsSet,
                    _Value => Execute(
                        _Value, _V =>
                    {
                        RemoteProperties.Tria2TextureSet =
                            JsonConvert.DeserializeObject<IList<Triangles2TextureProps>>(Convert.ToString(_V));
                    }),
                    true),
                new RemoteConfigPropertyInfo(_Filter, typeof(string), IdMainColorPropsSet,
                    _Value => Execute(
                        _Value, _V =>
                    {
                        var converter = new ColorJsonConverter();
                        RemoteProperties.MainColorsSet = JsonConvert.DeserializeObject<IList<MainColorsProps>>(
                            Convert.ToString(_V), converter);
                    }),
                    true),
                new RemoteConfigPropertyInfo(_Filter, typeof(string), IdColorGradingProps,
                    _Value => Execute(
                        _Value, _V =>
                    {
                        RemoteProperties.ColorGradingProps =
                            JsonConvert.DeserializeObject<ColorGradingProps>(Convert.ToString(_V));
                    }), true),
                new RemoteConfigPropertyInfo(_Filter, typeof(string), IdTestDeviceIdsForAdmob,
                    _Value => Execute(
                        _Value, _V =>
                    {
                        RemoteProperties.TestDeviceIdsForAdmob =
                            JsonConvert.DeserializeObject<List<string>>(Convert.ToString(_V));
                    }),
                    true),
            };
        }



        private GameDataFieldFilter GetFilter()
        {
            var fieldIds = new []
            {
                IdAdditionalColorPropsSet,
                IdFirstLevelToShowAds,
                IdAdsProvidersInfos,
                IdShowAdEveryLevel,
                IdAnimatePathFill,
                IdBackgroundTextureTriangles2PropsSet,
                IdCharacterSpeed,
                IdColorGradingProps,
                IdFirstLevelToRateGame,
                IdFirstLevelToRateGameThisSession,
                IdHammerShotPause,
                IdInAppNotificationList,
                IdInterstitialAdsRatio,
                IdLevelsInGroup,
                IdLineThickness,
                IdMainColorPropsSet,
                IdMazeItemTransitionTime,
                IdMazeRotationSpeed,
                IdGravityBlockSpeed,
                IdMoneyItemCoast,
                IdMovingTrapPause,
                IdMovingTrapSpeed,
                IdPayToContinueMoneyCount,
                IdShowFullTutorial,
                IdShredingerBlockTime,
                IdSkipButtonDelay,
                IdSpearProjectileSpeed,
                IdTestDeviceIds,
                IdTestDeviceIdsForAdmob,
                IdTrapIncreasingIdleTime,
                IdTrapIncreasingIncreasedTime,
                IdMoneyItemsFillRate,
                IdFinishLevelGroupPanelGetMoneyTextVar,
                IdFinishLevelGroupPanelBackgroundVar,
                IdCharacterDiedPanelBackgroundVar,
                IdExtraBordersIndices,
                IdBackgroundTextures,
                IdAdditionalBackgroundType,
            }
                .Select(CommonUtils.StringToHash)
                .Select(_Id => (ushort) _Id)
                .ToArray();
            return new GameDataFieldFilter(
                    GameClient, 
                    GameClientUtils.AccountId, 
                    CommonData.GameId,
                    fieldIds) 
                {OnlyLocal = true};
        }
        
        private static void Execute(object _Value, UnityAction<object> _Action)
        {
            if (_Value == null)
                return;
            _Action?.Invoke(_Value);
        }

        private static float ToFloat(object _Value)
        {
            return Convert.ToSingle(_Value);
        }

        private static int ToInt(object _Value)
        {
            return Convert.ToInt32(_Value);
        }

        private static string ToString(object _Value)
        {
            return Convert.ToString(_Value);
        }

        private static bool ToBool(object _Value)
        {
            return Convert.ToBoolean(_Value);
        }
        
        #endregion
    }
}