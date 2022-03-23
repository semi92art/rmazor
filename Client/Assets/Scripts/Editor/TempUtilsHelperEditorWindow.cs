using System;
using Common;
using Common.Extensions;
using Common.Managers;
using Common.Utils;
using RMAZOR;
using RMAZOR.Controllers;
using RMAZOR.Models;
using UnityEditor;
using UnityEngine;
using MathUtils = Common.Utils.MathUtils;

namespace Editor
{
    public class TempUtilsHelperEditorWindow : EditorWindow
    {
        private int m_LevelIndex;
        
        [MenuItem("Tools/Test Utils Helper", false)]
        public static void ShowWindow()
        {
            GetWindow<TempUtilsHelperEditorWindow>("Test Utils Helper");
        }

        private void OnGUI()
        {
            EditorUtilsEx.GuiButtonAction(SetNextBackgroundTexture);
        }

        private void SetNextBackgroundTexture()
        {
            if (!Application.isPlaying)
            {
                Dbg.LogWarning("This option is available only in play mode");
                return;
            }
            var gc = FindObjectOfType<GameController>();
            if (gc.IsNull())
            {
                Dbg.LogError("Game Controller was not found.");
                return;
            }
            var settings = new PrefabSetManager(new AssetBundleManagerFake()).GetObject<ViewSettings>(
                "configs", "view_settings");
            int group = RazorMazeUtils.GetGroupIndex(m_LevelIndex);
            int levels = RazorMazeUtils.GetLevelsInGroup(group);
            m_LevelIndex = MathUtils.ClampInverse(m_LevelIndex + levels, 0, settings.levelsCountMain - 1);
            var fakeArgs = new LevelStageArgs(
                m_LevelIndex, 
                ELevelStage.Loaded, 
                ELevelStage.Unloaded, 
                ELevelStage.ReadyToUnloadLevel)
            {
                Args = new [] {"set_back_editor"}
            };
            
            gc.View.Background.OnLevelStageChanged(fakeArgs);
            gc.View.AdditionalBackground.OnLevelStageChanged(fakeArgs);
        }
    }
}
