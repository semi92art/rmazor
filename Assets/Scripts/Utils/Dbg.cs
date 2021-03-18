using UnityEngine;

namespace Utils
{
    public static class Dbg
    {
        public static void Log(object _Message) => Debug.Log("MGCI: " + _Message);
        public static void LogWarning(object _Message) => Debug.LogWarning("MGCW: " + _Message);
        public static void LogError(object _Message) => Debug.LogError("MGCE: " + _Message);

        public static void LogError<T>(object _Message) where T : System.Exception
        {
            LogError(_Message);
            var exception = (T)System.Activator.CreateInstance(typeof(T), _Message.ToString());
            throw exception;
        }
    }
}