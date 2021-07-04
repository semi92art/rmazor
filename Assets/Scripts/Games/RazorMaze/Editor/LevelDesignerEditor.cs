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
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using Utils.Editor;

namespace Games.RazorMaze.Editor
{
    public class LevelDesignerEditor : EditorWindow
    {
        public static int GameId;
        public static int HeapIndex;
        
        private static int _heapIndexCheck;

        private List<MazeInfo> m_Levels;
        private ReorderableList m_ReorderableLevels;
            
        private List<string> m_CommentsCheck;
        private LevelDesigner m_Des;
        private int m_LevelIndex;
        private Vector2 m_HeapScroll;

        Color m_Color = Color.red;


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
            ReloadReorderableLevels();
        }
        private void ReloadReorderableLevels()
        {
            m_ReorderableLevels = new ReorderableList(m_Levels, typeof(MazeInfo),true,true,true,true);
            m_ReorderableLevels.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = m_ReorderableLevels.list[index] as MazeInfo;
                EditorGUI.LabelField(new Rect(rect.x,rect.y,40,EditorGUIUtility.singleLineHeight),$"{index}" );
                element.Comment = EditorGUI.TextField(new Rect(rect.x+40,rect.y,200,EditorGUIUtility.singleLineHeight),element
                    .Comment );
            };
            m_ReorderableLevels.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                if (index % 3 == 0)
                    m_Color = ChangeColor();
                GUI.backgroundColor = m_Color;
            };
        }

        private Color ChangeColor()
        {
            if (m_Color == Color.blue)
                return Color.red;
            if (m_Color == Color.red)
                return  Color.blue;
            return Color.black;
        }

        private void SaveHeap()
        {
            var index = 0;
            foreach (var level in m_ReorderableLevels.list)
            {
                SaveLevel(HeapIndex,index);
                index++;
            }
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
            
            // TODO скорее всего надо удалить
            // EditorUtilsEx.HorizontalZone(() =>
            // {
            //     GUILayout.Label("Group:", GUILayout.Width(45));
            //     m_LevelGroup = EditorGUILayout.IntField( m_LevelGroup, GUILayout.Width(50));
            //     GUILayout.Label("Index:", GUILayout.Width(45));
            //     m_LevelIndex = EditorGUILayout.Popup(m_LevelIndex, 
            //         Enumerable.Range(1, MazeLevelUtils.LevelsInGroup)
            //             .Select(_Idx => _Idx.ToString()).ToArray(), GUILayout.Width(50));
            //     if (m_Des.group != 0 && m_Des.index >= 0)
            //         GUILayout.Label($"Current: Group: {m_Des.group}, Index: {m_Des.index}");
            // });
            // EditorUtilsEx.HorizontalZone(() =>
            // {
            //     EditorUtilsEx.GuiButtonAction(LoadLevel, m_LevelGroup, m_LevelIndex + 1, GUILayout.Width(100));
            //     EditorUtilsEx.GuiButtonAction(SaveLevel, m_LevelGroup, m_LevelIndex + 1, 0, (MazeInfo)null, GUILayout.Width(100));
            // });
            EditorUtilsEx.HorizontalZone(() =>
            {
                EditorUtilsEx.GuiButtonAction("Add to heap", SaveLevel, HeapIndex, -1, true, GUILayout.Width(100));
                EditorUtilsEx.GuiButtonAction("Save Heap", SaveHeap, GUILayout.Width(100));
            });
            GUILayout.Label("Heap:");
            HeapIndex = 1 + EditorGUILayout.Popup(HeapIndex - 1,
                             Enumerable.Range(1, 10)
                                 .Select(_Idx => _Idx.ToString()).ToArray());
            if (HeapIndex != _heapIndexCheck)
                ReloadHeapLevels();
            _heapIndexCheck = HeapIndex;
            EditorUtilsEx.HorizontalZone(() =>
            {
                EditorUtilsEx.GuiButtonAction("Load", LoadLevel, GUILayout.Width(100));
                EditorUtilsEx.GuiButtonAction("Save", SaveLevel, HeapIndex, m_ReorderableLevels.index, false,
                GUILayout.Width(100));
                EditorUtilsEx.GuiButtonAction("Delete",DeleteLevelFromHeap, HeapIndex,GUILayout.Width(100));
            });
            
            // EditorUtilsEx.HorizontalZone(() =>
            // {
            //     EditorUtilsEx.GuiButtonAction("Load", SaveLevel, m_LevelGroup, -1, _heapIndex, (MazeInfo)null, GUILayout.Width(100));
            //     GUILayout.Label("Heap:");
            //     _heapIndex = 1 + EditorGUILayout.Popup(_heapIndex - 1,
            //                      Enumerable.Range(1, 10)
            //                          .Select(_Idx => _Idx.ToString()).ToArray());
            //     if (_heapIndex != _heapIndexCheck)
            //         ReloadHeapLevels();
            //     _heapIndexCheck = _heapIndex;
            // });

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
                m_Des.index = _Info.HeapLevelIndex;
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

        private void LoadLevel()
        {
            if (m_ReorderableLevels.index==-1)
                return;
            var info = MazeLevelUtils.LoadLevel(GameId, m_ReorderableLevels.index, 0, false);
            CreateObjects(info);
            FocusCamera(info.Size);
        }

        private void SaveLevel(int _HeapIndex, int _LevelIndex, bool newLevel = false)
        {
            if (_LevelIndex==-1 && !newLevel)
                return;
            var _Info = newLevel? (MazeInfo)null:  (MazeInfo) m_ReorderableLevels.list[_LevelIndex];
            var info = _Info ?? m_Des.GetLevelInfoFromScene(false);
            info.HeapLevelIndex = _LevelIndex;
            m_Des.index = _LevelIndex;
            if (_HeapIndex <= 0)
                MazeLevelUtils.SaveLevel(GameId, info);
            else
            {
                MazeLevelUtils.SaveLevelToHeap(GameId, info, _LevelIndex, HeapIndex);
                ReloadHeapLevels();
                ReloadReorderableLevels();
            }
        }

        private void ShowHeap()
        {
            EditorUtilsEx.ScrollViewZone(ref m_HeapScroll, () =>
            {
                m_ReorderableLevels.DoLayoutList();
            });
        }

        private void LoadLevel(MazeInfo _Info)
        {
            CreateObjects(_Info);
            FocusCamera(_Info.Size);
        }

        private void DeleteLevelFromHeap(int _HeapIndex)
        {
            var _LevelIndex = m_ReorderableLevels.index;
            if (_LevelIndex==-1)
                return;
            MazeLevelUtils.DeleteLevelFromHeap(GameId, _LevelIndex, _HeapIndex);
            m_Levels = MazeLevelUtils.LoadHeapLevels(GameId, _HeapIndex).Levels;
            ReloadReorderableLevels();
        }

        private void ReloadHeapLevels()
        {
            m_Levels = MazeLevelUtils.LoadHeapLevels(GameId, HeapIndex).Levels;
            m_CommentsCheck = m_Levels.Select(_L => _L.Comment).ToList();
        }
    }
}