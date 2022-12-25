#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using Common.Managers;
using Common.Utils;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Utils;
using Newtonsoft.Json;
using RMAZOR.Models.MazeInfos;
using UnityEditor;
using UnityEngine;

namespace RMAZOR.Helpers
{
    public class LevelsSaver : LevelsLoader
    {
        #region constructor
        
        public LevelsSaver(IPrefabSetManager _PrefabSetManager, IMazeInfoValidator _Validator) 
            : base(_PrefabSetManager, _Validator) { }

        #endregion

        #region api
        
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
        
        public override MazeInfo GetLevelInfo(int _GameId, long _Index, bool _IsBonus)
        {
            throw new System.NotSupportedException();
        }

        public override string GetLevelInfoRaw(int _GameId, long _Index, bool _IsBonus)
        {
            throw new System.NotSupportedException();
        }

        #endregion

        #region nonpublic methods

        private void CreateLevelsAssetIfNotExist(int _GameId, int _HeapIndex)
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
            PrefabSetManager.SetPrefab(setName, LevelsAssetName(0, false), asset);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();
        }

        private string LevelsAssetPath(int _GameId, int _HeapIndex)
        {
            return $"Assets/Prefabs/Levels/Game_{_GameId}/{LevelsAssetName(_HeapIndex, false)}.json";
        }
        
        #endregion
    }
}
#endif