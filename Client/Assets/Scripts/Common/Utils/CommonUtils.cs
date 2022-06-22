using System;
using System.Security.Cryptography;
using System.Text;
using Common.Entities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace Common.Utils
{
    public static class CommonUtils
    {
        private static object @lock = new object();
        
        public static RuntimePlatform Platform 
        { 
            get
            {
#if UNITY_ANDROID
                 return RuntimePlatform.Android;
#elif UNITY_IOS || UNITY_IPHONE
                 return RuntimePlatform.IPhonePlayer;
#elif UNITY_STANDALONE_OSX
                 return RuntimePlatform.OSXPlayer;
#elif UNITY_STANDALONE_WIN
                 return RuntimePlatform.WindowsPlayer;
#endif
            }
        }
        
        public static string GetOsName()
        {
            switch (Platform)
            {
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
                case RuntimePlatform.Android:
                    return "Android";
                default: return null;
            }
        }
        
        //https://answers.unity.com/questions/246116/how-can-i-generate-a-guid-or-a-uuid-from-within-un.html
        public static string GetUniqueId()
        {
            var epochStart = new DateTime(1970, 1, 1, 8, 0, 0, DateTimeKind.Utc);
            double timestamp = (DateTime.UtcNow - epochStart).TotalSeconds;
            string uniqueId = Application.systemLanguage                           // Language
                              + "-" + Platform                                     // Device    
                              + "-" + $"{Convert.ToInt32(timestamp):X}"            // Time
                              + "-" + $"{Convert.ToInt32(Time.time * 1000000):X}"  // Time in game
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
// #elif UNITY_ANDROID
            var message = new SA.Android.App.AN_AlertDialog(SA.Android.App.AN_DialogTheme.Material)
            {
                Title = _Title,
                Message = _Text
            };
            message.SetPositiveButton("Ok", () => { });
            message.Show();
// #elif UNITY_IPHONE || UNITY_IOS
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
        
        public static void CopyToClipboard(string _Text)
        {
            var te = new TextEditor {text = _Text};
            te.SelectAll();
            te.Copy();
        }

        public static Entity<string> GetIdfa()
        {
            var result = new Entity<string>();
            Application.RequestAdvertisingIdentifierAsync(
                (_AdvertisingId, _Success, _Error) =>
                {
                    lock (@lock)
                    {
                        if (result.Result != EEntityResult.Pending)
                            return;
                        result.Value = _AdvertisingId;
                        result.Result = _Success ? EEntityResult.Success : EEntityResult.Fail;
                    }
                });
            Cor.Run(Cor.Delay(Application.platform == RuntimePlatform.Android ? 0.5f : 2f,
                null,
                () =>
                {
#if UNITY_ANDROID
                    MiniIT.Utils.AdvertisingIdFetcher.RequestAdvertisingId(_AdvertisingId =>
                    {
                        lock (@lock)
                        {
                            if (result.Result != EEntityResult.Pending)
                                return;
                            result.Value = _AdvertisingId;
                            result.Result = EEntityResult.Success;
                        }
                    });
#elif UNITY_IOS
                    lock (@lock)
                    {
                        result.Result = EEntityResult.Fail;
                    }
#endif
                }));
            return result;
        }

        public static Vector3 GetAcceleration()
        {
#if ENABLE_INPUT_SYSTEM
            return Accelerometer.current.acceleration.ReadValue();
#else
            return Input.acceleration;
#endif
        }

        public static void DoOnInitializedEx<T>(T _InitObject, UnityAction _Action) where T : IInit
        {
            if (_InitObject.Initialized)
                _Action?.Invoke();
            else
                _InitObject.Initialize += _Action;
        }
    }
}