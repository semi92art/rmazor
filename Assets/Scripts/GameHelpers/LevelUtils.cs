using Games;
using UnityEngine;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using System.Collections.Generic;
using UI.Entities;
using UnityEditor;
#endif

namespace GameHelpers
{
    public static class LevelUtils
    {
        public static GameObject LoadLevel(GameMode _GameMode, int _Index)
        {
#if UNITY_EDITOR
            return Application.isPlaying ? 
                LoadLevelRuntime(_GameMode, _Index) :
                LoadLevelFromEditor(_GameMode, _Index);
#else
            return LoadLevelRuntime(_GameMode, _Index);
#endif
        }

        private static GameObject LoadLevelRuntime(GameMode _GameMode, int _Index)
        {
            string levelName = LevelName(_Index);
            return PrefabInitializer.GetPrefab(
                $"levels_{GameModeNames.Names[_GameMode]}", levelName);
        }

#if UNITY_EDITOR

        private static GameObject LoadLevelFromEditor(GameMode _GameMode, int _Index)
        {
            string levelPath = LevelPath(_GameMode, _Index);
            return PrefabUtility.LoadPrefabContents(levelPath);
        }
        
        public static void SaveLevel(GameMode _GameMode, int _Index, GameObject _Level)
        {
            string levelPath = LevelPath(_GameMode, _Index);
            string dirName = Path.Combine(Directory.GetCurrentDirectory(), levelPath);
            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);
            PrefabUtility.SaveAsPrefabAssetAndConnect(
                _Level, levelPath, InteractionMode.AutomatedAction, out bool success);
            if (success) Debug.Log($"Level {_Index} saved to {levelPath}");
            else Debug.LogError($"Failed to save level {_Index} to {levelPath}");

            if (!success)
                return;

            string styleName = $"levels_{GameModeNames.Names[_GameMode]}";
            string levelName = LevelName(_Index);
            PrefabInitializer.SetPrefab(styleName, levelName, _Level);
        }

        public static List<int> GetLevelIndexes(GameMode _GameMode)
        {
            string styleName = $"levels_{GameModeNames.Names[_GameMode]}";
            string levelPrefix = "level_";
            var prefabs = PrefabInitializer.GetAllPrefabs(styleName);
            if (!prefabs.Any())
                return new List<int>();
            return prefabs
                .Select(_P => _P.name.Replace(levelPrefix, string.Empty))
                .Select(_IdxStr => int.Parse(_IdxStr))
                .OrderBy(_Idx => _Idx)
                .ToList();
        }
        
#endif
        
        private static string LevelName(int _Index) => "level_" + _Index;
        private static string LevelPath(GameMode _GameMode, int _Index)
        {
            const string levelDirectoryParent = @"Assets\Prefabs\Levels";
            string levelFileName = $"{LevelName(_Index)}.prefab";
            string levelDirectory = Path.Combine(levelDirectoryParent, GameModeNames.Names[_GameMode]);
            return Path.Combine(levelDirectory, levelFileName);
        }
    }
}