using UnityEngine;

namespace Common
{
    public enum ELogLevel
    {
        Nothing = 0,
        Info    = 1,
        Warning = 2,
        Error   = 4
    }

    public static class Dbg
    {
        public static ELogLevel LogLevel { get; set; } = ELogLevel.Error;
    
        public static void Log(object _Message)
        {
            if (LogLevel >= ELogLevel.Info)
                Debug.Log("MGCI: " + _Message);
        }

        public static void LogWarning(object _Message)
        {
            if (LogLevel >= ELogLevel.Warning)
                Debug.LogWarning("MGCW: " + _Message);
        }

        public static void LogError(object _Message)
        {
            if (LogLevel >= ELogLevel.Error)
                Debug.LogError("MGCE: " + _Message);
        }
    }
}