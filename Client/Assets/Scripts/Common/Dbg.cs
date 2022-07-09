using System;
using Common.Exceptions;
using UnityEngine;
using UnityEngine.Events;

namespace Common
{
    public enum ELogLevel
    {
        Nothing,
        Exception,
        Error,
        Assert,
        Warning,
        Info
    }

    public static class Dbg
    {
        private static readonly ILogger   Logger    = Debug.unityLogger;
        private static          ELogLevel _logLevel = ELogLevel.Info;

        public static ELogLevel LogLevel
        {
            get => _logLevel;
            set
            {
                Logger.logEnabled = value != ELogLevel.Nothing;
                switch (value)
                {
                    case ELogLevel.Info:
                        Logger.filterLogType = LogType.Log;
                        break;
                    case ELogLevel.Warning:
                        Logger.filterLogType = LogType.Warning;
                        break;
                    case ELogLevel.Assert:
                        Logger.filterLogType = LogType.Assert;
                        break;
                    case ELogLevel.Error:
                        Logger.filterLogType = LogType.Error;
                        break;
                    case ELogLevel.Exception:
                        Logger.filterLogType = LogType.Exception;
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
            if (LogLevel >= ELogLevel.Info)
                Logger.Log(LogType.Log, "MGCI",  _Message);
        }

        public static void LogWarning(object _Message)
        {
            if (LogLevel >= ELogLevel.Warning)
                Logger.Log(LogType.Warning, "MGCW", _Message);
        }
        
        public static void LogAssert(object _Message)
        {
            if (LogLevel >= ELogLevel.Assert)
                Logger.Log(LogType.Warning, "MGCA", _Message);
        }

        public static void LogError(object _Message)
        {
            if (LogLevel >= ELogLevel.Error)
                Logger.Log(LogType.Error, "MGCE", _Message);
        }
        
        public static void LogException(Exception _Exception)
        {
            if (LogLevel < ELogLevel.Exception)
                return;
            Logger.Log(LogType.Exception, "MGCX", _Exception.Message);
            throw _Exception;
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