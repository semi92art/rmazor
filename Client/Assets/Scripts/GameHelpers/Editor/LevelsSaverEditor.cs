using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        private const int GroupsInAsset = 50;

        public static MazeInfo LoadGameLevel(int _GameId, int _Index, bool _FromBundle)
        {
            return LoadLevel(_GameId, _Index, 1, _FromBundle);
        }

        public static MazeInfo LoadLevel(int _GameId, int _Index, int _HeapIndex, bool _FromBundle)
        {
            var asset = PrefabUtilsEx.GetObject<TextAsset>(PrefabSetName(_GameId),
                LevelsAssetName(_HeapIndex), _FromBundle);
            var levelsListRaw = JsonConvert.DeserializeObject<MazeLevelsList>(asset.text);
            var levelsList = levelsListRaw.Levels;
            var result = levelsList[_Index];
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

        public static void SaveLevelsToHeap(int _GameId, int _HeapIndex, List<MazeInfo> _Levels)
        {
            CreateLevelsAssetIfNotExist(_GameId, _HeapIndex);
            string serialized = JsonConvert.SerializeObject(new MazeLevelsList{Levels = _Levels}, Formatting.None);
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
            string heapName = _HeapIndex <= 0 ? null : $"levels_{_HeapIndex}";
            return heapName ?? $"levels_{(Mathf.FloorToInt(_HeapIndex / (float)GroupsInAsset) + 1).ToString()}";
        }
    }
}