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
            var infos = new List<RemoteConfigPropertyInfo>
            {
                new RemoteConfigPropertyInfo(filter, typeof(float), "character_speed",
                    _Value => Execute(
                        _Value, _V =>
                        {
                            Dbg.Log("Character speed: " + ToFloat(_V));
                            ModelSettings.characterSpeed = ToFloat(_V);
                        })),
                new RemoteConfigPropertyInfo(filter, typeof(float), "mazeitems_gravityblock_speed",
                    _Value => Execute(
                        _Value, _V => ModelSettings.gravityBlockSpeed = ToFloat(_V))),
                new RemoteConfigPropertyInfo(filter, typeof(float), "hammer_shot_pause",
                    _Value => Execute(
                        _Value, _V => ModelSettings.hammerShotPause = ToFloat(_V))),
                new RemoteConfigPropertyInfo(filter, typeof(float), "shredinger_block_time",
                    _Value => Execute(
                        _Value, _V => ModelSettings.shredingerBlockProceedTime = ToFloat(_V))),
                new RemoteConfigPropertyInfo(filter, typeof(float), "trap_increasing_idle_time",
                    _Value => Execute(
                        _Value, _V => ModelSettings.trapIncreasingIdleTime = ToFloat(_V))),
                new RemoteConfigPropertyInfo(filter, typeof(float), "trap_increasing_increased_time",
                    _Value => Execute(
                        _Value, _V => ModelSettings.trapIncreasingIncreasedTime = ToFloat(_V))),
                new RemoteConfigPropertyInfo(filter, typeof(float), "moving_trap_speed",
                    _Value => Execute(
                        _Value, _V => ModelSettings.movingItemsSpeed = ToFloat(_V))),
                new RemoteConfigPropertyInfo(filter, typeof(float), "moving_trap_pause",
                    _Value => Execute(
                        _Value, _V => ModelSettings.movingItemsPause = ToFloat(_V))),
                new RemoteConfigPropertyInfo(filter, typeof(float), "mazeitems_movingtrap_speed",
                    _Value => Execute(
                        _Value, _V => ModelSettings.movingItemsSpeed = ToFloat(_V))),

                new RemoteConfigPropertyInfo(filter, typeof(bool), "animate_path_fill",
                    _Value => Execute(
                        _Value, _V => ViewSettings.animatePathFill = Convert.ToBoolean(_V))),
                new RemoteConfigPropertyInfo(filter, typeof(float), "skip_button_seconds",
                    _Value => Execute(
                        _Value, _V => ViewSettings.skipLevelSeconds = ToFloat(_V))),
                new RemoteConfigPropertyInfo(filter, typeof(float), "line_thickness",
                    _Value => Execute(
                        _Value, _V => ViewSettings.lineThickness = ToFloat(_V))),
                new RemoteConfigPropertyInfo(filter, typeof(float), "corner_radius",
                    _Value => Execute(
                        _Value, _V => ViewSettings.cornerRadius = ToFloat(_V))),
                new RemoteConfigPropertyInfo(filter, typeof(float), "maze_rotation_speed",
                    _Value => Execute(
                        _Value, _V => ViewSettings.mazeRotationSpeed = ToFloat(_V))),
                new RemoteConfigPropertyInfo(filter, typeof(float), "spear_projectile_speed",
                    _Value => Execute(
                        _Value, _V => ViewSettings.spearProjectileSpeed = ToFloat(_V))),
                new RemoteConfigPropertyInfo(filter, typeof(int), "first_level_to_rate_game",
                    _Value => Execute(
                        _Value, _V => ViewSettings.firstLevelToRateGame = ToInt(_V))),
                new RemoteConfigPropertyInfo(filter, typeof(int), "first_level_to_rate_game_this_session",
                    _Value => Execute(
                        _Value, _V => ViewSettings.firstLevelToRateGameThisSession = ToInt(_V))),
                new RemoteConfigPropertyInfo(filter, typeof(float), "maze_item_transition_time",
                    _Value => Execute(
                        _Value, _V => ViewSettings.betweenLevelTransitionTime = ToFloat(_V))),
                new RemoteConfigPropertyInfo(filter, typeof(string), "levels_in_group",
                    _Value => Execute(
                        _Value, _V =>
                        {
                            ViewSettings.LevelsInGroup =
                                JsonConvert.DeserializeObject<int[]>(Convert.ToString(_V));
                        }),
                    true),
                new RemoteConfigPropertyInfo(filter, typeof(bool), "show_full_tutorial",
                    _Value => Execute(
                        _Value, _V => ViewSettings.showFullTutorial = Convert.ToBoolean(_V))),
                new RemoteConfigPropertyInfo(filter, typeof(int), "fin_lev_g_pan_get_money_button_text_variant",
                    _Value => Execute(
                        _Value, _V => ViewSettings.finishLevelGroupPanelGetMoneyButtonTextVariant = Convert.ToInt32(_V))),
                
                new RemoteConfigPropertyInfo(filter, typeof(int), "ads_first_level_to_show_ads",
                    _Value => Execute(
                        _Value, _V => GlobalGameSettings.firstLevelToShowAds = ToInt(_V))),
                new RemoteConfigPropertyInfo(filter, typeof(int), "ads_show_ad_every_level",
                    _Value => Execute(
                        _Value, _V => GlobalGameSettings.showAdsEveryLevel = ToInt(_V))),
                new RemoteConfigPropertyInfo(filter, typeof(long), "money_item_coast",
                    _Value => Execute(
                        _Value, _V => GlobalGameSettings.moneyItemCoast = ToInt(_V))),
                new RemoteConfigPropertyInfo(filter, typeof(long), "pay_to_continue_money_count",
                    _Value => Execute(
                        _Value, _V => GlobalGameSettings.payToContinueMoneyCount = ToInt(_V))),
                new RemoteConfigPropertyInfo(filter, typeof(float), "interstitial_ads_ratio",
                    _Value => Execute(
                        _Value, _V => GlobalGameSettings.interstitialAdsRatio = ToFloat(_V))),
                new RemoteConfigPropertyInfo(filter, typeof(float), "money_items_fill_rate",
                    _Value => Execute(
                        _Value, _V => GlobalGameSettings.moneyItemsRate = ToFloat(_V))),

                new RemoteConfigPropertyInfo(filter, typeof(string), "ads_providers_infos",
                    _Value => Execute(
                        _Value, _V => RemoteProperties.AdsProviders =
                        JsonConvert.DeserializeObject<IList<AdProviderInfo>>(_V.ToString()))),
                new RemoteConfigPropertyInfo(filter, typeof(string), "inapp_notifications_list",
                    _Value => Execute(
                        _Value, _V => RemoteProperties.Notifications =
                        JsonConvert.DeserializeObject<IList<NotificationInfo>>(_V.ToString()))),
                new RemoteConfigPropertyInfo(filter, typeof(string), "additional_color_props_set",
                    _Value => Execute(
                        _Value, _V =>
                    {
                        RemoteProperties.BackAndFrontColorsSet =
                            JsonConvert.DeserializeObject<IList<AdditionalColorsProps>>(
                                Convert.ToString(_Value), new ColorJsonConverter());
                    }),
                    true),
                new RemoteConfigPropertyInfo(filter, typeof(string), "background_texture_triangles2_props_set",
                    _Value => Execute(
                        _Value, _V =>
                    {
                        RemoteProperties.Tria2TextureSet =
                            JsonConvert.DeserializeObject<IList<Triangles2TextureProps>>(Convert.ToString(_V));
                    }),
                    true),
                new RemoteConfigPropertyInfo(filter, typeof(string), "main_color_props_set",
                    _Value => Execute(
                        _Value, _V =>
                    {
                        var converter = new ColorJsonConverter();
                        RemoteProperties.MainColorsSet = JsonConvert.DeserializeObject<IList<MainColorsProps>>(
                            Convert.ToString(_V), converter);
                    }),
                    true),
                new RemoteConfigPropertyInfo(filter, typeof(string), "color_grading_props_1",
                    _Value => Execute(
                        _Value, _V =>
                    {
                        RemoteProperties.ColorGradingProps =
                            JsonConvert.DeserializeObject<ColorGradingProps>(Convert.ToString(_V));
                    }), true),
                new RemoteConfigPropertyInfo(filter, typeof(string), "test_device_ids_for_admob",
                    _Value => Execute(
                        _Value, _V =>
                    {
                        RemoteProperties.TestDeviceIdsForAdmob =
                            JsonConvert.DeserializeObject<List<string>>(Convert.ToString(_V));
                    }),
                    true),
                new RemoteConfigPropertyInfo(filter, typeof(string), "test_device_ids",
                    _Value => Execute(
                        _Value, _V =>{ }),
                    true),
            };
            return infos;
        }

        #endregion

        #region nonpublic methods

        private GameDataFieldFilter GetFilter()
        {
            var fieldIds = new []
            {
                CommonUtils.StringToHash("additional_color_props_set"),
                CommonUtils.StringToHash("ads_first_level_to_show_ads"),
                CommonUtils.StringToHash("ads_providers_infos"),
                CommonUtils.StringToHash("ads_show_ad_every_level"),
                CommonUtils.StringToHash("animate_path_fill"),
                CommonUtils.StringToHash("background_texture_triangles2_props_set"),
                CommonUtils.StringToHash("character_speed"),
                CommonUtils.StringToHash("color_grading_props_1"),
                CommonUtils.StringToHash("first_level_to_rate_game"),
                CommonUtils.StringToHash("first_level_to_rate_game_this_session"),
                CommonUtils.StringToHash("hammer_shot_pause"),
                CommonUtils.StringToHash("inapp_notifications_list"),
                CommonUtils.StringToHash("interstitial_ads_ratio"),
                CommonUtils.StringToHash("levels_in_group"),
                CommonUtils.StringToHash("line_thickness"),
                CommonUtils.StringToHash("main_color_props_set"),
                CommonUtils.StringToHash("maze_item_transition_time"),
                CommonUtils.StringToHash("maze_rotation_speed"),
                CommonUtils.StringToHash("mazeitems_gravityblock_speed"),
                CommonUtils.StringToHash("mazeitems_movingtrap_speed"),
                CommonUtils.StringToHash("money_item_coast"),
                CommonUtils.StringToHash("moving_trap_pause"),
                CommonUtils.StringToHash("moving_trap_speed"),
                CommonUtils.StringToHash("pay_to_continue_money_count"),
                CommonUtils.StringToHash("show_full_tutorial"),
                CommonUtils.StringToHash("shredinger_block_time"),
                CommonUtils.StringToHash("skip_button_seconds"),
                CommonUtils.StringToHash("spear_projectile_speed"),
                CommonUtils.StringToHash("test_device_ids"),
                CommonUtils.StringToHash("test_device_ids_for_admob"),
                CommonUtils.StringToHash("trap_increasing_idle_time"),
                CommonUtils.StringToHash("trap_increasing_increased_time"),
                CommonUtils.StringToHash("money_items_fill_rate"),
                CommonUtils.StringToHash("fin_lev_g_pan_get_money_button_text_variant"),
            }
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
        
        #endregion
    }
}