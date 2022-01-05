using System;
using System.Linq;
using Entities;

namespace DI.Extensions
{
    public static class NumericExtensions
    {
        public static bool InRange(this int _V, int _A, int _B)
        {
            return _V >= _A && _V <= _B;
        }

        public static bool InRange(this int _V, params V2Int[] _Values)
        {
            return _Values.Any(_Val => _V.InRange(_Val.X, _Val.Y));
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
    }
}