using System;
using System.Collections.Generic;

namespace Common.SpawnPools
{
    public interface ISpawnPool<T> : IList<T> where T : class
    {
        int CountActivated { get; }
        int CountDeactivated { get; }
        T FirstActive { get; }
        T FirstInactive { get; }
        T LastActive { get; }
        T LastInactive { get; }
        void Activate(T _Item, Func<bool> _Predicate = null, Action _OnFinish = null);
        void Deactivate(T _Item, Func<bool> _Predicate = null, Action _OnFinish = null);
    }
}