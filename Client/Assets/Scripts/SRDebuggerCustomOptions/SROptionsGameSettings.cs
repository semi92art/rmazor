// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PartialTypeWithSinglePart

using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Extensions;
using Common.Managers;
using Common.Managers.Advertising;
using Common.Utils;
using Firebase.Extensions;
using Firebase.Messaging;
using RMAZOR;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SRDebuggerCustomOptions
{
    public partial class SROptions
    {
        private const string CategoryMazeItems  = "Maze Items";
        private const string CategoryCharacter  = "Character";
        private const string CategoryCommon     = "Common";
        private const string CategoryLoadLevels = "Load Levels";
        private const string CategoryHaptics    = "Haptics";
        private const string CategoryAds        = "Ads";
        private const string CategoryMonitor    = "Monitor";

        private static int _adsNetworkIdx;

        private static ModelSettings               _modelSettings;
        private static ViewSettings                _viewSettings;
        private static IModelLevelStaging          _levelStaging;
        private static IManagersGetter             _managers;
        private static IViewInputCommandsProceeder _commandsProceeder;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void ResetState()
        {
            SRLauncher.Initialized += SRLauncherOnInitialized;
        }

        private static void SRLauncherOnInitialized()
        {
            Dbg.Log(nameof(SRLauncherOnInitialized));
            _modelSettings     = SRLauncher.ModelSettings;
            _viewSettings      = SRLauncher.ViewSettings;
            _levelStaging      = SRLauncher.LevelStaging;
            _managers          = SRLauncher.Managers;
            _commandsProceeder = SRLauncher.CommandsProceeder;
            SRDebug.Instance.PanelVisibilityChanged += OnPanelVisibilityChanged;
        }
        
        private static void OnPanelVisibilityChanged(bool _Visible)
        {
            var commands = new[] {EInputCommand.ShopMenu, EInputCommand.SettingsMenu}
                .Concat(RmazorUtils.MoveAndRotateCommands);
            if (_Visible)
                _commandsProceeder.LockCommands(commands, nameof(SROptions));
            else
                _commandsProceeder.UnlockCommands(commands, nameof(SROptions));
        }

        #region model settings

        [Category(CategoryCharacter)]
        public float Speed
        {
            get => _modelSettings.characterSpeed;
            set => _modelSettings.characterSpeed = value;
        }

        [Category(CategoryCharacter)]
        public float MoveThreshold
        {
            get => _viewSettings.moveSwipeThreshold;
            set => _viewSettings.moveSwipeThreshold = value;
        }

        [Category(CategoryMazeItems)]
        public float Turret_Projectile_Speed
        {
            get => _modelSettings.turretProjectileSpeed;
            set => _modelSettings.turretProjectileSpeed = value;
        }

        #endregion

        #region view settings

        [Category(CategoryCommon)]
        public float Maze_Rotation_Speed
        {
            get => _viewSettings.mazeRotationSpeed;
            set => _viewSettings.mazeRotationSpeed = value;
        }

        [Category(CategoryCommon)]
        public int Money { get; set; }

        [Category(CategoryCommon)]
        public bool Set_Money
        {
            get => false;
            set
            {
                if (!value)
                    return;
                var savedGame = new SavedGame
                {
                    FileName = CommonData.SavedGameFileName,
                    Money = Money,
                    Level = _levelStaging.LevelIndex
                };
                _managers.ScoreManager.SaveGameProgress(savedGame, false);
            }
        }

        #endregion

        #region other settings

        [Category(CategoryLoadLevels)]
        public int Level_Index { get; set; }

        [Category(CategoryLoadLevels)]
        public bool Load_By_Index
        {
            get => false;
            set
            {
                if (!value)
                    return;
                _commandsProceeder.RaiseCommand(
                    EInputCommand.LoadLevelByIndex, 
                    new object[] { Level_Index - 1 },
                    true);
            }
        }

        [Category(CategoryLoadLevels)]
        public bool Load_Next_Level
        {
            get => false;
            set
            {
                if (!value)
                    return;
                _commandsProceeder.RaiseCommand(EInputCommand.LoadNextLevel, null, true);
            }
        }

        [Category(CategoryLoadLevels)]
        public bool Load_Previous_Level
        {
            get => false;
            set
            {
                if (!value)
                    return;
                long levelIndex = _levelStaging.LevelIndex;
                _commandsProceeder.RaiseCommand(
                    EInputCommand.LoadLevelByIndex, 
                    new object[] { levelIndex - 1 },
                    true);
            }
        }
    
        [Category(CategoryLoadLevels)]
        public bool Load_Current_Level
        {
            get => false;
            set
            {
                if (!value)
                    return;
                _commandsProceeder.RaiseCommand(EInputCommand.LoadCurrentLevel, null, true);
            }
        }

        [Category(CategoryLoadLevels)]
        public bool Load_Random_Level
        {
            get => false;
            set
            {
                if (!value)
                    return;
                _commandsProceeder.RaiseCommand(EInputCommand.LoadRandomLevel, null, true);
            }
        }

        [Category(CategoryLoadLevels)]
        public bool Load_Random_Level_With_Rotation
        {
            get => false;
            set
            {
                if (!value)
                    return;
                _commandsProceeder.RaiseCommand(EInputCommand.LoadRandomLevelWithRotation, null, true);
            }
        }

        [Category(CategoryLoadLevels)]
        public bool Finish_Current_Level
        {
            get => false;
            set
            {
                if (!value)
                    return;
                _commandsProceeder.RaiseCommand(EInputCommand.FinishLevel, null, true);
            }
        }

        [Category(CategoryHaptics)]
        public float Amplitude { get; set; }
    
        [Category(CategoryHaptics)]
        public float Frequency { get; set; }
    
        [Category(CategoryHaptics)]
        public float Duration { get; set; }
    
        [Category(CategoryHaptics)]
        public bool Play_Constant
        {
            get => false;
            set
            {
                if (!value)
                    return;
                _managers.HapticsManager.Play(Amplitude, Frequency, Duration);
            }
        }
        
        [Category(CategoryHaptics)]
        public bool Play_Emphasis
        {
            get => false;
            set
            {
                if (!value)
                    return;
                _managers.HapticsManager.Play(Amplitude, Frequency);
            }
        }
        
        [Category(CategoryHaptics)]
        public bool Play_Random_Preset
        {
            get => false;
            set
            {
                if (!value)
                    return;
                var preset = (EHapticsPresetType) Mathf.FloorToInt(Random.value * 8.99f);
                Dbg.Log("Haptics preset: " +  Enum.GetName(typeof(EHapticsPresetType), preset));
                _managers.HapticsManager.PlayPreset(preset);
            }
        }
        
        [Category(CategoryHaptics)]
        public bool Is_Haptics_Supported
        {
            get => false;
            set
            {
                if (!value)
                    return;
                Dbg.Log("isVersionSupported: " + _managers.HapticsManager.IsHapticsSupported());
            }
        }

        [Category(CategoryAds)]
        public int AdsProviderIndex
        {
            get => _adsNetworkIdx;
            set => _adsNetworkIdx = value;
        }

        [Category(CategoryAds)]
        public bool Show_Rewarded_Ad
        {
            get => false;
            set
            {
                if (!value)
                    return;
                string adsNetworkName = GetAdsNetworkProviderName(_adsNetworkIdx);
                _managers.AdsManager.ShowRewardedAd(
                    null,
                    () => Dbg.Log("Rewarded ad was shown."),
                    adsNetworkName,
                    true);
            }
        }

        [Category(CategoryAds)]
        public bool Show_Interstitial_Ad
        {
            get => false;
            set
            {
                if (!value)
                    return;
                string adsNetworkName = GetAdsNetworkProviderName(_adsNetworkIdx);
                _managers.AdsManager.ShowInterstitialAd(
                    null,
                    () => Dbg.Log("Interstitial ad was shown."),
                    adsNetworkName,
                    true);
            }
        }
        
#if APPODEAL_3
        [Category(CategoryAds)]
        public bool Show_Appodeal_Test_Screen
        {
            get => false;
            set
            {
                if (!value)
                    return;
                AppodealStack.Monetization.Api.Appodeal.ShowTestScreen();
            }
        }
#endif

        private static string GetAdsNetworkProviderName(int _ProviderIndex)
        {
            Dbg.Log("Reference: 1 => Admob, 2 => Appodeal, 3 => Unity, other => auto");
            return _ProviderIndex switch
            {
                1 => AdvertisingNetworks.Admob,
                2 => AdvertisingNetworks.Appodeal,
                3 => AdvertisingNetworks.UnityAds,
                _ => null
            };
        }

        [Category(CategoryCommon)]
        public bool Show_Rate_Dialog
        {
            get => false;
            set
            {
                if (!value)
                    return;
                _managers.ShopManager.RateGame(true);
            }
        }

        [Category(CategoryCommon)]
        public bool Clear_Cache
        {
            get => false;
            set
            {
                if (!value)
                    return;
                Caching.ClearCache();
            }
        }

        [Category(CategoryCommon)]
        public bool Show_Locked_Groups_With_Rotation
        {
            get => false;
            set
            {
                if (!value)
                    return;
                var groupNames = (_commandsProceeder as ViewInputCommandsProceeder)?
                    .LockedCommands
                    .Where(_Kvp => _Kvp.Value.Contains(
                                       EInputCommand.RotateClockwise)
                                   || _Kvp.Value.Contains(EInputCommand.RotateCounterClockwise))
                    .Select(_Kvp => _Kvp.Key).ToList();
                var sb = new StringBuilder();
                sb.Append(nameof(Show_Locked_Groups_With_Rotation) + "\n");
                if (groupNames != null)
                    foreach (string gName in groupNames)
                        sb.Append(gName + "\n");
                Dbg.Log(sb.ToString());
            }
        }
        
        [Category(CategoryCommon)]
        public bool Show_Locked_Groups_With_Movememt
        {
            get => false;
            set
            {
                if (!value)
                    return;
                var groupNames = (_commandsProceeder as ViewInputCommandsProceeder)?
                    .LockedCommands
                    .Where(_Kvp => _Kvp.Value.Contains(
                                       EInputCommand.RotateClockwise)
                                   || _Kvp.Value.Contains(EInputCommand.MoveRight))
                    .Select(_Kvp => _Kvp.Key).ToList();
                var sb = new StringBuilder();
                sb.Append(nameof(Show_Locked_Groups_With_Rotation) + "\n");
                if (groupNames != null)
                    foreach (string gName in groupNames)
                        sb.Append(gName + "\n");
                Dbg.Log(sb.ToString());
            }
        }

        [Category(CategoryCommon)]
        public bool Get_Score_Test
        {
            get => false;
            set
            {
                if (!value)
                    return;
                var entity = _managers.ScoreManager.GetScoreFromLeaderboard(
                    DataFieldIds.Level, 
                    false);
                Cor.Run(Cor.WaitWhile(() => entity.Result == EEntityResult.Pending,
                    () =>
                    {
                        if (entity.Result == EEntityResult.Fail)
                        {
                            Dbg.LogError(nameof(Get_Score_Test) + ": " + entity.Result);
                            return;
                        }
                        var firstScore = entity.GetFirstScore();
                        if (firstScore.HasValue)
                            Dbg.Log("First score: " +  firstScore.Value);
                    }));
            }
        }
        
        [Category(CategoryCommon)]
        public bool Sava_Game_Test
        {
            get => false;
            set
            {
                if (!value)
                    return;
                var savedData = new SavedGame
                {
                    FileName = CommonData.SavedGameFileName,
                    Money = 10000,
                    Level = _levelStaging.LevelIndex
                };
                _managers.ScoreManager.SaveGameProgress(savedData, false);
            }
        }
        
        [Category(CategoryCommon)]
        public bool Show_Advertising_Id
        {
            get => false;
            set
            {
                if (!value)
                    return;
                var idfaEntity = CommonUtils.GetIdfa();
                Cor.Run(Cor.WaitWhile(() => idfaEntity.Result == EEntityResult.Pending,
                    () => Dbg.Log(idfaEntity.Value)));
            }
        }
        
        [Category(CategoryCommon)]
        public bool Delete_Saved_Game
        {
            get => false;
            set
            {
                if (!value)
                    return;
                _managers.ScoreManager.DeleteSavedGame(CommonData.SavedGameFileName);
            }
        }

        [Category(CategoryCommon)]
        public bool Show_Money
        {
            get => false;
            set
            {
                if (!value)
                    return;
                static void DisplaySavedGameMoney(bool _FromCache)
                {
                    string str = _FromCache ? "server" : "cache";
                    var entity =
                        _managers.ScoreManager.GetSavedGameProgress(CommonData.SavedGameFileName, _FromCache);
                    Cor.Run(Cor.WaitWhile(
                        () => entity.Result == EEntityResult.Pending,
                        () =>
                        {
                            bool castSuccess = entity.Value.CastTo(out SavedGame savedGame);
                            if (entity.Result == EEntityResult.Fail || !castSuccess)
                            {
                                Dbg.LogWarning($"Failed to load saved game from {str}, entity value: {entity.Value}");
                                return;
                            }
                            Dbg.Log($"{str}: Money: {savedGame.Money}, Level: {savedGame.Level}");
                        }));
                }
                DisplaySavedGameMoney(false);
                DisplaySavedGameMoney(true);
            }
        }

        [Category(CategoryCommon)]
        public bool Internet_Connection_Available
        {
            get => false;
            set
            {
                if (!value)
                    return;
                bool res = NetworkUtils.IsInternetConnectionAvailable();
                Dbg.Log("Internet connection available: " + res + " " + Application.internetReachability);
            }
        }

        [Category(CategoryCommon)]
        public bool ClearSaves
        {
            get => false;
            set
            {
                if (!value)
                    return;
                if(System.IO.File.Exists(SaveUtils.SavesPath))
                    System.IO.File.Delete(SaveUtils.SavesPath);
            }
        }
        
        [Category(CategoryCommon)]
        public bool GC_Collect
        {
            get => false;
            set
            {
                if (!value)
                    return;
                GC.Collect();
            }
        }
        
                
        [Category(CategoryCommon)]
        public bool Running_Coroutines_Count
        {
            get => false;
            set
            {
                if (!value)
                    return;
                Dbg.Log("Running coroutines count: " + Cor.GetRunningCoroutinesCount());
            }
        }

        [Category(CategoryCommon)]
        public bool Test_Notification
        {
            get => false;
            set
            {
                if (!value)
                    return;
                _managers.NotificationsManager.SendNotification(
                    "Come back! We are waiting for you!", 
                    string.Empty, 
                    DateTime.Now.AddSeconds(10),
                    _SmallIcon: "main_icon");
            }
        }

        [Category(CategoryCommon)]
        public bool Send_Test_Analytic
        {
            get => false;
            set
            {
                if (!value)
                    return;
                _managers.AnalyticsManager.SendAnalytic(AnalyticIds.TestAnalytic);
            }
        }

        [Category(CategoryCommon)]
        public bool Get_FCM_Token
        {
            get => false;
            set
            {
                if (!value)
                    return;
                FirebaseMessaging.GetTokenAsync()
                    .ContinueWithOnMainThread(_Task =>
                    {
                        string token = _Task.Result;
                        Dbg.Log("FCM Token: " + token);
                        CommonUtils.CopyToClipboard(token);
                    });
            }
        }

        [Category(CategoryCommon)]
        public bool Throw_Test_Exception
        {
            get => false;
            set
            {
                if (!value)
                    return;
                
                throw new Exception("test exception, ignore this");
            }
        }

        #endregion

        #region monitor
        
        [Category(CategoryMonitor)]
        public bool Acceleration
        {
            get => false;
            set
            {
                _managers.DebugManager.Monitor(
                    "Acceleration", 
                    value, 
                    () => CommonUtils.GetAcceleration());
            }
        }

        #endregion
    
    }
}