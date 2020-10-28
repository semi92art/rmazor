using System;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Utils
{
    public static class Utility
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

        public static Action WaitForSecs(float _Seconds, Action _OnFinish)
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
            if (_Number < 0 || _Number >= 40) throw new ArgumentOutOfRangeException();
            if (_Number < 1) return string.Empty;
            if (_Number >= 10) return "X" + ToRoman(_Number - 10);
            if (_Number >= 9) return "IX" + ToRoman(_Number - 9);
            if (_Number >= 5) return "V" + ToRoman(_Number - 5);
            if (_Number >= 4) return "IV" + ToRoman(_Number - 4);
            if (_Number >= 1) return "I" + ToRoman(_Number - 1);
            throw new ArgumentOutOfRangeException();
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
            int atInd = _Mail.IndexOf("@", StringComparison.Ordinal);
            if (atInd <= 0)
                return false;
		
            int dotInd = _Mail.IndexOf(".", atInd, StringComparison.Ordinal);
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
    
        public static bool IsNull<T>(T _Object)
        {
            return _Object == null;
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
    }
}
