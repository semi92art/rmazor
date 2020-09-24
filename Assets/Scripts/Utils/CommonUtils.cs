using System;
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
        
        //https://answers.unity.com/questions/246116/how-can-i-generate-a-guid-or-a-uuid-from-within-un.html
        public static string GetUniqueID()
        {
            string key = "ID";
 
            var random = new System.Random();                     
            var epochStart = new DateTime(1970, 1, 1, 8, 0, 0, System.DateTimeKind.Utc);
            double timestamp = (DateTime.UtcNow - epochStart).TotalSeconds;
         
            string uniqueId = Application.systemLanguage //Language
                              + "-" + Application.platform //Device    
                              + "-" + String.Format("{0:X}", Convert.ToInt32(timestamp)) //Time
                              + "-" + String.Format("{0:X}", Convert.ToInt32(Time.time * 1000000)) //Time in game
                              + "-" + String.Format("{0:X}", random.Next(1000000000)); //random number
         
            Debug.Log($"Generated Unique ID: {uniqueId}");
            
            return uniqueId;
        }

        public static void IncWithOverflow(ref int _Value, int _Threshold)
        {
            if (++_Value >= _Threshold)
                _Value = 0;
        }
    }
    
    public class Bool
    {
        public bool Value { get; set; }

        public static implicit operator bool(Bool b) => b.Value;
    }
}