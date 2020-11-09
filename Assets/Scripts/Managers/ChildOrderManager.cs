using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Managers
{
    public class ChildOrderManager : MonoBehaviour, ISingleton
    {
        #region singleton
        
        private static ChildOrderManager _instance;
        
        public static ChildOrderManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("ChildOrderManager");
                    _instance = go.AddComponent<ChildOrderManager>();
                }
                    
                return _instance;
            }
        }
        
        #endregion
        
        #region private members
        
        private List<OrderItem> OrderItems = new List<OrderItem>();
        private Dictionary<Transform, int> ChildCountDict = new Dictionary<Transform, int>();
        
        #endregion

        #region public methods

        public void Add(Transform _Item, int _Order, bool _FromBehind)
        {
            OrderItems.Add(new OrderItem
            {
                Parent = _Item.parent,
                Item = _Item,
                FromBehind = _FromBehind,
                Order = _Order
            });
            
            if (!ChildCountDict.ContainsKey(_Item.parent))
            {
                var parent = _Item.parent;
                ChildCountDict.Add(parent, parent.childCount);
            }
        }

        #endregion

        
        #region engine methods

        private void LateUpdate()
        {
            List<Transform> keys = new List<Transform>();
            
            foreach (var orderItem in OrderItems)
            {
                if (orderItem.Parent.childCount != ChildCountDict[orderItem.Parent]
                    && !keys.Contains(orderItem.Parent))
                {
                    keys.Add(orderItem.Parent);
                    var group = OrderItems
                        .GroupBy(_Item => _Item.Parent).FirstOrDefault(_G => _G.Key == orderItem.Parent);
                    UpdateOrdering(group);
                }
            }
        }
        
        #endregion
        
        #region private methods

        private void UpdateOrdering(IGrouping<Transform, OrderItem> _Group)
        {
            var fbGroups = _Group
                .GroupBy(_Item => _Item.FromBehind).ToArray();
            var forwardGroup = fbGroups
                .FirstOrDefault(_G => _G.First().FromBehind == false)
                ?.OrderBy(_Item => -_Item.Order).ToArray();
            var behindGroup = fbGroups
                .FirstOrDefault(_G => _G.First().FromBehind = true)
                ?.OrderBy(_Item => _Item.Order).ToArray();

            int? k = forwardGroup?.First().Parent.childCount - 1;
            if (!k.HasValue || behindGroup == null)
                return;
            
            foreach (var item in behindGroup)
                item.Item.SetSiblingIndex((k--).Value);
            k = 0;
            foreach (var item in forwardGroup)
                item.Item.SetSiblingIndex((k++).Value);
        }
        
        #endregion
        
        #region types

        private class OrderItem
        {
            public Transform Parent { get; set; }
            public Transform Item { get; set; }
            public bool FromBehind { get; set; }
            public int Order { get; set; }
        }
        
        #endregion
    }
}