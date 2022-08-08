using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Common.Entities
{
    [Serializable]
    public struct V2Int : ICloneable
    {
        [JsonIgnore] public int X => x;
        [JsonIgnore] public int Y => y;
        
        [JsonProperty, SerializeField] private int x;
        [JsonProperty, SerializeField] private int y;

        [JsonConstructor] public V2Int(int       _X, int _Y) { x = _X; y = _Y; }
        public V2Int(Vector2Int                  _V) { x = _V.x; y = _V.y; }
        public static V2Int operator -(V2Int     _V)                 => new V2Int(-_V.x, -_V.y);
        public static V2Int operator +(V2Int     _A,   V2Int   _B)   => new V2Int(_A.x + _B.x, _A.y + _B.y);
        public static Vector2 operator +(V2Int   _A,   Vector2 _B)   => new Vector2(_A.x + _B.x, _A.y + _B.y);
        public static Vector2 operator +(Vector2 _A,   V2Int   _B)   => new Vector2(_A.x + _B.x, _A.y + _B.y);
        public static V2Int operator -(V2Int     _A,   V2Int   _B)   => new V2Int(_A.x - _B.x, _A.y - _B.y);
        public static V2Int operator *(V2Int     _A,   V2Int   _B)   => new V2Int(_A.x * _B.x, _A.y * _B.y);
        public static Vector2 operator *(float   _A,   V2Int   _B)   => new Vector2(_A * _B.x, _A * _B.y);
        public static Vector2 operator *(V2Int   _B,   float   _A)   => new Vector2(_A * _B.x, _A * _B.y);
        public static V2Int operator *(int       _A,   V2Int   _B)   => new V2Int(_A * _B.x, _A * _B.y);
        public static V2Int operator *(V2Int     _A,   int     _B)   => new V2Int(_A.x * _B, _A.y * _B);
        public static V2Int operator /(V2Int     _A,   int     _B)   => new V2Int(_A.x / _B, _A.y / _B);
        public static bool operator ==(V2Int     _Lhs, V2Int   _Rhs) => _Lhs.x == _Rhs.x && _Lhs.y == _Rhs.y;
        public static bool operator !=(V2Int     _Lhs, V2Int   _Rhs) => !(_Lhs == _Rhs);
        
        public              bool    Equals(V2Int  _Other) => x == _Other.x && y == _Other.y;
        public override     bool    Equals(object _Obj)   => _Obj is V2Int other && Equals(other);
        public override     int     GetHashCode()         { unchecked { return (X * 397) ^ Y; } }
        public override     string  ToString()            => $"({X}, {Y})";
        public              object  Clone()               => new V2Int(x, y);
        [JsonIgnore] public Vector2 Normalized            => ToVector2().normalized;

        [JsonIgnore]
        public V2Int NormalizedAlt
        {
            get
            {
                if (x != 0 && y != 0)
                {
                    Dbg.LogError($"Unable to calculate {nameof(NormalizedAlt)}");
                    return Zero;
                }
                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (x == 0 && y == 0)
                    return Zero;
                if (x == 0)
                    return y > 0 ? Up : Down;
                if (y == 0)
                    return x > 0 ? Right : Left;
                return default;
            }
        }
        
        public V2Int PlusX(int _X) => new V2Int(x + _X, y);
        public V2Int MinusX(int _X) => new V2Int(x - _X,y);
        public V2Int PlusY(int _Y) => new V2Int(x, Y + _Y);
        public V2Int MinusY(int _Y) => new V2Int(x, y - _Y);
        
        public static float Distance(V2Int _V1, V2Int _V2) 
            => Vector2Int.Distance(_V1.ToVector2Int(), _V2.ToVector2Int());
        public static Vector2 Lerp(V2Int _V1, V2Int _V2, float _C) =>
            Vector2.Lerp(_V1.ToVector2(), _V2.ToVector2(), _C);
        public static V2Int Floor(Vector2 _V) => new V2Int(Mathf.FloorToInt(_V.x), Mathf.FloorToInt(_V.y));
        public static V2Int Ceil(Vector2 _V) => new V2Int(Mathf.CeilToInt(_V.x), Mathf.CeilToInt(_V.y));
        public static V2Int Round(Vector2 _V) => new V2Int(Mathf.RoundToInt(_V.x), Mathf.RoundToInt(_V.y));
        
        
        public static V2Int Up => new V2Int(Vector2Int.up);
        public static V2Int Down => new V2Int(Vector2Int.down);
        public static V2Int Left => new V2Int(Vector2Int.left);
        public static V2Int Right => new V2Int(Vector2Int.right);
        public static V2Int One => new V2Int(Vector2Int.one);
        public static V2Int Zero => new V2Int(Vector2Int.zero);
        
        public static implicit operator Vector2(V2Int _V)    => _V.ToVector2();
        
        public static implicit operator Vector2Int(V2Int _V) => _V.ToVector2Int();
        
        public static explicit operator V2Int(Vector2 _V)
        {
            return new V2Int(Mathf.RoundToInt(_V.x), Mathf.RoundToInt(_V.y));
        }
        public static explicit operator V2Int(Vector2Int _V)
        {
            return new V2Int(Mathf.RoundToInt(_V.x), Mathf.RoundToInt(_V.y));
        }
        
        private Vector2Int ToVector2Int() => new Vector2Int(x, y);
        private Vector2    ToVector2()    => new Vector2(x, y);
    }
}