using System.Linq;
using UnityEngine;

namespace Common.SpawnPools
{
    public class RendererSpawnPool<T> : ComponentsSpawnPool<T> where T : Renderer
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