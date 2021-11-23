using System;
using System.Net;
using System.Text;
using System.Threading;
using DI.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace Utils
{
    public static class CommonUtils
    {
        

#if UNITY_ANDROID
        public static int GetAndroidSdkLevel() 
        {
            using (var version = new AndroidJavaClass("android.os.Build$VERSION")) 
            {
                return version.GetStatic<int>("SDK_INT");
            }
        }
#endif

        public static T MonoBehSingleton<T>(ref T _Instance, string _Name) where T : MonoBehaviour
        {
            if (_Instance is T ptm && !ptm.IsNull())
                return _Instance;
            
            var go = GameObject.Find(_Name);
            if (go == null || go.transform.parent != null)
                go = new GameObject(_Name);
            _Instance = go.GetOrAddComponent<T>();
            UnityEngine.Object.DontDestroyOnLoad(go);
            return _Instance;
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
         
            string uniqueId = Application.systemLanguage //Language
                              + "-" + Application.platform //Device    
                              + "-" + $"{Convert.ToInt32(timestamp):X}" //Time
                              + "-" + $"{Convert.ToInt32(Time.time * 1000000):X}" //Time in game
                              + "-" + $"{MathUtils.RandomGen.Next(1000000000):X}"; //random number
            return uniqueId;
        }
        
        public static void CopyToClipboard(this string _Text)
        {
            var te = new TextEditor {text = _Text};
            te.SelectAll();
            te.Copy();
        }

        public static string GetMd5Hash(string _StrToEncrypt)
        {
            UTF8Encoding ue = new UTF8Encoding();
            return GetMd5Hash(ue.GetBytes(_StrToEncrypt));
        }

        public static string GetMd5Hash(byte[] _Bytes)
        {
            return Md5.GetMd5String(_Bytes);
        }
    
        public static bool IsEmailAddressValid(string _Mail)
        {
            int atInd = _Mail.IndexOf("@", StringComparison.Ordinal);
            if (atInd <= 0)
                return false;
            int dotInd = _Mail.IndexOf(".", atInd, StringComparison.Ordinal);
            return dotInd - atInd > 1 && dotInd < _Mail.Length - 1;
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
            int _Index,
            out int _ID,
            out Vector2 _Position,
            out float _Pressure,
            out bool _Began, 
            out bool _Ended)
        {
#if ENABLE_INPUT_SYSTEM
            var touch = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[_Index];
            _ID       = touch.finger.index;
            _Position = touch.screenPosition;
            _Pressure = touch.pressure;
            _Began    = touch.phase == UnityEngine.InputSystem.TouchPhase.Began;
            _Ended    = touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled;
#else
			var touch = Input.GetTouch(_Index);
			_ID       = touch.fingerId;
			_Position = touch.position;
			_Pressure = touch.pressure;
            _Began    = touch.phase == TouchPhase.Began;
            _Ended    = touch.phase == TouchPhase.Ended;
#endif
        }
    }
}
