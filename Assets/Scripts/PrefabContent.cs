using System.Linq;
using UnityEngine;
using Malee.List;

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

    public T GetItemComponent<T>(string _Name) where T : Component
    {
        return GetItem(_Name).GetComponent<T>();
    }
}
