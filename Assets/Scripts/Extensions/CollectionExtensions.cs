using System;
using System.Collections.Generic;
using System.Linq;

namespace Extensions
{
    public static class CollectionExtensions
    {
        public static T GetItem<T>(this Stack<T> _Stack, int _Index, bool _FromTop = true)
        {
            var list = new List<T>();
            while (_Stack.Any())
                list.Add(_Stack.Pop());
            for (int i = list.Count - 1; i >= 0; i--)
                _Stack.Push(list[i]);
            if (_Index > list.Count - 1 || _Index < 0)
                return default;
            return list[_FromTop ? _Index : list.Count - _Index - 1];
        }

        public static ICollection<T> RemoveRange<T>(this ICollection<T> _Collection, IEnumerable<T> _Items)
        {
            var result = _Collection.ToList();
            foreach (var item in _Items)
                result.Remove(item);
            return result;
        }
        
        public static List<T> Clone<T>(this IList<T> listToClone) where T: ICloneable
        {
            return listToClone.Select(_Item => (T)_Item.Clone()).ToList();
        }
    }
}