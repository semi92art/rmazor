using System.Collections.Generic;
using System.Linq;
using GameHelpers;
using UnityEditor;
using Utils.Editor;
using MathUtils = Utils.MathUtils;

namespace Games.Editor
{
    public abstract class LevelDesignerEditorBase : UnityEditor.Editor
    {
        protected abstract GameMode GMode { get; }

        private List<int> m_LevelIndexes;
        private int m_SelectedLevIndexToLoad;
        private int m_SelectedLevelToSave = 1;
        private int m_SelectedStageIndexToSelect;
        
        protected virtual void OnEnable()
        {
            m_LevelIndexes = LevelUtils.GetLevelIndexes(GMode);
        }

        public override void OnInspectorGUI()
        {
            if (m_LevelIndexes.Any())
            {
                EditorGUILayout.BeginHorizontal();
                var levelIdxsEnum = m_LevelIndexes.Select(_Idx => _Idx.ToString()).ToArray();
                m_SelectedLevIndexToLoad = EditorGUILayout.Popup(m_SelectedLevIndexToLoad, levelIdxsEnum);
                EditorUtilsEx.GuiButtonAction(LoadLevel, m_LevelIndexes[m_SelectedLevIndexToLoad]);
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.BeginHorizontal();
            EditorUtilsEx.GuiButtonAction(SaveLevel, m_SelectedLevelToSave);
            m_SelectedLevelToSave = EditorGUILayout.IntField(m_SelectedLevelToSave);
            m_SelectedLevelToSave = MathUtils.Clamp(m_SelectedLevelToSave, 1, 100);
            EditorGUILayout.EndHorizontal();
        }

        protected abstract void LoadLevel(int _Idx);
        protected abstract void SaveLevel(int _Idx);


        protected bool CreateLevelDialog => EditorUtility.DisplayDialog(
            "New Level",
            "Are you sure to create new level?\nCurrent changes will be lost.",
            "Ok",
            "Cancel");
    }
}