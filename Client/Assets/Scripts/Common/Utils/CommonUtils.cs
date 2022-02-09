using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace Common.Utils
{
    public static class CommonUtils
    {
        public static bool IsRunningOnDevice()
        {
            return new[] {RuntimePlatform.Android, RuntimePlatform.IPhonePlayer}.Contains(Application.platform);
        }

        public static string GetOsName()
        {
#if UNITY_ANDROID
            return "Android";
#elif UNITY_IPHONE || UNITY_IOS
            return "iOS";
#endif
            return null;
        }

        public static bool CheckForInternetConnection()
        {
            return CheckForConnection("http://google.com/generate_204");
        }

        private static bool CheckForConnection(string _Url)
        {
            int tryCount = 0;
            while (tryCount < 3)
            {
                try
                {
                    using (var client = new WebClient())
                    using (client.OpenRead(_Url)) 
                        return true; 
                }
                catch
                {
                    tryCount++;
                }
            }
            return false;
        }

        public static UnityAction WaitForSecs(float _Seconds, Action _OnFinish)
        {
            return () =>
            {
                int millisecs = Mathf.RoundToInt(_Seconds * 1000);
                Thread.Sleep(millisecs);
                _OnFinish?.Invoke();
            };
        }
        
        //https://answers.unity.com/questions/246116/how-can-i-generate-a-guid-or-a-uuid-from-within-un.html
        public static string GetUniqueId()
        {
            var epochStart = new DateTime(1970, 1, 1, 8, 0, 0, DateTimeKind.Utc);
            double timestamp = (DateTime.UtcNow - epochStart).TotalSeconds;
         
            string uniqueId = Application.systemLanguage                           //Language
                              + "-" + Application.platform                         //Device    
                              + "-" + $"{Convert.ToInt32(timestamp):X}"            //Time
                              + "-" + $"{Convert.ToInt32(Time.time * 1000000):X}"  //Time in game
                              + "-" + $"{MathUtils.RandomGen.Next(1000000000):X}"; //random number
            return uniqueId;
        }

        public static GameObject FindOrCreateGameObject(string _Name, out bool _WasFound)
        {
            var go = GameObject.Find(_Name);
            _WasFound = go != null;
            if (go == null)
                go = new GameObject(_Name);
            return go;
        }
        
        public static void GetTouch(
            int         _Index,
            out int     _ID,
            out Vector2 _Position,
            out float   _Pressure,
            out bool    _Began, 
            out bool    _Ended)
        {
#if ENABLE_INPUT_SYSTEM
            var touch = Touch.activeTouches[_Index];
            _ID       = touch.finger.index;
            _Position = touch.screenPosition;
            _Pressure = touch.pressure;
            _Began    = touch.phase == TouchPhase.Began;
            _Ended    = touch.phase == TouchPhase.Canceled;
#else
			var touch = Input.GetTouch(_Index);
			_ID       = touch.fingerId;
			_Position = touch.position;
			_Pressure = touch.pressure;
            _Began    = touch.phase == TouchPhase.Began;
            _Ended    = touch.phase == TouchPhase.Ended;
#endif
        }

        public static void ShowAlertDialog(string _Title, string _Text)
        {
#if UNITY_EDITOR
            EditorUtility.DisplayDialog(_Title, _Text, "OK");
#elif UNITY_ANDROID
            MTAssets.NativeAndroidToolkit.NativeAndroid.Dialogs
                .ShowSimpleAlertDialog(_Title, _Text);
#elif UNITY_IPHONE || UNITY_IOS
            var alert = new SA.iOS.UIKit.ISN_UIAlertController(
                _Title, _Text, SA.iOS.UIKit.ISN_UIAlertControllerStyle.Alert);
            var action = new SA.iOS.UIKit.ISN_UIAlertAction(
                "OK", SA.iOS.UIKit.ISN_UIAlertActionStyle.Default, () => { });
            alert.AddAction(action);
            alert.Present();
#endif
        }

        public static int StringToHash(string _S)
        {
            var hasher = MD5.Create();
            var hashed = hasher.ComputeHash(Encoding.UTF8.GetBytes(_S));
            return BitConverter.ToInt32(hashed, 0);
        }
    }
}