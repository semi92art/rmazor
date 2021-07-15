﻿using System;
using System.Linq;
using UnityEngine;

namespace Utils
{
    public static class MathUtils
    {
        public static long Lerp(long _A, long _B, float _T)
        {
            return (long)((1 - _T) * _A + _T * _B);
        }

        public static float Pow2(float _Value)
        {
            return Mathf.Pow(_Value, 2);
        }

        public static int Max(params int[] _Values)
        {
            return _Values.Max();
        }

        public static long Clamp(long _X, long _Min, long _Max)
        {
            return Math.Min(Math.Max(_X, _Min), _Max);
        }
        
        public static int Clamp(int _X, int _Min, int _Max)
        {
            return Math.Min(Math.Max(_X, _Min), _Max);
        }

        public static double Fraction(double _Value)
        {
            return _Value - Math.Truncate(_Value);
        }
        
        public static float Fraction(float _Value)
        {
            return Convert.ToSingle(Fraction(Convert.ToDouble(_Value)));
        }

        public static int ClampInverse(int _Val, int _Min, int _Max)
        {
            if (_Val > _Max) return _Min;
            if (_Val < _Min) return _Max;
            return _Val;
        }
    }
}