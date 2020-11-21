using Managers;
using UnityEngine;

namespace Helpers
{
    public class ChildOrderer : MonoBehaviour
    {
        [SerializeField] private int order;
        [SerializeField] private bool fromBehind = true;
        private int m_OrderCheck;
        private bool m_FromBehindCheck;
        private void Start()
        {
            HierarchyOrderManager.Instance.Add(transform, order, fromBehind);
        }

        private void Update()
        {
            if (order != m_OrderCheck || fromBehind != m_FromBehindCheck)
                HierarchyOrderManager.Instance.UpdateOrdering(transform, order, fromBehind);
            
            m_FromBehindCheck = fromBehind;
            m_OrderCheck = order;
        }
    }
}