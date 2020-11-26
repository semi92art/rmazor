using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

namespace Managers
{
    public class HierarchyOrderManager : MonoBehaviour, ISingleton
    {
        #region singleton
        
        private static HierarchyOrderManager _instance;
        
        public static HierarchyOrderManager Instance
        {
            get
            {
                if (_instance is HierarchyOrderManager ptm && !ptm.IsNull())
                    return _instance;
                var go = new GameObject("Hierarchy Order Manager");
                _instance = go.AddComponent<HierarchyOrderManager>();

                return _instance;
            }
        }
        
        #endregion
        
        #region private members
        
        private List<OrderItem> OrderItems = new List<OrderItem>();
        private Dictionary<Transform, int> ChildCountDict = new Dictionary<Transform, int>();
        
        #endregion

        #region api

        public void UpdateOrdering(Transform _Item, int _Order, bool _FromBehind)
        {
            var item = OrderItems.First(_OrderItem => _OrderItem.Item == _Item);
            item.Order = _Order;
            item.FromBehind = _FromBehind;
            
            var keys = new List<Transform>();
            foreach (var orderItem in OrderItems.Where(_OrderItem => !keys.Contains(_OrderItem.Parent)))
            {
                keys.Add(orderItem.Parent);
                var group = OrderItems
                    .GroupBy(_Itm => _Itm.Parent).FirstOrDefault(_G => _G.Key == orderItem.Parent);
                UpdateOrdering(@group);
            }
        }
        
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
            var keys = new List<Transform>();
            foreach (var orderItem in OrderItems.Where(_OrderItem => 
                _OrderItem.Parent.childCount != ChildCountDict[_OrderItem.Parent]
                && !keys.Contains(_OrderItem.Parent)))
            {
                ChildCountDict[orderItem.Parent] = orderItem.Parent.childCount;
                keys.Add(orderItem.Parent);
                var group = OrderItems
                    .GroupBy(_Item => _Item.Parent).FirstOrDefault(_G => _G.Key == orderItem.Parent);
                UpdateOrdering(@group);
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
                ?.OrderBy(_Item => _Item.Order).ToArray();
            var behindGroup = fbGroups
                .FirstOrDefault(_G => _G.First().FromBehind = true)
                ?.OrderBy(_Item => _Item.Order).ToArray();

            int? k = forwardGroup?.First().Parent.childCount - 1;
            if (!k.HasValue || behindGroup == null)
                return;

            foreach (var item in behindGroup)
            {
                item.Item.SetSiblingIndex(k.Value);
                k--;
            }
                
            k = 0;
            foreach (var item in forwardGroup)
            {
                item.Item.SetSiblingIndex(k.Value);
                k++;
            }
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