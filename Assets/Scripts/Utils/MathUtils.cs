using System.Linq;
using UnityEngine;

namespace Utils
{
    public class MathUtils
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
    }
}