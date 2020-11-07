
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Utils
{
    public static class Utility
    {
        public static System.Random RandomGenerator = new System.Random(); 
        
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
        public static string GetUniqueID()
        {
            var epochStart = new System.DateTime(1970, 1, 1, 8, 0, 0, System.DateTimeKind.Utc);
            double timestamp = (System.DateTime.UtcNow - epochStart).TotalSeconds;
         
            string uniqueId = Application.systemLanguage //Language
                              + "-" + Application.platform //Device    
                              + "-" + string.Format("{0:X}", System.Convert.ToInt32(timestamp)) //Time
                              + "-" + string.Format("{0:X}", System.Convert.ToInt32(Time.time * 1000000)) //Time in game
                              + "-" + string.Format("{0:X}", RandomGenerator.Next(1000000000)); //random number
         
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
    

        public static Vector2 HalfOne => Vector2.one * .5f;
        public static Color Transparent => new Color(0f, 0f, 0f, 0f);
    
        public static T CreateObject<T>(Transform _Parent, string _Name) where T : Component
        {
            GameObject go = new GameObject(_Name);
            // link object to parent
            if (_Parent != null)
                go.transform.SetParent(_Parent, false);
            // attach script
            return go.AddComponent<T>();
        }
    
        public static bool Intersect(Rect _R0, Rect _R1)
        {
            return _R0.xMin < _R1.xMax &&
                   _R0.xMax > _R1.xMin &&
                   _R0.yMin < _R1.yMax &&
                   _R0.yMax > _R1.yMin;
        }
    
        public static bool LineSegmentsIntersect(Vector2 _P1, Vector2 _P2, Vector2 _P3, Vector2 _P4, ref Vector2 _Result)
        {
            var d = (_P2.x - _P1.x) * (_P4.y - _P3.y) - (_P2.y - _P1.y) * (_P4.x - _P3.x);

            if (Mathf.Approximately(d, 0.0f))
                return false;

            var u = ((_P3.x - _P1.x) * (_P4.y - _P3.y) - (_P3.y - _P1.y) * (_P4.x - _P3.x)) / d;
            var v = ((_P3.x - _P1.x) * (_P2.y - _P1.y) - (_P3.y - _P1.y) * (_P2.x - _P1.x)) / d;

            if (u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f)
                return false;

            _Result.x = _P1.x + u * (_P2.x - _P1.x);
            _Result.y = _P1.y + u * (_P2.y - _P1.y);

            return true;
        }
    
        public static bool IsPointInPolygon(Vector2[] _PolyPoints, Vector2 _P)
        {
            var inside = false;
            for (int i = 0; i < _PolyPoints.Length; i++)
            {
                var p1 = _PolyPoints[i];
                var p2 = _PolyPoints[i == 0 ? _PolyPoints.Length - 1 : i - 1];
			
                if ((p1.y <= _P.y && _P.y < p2.y || p2.y <= _P.y && _P.y < p1.y) &&
                    _P.x < (p2.x - p1.x) * (_P.y - p1.y) / (p2.y - p1.y) + p1.x)
                    inside = !inside;
            }

            return inside;
        }
    
        public static bool Intersect(Vector2 _Position, Vector2[] _Vertices)
        {
            float x 		= _Position.x;
            float y 		= _Position.y;
            bool contains 	= false;
            int vertexCount = _Vertices.Length;
            for (int i = 0, j = vertexCount - 1; i < vertexCount; i++)
            {
                Vector2 a = _Vertices[i];
                Vector2 b = _Vertices[j];
                if ((a.y < y && b.y >= y || b.y < y && a.y >= y) && (a.x <= x || b.x <= x))
                {
                    if (a.x + (y - a.y) / (b.y - a.y) * (b.x - a.x) < x)
                        contains = !contains;
                }
                j = i;
            }
            return contains;
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
    
        public static Color CreateColor(int _R, int _G, int _B, int _A)
        {
            return new Color(_R/255.0f, _G/255.0f, _B/255.0f, _A/255.0f);
        }
	
        public static Color CreateColor(int _R, int _G, int _B)
        {
            return new Color(_R/255.0f, _G/255.0f, _B/255.0f, 1);
        }

        public static long Pow10(int _N)
        {
            return Pow(10, _N);
        }
	
        public static long Pow(long _Base, int _N)
        {
            long result = 1;

            while (_N > 0)
            {
                if ((_N & 1) != 0)
                {
                    result *= _Base;
                }

                _N    >>= 1;
                _Base *=  _Base;
            }

            return result;
        }

        public static bool IsInRange(long val, long min, long max)
        {
            return val >= min && val <= max;
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

        public static string ToNumeric(this int _Value)
        {
            return _Value.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"));
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
                if (IsPointInPolygon(polygon, point))
                    count++;
            }
            
            return count;
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
    }
}
