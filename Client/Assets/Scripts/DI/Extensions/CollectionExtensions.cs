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
        
        public static List<T> Clone<T>(this IEnumerable<T> _Collection) where T: ICloneable
        {
            return _Collection.Select(_Item => (T)_Item.Clone()).ToList();
        }

        public static bool ContainsRange<T>(this IEnumerable<T> _Collection, params T[] _Items)
        {
            return _Items.All(_Collection.Contains);
        }
        
        private static void MoveOnEnumerator<T>(IEnumerator<T> _Enumerator, int _Count)
        {
            foreach (var _ in Enumerable.Range(0, _Count))
            {
                _Enumerator.MoveNext();
            }
        }
        
        public static void Shuffle<T>(this IList<T> list)  
        {  
            var rng = new Random(); 
            int n = list.Count;  
            while (n > 1) {  
                n--;  
                int k = rng.Next(n + 1);  
                T value = list[k];  
                list[k] = list[n];  
                list[n] = value;  
            }  
        }
    }
}