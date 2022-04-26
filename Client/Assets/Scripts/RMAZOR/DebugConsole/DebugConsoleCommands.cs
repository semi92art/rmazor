﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Enums;
using Common.Extensions;
using Common.Utils;
using Lean.Localization;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RMAZOR.DebugConsole
{
    public static class DebugConsoleCommands
    {
        public static IDebugConsoleController Controller;
        
        public static void RegisterCommands()
        {
            var commandArgsList = new List<DebugCommandArgs>
            {
                new DebugCommandArgs("restart",          Restart, "Restart game."),
                new DebugCommandArgs("load",             Load, "Reload specified level."),
                new DebugCommandArgs("reload",           Reload, "Reload current level."),
                new DebugCommandArgs("clc",              ClearConsole, "Clear console."),
                new DebugCommandArgs("set_lang",         SetLanguage, "set language"),
                new DebugCommandArgs("target_fps",       SetTargetFps, "Set target frame rate"),
                new DebugCommandArgs("wof_spin_enable",  EnableSpinButton, "Enable wheel of fortune spin."),
                new DebugCommandArgs("enable_ads",       EnableAds, "Enable or disable advertising (true/false)"),
                new DebugCommandArgs("load_level",       LoadLevel, "Load level by index"),
                new DebugCommandArgs("set_money",        SetMoney, "Set money count"),
                new DebugCommandArgs("rate_game_panel",  ShowRateGamePanel, "Show rate game panel"),
                new DebugCommandArgs("int_conn",         InternetConnectionAvailable, "Check if internet connection available"),
                new DebugCommandArgs("show_money",       ShowMoney, "Show money"),
                new DebugCommandArgs("run_cor",          GetRunningCoroutinesCount, "Show running coroutines count"),
                new DebugCommandArgs("dis_rot_coms",     ShowDisabledRotationCommands, "Show disabled rotation commands on this moment"),
                new DebugCommandArgs("dis_mov_coms",     ShowDisabledMovementCommands, "Show disabled movement commands on this moment"),
            };

            foreach (var args in commandArgsList)
                Controller.RegisterCommand(args);
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
        
        private static void EnableSpinButton(string[] _Args)
        {
            SaveUtils.PutValue(SaveKeysRmazor.WheelOfFortuneLastDate, DateTime.Now.Date.AddDays(-1));
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
            var entity = new BoolEntity
            {
                Result = EEntityResult.Success,
                Value = _Args[0] == "true"
            };
            Controller.AdsManager.ShowAds = entity;
        }

        private static void LoadLevel(string[] _Args)
        {
            if (_Args == null || !_Args.Any() || _Args.Length > 1 || !int.TryParse(_Args[0], out int levelIndex))
            {
                Controller.AppendLogLine("Wrong. Need level index!");
                return;
            }
            Controller.CommandsProceeder.RaiseCommand(
                EInputCommand.LoadLevelByIndex, 
                new object [] { levelIndex - 1 },
                true);
        }

        private static void SetMoney(string[] _Args)
        {
            if (_Args == null || !_Args.Any() || _Args.Length > 1 || !int.TryParse(_Args[0], out int moneyCount))
            {
                Controller.AppendLogLine("Wrong. Need money count!");
                return;
            }
            var entity = new SavedGame
            {
                FileName = CommonData.SavedGameFileName,
                Money = moneyCount,
                Level = Controller.Model.LevelStaging.LevelIndex
            };
            Controller.ScoreManager.SaveGameProgress(entity, false);
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
            static void DisplaySavedGameMoney(bool _FromCache)
            {
                var entity = Controller.ScoreManager.GetSavedGameProgress(
                    CommonData.SavedGameFileName, _FromCache);
                Cor.Run(Cor.WaitWhile(
                    () => entity.Result == EEntityResult.Pending,
                    () =>
                    {
                        bool castSuccess = entity.Value.CastTo(out SavedGame savedGame);
                        if (entity.Result == EEntityResult.Fail || !castSuccess)
                        {
                            Dbg.LogWarning($"Failed to load saved game, entity value: {entity.Value}");
                            return;
                        }
                        long money = savedGame.Money;
                        Dbg.Log($"Money {(_FromCache ? "server" : "cache")}: " + money);
                    }));
            }
            DisplaySavedGameMoney(false);
            DisplaySavedGameMoney(true);
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
    }
}