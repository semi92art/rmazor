using Common;
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
        private const string WindowName = "Temp Utils Helper";
        
        private int             m_LevelIndex;
        private IGameController m_GameController;
        
        [MenuItem("Tools/" + WindowName, false, 108)]
        public static void ShowWindow()
        {
            GetWindow<TempUtilsHelperEditorWindow>(WindowName);
        }

        private void OnGUI()
        {
            EditorUtilsEx.GuiButtonAction(UpdateCurrentBackgroundTexture);
            EditorUtilsEx.GuiButtonAction(SetNextBackgroundTexture);
        }

        private void UpdateCurrentBackgroundTexture()
        {
            if (!ValidAction())
                return;
            var args = GetLevelStageArgsForBackgroundTexture(true);
            m_GameController.View.Background.OnLevelStageChanged(args);
            m_GameController.View.AdditionalBackground.OnLevelStageChanged(args);
        }

        private void SetNextBackgroundTexture()
        {
            if (!ValidAction())
                return;
            var args = GetLevelStageArgsForBackgroundTexture(false);
            m_GameController.View.Background.OnLevelStageChanged(args);
            m_GameController.View.AdditionalBackground.OnLevelStageChanged(args);
        }

        private LevelStageArgs GetLevelStageArgsForBackgroundTexture(bool _Current)
        {
            var settings = new PrefabSetManager(new AssetBundleManagerFake()).GetObject<ViewSettings>(
                "configs", "view_settings");
            int group = RmazorUtils.GetGroupIndex(m_LevelIndex);
            int levels = (_Current ? 0 : 1) * RmazorUtils.GetLevelsInGroup(group);
            m_LevelIndex = MathUtils.ClampInverse(
                m_LevelIndex + levels, 
                0, 
                settings.levelsCountMain - 1);
            var fakeArgs = new LevelStageArgs(
                m_LevelIndex, 
                ELevelStage.Loaded, 
                ELevelStage.Unloaded, 
                ELevelStage.ReadyToUnloadLevel)
            {
                Args = new [] {"set_back_editor"}
            };
            return fakeArgs;
        }

        private bool ValidAction()
        {
            if (!Application.isPlaying)
            {
                Dbg.LogWarning("This option is available only in play mode");
                return false;
            }
            m_GameController = FindObjectOfType<GameControllerMVC>();
            if (m_GameController != null)
                return true;
            Dbg.LogError("Game Controller was not found.");
            return false;
        }
    }
}
