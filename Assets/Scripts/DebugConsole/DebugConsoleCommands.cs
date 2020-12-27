#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Linq;
using Constants;
using Controllers;
using Entities;
using Lean.Localization;
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
            Controller.RegisterCommand("auth_google", AuthWithGoogle, "Authenticate with Google");
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
                Debug.Log("Print: command list");
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
                        case "menu":
                            SceneManager.LoadScene(SceneNames.Main);
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
            System.Array.Clear(Controller.Log, 0, Controller.Log.Length);
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
                        foreach (var lang in System.Enum.GetValues(typeof(Language)))
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
            SaveUtils.PutValue(SaveKey.WheelOfFortuneLastDate, System.DateTime.Now.Date.AddDays(-1));
        }

        private static void AuthWithGoogle(string[] _Args)
        {
            var auth = new AuthController();
            auth.AuthenticateWithGoogle();
        }
    }
}

#endif