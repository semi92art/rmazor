using UICreationSystem;
using UnityEngine;

public class ChildOrderer : MonoBehaviour
{
    public int order;
    public bool fromBehind = true;

    private void Start()
    {
        ChildOrderManager.Instance.Add(transform, order, fromBehind);
    }
    
    // private void LateUpdate()
    // {
    //     int childCount = transform.parent.childCount;
    //     int realOrder = fromBehind ? childCount - 1 - order : order;
    //     realOrder = Mathf.Clamp(realOrder, 0, childCount - 1);
    //     if (transform.GetSiblingIndex() != realOrder)
    //         transform.SetSiblingIndex(realOrder);
    // }
}