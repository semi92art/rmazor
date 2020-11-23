using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Utils;
using Object = UnityEngine.Object;

public interface IActivated
{
    bool Activated { get; set; }
}

public interface ISpawnPool<T> : IList<T>
{
    T FirstActive { get; }
    T FirstInactive { get; }
    T LastActive { get; }
    T LastInactive { get; }
    void Activate(T _Item, Vector3 _Position, Func<bool> _Predicate = null, Action _OnFinish = null);
    void Deactivate(T _Item, Func<bool> _Predicate = null, Action _OnFinish = null);
}

public class SpawnPool<T> : ISpawnPool<T> where T : Component, IActivated
{
    #region private members

    private readonly List<T> m_Collection = new List<T>();

    #endregion
    
    #region api
    
    #region inherited interface
    
    public int Count => m_Collection.Count;
    
    public bool IsReadOnly => false;
    
    public IEnumerator<T> GetEnumerator()
    {
        return m_Collection.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(T _Item)
    {
        if (_Item == null)
            return;
        m_Collection.Add(_Item);
        Activate(m_Collection.Last(), default, false);
    }

    public void AddRange(IEnumerable<T> _Items)
    {
        foreach (var item in _Items)
            Add(item);
    }

    public void Clear()
    {
        foreach (var item in m_Collection.ToArray())
            Object.Destroy(item);
        m_Collection.Clear();
    }

    public bool Contains(T _Item)
    {
        return m_Collection.Contains(_Item);
    }

    public void CopyTo(T[] _Array, int _ArrayIndex)
    {
        m_Collection.ToList().CopyTo(_Array, _ArrayIndex);
    }

    public bool Remove(T _Item)
    {
        if (!Contains( _Item))
            return false;
        Object.Destroy(_Item);
        return m_Collection.Remove(m_Collection[IndexOf(_Item)]);
    }
    
    public int IndexOf(T _Item)
    {
        return m_Collection.ToList().IndexOf(_Item);
    }

    public void Insert(int _Index, T _Item)
    {
        if (_Item == null)
            return;
        m_Collection.Insert(_Index, _Item);
        Activate(m_Collection[_Index], default, false);
    }

    public void RemoveAt(int _Index)
    {
        Object.Destroy(m_Collection[_Index]);
        m_Collection.RemoveAt(_Index);
    }

    public T this[int _Index]
    {
        get => m_Collection[_Index];
        set => m_Collection[_Index] = value;
    }
    
    #endregion

    public T FirstActive => GetFirstOrLastActiveOrInactive(true, true);
    public T FirstInactive => GetFirstOrLastActiveOrInactive(true, false);
    public T LastActive => GetFirstOrLastActiveOrInactive(false, true);
    public T LastInactive => GetFirstOrLastActiveOrInactive(false, false);
    
    public virtual void Activate(T _Item, Vector3 _Position, Func<bool> _Predicate = null, Action _OnFinish = null)
    {
        Activate(IndexOf(_Item), _Position, _Predicate, _OnFinish);
    }
    
    public virtual void Deactivate(T _Item, Func<bool> _Predicate = null, Action _OnFinish = null)
    {
        Deactivate(IndexOf(_Item), _Predicate, _OnFinish);
    }
    
    #endregion
    
    #region private methods
    
    private void Activate(int _Index, Vector3 _Position, Func<bool> _Predicate = null, Action _OnFinish = null)
    {
        ActivateOrDeactivate(_Index, _Position, _Predicate, _OnFinish, true);
    }
    
    private void Deactivate(int _Index, Func<bool> _Predicate = null, Action _OnFinish = null)
    {
        ActivateOrDeactivate(_Index, default, _Predicate, _OnFinish, false);
    }

    private void ActivateOrDeactivate(int _Index, Vector3 _Position, Func<bool> _Predicate, Action _OnFinish, bool _Activate)
    {
        Coroutines.Run(Coroutines.WaitWhile(
            () =>
            {
                ActivateOrDeactivate(_Index, _Position, _Activate);
                _OnFinish?.Invoke();
            },
            () => _Predicate?.Invoke() ?? false));
    }
    
    private void ActivateOrDeactivate(int _Index, Vector3 _Position, bool _Activate)
    {
        if (!Utility.IsInRange(_Index, 0, m_Collection.Count - 1))
            return;
        var item = m_Collection[_Index];
        Activate(item, _Position, _Activate);
    }

    private void Activate(T _Item, Vector3 _Position, bool _Active)
    {
        if (_Item == null || _Item.Activated == _Active)
            return;
        _Item.gameObject.SetActive(_Active);
        if (_Item is Behaviour beh)
            beh.enabled = _Active;
        _Item.Activated = _Active;
        _Item.transform.position = _Position;
    }

    private T GetFirstOrLastActiveOrInactive(bool _First, bool _Active)
    {
        var collection = _First ? m_Collection : m_Collection.ToArray().Reverse();
        return collection.FirstOrDefault(_Item => _Active ? _Item.Activated : !_Item.Activated);
    }

    #endregion
}
