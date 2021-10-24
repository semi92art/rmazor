#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Constants;
using DI.Extensions;
using Entities;
using Games.RazorMaze.Controllers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.MazeItems;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using Mono_Installers;
using UnityEditor;

namespace Games.RazorMaze
{
    [InitializeOnLoad]
    public class LevelDesigner : MonoBehaviour
    {
        private static LevelDesigner _instance;

        public static LevelDesigner Instance
        {
            get
            {
                if (_instance.IsNull())
                    _instance = FindObjectOfType<LevelDesigner>();
                return _instance;
            }
        }

        public const int MazeWidth = 12;
        public static int[] Heights = {12, 15, 16}; 
        
        [HideInInspector] public string pathLengths;
        [HideInInspector] public int sizeIdx;
        [HideInInspector] public float aParam;
        [HideInInspector] public bool valid;
        [SerializeField] public List<ViewMazeItemProt> maze;
        [HideInInspector] public V2Int size;
        
        public GameObject mazeObject;

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
            var sceneName = SceneManager.GetActiveScene().name;
            if (!sceneName.Contains(SceneNames.Prototyping))
                return;
            
            if (_Change != PlayModeStateChange.EnteredPlayMode)
                return;

            MazeInfo = Instance.GetLevelInfoFromScene();
            GameClientUtils.GameId = 1;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            LevelMonoInstaller.Release = false;
             
            SceneManager.sceneLoaded += (_Scene, _Mode) =>
            {
                var controller = RazorMazeGameController.CreateInstance();
                controller.Initialized += () =>
                {
                    int selectedLevel = SaveUtils.GetValue<int>(SaveKey.DesignerSelectedLevel);
                    controller.Model.LevelStaging.LoadLevel(MazeInfo, selectedLevel);
                };
                controller.Init();
            };
            SceneManager.LoadScene(SceneNames.Level);
        }
        
        public MazeInfo GetLevelInfoFromScene()
        {
            mazeObject = GameObject.Find("Maze Items");
            maze = new List<ViewMazeItemProt>();
            foreach (Transform mazeObj in mazeObject.transform)
            {
                maze.Add(mazeObj.gameObject.GetComponent<ViewMazeItemProt>());
            }
            var protItemStart = maze.FirstOrDefault(_Item => _Item.Props.IsStartNode);
            if (protItemStart == null)
            {
                Dbg.LogError("Maze must contain start item");
                return null;
            }
            var path = maze
                .Where(_Item => _Item.Props.IsNode && !_Item.Props.IsStartNode)
                .Select(_Item => _Item.Props.Position)
                .ToList();
            path.Insert(0, protItemStart.Props.Position);
            var mazeProtItems = maze
                .Where(_Item => !_Item.Props.IsNode)
                .Select(_Item => _Item.Props).ToList();

            return new MazeInfo{
                Size =  new V2Int(MazeWidth, Heights[sizeIdx]),
                Path = path,
                MazeItems = mazeProtItems
                    .SelectMany(_Item =>
                    {
                        var dirs = _Item.Directions.Clone();
                        if (!dirs.Any())
                            dirs.Add(V2Int.zero);
                        return dirs.Select(_Dir =>
                                new MazeItem
                                {
                                    Direction = _Dir,
                                    Pair = _Item.Pair,
                                    Path = _Item.Path,
                                    Type = _Item.Type,
                                    Position = _Item.Position
                                });
                    }).ToList()
            };
        }
    }
}

#endif