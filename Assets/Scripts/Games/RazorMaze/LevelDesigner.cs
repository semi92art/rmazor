using System;
using System.Collections.Generic;
using System.Linq;
using Constants;
using Entities;
using Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Prot;
using Malee.List;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace Games.RazorMaze
{
    [InitializeOnLoad]
    public class LevelDesigner : MonoBehaviour
    {
        public static LevelDesigner Instance => FindObjectOfType<LevelDesigner>();
        
        [Serializable] public class WallLengthList : ReorderableArray<int> { }
        
        private const int MinSize = 5;
        private const int MaxSize = 20;

        [Header("Wall lengths"), Reorderable(paginate = true, pageSize = 5)] public WallLengthList wallLengths;
        
        [HideInInspector] public List<int> sizes = Enumerable.Range(MinSize, MaxSize).ToList();
        [HideInInspector] public int sizeIdx;
        [HideInInspector] public float aParam;
        [HideInInspector] public bool valid;
        [HideInInspector] public MazeProtItems prototype;

        private static MazeInfo MazeInfo
        {
            get => SaveUtils.GetValue<MazeInfo>(SaveKey.DesignerMazeInfo);
            set => SaveUtils.PutValue(SaveKey.DesignerMazeInfo, value);
        }
        
        static LevelDesigner()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        
        private static void OnPlayModeStateChanged(PlayModeStateChange _Change)
        {
            //Dbg.Log($"PlayModeStateChange: {_Change}");
            var sceneName = SceneManager.GetActiveScene().name;
            if (!sceneName.Contains(SceneNames.Prototyping))
                return;
            
            if (_Change != PlayModeStateChange.EnteredPlayMode)
                return;
            
            MazeInfo = Instance.GetLevelInfoFromScene();
            GameClientUtils.GameId = 1;
            GameClientUtils.GameMode = (int)EGameMode.Prototyping;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
             
            SceneManager.sceneLoaded += (_Scene, _Mode) =>
            {
                 RazorMazeGameManager.Instance.SetMazeInfo(MazeInfo);
                 RazorMazeGameManager.Instance.Init();
            };
            SceneManager.LoadScene(SceneNames.Level);
        }
        
        public MazeInfo GetLevelInfoFromScene()
        {
            var prot = prototype;
            var nodes = prot.items
                .Where(_Item => _Item.type == PrototypingItemType.Node)
                .Select(_Item => _Item.transform.position.XY().ToVector2Int())
                .Select(_PosInt => new Node{Position = new V2Int(_PosInt)})
                .ToList();
            var nodeStart = prot.items.Where(_Item => _Item.type == PrototypingItemType.NodeStart)
                .Select(_Item => new Node{Position = new V2Int(_Item.transform.position.XY().ToVector2Int())}).First();
            nodes.Insert(0, nodeStart);
            var wallBlocks  = prot.items
                .Where(_Item => _Item.type == PrototypingItemType.WallBlockSimple)
                .Select(_Item => _Item.transform.position.XY().ToVector2Int())
                .Select(_PosInt => new WallBlock{Position = new V2Int(_PosInt)})
                .ToList();
            return new MazeInfo{
                Width =  prot.Width,
                Height = prot.Height,
                Nodes = nodes,
                WallBlocks = wallBlocks
            };
        }
    }
}