using System;
using System.Collections.Generic;
using System.Linq;
using Network;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
                if (!GameClient.Instance.IsTestMode)
                    DontDestroyOnLoad(go);
                return _instance;
            }
        }
        
        #endregion
        
        #region private members
        
        public readonly List<OrderItem> OrderItems = new List<OrderItem>();
        private readonly Dictionary<Transform, int> m_ChildCountDict = new Dictionary<Transform, int>();
        
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
            var keys = new List<Transform>();
            foreach (var orderItem in OrderItems.Where(_OrderItem => 
                _OrderItem.Parent.childCount != m_ChildCountDict[_OrderItem.Parent]
                && !keys.Contains(_OrderItem.Parent)))
            {
                m_ChildCountDict[orderItem.Parent] = orderItem.Parent.childCount;
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

        public class OrderItem
        {
            public Transform Parent { get; set; }
            public Transform Item { get; set; }
            public bool FromBehind { get; set; }
            public int Order { get; set; }
        }
        
        #endregion
    }
    
    #if UNITY_EDITOR

    [CustomEditor(typeof(HierarchyOrderManager))]
    public class HierarchyOrderManagerEditor : Editor
    {
        private readonly GUILayoutOption m_NameWidth = GUILayout.Width(150);
        private readonly GUILayoutOption m_OrderWidth = GUILayout.Width(30);
        private readonly GUILayoutOption m_FromBehWidth = GUILayout.Width(60);
        private HierarchyOrderManager m_Object;

        private void OnEnable()
        {
            m_Object = target as HierarchyOrderManager;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.Space(10);

            var groups = m_Object.OrderItems.GroupBy(_Item => _Item.Parent);

            foreach (var group in groups)
            {
                GUILayout.Label($"Parent: {group.Key.name}");
                GUILayout.BeginHorizontal();
                GUILayout.Label("Name:", m_NameWidth);
                GUILayout.Label("Ord:", m_OrderWidth);
                GUILayout.Label("From beh:", m_FromBehWidth);
                GUILayout.EndHorizontal();
                foreach (var item in group
                    .OrderBy(_Item => _Item.FromBehind)
                    .ThenBy(_Item => _Item.FromBehind ? -_Item.Order : _Item.Order))
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(item.Item.name, m_NameWidth);
                    GUILayout.Label(item.Order.ToNumeric(), m_OrderWidth);
                    GUILayout.Label(item.FromBehind.ToString(), m_FromBehWidth);
                    GUILayout.EndHorizontal();
                }
            }
        }
    }
    
    #endif
}