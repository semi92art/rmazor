using Newtonsoft.Json;
using UnityEngine;

namespace Entities
{
    public struct V2Int
    {
        [JsonProperty] public int X { get; set; }
        [JsonProperty] public int Y { get; set; }

        [JsonConstructor]
        public V2Int(int _X, int _Y)
        {
            X = _X;
            Y = _Y;
        }

        public V2Int(Vector2Int _V)
        {
            X = _V.x;
            Y = _V.y;
        }

        public Vector2Int ToVector2Int()
        {
            return new Vector2Int(X, Y);
        }

        public Vector2 ToVector2()
        {
            return new Vector2(X, Y);
        }

        public static V2Int operator +(V2Int _V1, V2Int _V2)
        {
            return new V2Int(_V1.X + _V2.X, _V1.Y + _V2.Y);
        }
        
        public static V2Int operator -(V2Int _V1, V2Int _V2)
        {
            return new V2Int(_V1.X - _V2.X, _V1.Y - _V2.Y);
        }

        public static bool operator ==(V2Int _V1, V2Int _V2)
        {
            return _V1.X == _V2.X && _V1.Y == _V2.Y;
        }

        public static bool operator !=(V2Int _V1, V2Int _V2)
        {
            return !(_V1 == _V2);
        }
        
        public bool Equals(V2Int other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is V2Int other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }
        
        public static V2Int up => new V2Int(Vector2Int.up);
        public static V2Int down => new V2Int(Vector2Int.down);
        public static V2Int left => new V2Int(Vector2Int.left);
        public static V2Int right => new V2Int(Vector2Int.right);
        public static V2Int one => new V2Int(Vector2Int.one);
    }
}