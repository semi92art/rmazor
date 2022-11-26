using System;
using Common.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace Common.Entities
{
    [Serializable]
    public class V2 : ICloneable
    {
        [JsonIgnore] public float X => x;
        [JsonIgnore] public float Y => y;
        
        [JsonProperty, SerializeField] private float x;
        [JsonProperty, SerializeField] private float y;

        [JsonConstructor] public V2(float _X, float _Y) { x = _X; y = _Y; }
        public V2(Vector2Int              _V) { x = _V.x; y = _V.y; }

        
        public bool Equals(V2  _Other)
        {
            return Math.Abs(x - _Other.x) < MathUtils.Epsilon 
                   && Math.Abs(y - _Other.y) < MathUtils.Epsilon;
        }

        public override     bool    Equals(object _Obj)   => _Obj is V2 other && Equals(other);
        public override     int     GetHashCode()         { unchecked { return (int)((X * 397f) + Y); } }
        public override     string  ToString()            => $"({X}, {Y})";
        public              object  Clone()               => new V2(x, y);
        
        
        public static implicit operator Vector2(V2 _V)    => _V.ToVector2();
        
        public static explicit operator V2(Vector2 _V) => new V2(_V.x, _V.y);


        private Vector2    ToVector2()    => new Vector2(x, y);
    }
}