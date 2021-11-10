using System.Collections.Generic;
using System.IO;
using Games.RazorMaze;
using Games.RazorMaze.Models;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Utils;

namespace GameHelpers.Editor
{
    public class LevelsSaverEditor : LevelsLoader
    {
        public MazeLevelsList LoadHeapLevels(int _GameId, int _HeapIndex)
        {
            CreateLevelsAssetIfNotExist(_GameId, _HeapIndex);
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(LevelsAssetPath(_GameId, _HeapIndex));
            var levelsListRaw = JsonConvert.DeserializeObject<MazeLevelsList>(
                asset.text, new JsonSerializerSettings{Formatting = Formatting.None});
            return levelsListRaw;
        }

        public void SaveLevelsToHeap(int _GameId, int _HeapIndex, List<MazeInfo> _Levels)
        {
            CreateLevelsAssetIfNotExist(_GameId, _HeapIndex);
            string serialized = JsonConvert.SerializeObject(new MazeLevelsList{Levels = _Levels}, Formatting.None);
            File.WriteAllText(LevelsAssetPath(_GameId, _HeapIndex), serialized);
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(LevelsAssetPath(_GameId, _HeapIndex));
            EditorUtility.SetDirty(asset);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
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

        private static string LevelsAssetPath(int _GameId, int _HeapIndex)
        {
            return $"Assets/Prefabs/Levels/Game_{_GameId}/{LevelsAssetName(_HeapIndex)}.json";
        }
    }
}