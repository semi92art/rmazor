using System;
using System.Linq;
using Malee.List;
using UnityEngine;

[DisallowMultipleComponent]
public class PrefabContent : MonoBehaviour
{
    [Serializable]
    public class ContentItem
    {
        public GameObject item;
        public string name;
    }
    
    [Serializable]
    public class ContentList : ReorderableArray<ContentItem>
    { }
    
    [Header("Content"), Reorderable(paginate = true, pageSize = 20)]
    public ContentList content;

    public GameObject GetItem(string _Name)
    {
        return content.FirstOrDefault(_Ci => _Ci.name == _Name)?.item;
    }

    public T GetItemComponent<T>(string _Name) where T : Component
    {
        return GetItem(_Name).GetComponent<T>();
    }
}
