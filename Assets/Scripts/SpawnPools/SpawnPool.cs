using System.Linq;

namespace SpawnPools
{
    public class SpawnPool<T> : SpawnPoolBase<T> where T : class, ISpawnPoolItem
    {
        public override int CountActivated => Collection.Count(_Item => _Item.Activated);
        
        public override void Clear()
        {
           // foreach (var item in Collection.ToArray())
               // item?.Destroy();
            base.Clear();
        }


        public override bool Remove(T _Item)
        {
            if (!Contains( _Item))
                return false;
            //_Item?.Destroy();
            return Collection.Remove(Collection[IndexOf(_Item)]);
        }
        
        public override void RemoveAt(int _Index)
        {
            //Collection[_Index]?.Destroy();
            base.RemoveAt(_Index);
        }

        protected override void Activate(T _Item, bool _Active)
        {
            _Item.Activated = _Active;
        }

        protected override T GetFirstOrLastActiveOrInactive(bool _First, bool _Active)
        {
            var collection = _First ? Collection : Collection.ToArray().Reverse();
            return collection.FirstOrDefault(_Item => _Active ? _Item.Activated : _Item.Activated == false);
        }
    }
}