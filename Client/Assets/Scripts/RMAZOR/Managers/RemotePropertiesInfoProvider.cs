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
        private CommonGameSettings      CommonGameSettings { get; }
        private ModelSettings           ModelSettings      { get; }
        private ViewSettings            ViewSettings       { get; }
        private IRemotePropertiesRmazor RemoteProperties   { get; }

        public RemotePropertiesInfoProvider(
            IGameClient             _GameClient,
            CommonGameSettings      _CommonGameSettings,
            ModelSettings           _ModelSettings,
            ViewSettings            _ViewSettings,
            IRemotePropertiesRmazor _RemoteProperties)
        {
            GameClient         = _GameClient;
            CommonGameSettings = _CommonGameSettings;
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
                    _Value => ModelSettings.characterSpeed = Convert.ToSingle(_Value)),
                new RemoteConfigPropertyInfo(filter, typeof(float), "mazeitems_gravityblock_speed",
                    _Value => ModelSettings.gravityBlockSpeed = Convert.ToSingle(_Value)),
                new RemoteConfigPropertyInfo(filter, typeof(float), "hammer_shot_pause",
                    _Value => ModelSettings.hammerShotPause = Convert.ToSingle(_Value)),
                new RemoteConfigPropertyInfo(filter, typeof(float), "shredinger_block_time",
                    _Value => ModelSettings.shredingerBlockProceedTime = Convert.ToSingle(_Value)),
                new RemoteConfigPropertyInfo(filter, typeof(float), "turret_pre_shoot_interval",
                    _Value => ModelSettings.turretPreShootInterval = Convert.ToSingle(_Value)),
                new RemoteConfigPropertyInfo(filter, typeof(float), "turret_projectile_speed",
                    _Value => ModelSettings.turretProjectileSpeed = Convert.ToSingle(_Value)),
                new RemoteConfigPropertyInfo(filter, typeof(float), "trap_increasing_idle_time",
                    _Value => ModelSettings.trapIncreasingIdleTime = Convert.ToSingle(_Value)),
                new RemoteConfigPropertyInfo(filter, typeof(float), "trap_increasing_increased_time",
                    _Value => ModelSettings.trapIncreasingIncreasedTime = Convert.ToSingle(_Value)),
                new RemoteConfigPropertyInfo(filter, typeof(float), "moving_trap_speed",
                    _Value => ModelSettings.movingItemsSpeed = Convert.ToSingle(_Value)),
                new RemoteConfigPropertyInfo(filter, typeof(float), "moving_trap_pause",
                    _Value => ModelSettings.movingItemsPause = Convert.ToSingle(_Value)),
                new RemoteConfigPropertyInfo(filter, typeof(float), "mazeitems_movingtrap_speed",
                    _Value => ModelSettings.movingItemsSpeed = Convert.ToSingle(_Value)),

                new RemoteConfigPropertyInfo(filter, typeof(bool), "animate_path_fill",
                    _Value => ViewSettings.animatePathFill = Convert.ToBoolean(_Value)),
                new RemoteConfigPropertyInfo(filter, typeof(float), "skip_button_seconds",
                    _Value => ViewSettings.skipLevelSeconds = Convert.ToSingle(_Value)),
                new RemoteConfigPropertyInfo(filter, typeof(float), "line_thickness",
                    _Value => ViewSettings.lineThickness = Convert.ToSingle(_Value)),
                new RemoteConfigPropertyInfo(filter, typeof(float), "corner_radius",
                    _Value => ViewSettings.cornerRadius = Convert.ToSingle(_Value)),
                new RemoteConfigPropertyInfo(filter, typeof(float), "maze_rotation_speed",
                    _Value => ViewSettings.mazeRotationSpeed = Convert.ToSingle(_Value)),
                new RemoteConfigPropertyInfo(filter, typeof(float), "spear_projectile_speed",
                    _Value => ViewSettings.spearProjectileSpeed = Convert.ToSingle(_Value)),
                new RemoteConfigPropertyInfo(filter, typeof(int), "first_level_to_rate_game",
                    _Value => ViewSettings.firstLevelToRateGame = Convert.ToInt32(_Value)),
                new RemoteConfigPropertyInfo(filter, typeof(float), "maze_item_transition_time",
                    _Value => ViewSettings.betweenLevelTransitionTime = Convert.ToSingle(_Value)),

                new RemoteConfigPropertyInfo(filter, typeof(int), "ads_first_level_to_show_ads",
                    _Value => CommonGameSettings.firstLevelToShowAds = Convert.ToInt32(_Value)),
                new RemoteConfigPropertyInfo(filter, typeof(int), "ads_show_ad_every_level",
                    _Value => CommonGameSettings.showAdsEveryLevel = Convert.ToInt32(_Value)),
                new RemoteConfigPropertyInfo(filter, typeof(long), "money_item_coast",
                    _Value => CommonGameSettings.moneyItemCoast = Convert.ToInt32(_Value)),
                new RemoteConfigPropertyInfo(filter, typeof(long), "pay_to_continue_money_count",
                    _Value => CommonGameSettings.payToContinueMoneyCount = Convert.ToInt32(_Value)),

                new RemoteConfigPropertyInfo(filter, typeof(string), "ads_providers_infos",
                    _Value => RemoteProperties.AdsProviders =
                        JsonConvert.DeserializeObject<IList<AdProviderInfo>>(_Value.ToString())),
                new RemoteConfigPropertyInfo(filter, typeof(string), "inapp_notifications_list",
                    _Value => RemoteProperties.Nofifications =
                        JsonConvert.DeserializeObject<IList<NotificationInfo>>(_Value.ToString())),
                new RemoteConfigPropertyInfo(filter, typeof(string), "additional_color_props_set",
                    _Value =>
                    {
                        RemoteProperties.BackAndFrontColorsSet =
                            JsonConvert.DeserializeObject<IList<AdditionalColorsProps>>(
                                Convert.ToString(_Value), new ColorJsonConverter());
                    },
                    true),
                new RemoteConfigPropertyInfo(filter, typeof(string), "background_texture_triangles2_props_set",
                    _Value =>
                    {
                        RemoteProperties.Tria2TextureSet =
                            JsonConvert.DeserializeObject<IList<Triangles2TextureProps>>(Convert.ToString(_Value));
                    },
                    true),
                new RemoteConfigPropertyInfo(filter, typeof(string), "main_color_props_set",
                    _Value =>
                    {
                        var converter = new ColorJsonConverter();
                        RemoteProperties.MainColorsSet = JsonConvert.DeserializeObject<IList<MainColorsProps>>(
                            Convert.ToString(_Value), converter);
                    },
                    true),
                new RemoteConfigPropertyInfo(filter, typeof(string), "color_grading_props_1",
                    _Value =>
                    {
                        RemoteProperties.ColorGradingProps =
                            JsonConvert.DeserializeObject<ColorGradingProps>(Convert.ToString(_Value));
                    }, true),

                new RemoteConfigPropertyInfo(filter, typeof(string), "test_device_ids",
                    _Value => { },
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
                CommonUtils.StringToHash("ads_first_level_to_show_ads"),
                CommonUtils.StringToHash("ads_providers_infos"),
                CommonUtils.StringToHash("ads_show_ad_every_level"),
                CommonUtils.StringToHash("character_speed"),
                CommonUtils.StringToHash("additional_color_props_set"),
                CommonUtils.StringToHash("background_texture_triangles2_props_set"),
                CommonUtils.StringToHash("first_level_to_rate_game"),
                CommonUtils.StringToHash("main_color_props_set"),
                CommonUtils.StringToHash("maze_item_transition_time"),
                CommonUtils.StringToHash("money_item_coast"),
                CommonUtils.StringToHash("pay_to_continue_money_count"),
                CommonUtils.StringToHash("mazeitems_gravityblock_speed"),
                CommonUtils.StringToHash("mazeitems_movingtrap_speed"),
                CommonUtils.StringToHash("test_device_ids"),
                CommonUtils.StringToHash("animate_path_fill"),
                CommonUtils.StringToHash("skip_button_seconds"),
                CommonUtils.StringToHash("line_thickness"),
                CommonUtils.StringToHash("maze_rotation_speed"),
                CommonUtils.StringToHash("hammer_shot_pause"),
                CommonUtils.StringToHash("shredinger_block_time"),
                CommonUtils.StringToHash("turret_pre_shoot_interval"),
                CommonUtils.StringToHash("turret_projectile_speed"),
                CommonUtils.StringToHash("trap_increasing_idle_time"),
                CommonUtils.StringToHash("trap_increasing_increased_time"),
                CommonUtils.StringToHash("moving_trap_speed"),
                CommonUtils.StringToHash("moving_trap_pause"),
                CommonUtils.StringToHash("spear_projectile_speed"),
                CommonUtils.StringToHash("inapp_notifications_list"),
                CommonUtils.StringToHash("color_grading_props_1"),
            }.Select(_Id => (ushort) _Id)
                .ToArray();
            return new GameDataFieldFilter(
                    GameClient, 
                    GameClientUtils.AccountId, 
                    CommonData.GameId,
                    fieldIds) 
                {OnlyLocal = true};
        }

        #endregion
    }
}