#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using mazing.common.Runtime;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Utils;
using Newtonsoft.Json;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views;
using UnityEditor;
using UnityEngine;

namespace RMAZOR.Helpers
{
    public class LevelsSaver : LevelsLoaderRmazor
    {
        #region constructor
        
        public LevelsSaver(
            IPrefabSetManager     _PrefabSetManager,
            IMazeInfoValidator    _Validator,
            ILevelGeneratorRmazor _LevelGenerator) 
            : base(_PrefabSetManager, _Validator, _LevelGenerator) { }

        #endregion

        #region api
        
        public MazeLevelsList LoadHeapLevels(int _HeapIndex)
        {
            CreateLevelsAssetIfNotExist(_HeapIndex);
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(LevelsAssetPath(_HeapIndex));
            var levelsListRaw = JsonConvert.DeserializeObject<MazeLevelsList>(
                asset.text, new JsonSerializerSettings{Formatting = Formatting.None});
            return levelsListRaw;
        }

        public void SaveLevelsToHeap(int _HeapIndex, List<MazeInfo> _Levels)
        {
            CreateLevelsAssetIfNotExist(_HeapIndex);
            string serialized = JsonConvert.SerializeObject(new MazeLevelsList{Levels = _Levels}, Formatting.None);
            File.WriteAllText(LevelsAssetPath(_HeapIndex), serialized);
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(LevelsAssetPath(_HeapIndex));
            EditorUtility.SetDirty(asset);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }
        
        public override Entity<MazeInfo> GetLevelInfo(LevelInfoArgs _Args)
        {
            throw new System.NotSupportedException();
        }

        public override string GetLevelInfoRaw(LevelInfoArgs _Args)
        {
            throw new System.NotSupportedException();
        }

        #endregion

        #region nonpublic methods

        private void CreateLevelsAssetIfNotExist(int _HeapIndex)
        {
            var tempInfo = new MazeLevelsList {Levels = new List<MazeInfo>()};
            string serialized = JsonConvert.SerializeObject(tempInfo, Formatting.None);
            string assetPath = LevelsAssetPath(_HeapIndex);
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
            if (!ResLoader.PrefabSetExist(PrefabSetName))
                ResLoader.CreatePrefabSetIfNotExist(PrefabSetName);
            PrefabSetManager.SetPrefab(PrefabSetName, LevelsAssetName(0), asset);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();
        }

        private static string LevelsAssetPath(int _HeapIndex)
        {
            return $"Assets/Prefabs/Levels/Game_{CommonData.GameId}/{LevelsAssetName(_HeapIndex)}.json";
        }
        
        #endregion
    }
}
#endif