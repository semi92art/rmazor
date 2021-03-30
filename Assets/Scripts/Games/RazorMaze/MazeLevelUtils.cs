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
        public const int LevelsInGroup = 3;
        private const int GroupsInAsset = 50;
        
        public static MazeInfo LoadLevel(int _GameId, int _LevelGroup, int _Index, bool _Heap, bool _FromBundle)
        {
            var asset = PrefabUtilsEx.GetObject<TextAsset>(PrefabSetName(_GameId),
                LevelsAssetName(_Heap, _LevelGroup), _FromBundle);
            var levelsListRaw = JsonConvert.DeserializeObject<MazeLevelsList>(asset.text);
            var levelsList = levelsListRaw.Levels;
            var result = levelsList
                .FirstOrDefault(_L => _L.LevelGroup == _LevelGroup && _L.LevelIndex == _Index);
            return result;
        }
        
#if UNITY_EDITOR
        
        public static MazeLevelsList LoadHeapLevels(int _GameId)
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(LevelsAssetPath(_GameId, true));
            var levelsListRaw = JsonConvert.DeserializeObject<MazeLevelsList>(
                asset.text, new JsonSerializerSettings{Formatting = Formatting.None});
            return levelsListRaw;
        }
        
        public static void SaveLevel(int _GameId, MazeInfo _Info)
        {
            CreateLevelsAssetIfNotExist(_GameId, false, _Info.LevelGroup);
            var asset = PrefabUtilsEx.GetObject<TextAsset>(
                PrefabSetName(_GameId), LevelsAssetName(false, _Info.LevelGroup), false);
            var mazes = new MazeLevelsList {Levels = new List<MazeInfo>()};
            if (!string.IsNullOrEmpty(asset.text))
                mazes = JsonConvert.DeserializeObject<MazeLevelsList>(
                    asset.text, new JsonSerializerSettings {Formatting = Formatting.None});
            var existingInfo = mazes.Levels.FirstOrDefault(_L => 
                _L.LevelGroup == _Info.LevelGroup && _L.LevelIndex == _Info.LevelIndex);
            if (existingInfo != null)
                mazes.Levels.Remove(existingInfo);
            mazes.Levels.Add(_Info);
            string serialized = JsonConvert.SerializeObject(mazes, Formatting.None);
            File.WriteAllText(LevelsAssetPath(_GameId, false, _Info.LevelGroup), serialized);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void SaveLevelToHeap(int _GameId, MazeInfo _Info, int? _Index)
        {
            CreateLevelsAssetIfNotExist(_GameId, true);
            var mazes = LoadHeapLevels(_GameId);
            if (_Index.HasValue)
                mazes.Levels[_Index.Value] = _Info;
            else
                mazes.Levels.Add(_Info);
            string serialized = JsonConvert.SerializeObject(mazes, Formatting.None);
            File.WriteAllText(LevelsAssetPath(_GameId, true), serialized);
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(LevelsAssetPath(_GameId, true));
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void DeleteLevelFromHeap(int _GameId, int _Index)
        {
            var mazes = LoadHeapLevels(_GameId);
            mazes.Levels.RemoveAt(_Index);
            string serialized = JsonConvert.SerializeObject(mazes, Formatting.None);
            File.WriteAllText(LevelsAssetPath(_GameId, true), serialized);
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(LevelsAssetPath(_GameId, true));
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateLevelsAssetIfNotExist(int _GameId, bool _Heap, int _LevelGroup = 0)
        {
            var tempInfo = new MazeLevelsList {Levels = new List<MazeInfo>()};
            string serialized = JsonConvert.SerializeObject(tempInfo, Formatting.None);
            string assetPath = LevelsAssetPath(_GameId, _Heap, _LevelGroup);
            if (!File.Exists(assetPath))
            {
                string directory = Path.GetDirectoryName(assetPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                File.WriteAllText(assetPath, serialized);
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }

            if (_Heap)
                return;
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            string setName = PrefabSetName(_GameId);
            if (!ResLoader.PrefabSetExist(setName))
                ResLoader.CreatePrefabSetIfNotExist(setName);
            PrefabUtilsEx.SetPrefab(setName, LevelsAssetName(false, _LevelGroup), asset);
            
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();
        }

        private static string LevelsAssetPath(int _GameId, bool _Heap, int _LevelGroup = 0) =>
            $"Assets/Prefabs/Levels/Game_{_GameId}/{LevelsAssetName(_Heap, _LevelGroup)}.json";
        
#endif
        private static string PrefabSetName(int _GameId) => $"game_{_GameId}_levels";

        private static string LevelsAssetName(bool _Heap, int _LevelGroup = 0)
        {
            return _Heap ? "heap" : $"levels_{(Mathf.FloorToInt(_LevelGroup / (float)GroupsInAsset) + 1).ToString()}";
        }

    }
}