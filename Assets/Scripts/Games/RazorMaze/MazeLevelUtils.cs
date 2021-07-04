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
        public List<string> Comments { get; set; }
    }
    
    public static class MazeLevelUtils
    {
        public const int LevelsInGroup = 3;
        private const int GroupsInAsset = 50;
        private static int _heapIndex;
        private static MazeLevelsList _levelsList;
        
        public static MazeInfo LoadLevel(int _GameId, int _Index, int _HeapIndex, bool _FromBundle)
        {
            var asset = PrefabUtilsEx.GetObject<TextAsset>(PrefabSetName(_GameId),
                LevelsAssetName(_HeapIndex), _FromBundle);
            var levelsListRaw = JsonConvert.DeserializeObject<MazeLevelsList>(asset.text);
            var levelsList = levelsListRaw.Levels;
            var result = levelsList
                .FirstOrDefault(_L => _L.HeapLevelIndex == _Index);
            return result;
        }
        
#if UNITY_EDITOR
        
        public static MazeLevelsList LoadHeapLevels(int _GameId, int _HeapIndex)
        {
            CreateLevelsAssetIfNotExist(_GameId, _HeapIndex);
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(LevelsAssetPath(_GameId, _HeapIndex));
            var levelsListRaw = JsonConvert.DeserializeObject<MazeLevelsList>(
                asset.text, new JsonSerializerSettings{Formatting = Formatting.None});
            return levelsListRaw;
        }
        
        public static void SaveLevel(int _GameId, MazeInfo _Info)
        {
            CreateLevelsAssetIfNotExist(_GameId, 0);
            var asset = PrefabUtilsEx.GetObject<TextAsset>(
                PrefabSetName(_GameId), LevelsAssetName(0), false);
            var mazes = new MazeLevelsList {Levels = new List<MazeInfo>()};
            if (!string.IsNullOrEmpty(asset.text))
                mazes = JsonConvert.DeserializeObject<MazeLevelsList>(
                    asset.text, new JsonSerializerSettings {Formatting = Formatting.None});
            var existingInfo = mazes.Levels.FirstOrDefault(_L => 
                _L.HeapLevelIndex == _Info.HeapLevelIndex);
            if (existingInfo != null)
                mazes.Levels.Remove(existingInfo);
            mazes.Levels.Add(_Info);
            string serialized = JsonConvert.SerializeObject(mazes, Formatting.None);
            File.WriteAllText(LevelsAssetPath(_GameId, 0), serialized);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void SaveLevelToHeap(int _GameId, MazeInfo _Info, int? _Index, int _HeapIndex, bool _ToFile = true)
        {
            CreateLevelsAssetIfNotExist(_GameId, _HeapIndex);
            if (_levelsList == null || _heapIndex != _HeapIndex)
            {
                _levelsList = LoadHeapLevels(_GameId, _HeapIndex);
                _heapIndex = _HeapIndex;
            }
            if (_Index.HasValue && _Index >= 0 && _Index.Value < _levelsList.Levels.Count)
                _levelsList.Levels[_Index.Value] = _Info;
            else
                _levelsList.Levels.Add(_Info);
            if (!_ToFile)
                return;
            string serialized = JsonConvert.SerializeObject(_levelsList, Formatting.None);
            File.WriteAllText(LevelsAssetPath(_GameId, _HeapIndex), serialized);
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(LevelsAssetPath(_GameId, _HeapIndex));
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void DeleteLevelFromHeap(int _GameId, int _Index, int _HeapIndex)
        {
            var mazes = LoadHeapLevels(_GameId, _HeapIndex);
            mazes.Levels.RemoveAt(_Index);
            string serialized = JsonConvert.SerializeObject(mazes, Formatting.None);
            File.WriteAllText(LevelsAssetPath(_GameId, _HeapIndex), serialized);
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(LevelsAssetPath(_GameId, _HeapIndex));
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateLevelsAssetIfNotExist(int _GameId, int _HeapIndex)
        {
            var tempInfo = new MazeLevelsList {Levels = new List<MazeInfo>()};
            string serialized = JsonConvert.SerializeObject(tempInfo, Formatting.None);
            string assetPath = LevelsAssetPath(_GameId, _HeapIndex);
            if (!File.Exists(assetPath))
            {
                string directory = Path.GetDirectoryName(assetPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                File.WriteAllText(assetPath, serialized);
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }

            if (_HeapIndex > 0)
                return;
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            string setName = PrefabSetName(_GameId);
            if (!ResLoader.PrefabSetExist(setName))
                ResLoader.CreatePrefabSetIfNotExist(setName);
            PrefabUtilsEx.SetPrefab(setName, LevelsAssetName(0), asset);
            
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();
        }

        private static string LevelsAssetPath(int _GameId, int _HeapIndex) =>
            $"Assets/Prefabs/Levels/Game_{_GameId}/{LevelsAssetName(_HeapIndex)}.json";
        
#endif
        private static string PrefabSetName(int _GameId) => $"game_{_GameId}_levels";

        private static string LevelsAssetName(int _HeapIndex)
        {
            string heapName;
            if (_HeapIndex <= 0)
                heapName = null;
            else if (_HeapIndex == 1)
                heapName = "heap";
            else heapName = $"heap_{_HeapIndex}";
            return _HeapIndex > 0 ? heapName :
                $"levels_{(Mathf.FloorToInt(_HeapIndex / (float)GroupsInAsset) + 1).ToString()}";
        }
    }
}