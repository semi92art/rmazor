using System.Linq;

namespace Common.SpawnPools
{
    public class SpawnPool<T> : SpawnPoolBase<T> where T : class, ISpawnPoolItem
    {
        public override int CountActivated => Collection.Count(_Item => _Item.ActivatedInSpawnPool);
        
        
        public override bool Remove(T _Item)
        {
            if (!Contains( _Item))
                return false;
            //_Item?.Destroy();
            return Collection.Remove(Collection[IndexOf(_Item)]);
        }
        
        protected override void Activate(T _Item, bool _Active)
        {
            _Item.ActivatedInSpawnPool = _Active;
        }

        protected override bool IsActive(T _Item)
        {
            return _Item.ActivatedInSpawnPool;
        }
    }
}