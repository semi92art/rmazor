using System;
using System.Linq;
using UnityEngine;

namespace Utils
{
    public static class MathUtils
    {
        public static readonly System.Random RandomGen = new System.Random();
        
        public static int Lerp(int _A, int _B, float _T)
        {
            return (int)((1 - _T) * _A + _T * _B);
        }
        
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
            return _Val > _Max ? _Min : _Val < _Min ? _Max : _Val;
        }
        
        public static bool IsInRange(long _Val, long _Min, long _Max)
        {
            return _Val >= _Min && _Val <= _Max;
        }
    
        public static bool IsInRange(int _Val, int _Min, int _Max)
        {
            return _Val >= _Min && _Val <= _Max;
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
    }
}