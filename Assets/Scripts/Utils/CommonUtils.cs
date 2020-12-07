using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Utils
{
    public static class CommonUtils
    {
        public const float SymbolWidth = 19;
        
        public static readonly System.Random RandomGen = new System.Random();

        public static bool IsNull<T>(this T _Item) where T : Component
        {
            return _Item == null || _Item.ToString() == "null";
        }
        
        public static string GetOsName()
        {
#if UNITY_ANDROID
            return "Android";
#elif UNITY_IPHONE
            return "iOS";
#endif
            throw new NotImplementedException();
        }

        public static void SetGoActive<T>(this T _Item, bool _Active) where T : Component
        {
            _Item.gameObject.SetActive(_Active);
        }

        public static void SetGoActive<T>(bool _Active, params T[] _Items) where T : Component
        {
            foreach (var item in _Items)
                item.gameObject.SetActive(_Active);
        }

        public static T[] EnumToList<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T))
                .Cast<T>().ToArray();
        }
        
        public static bool CheckForMainServerInternetConnection()
        {
            return CheckForConnection("77.37.152.15");
        }

        public static void SetEvenIfNotContainKey<T1, T2>(
            this Dictionary<T1, T2> _Dictionary,
            T1 _Key,
            T2 _Value)
        {
            if (_Dictionary.ContainsKey(_Key))
                _Dictionary[_Key] = _Value;
            else
                _Dictionary.Add(_Key, _Value);
        }

        public static string Shortened(this string _Text, int _Length, bool _Ellipsis = true)
        {
            return _Text.Substring(0, _Length) + (_Ellipsis ? "..." : string.Empty);
        }

        /// <summary>
        /// Generates random float value in range of 0.0 and 1.0
        /// </summary>
        /// <param name="_Random">Random generator</param>
        /// <returns>Random value in range of 0.0 and 1.0</returns>
        public static float NextFloat(this System.Random _Random)
        {
            return (float) _Random.NextDouble();
        }
        
        /// <summary>
        /// Generates random float value in range of -1.0 and 1.0
        /// </summary>
        /// <param name="_Random">Random generator</param>
        /// <returns>Random value in range of -1.0 and 1.0</returns>
        public static float NextFloatAlt(this System.Random _Random)
        {
            return 2f * ((float) _Random.NextDouble() - 0.5f);
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

        public static System.Action WaitForSecs(float _Seconds, System.Action _OnFinish)
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
            var epochStart = new System.DateTime(1970, 1, 1, 8, 0, 0, System.DateTimeKind.Utc);
            double timestamp = (System.DateTime.UtcNow - epochStart).TotalSeconds;
         
            string uniqueId = Application.systemLanguage //Language
                              + "-" + Application.platform //Device    
                              + "-" + string.Format("{0:X}", System.Convert.ToInt32(timestamp)) //Time
                              + "-" + string.Format("{0:X}", System.Convert.ToInt32(Time.time * 1000000)) //Time in game
                              + "-" + string.Format("{0:X}", RandomGen.Next(1000000000)); //random number
         
            Debug.Log($"Generated Unique ID: {uniqueId}");
            
            return uniqueId;
        }

        public static void IncWithOverflow(ref int _Value, int _Threshold)
        {
            if (++_Value >= _Threshold)
                _Value = 0;
        }
        
        public static void CopyToClipboard(this string _Text)
        {
            TextEditor te = new TextEditor();
            te.text = _Text;
            te.SelectAll();
            te.Copy();
        }

        public static string ToStringAlt(this Vector2 _Value)
        {
            return $"({_Value.x.ToString("F2").Replace(',', '.')}f, " +
                   $"{_Value.y.ToString("F2").Replace(',', '.')}f)";
        }
        
        public static T CreateObject<T>(Transform _Parent, string _Name) where T : Component
        {
            GameObject go = new GameObject(_Name);
            // link object to parent
            if (_Parent != null)
                go.transform.SetParent(_Parent, false);
            // attach script
            return go.AddComponent<T>();
        }
        
        public static string ToRoman(int _Number)
        {
            if (_Number < 0 || _Number >= 40) throw new System.ArgumentOutOfRangeException();
            if (_Number < 1) return string.Empty;
            if (_Number >= 10) return "X" + ToRoman(_Number - 10);
            if (_Number >= 9) return "IX" + ToRoman(_Number - 9);
            if (_Number >= 5) return "V" + ToRoman(_Number - 5);
            if (_Number >= 4) return "IV" + ToRoman(_Number - 4);
            if (_Number >= 1) return "I" + ToRoman(_Number - 1);
            throw new System.ArgumentOutOfRangeException();
        }
    
        public static string GetMD5Hash(string _StrToEncrypt)
        {
            UTF8Encoding ue = new UTF8Encoding();
            return GetMD5Hash(ue.GetBytes(_StrToEncrypt));
        }

        public static string GetMD5Hash(byte[] _Bytes)
        {
            return Md5.GetMd5String(_Bytes);
        }
    
        public static bool IsEmailAddressValid(string _Mail)
        {
            int atInd = _Mail.IndexOf("@", System.StringComparison.Ordinal);
            if (atInd <= 0)
                return false;
            int dotInd = _Mail.IndexOf(".", atInd,System.StringComparison.Ordinal);
            return dotInd - atInd > 1 && dotInd < _Mail.Length - 1;
        }
        
        public static bool IsInRange(long _Val, long _Min, long _Max)
        {
            return _Val >= _Min && _Val <= _Max;
        }
    
        public static bool IsInRange(int val, int min, int max)
        {
            return val >= min && val <= max;
        }
        
        public static Dictionary<TKey, TValue> Clone<TKey, TValue>
            (this Dictionary<TKey, TValue> _Original) where TValue : System.ICloneable
        {
            Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>(_Original.Count,
                _Original.Comparer);
            foreach (KeyValuePair<TKey, TValue> entry in _Original)
            {
                ret.Add(entry.Key, (TValue) entry.Value.Clone());
            }
            return ret;
        }
        
        public static Dictionary<TKey, TValue> CloneAlt<TKey, TValue>
            (this Dictionary<TKey, TValue> _Original) where TValue : struct
        {
            Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>(_Original.Count,
                _Original.Comparer);
            foreach (KeyValuePair<TKey, TValue> entry in _Original)
            {
                ret.Add(entry.Key, entry.Value);
            }
            return ret;
        }

        public static string ToNumeric(this long _Value)
        {
            return _Value.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"));
        }

        public static string ToNumeric(this int _Value)
        {
            return ((long) _Value).ToNumeric();
        }

        public static string ToTimer(this float _Value)
        {
            return $"{_Value:f1}";
        }
        
        public static bool IsFullyVisibleFrom(this RectTransform _Item, RectTransform _Rect)
        {
            if (!_Item.gameObject.activeInHierarchy)
                return false;
 
            return _Item.CountCornersVisibleFrom(_Rect) == 4;
        }
        
        public static bool IsVisibleFrom(this RectTransform _Item, RectTransform _Rect)
        {
            if (!_Item.gameObject.activeInHierarchy)
                return false;
            return _Item.CountCornersVisibleFrom(_Rect) > 0;
        }
        
        public static void SetActive(IEnumerable<GameObject> _Items, bool _Active)
        {
            foreach (var item in _Items)
                item.SetActive(_Active);
        }
        
        public static void SetActive<T>(IEnumerable<T> _Items, bool _Active) where T : Component
        {
            SetActive(_Items.Select(_Item => _Item.gameObject), _Active);
        }
        
        private static int CountCornersVisibleFrom(this RectTransform _Item, RectTransform _Rect)
        {
            Vector3[] itemCorners = new Vector3[4];
            _Item.GetWorldCorners(itemCorners);
            Vector3[] rectCorners = new Vector3[4];
            _Rect.GetWorldCorners(rectCorners);

            int count = 0;

            Vector2[] polygon = rectCorners.Select(_P => new Vector2(_P.x, _P.y)).ToArray();
            foreach (var corner in itemCorners)
            {
                Vector2 point = new Vector2(corner.x, corner.y);
                if (GeometryUtils.IsPointInPolygon(polygon, point))
                    count++;
            }
            
            return count;
        }
    }
}
