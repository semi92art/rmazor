using System.Linq;
using UnityEngine;
using Malee.List;
using UICreationSystem;


public class PrefabContent : MonoBehaviour
{
    [System.Serializable]
    public class ContentItem
    {
        public GameObject item;
        public string name;
    }
    
    [System.Serializable]
    public class ContentList : ReorderableArray<ContentItem>
    { }
    
    [Header("Content"), Reorderable(paginate = true, pageSize = 5)]
    public ContentList content;

    public GameObject GetItem(string _Name)
    {
        return content.FirstOrDefault(_Ci => _Ci.name == _Name)?.item;
    }

    public Transform GetItemTransform(string _Name)
    {
        return GetItem(_Name).transform;
    }

    public RectTransform GetItemRTransform(string _Name)
    {
        return GetItem(_Name).RTransform();
    }
}
