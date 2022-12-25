#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Entities;
using Common.Extensions;
using Common.Managers;
using mazing.common.Runtime;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using RMAZOR.Helpers;
using RMAZOR.Models.MazeInfos;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;

namespace RMAZOR
{
    [Serializable]
    public class HeapReorderableList
    {
        #region constants

        private const int LevelsOnPage = 60;

        #endregion
        
        #region nonpublic members

        private static readonly float LineHeight                   = EditorGUIUtility.singleLineHeight;
        private static readonly Color ContentColor                 = Color.white;
        private static readonly Color BackgroundSelectedLevelColor = new Color(0.16f, 0.27f, 0.58f);
        private static readonly Color BackgroundLoadedLevelColor   = new Color(0f, 1f, 0.19f, 0.43f);

        private ReorderableList List { get; set; }

        #endregion
        
        #region serialized fields

        [SerializeField] private SerializableDictionary<EMazeItemType, bool> filters;
        [SerializeField] private List<MazeInfo>                              levelsCached;

        [SerializeField] private int  gameId;
        [SerializeField] private int  heapIndex;
        [SerializeField] private int  selectedIndexCheck   = -1;
        [SerializeField] private int  loadedLevelIndex     = -1;
        [SerializeField] private int  loadedLevelHeapIndex = -1;
        [SerializeField] private int  page = 1;
        [SerializeField] private bool showTimes;

        #endregion

        #region api

        public UnityAction<int> OnSelect;

        public int GameId
        {
            get => gameId;
            set => gameId = value;
        }
        
        public List<MazeInfo> Levels
        {
            get => levelsCached;
            set => levelsCached = value;
        }

        public int SelectedIndex => (page - 1) * LevelsOnPage + List.index;
        public int Count         => levelsCached.Count;

        public HeapReorderableList(
            int              _GameId,
            int              _HeapIndex,
            UnityAction<int> _OnSelect)
        {
            gameId = _GameId;
            heapIndex = _HeapIndex;
            levelsCached = GetLevelsSaver().LoadHeapLevels(_GameId, _HeapIndex).Levels;
            OnSelect = _OnSelect;
            InitFilters();
            ReloadList();
        }
        
        public void ReloadList()
        {
            if (filters.NullOrEmpty())
                InitFilters();
            var lvls = levelsCached;
            var levelsOnPage = Enumerable.Range(
                    (page - 1) * LevelsOnPage,
                    Math.Min(LevelsOnPage, lvls.Count - (page - 1) * LevelsOnPage))
                .Select(_I => lvls[_I])
                .ToList();
            List = new ReorderableList(
                levelsOnPage,
                typeof(MazeInfo),
                true,
                true,
                false,
                false)
            {
                headerHeight = LineHeight * 2f + LineHeight * filters.Count * 0.5f,
                elementHeight = showTimes ? LineHeight * 2.1f : LineHeight,
                drawElementBackgroundCallback = OnDrawElementBackgroundCallback,
                drawHeaderCallback = OnDrawHeaderCallback,
                onSelectCallback = _List =>
                {
                    selectedIndexCheck = _List.index;
                    OnSelect?.Invoke(_List.index);
                },
                onChangedCallback = _List => OnReorderableListChanged(_List.list.Cast<MazeInfo>().ToList()),
                drawElementCallback = OnDrawElementCallback
            };
        }
        
        public bool NeedToReload()
        {
            bool needToReload = List == null || filters == null;
            if (needToReload) 
                return true;
            try
            {
                _ = List.count; //-V3125
            }
            catch
            {
                return true;
            }
            return false;
        }

        public void DoLayoutList()
        {
            List.DoLayoutList();
        }

        public void Insert(int _Index, MazeInfo _Info)
        {
            var lvls = levelsCached;
            lvls?.Insert(_Index, _Info);
            ReloadList();
        }

        public void Add(MazeInfo _Info)
        {
            var lvls = levelsCached;
            lvls?.Add(_Info);
            ReloadList();
        }

        public void Reload(int _GameId, int _HeapIndex, bool _Forced = false)
        {
            if (levelsCached != null && levelsCached.Any() && gameId > 0 && heapIndex > 0 && !_Forced)
            {
                ReloadList();
                return;
            }
            gameId = _GameId;
            heapIndex = _HeapIndex;
            levelsCached = GetLevelsSaver().LoadHeapLevels(gameId, heapIndex).Levels;
            page = 1;
            ReloadList();
        }

        public void Delete(int _Index)
        {
            var lvls = levelsCached;
            if (lvls == null)
            {
                Dbg.LogError("Failed to delete level");
                return;
            }
            if (_Index < 0 || _Index >= lvls.Count)
                return;
            lvls.RemoveAt(_Index);
            ReloadList();
            Save();
        }

        public void Save()
        {
            var lvls = levelsCached;
            GetLevelsSaver().SaveLevelsToHeap(gameId, heapIndex, lvls);
        }

        public void Save(MazeInfo _Info, int _Index)
        {
            var lvls = levelsCached;
            _Info.AdditionalInfo.Arguments = lvls[_Index].AdditionalInfo.Arguments;
            _Info.AdditionalInfo.Comment   = lvls[_Index].AdditionalInfo.Comment;
            _Info.AdditionalInfo.Time3Stars     = lvls[_Index].AdditionalInfo.Time3Stars;
            _Info.AdditionalInfo.Time2Stars     = lvls[_Index].AdditionalInfo.Time2Stars;
            _Info.AdditionalInfo.Time1Star     = lvls[_Index].AdditionalInfo.Time1Star;
            lvls[_Index] = _Info;
            ReloadList();
            Save();
        }
        
        public void PreviousPage()
        {
            page = Math.Max(1, page - 1);
            ReloadList();
        }

        public void NextPage()
        {
            var lvls = levelsCached;
            page = Math.Min(page + 1, lvls.Count / LevelsOnPage + 1);
            ReloadList();
        }

        #endregion

        #region nonpublic methods

        private void OnReorderableListChanged(IReadOnlyList<MazeInfo> _Levels)
        {
            for (int i = 0; i < _Levels.Count; i++)
            {
                int i1 = LevelsOnPage * (page - 1) + i;
                levelsCached[i1] = _Levels[i];
            }
            GetLevelsSaver().SaveLevelsToHeap(gameId, heapIndex, levelsCached);
        }

        private void InitFilters()
        {
            var filterKeys = Enum.GetValues(typeof(EMazeItemType))
                .Cast<EMazeItemType>()
                .Except(new[] {EMazeItemType.Block})
                .ToList();
            filters = new SerializableDictionary<EMazeItemType, bool>();
            foreach (var filterKey in filterKeys)
                filters.Add(filterKey, true);
        }
        
        private void OnDrawHeaderCallback(Rect _Rect)
        {
            float x, y, w;
            Rect UpdateRect() { return new Rect(x, y, w, LineHeight); }
            (x, y, w) = (_Rect.x, _Rect.y, _Rect.width * 0.5f);
            var lvls = levelsCached;
            EditorGUI.LabelField(UpdateRect(), $"Levels in heap: {lvls.Count}");
            x = _Rect.width * 0.5f;
            showTimes = EditorGUI.Toggle(UpdateRect(), "Show times", showTimes);
            var filterKeys = Enum.GetValues(typeof(EMazeItemType))
                .Cast<EMazeItemType>()
                .Except(new[] {EMazeItemType.Block})
                .ToList();
            for (int i = 0; i < filterKeys.Count; i++)
            {
                var key = filterKeys[i];
                if (i % 2 == 0)
                {
                    (x, y, w) = (_Rect.x, y + LineHeight, 20f);
                    EditorGUI.DrawRect(UpdateRect(), FilterColors[key]);
                    (x, w) = (x + w, 20f);
                    filters[key] = EditorGUI.Toggle(UpdateRect(), filters[key]);
                    (x, w) = (x + w, 100f);
                    EditorGUI.LabelField(UpdateRect(), key.ToString());
                    (x, w) = (x + w, 100f);
                    int levelsWithThisFilterCount = Levels
                        .Count(_Level => _Level.MazeItems
                            .Any(_Item => _Item.Type == key));
                    EditorGUI.LabelField(UpdateRect(), $"Count: {levelsWithThisFilterCount}");
                }
                else
                {
                    (x, w) = (_Rect.x + 220f, 20f);
                    EditorGUI.DrawRect(UpdateRect(), FilterColors[key]);
                    (x, w) = (x + w, 20f);
                    filters[key] = EditorGUI.Toggle(UpdateRect(), filters[key]);
                    (x, w) = (x + w, 100f);
                    EditorGUI.LabelField(UpdateRect(), key.ToString());
                    (x, w) = (x + w, 100f);
                    int levelsWithThisFilterCount = Levels
                        .Count(_Level => _Level.MazeItems
                            .Any(_Item => _Item.Type == key));
                    EditorGUI.LabelField(UpdateRect(), $"Count: {levelsWithThisFilterCount}");
                }
            }
        }
        
        private static void OnDrawElementBackgroundCallback(Rect _Rect, int _Index, bool _IsActive, bool _IsFocused)
        {
            GUI.contentColor = ContentColor;
        }

        public void SetupLoadedLevel(int _LevelIndex, int _HeapIndex, int _GameId)
        {
            loadedLevelIndex     = _LevelIndex;
            loadedLevelHeapIndex = _HeapIndex;
            gameId               = _GameId;
        }

        private void OnDrawElementCallback(Rect _Rect, int _Index, bool _IsActive, bool _IsFocused)
        {
            var elementColor = _IsFocused || selectedIndexCheck == _Index
                ? BackgroundSelectedLevelColor
                : GetContentColor(_Index);
            if (loadedLevelIndex == _Index && loadedLevelHeapIndex == heapIndex)
                elementColor = BackgroundLoadedLevelColor;
            float x = _Rect.x;
            float y = _Rect.y;
            float w = 25f;
            Rect GetRect() { return new Rect(x, y, w, LineHeight); }
            EditorGUI.DrawRect(new Rect(
                _Rect.x,
                _Rect.y,
                _Rect.width,
                LineHeight), elementColor);
            EditorGUI.LabelField(GetRect(), $"{(page - 1) * LevelsOnPage + _Index + 1}");
            (x, w) = (x + w, 60f);
            var info = (MazeInfo)List.list[_Index];
            EditorGUI.LabelField(GetRect(), $"S: {info.Size.X}x{info.Size.Y}");
            (x, w) = (x + w, 30f);
            EditorGUI.LabelField(GetRect(), "Args:");
            (x, w) = (x + w, _Rect.width * 0.5f - 2f * 40f - 30f);
            info.AdditionalInfo.Arguments = EditorGUI.TextField(GetRect(), info.AdditionalInfo.Arguments);
            (x, w) = (x + w, 15f);
            EditorGUI.LabelField(GetRect(), "C:");
            (x, w) = (x + w, _Rect.width * 0.5f - 2f * 40f - 15f);
            info.AdditionalInfo.Comment = EditorGUI.TextField(GetRect(), info.AdditionalInfo.Comment);
            var selectedFilters = filters
                .Where(_Kvp => _Kvp.Value)
                .Select(_Kvp => _Kvp.Key)
                .ToList();
            int k = 0;
            foreach (var filter in filters
                .ToList()
                .Select(_Kvp => _Kvp.Key))
            {
                if (selectedFilters.Contains(filter)
                    && info.MazeItems.Any(_Item => _Item.Type == filter))
                {
                    const float w1 = 2f * 40f - 10f;
                    EditorGUI.DrawRect(new Rect(
                        _Rect.x + 40f + 5f + _Rect.width - 3f * 40f + w1 / filters.Count * k,
                        _Rect.y,
                        w1 / filters.Count,
                        LineHeight), FilterColors[filter]);
                }
                k++;
            }
            if (!showTimes)
                return;
            const float w2 = 25f;
            float w3 = (_Rect.width - 3f * w2 - 25f) * 0.3333f;
            (x, y, w) = (25f, _Rect.y + LineHeight * 1.1f, w2);
            GUI.Label(GetRect(), "T1");
            (x, w) = (x + w2, w3);
            info.AdditionalInfo.Time3Stars = EditorGUI.FloatField(GetRect(), info.AdditionalInfo.Time3Stars);
            (x, w) = (x + w3, w2);
            GUI.Label(GetRect(), "T2");
            (x, w) = (x + w2, w3);
            info.AdditionalInfo.Time2Stars = EditorGUI.FloatField(GetRect(), info.AdditionalInfo.Time2Stars);
            (x, w) = (x + w3, w2);
            GUI.Label(GetRect(), "T3");
            (x, w) = (x + w2, w3);
            info.AdditionalInfo.Time1Star = EditorGUI.FloatField(GetRect(), info.AdditionalInfo.Time1Star);
        }

        private static Color GetContentColor(int _Index)
        {
            int groupIndex = RmazorUtils.GetLevelsGroupIndex(_Index);
            int idx = (groupIndex - 1) % CommonDataRmazor.LevelsInGroupArray.Length;
            return GroupColors[idx];
        }

        private static readonly Color[] GroupColors =
        {
            new Color(0.2f, 0.2f, 0.2f),
            new Color(0.4f, 0.34f, 0.34f),
            new Color(0.32f, 0.34f, 0.4f),
        };
        
        private static readonly Dictionary<EMazeItemType, Color> FilterColors = new Dictionary<EMazeItemType, Color>
        {
            {EMazeItemType.GravityBlock,     new Color(1f, 0f, 0.01f)},
            {EMazeItemType.ShredingerBlock,  new Color(1f, 0.66f, 0f)},
            {EMazeItemType.Portal,           new Color(0.8f, 1f, 0f)},
            {EMazeItemType.TrapReact,        new Color(0.25f, 1f, 0f)},
            {EMazeItemType.TrapIncreasing,   new Color(0f, 0.53f, 1f)},
            {EMazeItemType.TrapMoving,       new Color(0.09f, 0f, 1f)},
            {EMazeItemType.GravityTrap,      new Color(0.7f, 0f, 1f)},
            {EMazeItemType.Turret,           new Color(1f, 0f, 0.64f)},
            {EMazeItemType.GravityBlockFree, new Color(0.94f, 1f, 0.96f)},
            {EMazeItemType.Springboard,      Color.black},
            {EMazeItemType.Hammer,           new Color(1f, 0.54f, 0.55f)},
            {EMazeItemType.Spear,            new Color(0.05f, 0.05f, 0.37f)},
            {EMazeItemType.Diode,            new Color(0.37f, 0.07f, 0.25f)},
            {EMazeItemType.KeyLock,          new Color(0f, 0.81f, 0.53f)},
        };
        
        private static LevelsSaver GetLevelsSaver()
        {
            var assetBundleManager = new AssetBundleManagerFake();
            var prefabSetManager = new PrefabSetManager(assetBundleManager);
            var mazeInfoValidator = new MazeInfoValidator();
            return new LevelsSaver(prefabSetManager, mazeInfoValidator);
        }

        #endregion
    }
}
#endif