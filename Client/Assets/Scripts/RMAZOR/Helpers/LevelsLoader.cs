using System;
using System.Linq;
using Common.Extensions;
using Common.Managers;
using Common.Utils;
using Newtonsoft.Json;
using RMAZOR.Models.MazeInfos;
using UnityEngine;

namespace RMAZOR.Helpers
{
    public interface ILevelsLoader
    {
        MazeInfo LoadLevel(int      _GameId, long _Index);
        int      GetLevelsCount(int _GameId);
    }
    
    public class LevelsLoader : ILevelsLoader
    {
        #region nonpublic members
        
        private int      m_GameId;
        private string[] m_CachedSerializedLevels;

        #endregion

        #region inject
        
        protected IPrefabSetManager PrefabSetManager { get; }

        public LevelsLoader(IPrefabSetManager _PrefabSetManager)
        {
            PrefabSetManager = _PrefabSetManager;
        }

        #endregion

        #region api
        
        public MazeInfo LoadLevel(int _GameId, long _Index)
        {
            if (m_CachedSerializedLevels == null || m_GameId != _GameId)
                CacheLevels(_GameId);
            return JsonConvert.DeserializeObject<MazeInfo>(m_CachedSerializedLevels[_Index]);
        }

        public int GetLevelsCount(int _GameId)
        {
            if (m_CachedSerializedLevels == null || m_GameId != _GameId)
                CacheLevels(_GameId);
            return m_CachedSerializedLevels.Length;
        }

        #endregion

        #region nonpublic methods
        
        private void CacheLevels(int _GameId)
        {
            int heapIndex = Application.isEditor ? SaveUtilsInEditor.GetValue(SaveKeysInEditor.StartHeapIndex) : 1;
            m_GameId = _GameId;
            var asset = PrefabSetManager.GetObject<TextAsset>(PrefabSetName(_GameId),
                LevelsAssetName(heapIndex));
            var t = typeof(MazeInfo);
            var firstProp = t.GetProperties()[0];
            var levelsText = asset.text;
            levelsText = levelsText.Remove(levelsText.Length - 2, 2);
            string splitter = "{" + "\"" + firstProp.Name + "\"";
            var serializedLevels = levelsText.Split(new []{splitter, "," + splitter}, StringSplitOptions.None);
            serializedLevels = serializedLevels
                .RemoveRange(new[] {serializedLevels[0]})
                .Select(_MazeSerialized => splitter + _MazeSerialized).ToArray();
            m_CachedSerializedLevels = serializedLevels;
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

        #endregion
    }
}