using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Utils;
using Newtonsoft.Json;
using RMAZOR.Models.MazeInfos;
using UnityEngine;

namespace RMAZOR.Helpers
{
    public interface ILevelsLoader : IInit
    {
        MazeInfo LoadLevel(int      _GameId, long _Index);
        int      GetLevelsCount(int _GameId);
    }
    
    public class LevelsLoader : InitBase, ILevelsLoader
    {
        #region nonpublic members
        
        private readonly Dictionary<int, string[]> m_SerializedLevelsFromCacheDict = new Dictionary<int, string[]>();
        private readonly Dictionary<int, string[]> m_SerializedLevelsFromRemoteDict = new Dictionary<int, string[]>();

        #endregion

        #region inject

        protected IPrefabSetManager  PrefabSetManager  { get; }
        private   IMazeInfoValidator MazeInfoValidator { get; }

        public LevelsLoader(
            IPrefabSetManager  _PrefabSetManager,
            IMazeInfoValidator _MazeInfoValidator)
        {
            PrefabSetManager = _PrefabSetManager;
            MazeInfoValidator = _MazeInfoValidator;
        }

        #endregion

        #region api

        public override void Init()
        {
            PreloadLevels(CommonData.GameId, true);
            PreloadLevels(CommonData.GameId, false);
            base.Init();
        }

        public MazeInfo LoadLevel(int _GameId, long _Index)
        {
            PreloadLevelsIfWereNotLoaded(_GameId);
            MazeInfo Deserialize(IReadOnlyDictionary<int, string[]> _Dict) => JsonConvert.DeserializeObject<MazeInfo>(
                _Dict[_GameId][_Index]);
            var mazeInfo = Deserialize(m_SerializedLevelsFromRemoteDict);
            bool valid = MazeInfoValidator.Validate(mazeInfo);
            if (!valid)
                mazeInfo = Deserialize(m_SerializedLevelsFromCacheDict);
            valid = MazeInfoValidator.Validate(mazeInfo);
            if (valid)
                return mazeInfo;
            throw new Exception("Maze info is not valid!");
        }

        public int GetLevelsCount(int _GameId)
        {
            PreloadLevelsIfWereNotLoaded(_GameId);
            return Math.Min(
                m_SerializedLevelsFromRemoteDict[_GameId].Length,
                m_SerializedLevelsFromCacheDict[_GameId].Length);
        }

        #endregion

        #region nonpublic methods
        
        private void PreloadLevels(int _GameId, bool _Main)
        {
            int heapIndex = Application.isEditor ? SaveUtilsInEditor.GetValue(SaveKeysInEditor.StartHeapIndex) : 1;
            var asset = PrefabSetManager.GetObject<TextAsset>(PrefabSetName(_GameId),
                LevelsAssetName(heapIndex), _Main ? EPrefabSource.Bundle : EPrefabSource.Asset);
            var t = typeof(MazeInfo);
            var firstProp = t.GetProperties()[0];
            string levelsText = asset.text;
            levelsText = levelsText.Remove(levelsText.Length - 2, 2);
            string splitter = "{" + "\"" + firstProp.Name + "\"";
            var serializedLevels = levelsText.Split(new []{splitter, "," + splitter}, StringSplitOptions.None);
            serializedLevels = serializedLevels
                .RemoveRange(new[] {serializedLevels[0]})
                .Select(_MazeSerialized => splitter + _MazeSerialized).ToArray();
            if (_Main)
                m_SerializedLevelsFromRemoteDict.SetSafe(_GameId, serializedLevels);
            else
                m_SerializedLevelsFromCacheDict.SetSafe(_GameId, serializedLevels);
        }
        
        protected static string PrefabSetName(int _GameId)
        {
            return $"game_{_GameId}_levels";
        }

        protected static string LevelsAssetName(int _HeapIndex)
        {
            string heapName = _HeapIndex <= 0 ? null : $"levels_{_HeapIndex}";
            return heapName ?? $"levels_{_HeapIndex}";
        }

        private void PreloadLevelsIfWereNotLoaded(int _GameId)
        {
            if (m_SerializedLevelsFromRemoteDict.GetSafe(_GameId, out _) == null)
                PreloadLevels(_GameId, true);
            if (m_SerializedLevelsFromCacheDict.GetSafe(_GameId, out _) == null)
                PreloadLevels(_GameId, false);
        }

        #endregion
    }
}