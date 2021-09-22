using System;
using System.Collections.Generic;
using System.Linq;
using GameHelpers.Editor;
using Games.RazorMaze.Models;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Games.RazorMaze.Editor
{
    public class HeapReorderableList
    {
        #region nonpublic members
        
        private readonly ReorderableList m_List;
        private readonly LevelsSaverEditor m_LevelsSaver = new LevelsSaverEditor();
        private readonly int m_GameId;
        private readonly Dictionary<EMazeItemType, bool> m_Filters = new Dictionary<EMazeItemType, bool>();
        private int m_HeapIndex;
        private int m_SelectedIndexCheck;

        #endregion
        
        #region api

        public List<MazeInfo> Levels => m_List.list.Cast<MazeInfo>().ToList();
        public int Index => m_List.index;
        public int Count => m_List.count;
        

        public HeapReorderableList(int _GameId, int _HeapIndex)
        {
            m_GameId = _GameId;
            m_HeapIndex = _HeapIndex;
            var levels = m_LevelsSaver.LoadHeapLevels(_GameId, _HeapIndex).Levels;
            var filters = Enum.GetValues(typeof(EMazeItemType))
                .Cast<EMazeItemType>()
                .Except(new[] {EMazeItemType.Block, EMazeItemType.Attenuator})
                .ToList();
            foreach (var filter in filters)
                m_Filters.Add(filter, true);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            m_List = new ReorderableList(
                levels, 
                typeof(MazeInfo),
                true, 
                true,
                false,
                false)
            {
                headerHeight = lineHeight * 2f + lineHeight * filters.Count
            };

            m_List.drawElementCallback = (_Rect, _Idx, _IsActive, _IsFocused) =>
            {
                var elementColor = _IsFocused || m_SelectedIndexCheck == _Idx ? 
                    BackgroundSelectedColor : GetContentColor(_Idx);
                EditorGUI.DrawRect(new Rect(
                    _Rect.x,
                    _Rect.y, 
                    _Rect.width,
                    lineHeight), elementColor);
                
                var element = m_List.list[_Idx] as MazeInfo;
                EditorGUI.LabelField(new Rect(_Rect.x,_Rect.y,40, lineHeight),$"{_Idx + 1}" );
                if (element == null)
                    return;
                
                element.Comment = EditorGUI.TextField(
                    new Rect(
                        _Rect.x + 40,
                        _Rect.y, 
                        _Rect.width - 2f * 40f,
                        lineHeight),
                    element.Comment);

                var selectedFilters = m_Filters
                    .Where(_Kvp => _Kvp.Value)
                    .Select(_Kvp => _Kvp.Key)
                    .ToList();
                if (element.MazeItems.Any(_Item => selectedFilters.Contains(_Item.Type)))
                {
                    EditorGUI.DrawRect(new Rect(
                        _Rect.x + 40f + _Rect.width - 2f * 40f,
                        _Rect.y, 
                        40f,
                        lineHeight), Color.red);
                }
            };
            
            m_List.drawElementBackgroundCallback = (_Rect, _Idx, _IsActive, _IsFocused) => 
                GUI.contentColor = ContentColor;
            
            
            m_List.drawHeaderCallback = _Rect =>
            {
                var rect = new Rect(_Rect.x, _Rect.y, _Rect.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(rect, $"Levels in heap: {m_List.list.Count}");
                rect = new Rect(_Rect.x, _Rect.y + lineHeight, _Rect.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(rect, "Filters:");
                float width = _Rect.width;
                int k = 0;
                foreach (var filter in filters)
                {
                    rect = new Rect(
                    _Rect.x,
                    _Rect.y + 2f * lineHeight + lineHeight * k,
                    width * 0.5f,
                    EditorGUIUtility.singleLineHeight);
                    m_Filters[filter] = EditorGUI.Toggle(rect, filter.ToString(), m_Filters[filter]);
                    rect = new Rect(
                        _Rect.x + width * 0.5f,
                        _Rect.y + 2f * lineHeight + lineHeight * k++,
                        width * 0.5f,
                        EditorGUIUtility.singleLineHeight);
                    int levelsWithThisFilterCount = Levels
                        .Count(_Level => Enumerable
                            .Any(_Level.MazeItems, _Item => _Item.Type == filter));
                    EditorGUI.LabelField(rect, $"Count: {levelsWithThisFilterCount}");
                }
            };
            m_List.onSelectCallback = _List => m_SelectedIndexCheck = _List.index;
            
            m_List.onChangedCallback += _List => Save();
        }

        public void DoLayoutList() => m_List.DoLayoutList();
        public void Insert(int _Index, MazeInfo _Info) => m_List.list.Insert(_Index, _Info);
        public void Add(MazeInfo _Info) => m_List.list.Add(_Info);
        public void Reload(int _HeapIndex)
        {
            m_HeapIndex = _HeapIndex;
            m_List.list = m_LevelsSaver.LoadHeapLevels(m_GameId, _HeapIndex).Levels;
        }

        public void Delete(int _Index)
        {
            if (_Index < 0 || _Index >= m_List.list.Count)
                return;
            m_List.list.RemoveAt(_Index);
            Save();
        }

        public void Save()
        {
            m_LevelsSaver.SaveLevelsToHeap(m_GameId, m_HeapIndex, Levels);
        }

        public void Save(MazeInfo _Info, int _Index)
        {
            _Info.Comment = (m_List.list[_Index] as MazeInfo)?.Comment;
            m_List.list[_Index] = _Info;
            Save();
        }

        #endregion
        
        #region nonpublic methods

        private static Color GetContentColor(int _Index)
        {
            int a = _Index % 6;
            return a < 3 ? new Color(0.2f, 0.2f, 0.2f) : new Color(0.32f, 0.32f, 0.32f);
        }

        private static Color ContentColor => Color.white;
        private static Color BackgroundSelectedColor => new Color(0.14f, 0.18f, 0.25f);

        #endregion
    }
}