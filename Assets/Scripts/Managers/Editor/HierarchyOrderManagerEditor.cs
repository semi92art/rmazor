using System.Linq;
using Extensions;
using UnityEngine;
using UnityEditor;
using Utils;

namespace Managers.Editor
{
    [CustomEditor(typeof(HierarchyOrderManager))]
    public class HierarchyOrderManagerEditor : UnityEditor.Editor
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

            var groups = 
                m_Object.OrderItems.GroupBy(_Item => _Item.Parent);

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
}