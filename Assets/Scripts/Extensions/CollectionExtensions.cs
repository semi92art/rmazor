using System.Collections.Generic;
using System.Linq;

namespace Extensions
{
    public static class CollectionExtensions
    {
        public static T GetItem<T>(this Stack<T> _Stack, int _Index, bool _FromTop = true)
        {
            var list = _Stack.ToList();
            return _Index > list.Count - 1 ? default : list[_FromTop ? _Index : list.Count - _Index - 1];
        }
    }
}