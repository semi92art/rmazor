using System;
using Exceptions;
using Extensions;
using GameHelpers;
using UnityEngine;
using Utils;
using Object = UnityEngine.Object;

namespace Games.PointsTapper
{
    public class PointsPool : ActivatedMonoBehavioursSpawnPool<PointItem>
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

        public override void Deactivate(PointItem _Item, Func<bool> _Predicate = null, Action _OnFinish = null)
        {
            _Item.Activated = false;
            Coroutines.Run(Coroutines.WaitWhile(
                () => base.Deactivate(_Item, _Predicate, _OnFinish),
                () => _Item.Activated));
        }
        
        #endregion

        #region nonpublic methods

        private GameObject GetPrefab(PointType _Type)
        {
            GameObject result;
            switch (_Type)
            {
                case PointType.Default:
                    result = PrefabInitializer.GetPrefab("points_tapper", "point_default");
                    break;
                case PointType.Bad:
                    result = PrefabInitializer.GetPrefab("points_tapper", "point_bad");
                    break;
                case PointType.BonusGold:
                    result = PrefabInitializer.GetPrefab("points_tapper", "point_bonus_gold");
                    break;
                case PointType.BonusDiamonds:
                    result = PrefabInitializer.GetPrefab("points_tapper", "point_bonus_diamonds");
                    break;
                case PointType.Unknown:
                    result = PrefabInitializer.GetPrefab("points_tapper", "point_unknown");
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Type);
            }

            return result;
        }

        #endregion
    }
}