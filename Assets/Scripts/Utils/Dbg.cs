using UnityEngine;

namespace Utils
{
    public static class Dbg
    {
        public static void Log(object _Message)
        {
            Debug.Log("MGCI: " + _Message);
        }

        public static void LogWarning(object _Message)
        {
            Debug.LogWarning("MGCW: " + _Message);
        }

        public static void LogError(object _Message)
        {
            Debug.LogError("MGCE: " + _Message);
        }
    }
}