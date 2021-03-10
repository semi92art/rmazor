using System.Collections.Generic;
using System.Linq;
using Games.RazorMaze.Prot;
using Malee.List;
using UnityEngine;

namespace Games.RazorMaze
{
    public class LevelDesigner : MonoBehaviour
    {
        [System.Serializable] public class WallLengthList : ReorderableArray<int> { }
        
        private const int MinSize = 5;
        private const int MaxSize = 20;
        
        [Header("Wall lengths"), Reorderable(paginate = true, pageSize = 5)] public WallLengthList wallLengths;
        
        [HideInInspector] public List<int> sizes = Enumerable.Range(MinSize, MaxSize).ToList();
        [HideInInspector] public int sizeIdx;
        [HideInInspector] public float aParam;
        [HideInInspector] public bool valid;
        [HideInInspector] public LevelProt prototype;
    }
}