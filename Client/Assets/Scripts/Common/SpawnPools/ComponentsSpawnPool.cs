using System.Linq;
using UnityEngine;

namespace Common.SpawnPools
{
    /// <summary>
    /// Spawn pool for Component objects
    /// </summary>
    /// <typeparam name="T">Type, inherited from Component</typeparam>
    public class ComponentsSpawnPool<T> : SpawnPoolBase<T> where T : Component
    {
        #region api

        public override int CountActivated => Collection.Count(_Item => _Item.gameObject.activeSelf);
    
        public override void Clear()
        {
            foreach (var item in Collection.ToArray())
                Object.Destroy(item);
            base.Clear();
        }


        public override bool Remove(T _Item)
        {
            if (!Contains( _Item))
                return false;
            Object.Destroy(_Item);
            return Collection.Remove(Collection[IndexOf(_Item)]);
        }
    
        public override void RemoveAt(int _Index)
        {
            Object.Destroy(Collection[_Index]);
            base.RemoveAt(_Index);
        }


        #endregion
    
        #region nonpublic methods
    
        protected override void Activate(T _Item, bool _Active)
        {
            if (_Item == null || _Item.gameObject.activeSelf == _Active)
                return;
            _Item.gameObject.SetActive(_Active);
        }

        protected override T GetFirstOrLastActiveOrInactive(bool _First, bool _Active)
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
}