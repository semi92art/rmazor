using System;
using Exceptions;
using Extensions;
using Helpers;
using UnityEngine;
using Utils;
using Object = UnityEngine.Object;

namespace PointsTapper
{
    public enum PointType
    {
        Normal,
        Bad
    }
    
    public class PointsPool : SpawnPool<PointItem>
    {
        #region api

        public PointsPool(PointType _PointType, int _Count)
        {
            GameObject container = new GameObject($"Points Pool {_PointType}");
            GameObject prefab = GetPrefab(_PointType);
            for (int i = 0; i < _Count; i++)
            {
                var go = prefab.Clone();
                Add(go.GetComponent<PointItem>());
                go.transform.SetParent(container.transform);
            }
            Object.Destroy(prefab);
        }
        
        public override void Activate(PointItem _Item, Vector3 _Position, Func<bool> _Predicate = null, Action _OnFinish = null)
        {
            base.Activate(_Item, _Position, _Predicate, _OnFinish);
            _Item.Activate();
        }

        public override void Deactivate(PointItem _Item, Func<bool> _Predicate = null, Action _OnFinish = null)
        {
            _Item.Deactivate();
            Coroutines.Run(Coroutines.WaitWhile(
                () => base.Deactivate(_Item, _Predicate, _OnFinish),
                () => _Item.Activated));
        }
        
        #endregion

        #region private methods

        private GameObject GetPrefab(PointType _Type)
        {
            GameObject result;
            switch (_Type)
            {
                case PointType.Normal:
                    result = PrefabInitializer.GetPrefab("points_tapper", "point_normal");
                    break;
                case PointType.Bad:
                    result = PrefabInitializer.GetPrefab("points_tapper", "point_bad");
                    break;
                default:
                    throw new InvalidEnumArgumentExceptionEx(_Type);
            }

            return result;
        }

        #endregion
    }
}