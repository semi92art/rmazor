using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Network;
using UnityEngine;
using UnityEngine.SceneManagement;

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
                if (!GameClient.Instance.IsModuleTestsMode)
                    DontDestroyOnLoad(go);
                return _instance;
            }
        }
        
        #endregion
        
        #region nonpublic members
        
        public readonly List<OrderItem> OrderItems = new List<OrderItem>();
        private readonly Dictionary<Transform, int> m_ChildCountDict = new Dictionary<Transform, int>();
        private readonly Transform[] m_TempArr = new Transform[20];
        
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

            if (m_ChildCountDict.ContainsKey(_Item.parent)) 
                return;
            var parent = _Item.parent;
            m_ChildCountDict.Add(parent, parent.childCount);
        }

        #endregion
        
        #region engine methods

        private void Start()
        {
            SceneManager.sceneLoaded += (_Scene, _Mode) =>
            {
                OrderItems.Clear();
                m_ChildCountDict.Clear();
            };
        }

        private void LateUpdate()
        {
            Array.Clear(m_TempArr, 0, m_TempArr.Length);
            foreach (var orderItem in OrderItems.Where(_OrderItem => 
                _OrderItem.Parent.childCount != m_ChildCountDict[_OrderItem.Parent]
                && !m_TempArr.Contains(_OrderItem.Parent)))
            {
                m_ChildCountDict[orderItem.Parent] = orderItem.Parent.childCount;

                for (int i = 0; i < m_TempArr.Length; i++)
                {
                    if (m_TempArr[i] == null)
                        m_TempArr[i] = orderItem.Parent;
                }
                
                var group = OrderItems
                    .GroupBy(_Item => _Item.Parent)
                    .FirstOrDefault(_G => _G.Key == orderItem.Parent);
                UpdateOrdering(group);
            }
        }
        
        #endregion
        
        #region nonpublic methods

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

        public class OrderItem
        {
            public Transform Parent { get; set; }
            public Transform Item { get; set; }
            public bool FromBehind { get; set; }
            public int Order { get; set; }
        }
        
        #endregion
    }
}