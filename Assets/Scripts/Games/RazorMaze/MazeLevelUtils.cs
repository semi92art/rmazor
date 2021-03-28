using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameHelpers;
using Games.RazorMaze.Models;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Utils;

namespace Games.RazorMaze
{
    public class MazeLevelsList
    {
        public List<MazeInfo> Levels { get; set; }
    }
    
    public static class MazeLevelUtils
    {
        public static MazeInfo LoadLevel(int _GameId, int _LevelGroup, int _Index)
        {
            bool fromBundles = true;
#if UNITY_EDITOR
            fromBundles = false;
#endif
            var asset = PrefabUtilsEx.GetObject<TextAsset>(PrefabSetName(_GameId),
                LevelsAssetName(_LevelGroup), fromBundles);
            var levelsListRaw = JsonConvert.DeserializeObject<MazeLevelsList>(asset.text);
            var levelsList = levelsListRaw.Levels;
            var result = levelsList
                .FirstOrDefault(_L => _L.LevelGroup == _LevelGroup && _L.LevelIndex == _Index);
            return result;
        }

#if UNITY_EDITOR
        public static void SaveLevel(int _GameId, MazeInfo _Info)
        {
            CreateLevelsAssetIfNotExist(_GameId, _Info.LevelGroup);
            var asset = PrefabUtilsEx.GetObject<TextAsset>(
                PrefabSetName(_GameId), LevelsAssetName(_Info.LevelGroup), false);
            var mazeLevelsList = new MazeLevelsList {Levels = new List<MazeInfo>()};
            if (!string.IsNullOrEmpty(asset.text))
                mazeLevelsList = JsonConvert.DeserializeObject<MazeLevelsList>(asset.text, new JsonSerializerSettings
                {
                    Formatting = Formatting.None
                });
            var levelsList = mazeLevelsList.Levels;
            var existingInfo = levelsList.FirstOrDefault(_L => 
                _L.LevelGroup == _Info.LevelGroup && _L.LevelIndex == _Info.LevelIndex);
            if (existingInfo != null)
                levelsList.Remove(existingInfo);
            levelsList.Add(_Info);
            string serializedText = JsonConvert.SerializeObject(mazeLevelsList, Formatting.None);
            File.WriteAllText(LevelsAssetPath(_GameId, _Info.LevelGroup), serializedText);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateLevelsAssetIfNotExist(int _GameId, int _LevelGroup)
        {
            string setName = PrefabSetName(_GameId);
            if (!ResLoader.PrefabSetExist(setName))
                ResLoader.CreatePrefabSetIfNotExist(setName);
            string assetPath = LevelsAssetPath(_GameId, _LevelGroup);
            if (File.Exists(assetPath))
                return;
            string directory = Path.GetDirectoryName(assetPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            var tempInfo = new MazeLevelsList
            {
                Levels = new List<MazeInfo>()
            };
            string serialized = JsonConvert.SerializeObject(tempInfo, Formatting.None);
            File.WriteAllText(assetPath, serialized);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            PrefabUtilsEx.SetPrefab(setName, LevelsAssetName(_LevelGroup), asset);
        }

        private static string PrefabSetName(int _GameId) => $"game_{_GameId}_levels";
        private static string LevelsAssetName(int _LevelGroup) =>
            $"levels_{(Mathf.FloorToInt(_LevelGroup / 50f) + 1).ToString()}";
        private static string LevelsAssetPath(int _GameId, int _LevelGroup) =>
            $"Assets/Prefabs/Levels/Game_{_GameId}/{LevelsAssetName(_LevelGroup)}.json";
#endif
    }
}