using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Lean.Localization;
using mazing.common.Runtime;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Utils;
using RMAZOR.Constants;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
using UnityEngine;
using UnityEngine.SceneManagement;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.DebugConsole
{
    public static class DebugConsoleCommands
    {
        public static IDebugConsoleController Controller;
        
        public static void RegisterCommands()
        {
            var commandArgsList = new List<DebugCommandArgs>
            {
                new DebugCommandArgs(
                    "help",             
                    Help, 
                    "Print command list."),
                new DebugCommandArgs(
                    "restart",         
                    Restart, 
                    "Restart game."),
                new DebugCommandArgs(
                    "load",           
                    Load, 
                    "Reload specified level."),
                new DebugCommandArgs(
                    "reload",        
                    Reload, 
                    "Reload current level."),
                new DebugCommandArgs(
                    "clc",            
                    ClearConsole, 
                    "Clear console."),
                new DebugCommandArgs(
                    "set_lang",    
                    SetLanguage, 
                    "set language"),
                new DebugCommandArgs(
                    "target_fps",    
                    SetTargetFps, 
                    "Set target frame rate"),
                new DebugCommandArgs(
                    "enable_ads",  
                    EnableAds, 
                    "Enable or disable advertising (true/false)"),
                new DebugCommandArgs(
                    "load_level",   
                    LoadLevel, 
                    "Load level by index"),
                new DebugCommandArgs(
                    "load_next_level",   
                    LoadNextLevel, 
                    "Load next level"),
                new DebugCommandArgs(
                    "load_prev_level",   
                    LoadPreviousLevel, 
                    "Load previous level"),
                new DebugCommandArgs(
                    "set_money",   
                    SetMoney, 
                    "Set money count"),
                new DebugCommandArgs(
                    "rate_game_panel",
                    ShowRateGamePanel, 
                    "Show rate game panel"),
                new DebugCommandArgs(
                    "int_conn",     
                    InternetConnectionAvailable,
                    "Check if internet connection available"),
                new DebugCommandArgs(
                    "show_money",    
                    ShowMoney,
                    "Show money"),
                new DebugCommandArgs(
                    "run_cor",     
                    GetRunningCoroutinesCount,
                    "Show running coroutines count"),
                new DebugCommandArgs(
                    "dis_rot_coms",
                        ShowDisabledRotationCommands,
                    "Show disabled rotation commands on this moment"),
                new DebugCommandArgs(
                    "dis_mov_coms",
                    ShowDisabledMovementCommands, 
                    "Show disabled movement commands on this moment"),
                new DebugCommandArgs(
                    "mute_music",    
                    MuteGameMusic, 
                    "Mute/unmute game music, true/false"),
                new DebugCommandArgs(
                    "show_ad",      
                    ShowAd, 
                    "Show ad rewarded/interstitial"),
                new DebugCommandArgs(
                    "record_fps",
                    RecordFps,
                    "Record fps values, first argument - duration in seconds, example: record_fps 5"),
                new DebugCommandArgs(
                    "send_test_analytic",
                    SendTestAnalytic,
                    "Send test analytic"),
                new DebugCommandArgs(
                    "set_character",
                    SetCharacter,
                    "Set character by id")
            };
            foreach (var args in commandArgsList)
                Controller.RegisterCommand(args);
        }

        private static void ShowAd(string[] _Args)
        {
            string adType = _Args[0];
            switch (adType)
            {
                case "rewarded":
                    Controller.AdsManager.ShowRewardedAd();
                    break;
                case "interstitial":
                    Controller.AdsManager.ShowInterstitialAd();
                    break;
            }
        }

        private static void Reload(string[] _Args)
        {
            if (_Args.Length == 0)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            else if (_Args.Any(_Arg => _Arg == "-h"))
                Controller.AppendLogLine("Reload current level.");
        }

        private static void Help(string[] _Args)
        {
            if (_Args.Length == 0)
            {
                Dbg.Log("Print: command list");
                Controller.AppendLogLine("Current command list:");
                foreach ((string key, var value) in Controller.Commands)
                    Controller.AppendLogLine($"[{key}]: {value.Description}");
            }
            else if (_Args.Any(_Arg => _Arg == "woodpecker"))
                Controller.AppendLogLine("Work is filled up, woodpeckers!");
        }

        private static void Restart(string[] _Args)
        {
            if (_Args.Length == 0)
                SceneManager.LoadScene(0);
            else if (_Args.Any(_Arg => _Arg == "-h"))
                Controller.AppendLogLine("Reload game.");
        }

        private static void Load(string[] _Args)
        {
            if (_Args.Length == 0)
                Controller.AppendLogLine("Specify level");
            else
            {
                foreach (string arg in _Args)
                {
                    if (arg == "-h")
                    {
                        Controller.AppendLogLine("Load level:");
                        Controller.AppendLogLine("preload: restart game.");
                        Controller.AppendLogLine("menu: load menu.");
                        Controller.AppendLogLine("level: load field.");
                        break;
                    }

                    switch (arg)
                    {
                        case "preload":
                            SceneManager.LoadScene(SceneNames.Preload);
                            break;
                        case "level":
                            SceneManager.LoadScene(SceneNames.Level);
                            break;
                        default:
                            Controller.AppendLogLine("No such level.");
                            break;
                    }
                }
            }
        }
        
        private static void ClearConsole(string[] _Args)
        {
            Array.Clear(Controller.Log, 0, Controller.Log.Length);
            Controller.Scrollback.Clear();
            Controller.RaiseLogChangedEvent(Controller.Log);
        }

        private static void SetLanguage(string[] _Args)
        {
            if (_Args.Length == 0)
                Controller.AppendLogLine("Specify language");
            else
            {
                foreach (string arg in _Args)
                {
                    if (arg == "-h")
                    {
                        Controller.AppendLogLine("Current language list:");
                        foreach (var lang in Enum.GetValues(typeof(ELanguage)))
                        {
                            Controller.AppendLogLine(lang.ToString());
                        }
                        break;
                    }

                    LeanLocalization.CurrentLanguage = arg;
                }
            }  
        }

        private static void SetTargetFps(string[] _Args)
        {
            if (_Args == null || !int.TryParse(_Args[0], out int fps)) 
                return;
            if (fps >= 5 && fps <= 120)
                Application.targetFrameRate = fps;
            else
                Controller.AppendLogLine("Target FPS must be in range [5,120]");
        }

        private static void EnableAds(string[] _Args)
        {
            if (_Args == null || !_Args.Any())
            {
                Controller.AppendLogLine(@"Argument need! ""true"" or ""false""");
                return;
            }
            if (_Args[0] != "true" && _Args[0] != "false")
            {
                Controller.AppendLogLine(@"Wrong argument! Need ""true"" or ""false""");
                return;
            }
            Controller.AdsManager.ShowAds = _Args[0] == "true";
        }

        private static void LoadLevel(string[] _Args)
        {
            if (_Args == null || !_Args.Any() || _Args.Length > 1 || !int.TryParse(_Args[0], out int levelIndex))
            {
                Controller.AppendLogLine("Wrong. Need level index!");
                return;
            }
            levelIndex -= 1;
            string gameMode = (string) Controller.Model.LevelStaging.Arguments.GetSafe(KeyGameMode, out _);
            var args = new Dictionary<string, object>
            {
                {KeyNextLevelType, ParameterLevelTypeDefault},
                {KeyGameMode, gameMode},
                {KeyLevelIndex, levelIndex}
            };
            Controller.CommandsProceeder.RaiseCommand(
                EInputCommand.LoadLevel, 
                args,
                true);
        }

        private static void LoadNextLevel(string[] _Args)
        {
            long levelIndex = Controller.Model.LevelStaging.LevelIndex + 1;
            string gameMode = (string) Controller.Model.LevelStaging.Arguments.GetSafe(KeyGameMode, out _);
            var args = new Dictionary<string, object>
            {
                {KeyNextLevelType, ParameterLevelTypeDefault},
                {KeyGameMode, gameMode},
                {KeyLevelIndex, levelIndex}
            };
            Controller.CommandsProceeder.RaiseCommand(EInputCommand.LoadLevel, args, true);
        }
        
        private static void LoadPreviousLevel(string[] _Args)
        {
            long levelIndex = Controller.Model.LevelStaging.LevelIndex - 1;
            string gameMode = (string) Controller.Model.LevelStaging.Arguments.GetSafe(KeyGameMode, out _);
            var args = new Dictionary<string, object>
            {
                {KeyNextLevelType, ParameterLevelTypeDefault},
                {KeyGameMode, gameMode},
                {KeyLevelIndex, levelIndex}
            };
            Controller.CommandsProceeder.RaiseCommand(
                EInputCommand.LoadLevel, 
                args,
                true);
        }
        
        private static void SetMoney(string[] _Args)
        {
            if (_Args == null || !_Args.Any() || _Args.Length > 1 || !int.TryParse(_Args[0], out int moneyCount))
            {
                Controller.AppendLogLine("Wrong. Need money count!");
                return;
            }
            var savedGame = Controller.ScoreManager.GetSavedGame(MazorCommonData.SavedGameFileName);
            savedGame.Arguments.SetSafe(KeyMoneyCount, moneyCount);
            Controller.ScoreManager.SaveGame(savedGame);
        }
        
        private static void ShowRateGamePanel(string[] _Args)
        {
            Controller.CommandsProceeder.RaiseCommand(EInputCommand.RateGamePanel, null, true);
        }

        private static void InternetConnectionAvailable(string[] _Args)
        {
            bool res = NetworkUtils.IsInternetConnectionAvailable();
            Dbg.Log("Internet connection available: " + res);
        }

        private static void ShowMoney(string[] _Args)
        {
            var savedGame = Controller.ScoreManager.GetSavedGame(MazorCommonData.SavedGameFileName);
            object bankMoneyCountArg = savedGame.Arguments.GetSafe(KeyMoneyCount, out _);
            long money = Convert.ToInt64(bankMoneyCountArg);
            Dbg.Log("Money: " + money);
        }
        
        private static void GetRunningCoroutinesCount(string[] _Args)
        {
            Dbg.Log("Running coroutines count: " + Cor.GetRunningCoroutinesCount());
        }

        private static void ShowDisabledRotationCommands(string[] _Args)
        {
            var groupNames = (Controller.CommandsProceeder as ViewInputCommandsProceeder)?
                .LockedCommands
                .Where(_Kvp => RmazorUtils.RotateCommands.Any(
                    _Command => _Kvp.Value.Contains(_Command)))
                .Select(_Kvp => _Kvp.Key).ToList();
            var sb = new StringBuilder();
            sb.AppendLine("Locked commands by groups: ");
            if (groupNames != null)
                foreach (string gName in groupNames)
                    sb.AppendLine(gName);
            Dbg.Log(sb.ToString());
        }
        
        private static void ShowDisabledMovementCommands(string[] _Args)
        {
            var groupNames = (Controller.CommandsProceeder as ViewInputCommandsProceeder)?
                .LockedCommands
                .Where(_Kvp => RmazorUtils.MoveCommands.Any(
                    _Command => _Kvp.Value.Contains(_Command)))
                .Select(_Kvp => _Kvp.Key).ToList();
            var sb = new StringBuilder();
            sb.AppendLine("Locked commands by groups: ");
            if (groupNames != null)
                foreach (string gName in groupNames)
                    sb.AppendLine(gName);
            Dbg.Log(sb.ToString());
        }

        private static void MuteGameMusic(string[] _Args)
        {
            bool mute = Convert.ToBoolean(_Args[0]);
            if (mute)
                Controller.AudioManager.MuteAudio(EAudioClipType.Music);
            else 
                Controller.AudioManager.UnmuteAudio(EAudioClipType.Music);
        }
        
        private static void RecordFps(string[] _Args)
        {
            if (_Args == null || !_Args.Any() || _Args.Length > 1 || !int.TryParse(_Args[0], out int seconds))
            {
                Controller.AppendLogLine("Wrong. First argument must be numeric!");
                return;
            }
            Controller.FpsCounter.Record(seconds);
            Cor.Run(Cor.Delay(
                seconds + 0.1f, 
                null, 
                () =>
            {
                var recording = Controller.FpsCounter.GetRecording();
                string recordingString = recording.ToString();
                CommonUtils.CopyToClipboard(recordingString);
                Dbg.Log(recordingString);
            }));
        }

        private static void SendTestAnalytic(string[] _Args)
        {
            var eventData = new Dictionary<string, object>
            {
                {AnalyticIds.Parameter1ForTestAnalytic, Mathf.RoundToInt(UnityEngine.Random.value * 100)}
            };
            Controller.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.TestAnalytic, eventData);
        }
        
        private static void SetCharacter(string[] _Args)
        {
            if (_Args == null || !_Args.Any() || _Args.Length > 1 || !int.TryParse(_Args[0], out int characterId))
            {
                Controller.AppendLogLine("Wrong character id format.");
                return;
            }
            var args = new Dictionary<string, object>
            {
                {KeyCharacterId, characterId}
            };
            Controller.CommandsProceeder.RaiseCommand(EInputCommand.SelectCharacter, args);
        }
    }
}