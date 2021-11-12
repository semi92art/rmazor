#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Linq;
using Constants;
using Entities;
using Games.RazorMaze;
using Games.RazorMaze.Models;
using Lean.Localization;
using Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace DebugConsole
{
    public static class DebugConsoleCommands
    {
        public static DebugConsoleController Controller;
        
        public static void RegisterCommands()
        {
            // When adding commands, you must add a call below to registerCommand() with its name,
            // implementation method, and description text.
            Controller.RegisterCommand("help", Help, "Print command list.");
            Controller.RegisterCommand("restart", Restart, "Restart game.");
            Controller.RegisterCommand("load", Load, "Reload specified level.");
            Controller.RegisterCommand("reload", Reload, "Reload current level.");
            Controller.RegisterCommand("clc", ClearConsole, "Clear console.");
            Controller.RegisterCommand("set_lang", SetLanguage,"set language");
            Controller.RegisterCommand("target_fps", SetTargetFps, "Set target frame rate");
            Controller.RegisterCommand("wof_spin_enable", EnableSpinButton, "Enable wheel of fortune spin.");
            Controller.RegisterCommand("enable_ads", EnableAds, "Enable or disable advertising (true/false)");
            Controller.RegisterCommand("finish_level", FinishLevel, "Finish current level");
            Controller.RegisterCommand("load_level", LoadLevel, "Load level by index");
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
                foreach (var kvp in Controller.Commands)
                    Controller.AppendLogLine($"{kvp.Key}: {kvp.Value.Description}");
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
                        foreach (var lang in Enum.GetValues(typeof(Language)))
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
            SaveUtils.PutValue(SaveKey.WheelOfFortuneLastDate, DateTime.Now.Date.AddDays(-1));
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
            AdsManager.Instance.ShowAds = _Args[0] == "true";
        }

        private static void FinishLevel(string[] _Args)
        {
            RazorMazeUtils.LoadNextLevelAutomatically = false;
            Controller.CommandsProceeder.RaiseCommand(EInputCommand.FinishLevel, null, true);
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
                new object [] { levelIndex },
                true);
        }
    }
}

#endif