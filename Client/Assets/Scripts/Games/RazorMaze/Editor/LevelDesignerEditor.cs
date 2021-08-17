using System;
using System.Collections.Generic;
using System.Linq;
using Constants;
using Entities;
using Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.MazeItems;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using Utils.Editor;

namespace Games.RazorMaze.Editor
{
    public class LevelDesignerEditor : EditorWindow
    {
        public static List<MazeInfo> Levels;
        public static int GameId;
        public static int HeapIndex;
        
        private static int _heapIndexCheck;
        
        private List<string> m_CommentsCheck;
        private LevelDesigner m_Des;
        private int m_LevelGroup = 1;
        private int m_LevelIndex;
        private Vector2 m_HeapScroll;
        private int m_TabPage;

        [MenuItem("Tools/Level Designer", false)]
        public static void ShowWindow()
        {
            var window = GetWindow<LevelDesignerEditor>("Level Designer");
            window.minSize = new Vector2(300, 200);
        }
        
        private void OnEnable()
        {
            if (SceneManager.GetActiveScene().name != SceneNames.Prototyping)
                return;
            
            GameId = 1;
            HeapIndex = 1;
            m_Des = LevelDesigner.Instance;
            ReloadHeapLevels();
        }

        private void OnFocus()
        {
            GameId = 1;
            if (SceneManager.GetActiveScene().name != SceneNames.Prototyping)
                return;
            m_Des = LevelDesigner.Instance;
        }

        public void OnGUI()
        {
            m_TabPage = GUILayout.Toolbar (m_TabPage, new [] {"Levels", "Fix Utils"});
            switch (m_TabPage)
            {
                case 0: ShowLevelsTabPage(); break;
                case 1: ShowFixUtilsTabPage(); break;
            }
        }

        private void ShowLevelsTabPage()
        {
            if (SceneManager.GetActiveScene().name != SceneNames.Prototyping)
                return;
            
            EditorUtilsEx.HorizontalZone(() =>
            {
                GUILayout.Label("Size:", GUILayout.Width(35));
                m_Des.sizeIdx = EditorGUILayout.Popup(m_Des.sizeIdx, 
                    LevelDesigner.Sizes.Select(_S => $"{LevelDesigner.MazeWidth}x{_S}").ToArray(),
                    GUILayout.Width(100));
                GUILayout.Label("Fullness:", GUILayout.Width(50));
                m_Des.aParam = EditorGUILayout.Slider(m_Des.aParam, 0, 1, GUILayout.Width(150));
                
            });
            EditorUtilsEx.HorizontalZone(() =>
            {
                GUILayout.Label("Path lengths:", GUILayout.Width(80));
                m_Des.pathLengths = EditorGUILayout.TextField(m_Des.pathLengths, GUILayout.Width(100));
                EditorUtilsEx.GuiButtonAction("Default", () => m_Des.pathLengths = "1,2,3", GUILayout.Width(60));
            });
            EditorUtilsEx.HorizontalZone(() =>
            {
                EditorUtilsEx.GuiButtonAction("Create", CreateLevel, GUILayout.Width(50));
                EditorUtilsEx.GuiButtonAction("Create Default", CreateDefault, GUILayout.Width(100));
                EditorUtilsEx.GuiButtonAction("Check for validity", CheckForValidity, GUILayout.Width(120));
                
                EditorUtilsEx.GUIColorZone(m_Des.valid ? Color.green : Color.red, 
                    () => GUILayout.Label($"Level is {(m_Des.valid ? "" : "not ")}valid"));
            });

            EditorUtilsEx.HorizontalZone(() =>
            {
                EditorUtilsEx.GuiButtonAction("Add to heap", SaveLevel, m_LevelGroup, -1, HeapIndex, (MazeInfo)null, GUILayout.Width(100));
                GUILayout.Label("Heap:");
                HeapIndex = 1 + EditorGUILayout.Popup(HeapIndex - 1,
                    Enumerable.Range(1, 10)
                        .Select(_Idx => _Idx.ToString()).ToArray());
                if (HeapIndex != _heapIndexCheck)
                    ReloadHeapLevels();
                _heapIndexCheck = HeapIndex;
            });

            ShowHeap();
        }

        private void ShowFixUtilsTabPage()
        {
            EditorUtilsEx.GuiButtonAction("Fix Paths", LevelDesignerFixUtils.FixPaths);
        }

        private void CreateLevel()
        {
            EditorUtilsEx.SceneDirtyAction(() =>
            {
                EditorUtilsEx.ClearConsole();
                ClearLevel();
                var size = new V2Int(LevelDesigner.MazeWidth, LevelDesigner.Sizes[m_Des.sizeIdx]);
                var parms = new MazeGenerationParams(
                    size,
                    m_Des.aParam,
                    m_Des.pathLengths.Split(',').Select(_S => int.Parse(_S)).ToArray());
                var info = LevelGenerator.CreateRandomLevelInfo(parms, out m_Des.valid);
                LoadLevel(info);
            });
        }

        private void CreateDefault()
        {
            EditorUtilsEx.SceneDirtyAction(() =>
            {
                EditorUtilsEx.ClearConsole();
                ClearLevel();
                var size = new V2Int(LevelDesigner.MazeWidth, LevelDesigner.Sizes[m_Des.sizeIdx]);
                var info = LevelGenerator.CreateDefaultLevelInfo(size, true);
                LoadLevel(info);
            });
        }

        private void CreateObjects(MazeInfo _Info)
        {
            EditorUtilsEx.SceneDirtyAction(() =>
            {
                var container = CommonUtils.FindOrCreateGameObject("Maze", out _).transform;
                container.gameObject.DestroyChildrenSafe();
                var converter = new CoordinateConverter();
                converter.Init(_Info.Size);
                var contGetter = new ContainersGetter(converter);
                var mazeItemsCreator = new MazeItemsCreator(contGetter, converter);
                mazeItemsCreator.Editor = true;
                m_Des.maze = mazeItemsCreator.CreateMazeItems(_Info)
                    .Cast<ViewMazeItemProt>()
                    .ToList();
                m_Des.group = _Info.LevelGroup;
                m_Des.index = _Info.LevelIndex;
                m_Des.size = _Info.Size;
            });
        }

        public static void FocusCamera(V2Int _Size)
        {
            var converter = new CoordinateConverter();
            converter.Init(_Size);
            var bounds = new Bounds(converter.GetCenter(), GameUtils.GetVisibleBounds().size * 0.7f);
            EditorUtilsEx.FocusSceneCamera(bounds);
        }

        private void ClearLevel()
        {
            var items = m_Des.maze;
            if (items == null)
                return;
            foreach (var item in items.Where(_Item => _Item != null))
                item.gameObject.DestroySafe();
            items.Clear();
        }

        private void CheckForValidity()
        {
            var info = m_Des.GetLevelInfoFromScene();
            m_Des.valid = LevelAnalizator.IsValid(info, false);
        }

        private void LoadLevel(int _Group, int _Index)
        {
            var info = MazeLevelUtils.LoadLevel(GameId, _Group, _Index, HeapIndex, false);
            CreateObjects(info);
            FocusCamera(info.Size);
        }

        private void SaveLevel(int _Group, int _LevelIndex, int _HeapIndex, MazeInfo _Info)
        {
            var info = _Info ?? m_Des.GetLevelInfoFromScene(false);
            info.LevelGroup = _Group;
            info.LevelIndex = _LevelIndex;
            m_Des.group = _Group;
            m_Des.index = _LevelIndex;
            if (_HeapIndex <= 0)
                MazeLevelUtils.SaveLevel(GameId, info);
            else
            {
                if (_LevelIndex < Levels.Count && _LevelIndex >= 0)
                    info.Comment = Levels[_LevelIndex].Comment;
                MazeLevelUtils.SaveLevelToHeap(GameId, info, _LevelIndex, HeapIndex);
                ReloadHeapLevels();
            }
        }

        private void ShowHeap()
        {
            if (Levels == null || !Levels.Any())
            {
                ReloadHeapLevels();
            }
            
            int k = 0;
            EditorUtilsEx.ScrollViewZone(ref m_HeapScroll, () =>
            {
                foreach (var level in Levels)
                {
                    EditorUtilsEx.HorizontalZone(() =>
                    {
                        GUILayout.Label($"{++k}", GUILayout.Width(30));
                        EditorUtilsEx.GuiButtonAction("Load", LoadLevel, level, GUILayout.Width(40));
                        EditorUtilsEx.GuiButtonAction("Save here", SaveLevel, 0, k - 1, HeapIndex, level, GUILayout.Width(100));
                        EditorUtilsEx.GuiButtonAction("Delete", DeleteLevelFromHeap, k - 1, HeapIndex, GUILayout.Width(50));
                        
                        level.Comment = EditorGUILayout.TextField(level.Comment);
                        if (level.Comment != m_CommentsCheck[k - 1])
                            SaveLevel(0, k - 1, HeapIndex, level);
                    });
                }
            });
        }

        private void LoadLevel(MazeInfo _Info)
        {
            CreateObjects(_Info);
            FocusCamera(_Info.Size);
        }

        private void DeleteLevelFromHeap(int _LevelIndex, int _HeapIndex)
        {
            MazeLevelUtils.DeleteLevelFromHeap(GameId, _LevelIndex, _HeapIndex);
            Levels = MazeLevelUtils.LoadHeapLevels(GameId, _HeapIndex).Levels;
        }

        private void ReloadHeapLevels()
        {
            Levels = MazeLevelUtils.LoadHeapLevels(GameId, HeapIndex).Levels;
            m_CommentsCheck = Levels.Select(_L => _L.Comment).ToList();
        }
    }
}