using System.Collections.Generic;
using System.IO;
using GameHelpers;
using Games.RazorMaze.Models;
using Newtonsoft.Json;
using UnityEngine;

namespace Games.RazorMaze
{
    public interface ILevelsLoader
    {
        MazeInfo LoadLevel(int _GameId, int _Index, bool _FromBundle);
    }
    
    public class LevelsLoader : ILevelsLoader
    {
        private const int GroupsInAsset = 50;

        private int m_GameId;
        private List<MazeInfo> m_CachedList;
        

        public MazeInfo LoadLevel(int _GameId, int _Index, bool _FromBundle)
        {
            if (m_CachedList != null && m_GameId == _GameId)
                return m_CachedList[_Index];
            m_GameId = _GameId;
            var asset = PrefabUtilsEx.GetObject<TextAsset>(PrefabSetName(_GameId),
                LevelsAssetName(1), _FromBundle);
            var levelsListRaw = JsonConvert.DeserializeObject<MazeLevelsList>(asset.text);
            m_CachedList = levelsListRaw.Levels;
            return m_CachedList[_Index];
        }
        
        protected static string PrefabSetName(int _GameId) => $"game_{_GameId}_levels";

        protected static string LevelsAssetName(int _HeapIndex)
        {
            string heapName = _HeapIndex <= 0 ? null : $"levels_{_HeapIndex}";
            return heapName ?? $"levels_{(Mathf.FloorToInt(_HeapIndex / (float)GroupsInAsset) + 1).ToString()}";
        }
    }
}