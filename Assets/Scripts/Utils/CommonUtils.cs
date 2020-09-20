using System.Net;
using System.Threading;
using UnityEngine;

namespace Utils
{
    public static class CommonUtils
    {
    
        public static bool CheckForMainServerInternetConnection()
        {
            return CheckForConnection("77.37.152.15");
        }
    
        public static bool CheckForInternetConnection()
        {
            return CheckForConnection("http://google.com/generate_204");
        }

        private static bool CheckForConnection(string url)
        {
            int tryCount = 0;
            while (tryCount < 3)
            {
                try
                {
                    using (var client = new WebClient())
                    using (client.OpenRead(url)) 
                        return true; 
                }
                catch
                {
                    tryCount++;
                }
            }
            return false;
        }

        public static System.Action WaitForSecs(float _Seconds, Bool _StopWaiting)
        {
            return () =>
            {
                int millisecs = Mathf.RoundToInt(_Seconds * 1000);
                Thread.Sleep(millisecs);
                _StopWaiting.Value = true;
            };
        }
    }
    
    public class Bool
    {
        public bool Value { get; set; }

        public static implicit operator bool(Bool b) => b.Value;
    }
}
