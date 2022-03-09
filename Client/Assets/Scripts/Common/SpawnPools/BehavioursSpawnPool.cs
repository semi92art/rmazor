using System.Linq;
using UnityEngine;

namespace Common.SpawnPools
{
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
    
        protected override void Activate(T _Item, bool _Active)
        {
            if (_Item == null || _Item.enabled == _Active)
                return;
            _Item.gameObject.SetActive(_Active);
            _Item.enabled = _Active;
        }

        protected override bool IsActive(T _Item)
        {
            return _Item.enabled;
        }

        #endregion
    }
}