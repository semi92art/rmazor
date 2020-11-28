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
    void ForceActivate(bool _Active);
}

public interface ISpawnPool<T> : IList<T>
{
    int CountActivated { get; }
    int CountDeactivated { get; }
    T FirstActive { get; }
    T FirstInactive { get; }
    T LastActive { get; }
    T LastInactive { get; }
    void Activate(T _Item, Vector3 _Position, Func<bool> _Predicate = null, Action _OnFinish = null);
    void Deactivate(T _Item, Func<bool> _Predicate = null, Action _OnFinish = null);
}

/// <summary>
/// Spawn pool for Component objects
/// </summary>
/// <typeparam name="T">Type, inherited from Component</typeparam>
public class ComponentsSpawnPool<T> : ISpawnPool<T> where T : Component
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
        Activate(Collection.Last(), default, false);
    }

    public void AddRange(IEnumerable<T> _Items)
    {
        foreach (var item in _Items)
            Add(item);
    }

    public void Clear()
    {
        foreach (var item in Collection.ToArray())
            Object.Destroy(item);
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

    public bool Remove(T _Item)
    {
        if (!Contains( _Item))
            return false;
        Object.Destroy(_Item);
        return Collection.Remove(Collection[IndexOf(_Item)]);
    }
    
    public int IndexOf(T _Item)
    {
        return Collection.ToList().IndexOf(_Item);
    }

    public void Insert(int _Index, T _Item)
    {
        if (_Item == null)
            return;
        Collection.Insert(_Index, _Item);
        Activate(Collection[_Index], default, false);
    }

    public void RemoveAt(int _Index)
    {
        Object.Destroy(Collection[_Index]);
        Collection.RemoveAt(_Index);
    }

    public T this[int _Index]
    {
        get => Collection[_Index];
        set => Collection[_Index] = value;
    }
    
    #endregion

    public virtual int CountActivated => Collection.Count(_Item => _Item.gameObject.activeSelf);
    public int CountDeactivated => Collection.Count - CountActivated;
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
    
    #region nonpublic methods
    
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
        if (!Utility.IsInRange(_Index, 0, Collection.Count - 1))
            return;
        var item = Collection[_Index];
        Activate(item, _Position, _Activate);
    }

    protected virtual void Activate(T _Item, Vector3 _Position, bool _Active)
    {
        if (_Item == null || _Item.gameObject.activeSelf == _Active)
            return;
        _Item.gameObject.SetActive(_Active);
        _Item.transform.position = _Position;
    }

    protected virtual T GetFirstOrLastActiveOrInactive(bool _First, bool _Active)
    {
        var collection = _First ? Collection : Collection.ToArray().Reverse();
        return collection.FirstOrDefault(_Item =>
        {
            var go = _Item.gameObject;
            return _Active ? go.activeSelf : !go.activeSelf;
        });
    }
    
    #endregion
}

/// <summary>
/// Spawn pool for Behaviour objects
/// </summary>
/// <typeparam name="T">Type, inherited from Behaviour</typeparam>
public class BehavioursSpawnPool<T> : ComponentsSpawnPool<T> where T : Behaviour
{
    #region api
    
    public override int CountActivated => Collection.ToArray().Count(_Item => _Item.enabled);

    #endregion
    
    #region nonpublic methods
    
    protected override void Activate(T _Item, Vector3 _Position, bool _Active)
    {
        if (_Item == null || _Item.enabled == _Active)
            return;
        _Item.gameObject.SetActive(_Active);
        _Item.enabled = _Active;
        _Item.transform.position = _Position;
    }

    protected override T GetFirstOrLastActiveOrInactive(bool _First, bool _Active)
    {
        var collection = _First ? Collection : Collection.ToArray().Reverse();
        return collection.FirstOrDefault(_Item => _Active ? _Item.enabled : !_Item.enabled);
    }
    
    #endregion
}

/// <summary>
/// Spawn pool for MonoBehaviour object with implemented IActiveted interface
/// </summary>
/// <typeparam name="T">MonoBehaviour type with implemented IActivated interface</typeparam>
public class ActivatedMonoBehavioursSpawnPool<T> : BehavioursSpawnPool<T> where T : MonoBehaviour, IActivated
{
    #region api
    
    public override int CountActivated => Collection.ToArray().Count(_Item => _Item.Activated);

    #endregion
    
    #region nonpublic methods
    
    protected override void Activate(T _Item, Vector3 _Position, bool _Active)
    {
        if (_Item == null || _Item.Activated == _Active)
            return;
        _Item.gameObject.SetActive(_Active);
        _Item.enabled = _Active;
        _Item.Activated = _Active;
        _Item.transform.position = _Position;
    }

    protected override T GetFirstOrLastActiveOrInactive(bool _First, bool _Active)
    {
        var collection = _First ? Collection : Collection.ToArray().Reverse();
        return collection.FirstOrDefault(_Item => _Active ? _Item.Activated : !_Item.Activated);
    }

    #endregion
}
