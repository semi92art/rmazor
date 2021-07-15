﻿using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Entities
{
    [Serializable]
    public struct V2Int : ICloneable
    {
        [JsonIgnore] public int X => x;
        [JsonIgnore] public int Y => y;
        
        [JsonProperty, SerializeField] private int x;
        [JsonProperty, SerializeField] private int y;

        [JsonConstructor] public V2Int(int _X, int _Y) { x = _X; y = _Y; }
        public V2Int(Vector2Int _V) { x = _V.x; y = _V.y; }
        public V2Int(Vector2 _V) { x = Mathf.RoundToInt(_V.x); y = Mathf.RoundToInt(_V.y); }
        public Vector2Int ToVector2Int() => new Vector2Int(X, Y);
        public Vector2 ToVector2() => new Vector2(X, Y);
        public static V2Int operator +(V2Int _V1, V2Int _V2) => new V2Int(_V1.X + _V2.X, _V1.Y + _V2.Y);
        public static V2Int operator -(V2Int _V) => new V2Int(-_V.X, -_V.Y);
        public static V2Int operator -(V2Int _V1, V2Int _V2) => new V2Int(_V1.X - _V2.X, _V1.Y - _V2.Y);
        public static bool operator ==(V2Int _V1, V2Int _V2) => _V1.X == _V2.X && _V1.Y == _V2.Y;
        public static bool operator !=(V2Int _V1, V2Int _V2) => !(_V1 == _V2);
        public bool Equals(V2Int other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is V2Int other && Equals(other);
        public override int GetHashCode() { unchecked { return (X * 397) ^ Y; } }
        public override string ToString() => $"({X}, {Y})";
        public object Clone() => new V2Int(X, Y);
        [JsonIgnore] public Vector2 Normalized => ToVector2().normalized;
        
        public V2Int PlusX(int _X) => new V2Int(X + _X, Y);
        public V2Int MinusX(int _X) => new V2Int(X - _X, Y);
        public V2Int PlusY(int _Y) => new V2Int(X, Y + _Y);
        public V2Int MinusY(int _Y) => new V2Int(X, Y - _Y);
        
        public static float Distance(V2Int _V1, V2Int _V2) => Vector2Int.Distance(_V1.ToVector2Int(), _V2.ToVector2Int());
        public static V2Int up => new V2Int(Vector2Int.up);
        public static V2Int down => new V2Int(Vector2Int.down);
        public static V2Int left => new V2Int(Vector2Int.left);
        public static V2Int right => new V2Int(Vector2Int.right);
        public static V2Int one => new V2Int(Vector2Int.one);
        public static V2Int zero => new V2Int(Vector2Int.zero);
    }
}