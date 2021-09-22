using System;
using System.Linq;
using DI.Extensions;
using Games.RazorMaze.Models;
using Newtonsoft.Json;
using UnityEngine;

namespace GameHelpers
{
    public interface ILevelsLoader
    {
        MazeInfo LoadLevel(int _GameId, int _Index, bool _FromBundle);
    }
    
    public class LevelsLoader : ILevelsLoader
    {
        private const int GroupsInAsset = 50;

        private int m_GameId;
        private string[] m_CachedSerializedLevels;
        
        public MazeInfo LoadLevel(int _GameId, int _Index, bool _FromBundle)
        {
            if (m_CachedSerializedLevels != null && m_GameId == _GameId)
                return JsonConvert.DeserializeObject<MazeInfo>(m_CachedSerializedLevels[_Index]);
            m_GameId = _GameId;
            var asset = PrefabUtilsEx.GetObject<TextAsset>(PrefabSetName(_GameId),
                LevelsAssetName(1), _FromBundle);
            var t = typeof(MazeInfo);
            var firstProp = t.GetProperties()[0];
            var levelsText = asset.text;
            levelsText = levelsText.Remove(levelsText.Length - 2, 2);
            string splitter = "{" + "\"" + firstProp.Name + "\"";
            var serializedLevels = levelsText.Split(new []{splitter, "," + splitter}, StringSplitOptions.None);
            serializedLevels = serializedLevels
                .RemoveRange(new[] {serializedLevels[0]})
                .Select(_MazeSerialized => splitter + _MazeSerialized).ToArray();
            var mazeInfo = JsonConvert.DeserializeObject<MazeInfo>(serializedLevels[_Index]);
            m_CachedSerializedLevels = serializedLevels;
            return mazeInfo;
        }
        
        protected static string PrefabSetName(int _GameId) => $"game_{_GameId}_levels";

        protected static string LevelsAssetName(int _HeapIndex)
        {
            string heapName = _HeapIndex <= 0 ? null : $"levels_{_HeapIndex}";
            return heapName ?? $"levels_{(Mathf.FloorToInt(_HeapIndex / (float)GroupsInAsset) + 1).ToString()}";
        }
    }
}