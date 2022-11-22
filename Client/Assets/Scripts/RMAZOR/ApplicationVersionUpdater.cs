using System;
using System.IO;
using Common;
using Common.Utils;
using UnityEngine;

namespace RMAZOR
{
    public interface IApplicationVersionUpdater
    {
        void UpdateToCurrentVersion();
    }
    
    public class ApplicationVersionUpdater : IApplicationVersionUpdater
    {
        public void UpdateToCurrentVersion()
        {
            string prevAppVersion = SaveUtils.GetValue(SaveKeysCommon.AppVersion);
            if (prevAppVersion == null)
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
            SaveUtils.PutValue(SaveKeysCommon.AppVersion, Application.version);
        }
    }
}