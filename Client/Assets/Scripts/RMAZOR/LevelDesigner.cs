#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Exceptions;
using Common.Extensions;
using Common.Utils;
using Mono_Installers;
using RMAZOR.Controllers;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.ContainerGetters;
using RMAZOR.Views.MazeItems;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace RMAZOR
{
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
        
        [HideInInspector] public HeapReorderableList    LevelsList;
        [HideInInspector] public string                 pathLengths;
        [HideInInspector] public int                    sizeIdx;
        [HideInInspector] public float                  aParam;
        [HideInInspector] public bool                   valid;
        [SerializeField]  public List<ViewMazeItemProt> maze;
        [HideInInspector] public V2Int                  size;
        [SerializeField]  public int                    loadedLevelIndex     = -1;
        [SerializeField]  public int                    loadedLevelHeapIndex = -1;
        public                   GameObject             mazeObject;
        public static            UnityAction            SceneUnloaded;

        private static MazeInfo MazeInfo
        {
            get => SaveUtilsInEditor.GetValue(SaveKeysInEditor.DesignerMazeInfo);
            set => SaveUtilsInEditor.PutValue(SaveKeysInEditor.DesignerMazeInfo, value);
        }
        
        [RuntimeInitializeOnLoadMethod]
        public static void ResetState()
        {
            _instance = null;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        
        public MazeInfo GetLevelInfoFromScene()
        {
            mazeObject = GameObject.Find(ContainerNames.MazeItems);
            maze = new List<ViewMazeItemProt>();
            foreach (Transform mazeObj in mazeObject.transform)
            {
                var mazeItem = mazeObj.gameObject.GetComponent<ViewMazeItemProt>();
                if (mazeItem.IsNotNull()) 
                    maze.Add(mazeItem);
            }
            var protItemStart = maze.FirstOrDefault(_Item => _Item.Props.IsStartNode);
            if (protItemStart == null)
            {
                Dbg.LogError("Maze must contain start item");
                return null;
            }
            var pathItems = maze
                .Where(_Item => _Item.Props.IsNode && !_Item.Props.IsStartNode)
                .Select(_Item => new PathItem{Blank = _Item.Props.Blank, Position = _Item.Props.Position})
                .ToList();
            pathItems.Insert(
                0, 
                new PathItem
                {
                    Blank = protItemStart.Props.Blank,
                    Position = protItemStart.Props.Position
                });
            var mazeProtItems = maze
                .Where(_Item => !_Item.Props.IsNode)
                .Select(_Item => _Item.Props).ToList();
            int maxX = mazeProtItems.Any() ? mazeProtItems.Max(_Item => _Item.Position.X + 1) : 0;
            maxX = Math.Max(maxX, pathItems.Max(_Item => _Item.Position.X + 1));
            int maxY = mazeProtItems.Any() ? mazeProtItems.Max(_Item => _Item.Position.Y + 1) : 0;
            maxY = Math.Max(maxY, pathItems.Max(_Item => _Item.Position.Y + 1));
            var mazeSize = new V2Int(maxX, maxY);
            return new MazeInfo{
                Size =  mazeSize,
                PathItems = pathItems,
                MazeItems = mazeProtItems
                    .SelectMany(_Item =>
                    {
                        var dirs = _Item.Directions.Clone();
                        if (!dirs.Any())
                            dirs.Add(V2Int.Zero);
                        return dirs.Select(_Dir =>
                            new MazeItem
                            {
                                Directions = new List<V2Int> {_Dir},
                                Pair = _Item.Pair,
                                Path = _Item.Path,
                                Type = _Item.Type,
                                Position = _Item.Position,
                                Blank = _Item.Blank
                            });
                    }).ToList()
            };
        }
        
        public static List<Tuple<int, int>> GetSizes()
        {
            var widths  = new [] {12, 13, 14, 15};
            var heights = new[] {12, 13, 14, 15, 16}; 
            return (from width in widths from height in heights
                    select new Tuple<int, int>(width, height))
                .ToList();
        }

        public V2Int GetSizeByIndex()
        {
            (int item1, int item2) = GetSizes()[sizeIdx];
            return new V2Int(item1, item2);
        }
        
        private static void OnPlayModeStateChanged(PlayModeStateChange _Change)
        {
            var sceneName = SceneManager.GetActiveScene().name;
            if (!sceneName.Contains(SceneNames.Prototyping))
                return;

            switch (_Change)
            {
                case PlayModeStateChange.EnteredEditMode:
                    // SceneManager.sceneLoaded -= OnSceneLoaded;
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    MazeInfo = Instance.GetLevelInfoFromScene();
                    GameClientUtils.GameId = 1;
                    LevelMonoInstaller.Release = false;
                    SceneManager.sceneLoaded -= OnSceneLoaded;
                    SceneManager.sceneLoaded += OnSceneLoaded;
                    SceneManager.LoadScene(SceneNames.Level);
                    EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Change);
            }
        }

        private static void OnSceneUnloaded(PlayModeStateChange _State)
        {
            Dbg.Log(nameof(OnSceneUnloaded));
#if UNITY_EDITOR
            Dbg.Log(SceneUnloaded == null);
            SceneUnloaded?.Invoke();
            EditorApplication.playModeStateChanged -= OnSceneUnloaded;
#endif
        }

        private static void OnSceneLoaded(Scene _Scene, LoadSceneMode _Mode)
        {
            if (_Scene.name != SceneNames.Level)
                return;
            #if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnSceneUnloaded;
            #endif
            Application.targetFrameRate = GraphicUtils.GetTargetFps();
            var controller = GameController.CreateInstance();
            controller.Initialize += () =>
            {
                int selectedLevel = SaveUtilsInEditor.GetValue(SaveKeysInEditor.DesignerSelectedLevel);
                controller.Model.LevelStaging.LoadLevel(MazeInfo, selectedLevel);
                RazorMazeUtils.LoadNextLevelAutomatically = false;
            };
            controller.Init();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}

#endif