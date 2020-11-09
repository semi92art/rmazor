using Managers;
using UnityEngine;

namespace Helpers
{
    public class ChildOrderer : MonoBehaviour
    {
        public int order;
        public bool fromBehind = true;

        private void Start()
        {
            ChildOrderManager.Instance.Add(transform, order, fromBehind);
        }
    }
}