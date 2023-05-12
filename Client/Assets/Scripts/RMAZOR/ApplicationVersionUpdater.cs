using System;
using System.Collections.Generic;
using System.IO;
using Common;
using Common.Entities;
using Common.Managers.PlatformGameServices;
using mazing.common.Runtime;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Utils;
using UnityEngine;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR
{
    public interface IApplicationVersionUpdater
    {
        void UpdateToCurrentVersion();
    }
    
    public class ApplicationVersionUpdater : IApplicationVersionUpdater
    {
        #region inject

        private IScoreManager ScoreManager { get; }

        public ApplicationVersionUpdater(IScoreManager _ScoreManager)
        {
            ScoreManager = _ScoreManager;
        }

        #endregion

        #region api

        public void UpdateToCurrentVersion()
        {
            string prevAppVersion = SaveUtils.GetValue(SaveKeysMazor.AppVersion);
            UpdateTo_2_0_0(prevAppVersion);
            SaveAppVersion(prevAppVersion);
        }

        #endregion

        #region nonpublic methods

        private static void SaveAppVersion(string _PrevAppVersion)
        {
            if (_PrevAppVersion == null)
            {
                try
                {
                    if (File.Exists(SaveUtils.SavesPath))
                        File.Delete(SaveUtils.SavesPath);
                }
                catch (Exception ex)
                {
                    Dbg.LogError(ex);                    
                }
            }
            SaveUtils.PutValue(SaveKeysMazor.AppVersion, Application.version);
        }
        

        private void UpdateTo_2_0_0(string _PrevAppVersion)
        {
            DeconstructAppVersion(_PrevAppVersion, out int? major, out int? minor, out int? patch);
            if (!major.HasValue 
                || !minor.HasValue 
                || !patch.HasValue 
                || major >= 2)
            {
                return;
            }
#pragma warning disable 612
            var entity = ScoreManager.GetSavedGameProgress(CommonDataMazor.SavedGameFileName);
#pragma warning restore 612
            if (entity == null)
                return;
            Cor.Run(Cor.WaitWhile(
                () => entity.Result == EEntityResult.Pending,
                () =>
                {
                    if (entity.Result == EEntityResult.Fail)
                        return;
                    bool castSuccess = entity.Value.CastTo(out SavedGame savedGameOldVer);
                    if (!castSuccess || savedGameOldVer == null)
                        return;
                    var savedGameNewVer = new SavedGameV2 {Arguments = new Dictionary<string, object>()};
                    if (savedGameOldVer.Args != null)
                        foreach ((string key, var value) in savedGameOldVer.Args)
                            savedGameNewVer.Arguments.SetSafe(key, value);
                    var levelTypeKeys = new[] {KeyCurrentLevelType, KeyNextLevelType};
                    foreach (string levelTypeKey in levelTypeKeys)
                    {
                        if (!savedGameNewVer.Arguments.ContainsKey(levelTypeKey))
                            continue;
                        string currentLevelType = (string)savedGameNewVer.Arguments[levelTypeKey];
                        if (currentLevelType == "main")
                            currentLevelType = ParameterLevelTypeDefault;
                        savedGameNewVer.Arguments[levelTypeKey] = currentLevelType;
                    }
                    savedGameNewVer.Arguments.SetSafe(KeyLevelIndexMainLevels, savedGameOldVer.Level);
                    savedGameNewVer.Arguments.SetSafe(KeyMoneyCount, savedGameOldVer.Money);
                    ScoreManager.SaveGame(savedGameNewVer);
                }));
        }

        private static void DeconstructAppVersion(
            string _Version,
            out int? _Major,
            out int? _Minor, 
            out int? _Patch)
        {
            if (string.IsNullOrEmpty(_Version))
            {
                _Major = _Minor = _Patch = null;
                return;
            }
            var verStrArr = _Version.Split('.');
            int.TryParse(verStrArr[0], out int major);
            int.TryParse(verStrArr[1], out int minor);
            int.TryParse(verStrArr[2], out int patch);
            (_Major, _Minor, _Patch) = (major, minor, patch);
        }

        #endregion
    }
}