using System.Linq;
using System.Reflection;
using Common.Constants;
using Common.Utils;
using Editor;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Exceptions;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Utils;
using ModestTree;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views;
using RMAZOR.Views.ContainerGetters;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.Debug;
using RMAZOR.Views.Helpers.MazeItemsCreators;
using RMAZOR.Views.MazeItems;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using LD = RMAZOR.LevelDesigner;

// ReSharper disable once CheckNamespace
namespace RMAZOR.Editor
{
    public partial class LevelDesignerEditorWindow : EditorWindow
    {
        #region singleton

        public static LevelDesignerEditorWindow Instance { get; private set; }

        #endregion
        
        #region nonpublic members
        
        private static LD Des => LD.Instance;

        private static readonly ILevelAnalyzerRmazor  LevelsAnalyzer = new LevelAnalyzerRmazor();
        private static readonly ILevelGeneratorRmazor LevelGenerator = new LevelGeneratorRmazor(LevelsAnalyzer);

        private ViewSettings                       m_ViewSettings;
        private IPrefabSetManager                  m_PrefabSetManager;
        private IAssetBundleManager                m_AssetBundleManager;
        private ICoordinateConverterInEditor       m_CoordinateConverter;
        private IContainersGetterRmazorInEditor    m_ContainersGetter;
        private IMazeItemsCreator                  m_MazeItemsCreator;

        private static HeapReorderableList LevelsList
        {
            get => Des.levelsList;
            set => Des.levelsList = value;
        }

        private static int _levelDesignerHeapIndexCheck;

        private Vector2  m_HeapScroll;
        private int      m_TabPage;
        private GUIStyle
            m_HeaderStyle1, 
            m_HeaderStyle2;
        private bool     m_ShowDebugInfo;

        #endregion

        #region engine methods
        
        [MenuItem("Tools/Level Designer _%&l", false, 101)]
        public static void ShowWindow()
        {
            var window = GetWindow<LevelDesignerEditorWindow>("Level Designer");
            window.minSize = new Vector2(300, 200);
        }

        [InitializeOnLoadMethod]
        private static void InitStartHeapIndex()
        {
            var saveKey = SaveKeysInEditor.StartHeapIndex;
            int startHeapIndex = SaveUtilsInEditor.GetValue(saveKey);
            if (startHeapIndex == default)
                SaveUtilsInEditor.PutValue(saveKey, 1);
            if (LD.LevelDesignerHeapIndex > 0 && _levelDesignerHeapIndexCheck == 0)
                _levelDesignerHeapIndexCheck = LD.LevelDesignerHeapIndex;
        }

        private void OnEnable()
        {
            Instance = this;
            InitEditor();
        }

        private void OnFocus()
        {
            LD.SceneUnloaded = Focus;
            InitEditor();
        }

        private void OnBecameVisible()
        {
            m_HeaderStyle1 = new GUIStyle
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = {textColor = GUI.contentColor}
            };
            m_HeaderStyle2 = new GUIStyle
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = {textColor = GUI.contentColor}
            };
        }

        public void OnGUI()
        {
            if (SceneManager.GetActiveScene().name == SceneNames.Prototyping)
            {
                SetReorderableLevelsListOnSelectAction();
                ReloadReorderableLevels();
                m_TabPage = GUILayout.Toolbar (m_TabPage, new [] {"Levels", "Fix Utils"});
                switch (m_TabPage)
                {
                    case 0: ShowLevelsTabPage(); break;
                    case 1: ShowFixUtilsTabPage(); break;
                }
                return;
            }
            GUILayout.Label($"Level designer available only on scene {SceneNames.Prototyping}.unity");
            EditorUtilsEx.GuiButtonAction("Load scene", () =>
            {
                InitEditor(true);
                string sceneName = AssetDatabase
                    .FindAssets("l:Scene t:Scene", new[] {"Assets\\Scenes"})
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .FirstOrDefault(_SceneName => _SceneName.Contains(SceneNames.Prototyping));
                EditorHelperWindow.LoadScene(sceneName);
            });
        }
        
        #endregion

        #region api
        
        public void LoadLevel(MazeInfo _Info)
        {
            CreateObjects(_Info);
            FocusCamera(_Info.Size);
        }

        public void FocusCamera(V2Int _Size)
        {
            m_CoordinateConverter.SetMazeSize(_Size);
            var bounds = m_CoordinateConverter.GetMazeBounds();
            EditorUtilsEx.FocusSceneCamera(bounds);
        }

        #endregion
        
        #region nonpublic methods
        
        private void LoadLevel()
        {
            if (LevelsList.SelectedIndex == -1)
                return;
            var info = LevelsList.Levels[LevelsList.SelectedIndex];
            LoadLevel(info);
            LD.Instance.loadedLevelIndex     = LevelsList.SelectedIndex;
            LD.Instance.loadedLevelHeapIndex = LD.LevelDesignerHeapIndex;
            LevelsList.SetupLoadedLevel(
                LevelDesigner.Instance.loadedLevelIndex, 
                LevelDesigner.Instance.loadedLevelHeapIndex);
        }

        private static void SetReorderableLevelsListOnSelectAction()
        {
            if (LevelsList == null || LevelsList.OnSelect != null)
                return;
            LevelsList!.OnSelect = _SelectedIndex =>
            {
                SaveUtilsInEditor.PutValue(SaveKeysInEditor.DesignerSelectedLevel, _SelectedIndex);
            };
        }
        
        private static void ReloadReorderableLevels(bool _Forced = false)
        {
            if (!_Forced && LevelsList != null && !LevelsList.NeedToReload())
                return;
            if (LevelsList == null)
            {
                LevelsList = new HeapReorderableList(
                    LD.LevelDesignerHeapIndex, _SelectedIndex =>
                    {
                        SaveUtilsInEditor.PutValue(SaveKeysInEditor.DesignerSelectedLevel, _SelectedIndex);
                    });
            }
            else if ((LevelsList.NeedToReload() || _Forced) 
                     && SceneManager.GetActiveScene().name == SceneNames.Prototyping)
            {
                LevelsList.Reload(LD.LevelDesignerHeapIndex, _Forced);
            }
            else
            {
                LevelsList.Reload(LD.LevelDesignerHeapIndex);
            }
            if (LD.Instance.loadedLevelIndex != -1)
            {
                LevelsList.SetupLoadedLevel(LD.Instance.loadedLevelIndex, LD.Instance.loadedLevelHeapIndex);
            }
        }

        private static void SaveLevel(int _HeapIndex, int _LevelIndex)
        {
            if (_LevelIndex == -1)
                return;
            var info = Des.GetLevelInfoFromScene();
            LevelsList.Save(info, _LevelIndex);
        }
        
        private void CreateLevel()
        {
            EditorUtilsEx.SceneDirtyAction(() =>
            {
                EditorUtilsEx.ClearConsole();
                ClearLevel();
                var size = Des.GetEnteredMazeSize();
                var parms = new LevelGenerationParams(
                    size,
                    Des.aParam,
                    Des.pathLengths.Split(',').Select(int.Parse).ToArray());
                var entityLevelInfo = LevelGenerator.GetLevelInfoRandom(parms);
                Des.valid = entityLevelInfo.Result != EEntityResult.Fail;
                if (!Des.valid)
                {
                    MazorCommonUtils.ShowAlertDialog(
                        "Failed to create level!", 
                        $"Level info is not valid: {entityLevelInfo.Error}");
                    return;
                }
                LoadLevel(entityLevelInfo.Value);
            });
        }
        
        private void CreateDefault()
        {
            EditorUtilsEx.SceneDirtyAction(() =>
            {
                EditorUtilsEx.ClearConsole();
                ClearLevel();
                var size = Des.GetEnteredMazeSize();
                var info = LevelGenerator.CreateDefaultLevelInfo(size, true);
                LoadLevel(info);
            });
        }
        
        private static void AddEmptyLevel()
        {
            int idx = LevelsList.SelectedIndex;
            var size = Des.GetEnteredMazeSize();
            var info = LevelGenerator.CreateDefaultLevelInfo(size, true);
            if (idx < LevelsList.Count - 1 && idx != -1)
                LevelsList.Insert(idx + 1, info);
            else 
                LevelsList.Add(info);
            LevelsList.Save();
        }
        
        private void ShowLevelsTabPage()
        {
            ShowDebugInfo();
            ShowMazeGeneratorZone();
            ShowHeapZone();
        }

        private void ShowDebugInfo()
        {
            GUILayout.Label("Debug Info", m_HeaderStyle1);
            m_ShowDebugInfo = GUILayout.Toggle(m_ShowDebugInfo, "Show debug info");
            if (!m_ShowDebugInfo)
            {
                EditorUtilsEx.HorizontalLine();
                return;
            }
            GUILayout.Label("LevelDesigner.Instance:", m_HeaderStyle2);
            GUILayout.Label($"Loaded Level Heap Index: {LD.Instance.loadedLevelHeapIndex}");
            GUILayout.Label($"Loaded Level Index: {LD.Instance.loadedLevelIndex}");
            GUILayout.Label("LevelDesigner (Static):", m_HeaderStyle2);
            GUILayout.Label($"Heap Index: {LD.LevelDesignerHeapIndex}");
            GUILayout.Label("LevelsList:", m_HeaderStyle2);
            GUILayout.Label($"Selected Level Index: {LevelsList.SelectedIndex}");
            EditorUtilsEx.HorizontalLine();
        }

        private static void ShowFixUtilsTabPage()
        {
            var type = typeof(LevelDesignerEditorWindow);
            var methods = type
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(_M => _M.HasAttribute(typeof(FixUtilAttribute)))
                .OrderBy(_M => _M.GetAttribute<FixUtilAttribute>().Color);
            
            foreach (var method in methods)
            {
                var attr = method.GetAttribute<FixUtilAttribute>();
                var col = GUI.color;
                col = attr.Color switch
                {
                    FixUtilColor.Default => col,
                    FixUtilColor.Red     => Color.red,
                    FixUtilColor.Green   => Color.green,
                    FixUtilColor.Blue    => new Color(0.41f, 0.69f, 1f),
                    _                    => throw new SwitchCaseNotImplementedException(attr.Color)
                };
                EditorUtilsEx.GUIColorZone(
                    col, 
                    () =>
                {
                    void InvokeMethod() => method.Invoke(Instance, null);
                    EditorUtilsEx.GuiButtonAction(
                        method.Name.WithSpaces(), 
                        InvokeMethod);
                });
            }
        }

        private void ShowMazeGeneratorZone()
        {
            if (Des.IsNull())
                return;
            EditorUtilsEx.GuiButtonAction("Open Another Level Designer", () =>
            {
                var window = CreateInstance<LevelDesignerEditorWindow>();
                window.titleContent = new GUIContent("Level Designer");
                window.minSize = new Vector2(300, 200);
                window.Show();
            });
            GUILayout.Label("Maze Generator", m_HeaderStyle1);
            EditorUtilsEx.HorizontalZone(() =>
            {
                GUILayout.Label("w", GUILayout.Width(15));
                Des.width = EditorGUILayout.IntField(Des.width, GUILayout.Width(40));
                GUILayout.Label("h", GUILayout.Width(15));
                Des.height = EditorGUILayout.IntField(Des.height, GUILayout.Width(40));
                GUILayout.Label("Fullness:", GUILayout.Width(50));
                Des.aParam = EditorGUILayout.Slider(Des.aParam, 0, 1);
            });
            EditorUtilsEx.HorizontalZone(() =>
            {
                GUILayout.Label("Path lengths:", GUILayout.Width(80));
                Des.pathLengths = EditorGUILayout.TextField(Des.pathLengths);
                EditorUtilsEx.GuiButtonAction("Default", () => Des.pathLengths = "1,2,3", GUILayout.Width(60));
            });
            EditorUtilsEx.HorizontalZone(() =>
            {
                EditorUtilsEx.GuiButtonAction("Create", CreateLevel);
                EditorUtilsEx.GuiButtonAction("Create Default", CreateDefault);
            });
            EditorUtilsEx.HorizontalZone(() =>
            {
                EditorUtilsEx.GuiButtonAction("Check validity", () =>
                {
                    var info = Des.GetLevelInfoFromScene();
                    Des.valid = LevelsAnalyzer.IsValid(info);
                }, GUILayout.Width(120));
                EditorUtilsEx.GUIColorZone(Des.valid ?
                        new Color(0.37f, 1f, 0.4f) : new Color(1f, 0.32f, 0.31f), 
                    () => GUILayout.Label($"Level is {(Des.valid ? "" : "not ")}valid"));
            });
            
            EditorUtilsEx.HorizontalLine(Color.gray);
        }

        private void ShowHeapZone()
        {
            GUILayout.Label("Heap", m_HeaderStyle1);
            EditorUtilsEx.HorizontalZone(() =>
            {
                GUILayout.Label("Heap:");
                LD.LevelDesignerHeapIndex = 1 + EditorGUILayout.Popup(
                    LD.LevelDesignerHeapIndex - 1,
                                Enumerable.Range(1, 10)
                                    .Select(_Idx =>
                                    {
                                        string str = $"{_Idx}";
                                        if (_Idx == 1)
                                            return str + " (Release)";
                                        return str;
                                    }).ToArray());
                EditorUtilsEx.GuiButtonAction("Reload", () => ReloadReorderableLevels(true));
                int startHeapIndex = SaveUtilsInEditor.GetValue(SaveKeysInEditor.StartHeapIndex);
                bool isThisHeapStart = startHeapIndex == LD.LevelDesignerHeapIndex;
                bool isThisHeapStart1 = GUILayout.Toggle(isThisHeapStart, "Start Heap");
                if (isThisHeapStart1 != isThisHeapStart)
                    SaveUtilsInEditor.PutValue(SaveKeysInEditor.StartHeapIndex, isThisHeapStart1 ? LD.LevelDesignerHeapIndex : 1);
            });
            if (LD.LevelDesignerHeapIndex != _levelDesignerHeapIndexCheck)
                ReloadReorderableLevels(true);
            _levelDesignerHeapIndexCheck = LD.LevelDesignerHeapIndex;
            if (LD.Instance.loadedLevelIndex >= 0 && LD.Instance.loadedLevelHeapIndex >= 0)
            {
                EditorUtilsEx.HorizontalZone(() =>
                {
                    GUILayout.Label("Current level: " +
                                    $"heap {LD.Instance.loadedLevelHeapIndex}, " +
                                    $"index {LD.Instance.loadedLevelIndex + 1}", EditorStyles.boldLabel);
                });    
            }
            EditorUtilsEx.HorizontalZone(() =>
            {
                EditorUtilsEx.GuiButtonAction("Load",      LoadLevel);
                EditorUtilsEx.GuiButtonAction("Save",      SaveLevel, LD.LevelDesignerHeapIndex, LevelsList.SelectedIndex);
                EditorUtilsEx.GuiButtonAction("Delete",    LevelsList.Delete, LevelsList.SelectedIndex);
                EditorUtilsEx.GuiButtonAction("Add Empty", AddEmptyLevel);
                EditorUtilsEx.GuiButtonAction("Set Pass Commands Record", OpenSetPassCommandsWindow);
            });
            EditorUtilsEx.HorizontalZone(() =>
            {
                EditorUtilsEx.GuiButtonAction("<", () => LevelsList.PreviousPage());
                EditorUtilsEx.GuiButtonAction(">", () => LevelsList.NextPage());
            });
            EditorUtilsEx.ScrollViewZone(ref m_HeapScroll, () => LevelsList.DoLayoutList());
        }
        
        private static void OpenSetPassCommandsWindow()
        {
            var levelInfo = LevelsList.Levels[LevelsList.SelectedIndex];
            SetPassCommandsEditorWindow.ShowWindow(levelInfo);
        }

        private void InitEditor(bool _Forced = false)
        {
            if (SceneManager.GetActiveScene().name != SceneNames.Prototyping && !_Forced)
                return;
            m_AssetBundleManager = new AssetBundleManagerFake();
            m_PrefabSetManager = new PrefabSetManager(m_AssetBundleManager);
            m_ViewSettings = m_PrefabSetManager.GetObject<ViewSettings>(
                CommonPrefabSetNames.Configs, "view_settings");
            m_CoordinateConverter = CoordinateConverterRmazorInEditor.Create(m_ViewSettings, null, false);
            m_ContainersGetter = new ContainersGetterRmazorInEditor(null, m_CoordinateConverter);
            m_CoordinateConverter.GetContainer = m_ContainersGetter.GetContainer;
            m_CoordinateConverter.Init();
            m_MazeItemsCreator = new MazeItemsCreatorInEditor();
        }
        
        private void CreateObjects(MazeInfo _Info)
        {
            EditorUtilsEx.SceneDirtyAction(() =>
            {
                var container = CommonUtils.FindOrCreateGameObject(ContainerNamesMazor.MazeHolder, out _).transform;
                container.gameObject.DestroyChildrenSafe();
                m_CoordinateConverter.SetMazeSize(_Info.Size);
                Des.maze = m_MazeItemsCreator.CreateMazeItems(_Info)
                    .Cast<ViewMazeItemProt>()
                    .ToList();
                Des.size = _Info.Size;
                if (ScreenViewDebug.Instance.IsNotNull())
                    ScreenViewDebug.Instance.MazeSize = _Info.Size;
            });
        }
        
        private static void ClearLevel()
        {
            var items = Des.maze;
            if (items == null)
                return;
            foreach (var item in items.Where(_Item => _Item != null))
                item.gameObject.DestroySafe();
            items.Clear();
        }

        #endregion
    }
}