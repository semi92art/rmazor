using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.Utils;

namespace Common.SpawnPools
{
public abstract class SpawnPoolBase<T> : ISpawnPool<T>
    {
        #region nonpublic members

        protected readonly List<T> Collection = new List<T>();

        #endregion

        #region api

        #region inherited interface

        public int Count => Collection.Count;

        public bool IsReadOnly => false;

        public IEnumerator<T> GetEnumerator()
        {
            return Collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T _Item)
        {
            if (_Item == null)
                return;
            Collection.Add(_Item);
        }

        public void AddRange(IEnumerable<T> _Items)
        {
            foreach (var item in _Items)
                Add(item);
        }

        public virtual void Clear()
        {
            Collection.Clear();
        }

        public bool Contains(T _Item)
        {
            return Collection.Contains(_Item);
        }

        public void CopyTo(T[] _Array, int _ArrayIndex)
        {
            Collection.ToList().CopyTo(_Array, _ArrayIndex);
        }

        public abstract bool Remove(T _Item);

        public int IndexOf(T _Item)
        {
            return Collection.ToList().IndexOf(_Item);
        }

        public void Insert(int _Index, T _Item)
        {
            if (_Item == null)
                return;
            Collection.Insert(_Index, _Item);
            Activate(Collection[_Index], false);
        }

        public virtual void RemoveAt(int _Index)
        {
            Collection.RemoveAt(_Index);
        }

        public T this[int _Index]
        {
            get => Collection[_Index];
            set => Collection[_Index] = value;
        }

        #endregion

        public abstract int CountActivated { get; }
        public int CountDeactivated => Collection.Count - CountActivated;
        public T FirstActive => GetFirstOrLastActiveOrInactive(true, true);
        public T FirstInactive => GetFirstOrLastActiveOrInactive(true, false);
        public T LastActive => GetFirstOrLastActiveOrInactive(false, true);
        public T LastInactive => GetFirstOrLastActiveOrInactive(false, false);

        public virtual void Activate(T _Item, Func<bool> _Predicate = null, Action _OnFinish = null)
        {
            Activate(IndexOf(_Item), _Predicate, _OnFinish);
        }

        public virtual void Deactivate(T _Item, Func<bool> _Predicate = null, Action _OnFinish = null)
        {
            Deactivate(IndexOf(_Item), _Predicate, _OnFinish);
        }

        #endregion
    
        #region nonpublic methods
    
        private void Activate(int _Index, Func<bool> _Predicate = null, Action _OnFinish = null)
        {
            ActivateOrDeactivate(_Index, _Predicate, _OnFinish, true);
        }
    
        private void Deactivate(int _Index, Func<bool> _Predicate = null, Action _OnFinish = null)
        {
            ActivateOrDeactivate(_Index, _Predicate, _OnFinish, false);
        }

        private void ActivateOrDeactivate(int _Index, Func<bool> _Predicate, Action _OnFinish, bool _Activate)
        {
            if (_Predicate == null)
            {
                ActivateOrDeactivate(_Index, _Activate);
                _OnFinish?.Invoke();
                return;
            }
            Cor.Run(Cor.WaitWhile(
                _Predicate.Invoke,
                () =>
                {
                    ActivateOrDeactivate(_Index, _Activate);
                    _OnFinish?.Invoke();
                }));
        }
    
        private void ActivateOrDeactivate(int _Index, bool _Activate)
        {
            if (!MathUtils.IsInRange(_Index, 0, Collection.Count - 1))
                return;
            var item = Collection[_Index];
            Activate(item, _Activate);
        }

        protected abstract void Activate(T _Item, bool _Active);
        protected abstract T GetFirstOrLastActiveOrInactive(bool _First, bool _Active);

        #endregion
    }
}