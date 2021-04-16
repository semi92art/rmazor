using System.Linq;
using Entities;

namespace Extensions
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
    }
}