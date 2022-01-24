using Common.Exceptions;
using UnityEngine;
using UnityEngine.Events;

namespace Common
{
    public enum ELogLevel
    {
        Info      = 1,
        Warning   = 2,
        Assert    = 4,
        Error     = 8,
        Exception = 16,
        Nothing   = 32,
    }

    public static class Dbg
    {
        private static ELogLevel _logLevel = ELogLevel.Error;

        public static ELogLevel LogLevel
        {
            get => _logLevel;
            set
            {
                Debug.unityLogger.logEnabled = value < ELogLevel.Nothing;
                switch (value)
                {
                    case ELogLevel.Info:
                        Debug.unityLogger.filterLogType = LogType.Log;
                        break;
                    case ELogLevel.Warning:
                        Debug.unityLogger.filterLogType = LogType.Warning;
                        break;
                    case ELogLevel.Assert:
                        Debug.unityLogger.filterLogType = LogType.Assert;
                        break;
                    case ELogLevel.Error:
                        Debug.unityLogger.filterLogType = LogType.Error;
                        break;
                    case ELogLevel.Exception:
                        Debug.unityLogger.filterLogType = LogType.Exception;
                        break;
                    case ELogLevel.Nothing:
                        break;
                    default:
                        throw new SwitchCaseNotImplementedException(value);
                }
                _logLevel = value;
            }
        }
        
        public static void Log(object _Message)
        {
            if (LogLevel <= ELogLevel.Info)
                Debug.Log("MGCI: " + _Message);
        }

        public static void LogWarning(object _Message)
        {
            if (LogLevel <= ELogLevel.Warning)
                Debug.LogWarning("MGCW: " + _Message);
        }

        public static void LogError(object _Message)
        {
            if (LogLevel <= ELogLevel.Error)
                Debug.LogError("MGCE: " + _Message);
        }

        public static void LogZone(ELogLevel _LogLevel, UnityAction _Action)
        {
            var logLevel = LogLevel;
            LogLevel = _LogLevel;
            _Action?.Invoke();
            LogLevel = logLevel;
        }
    }
}