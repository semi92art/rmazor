using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        MazeInfo GetLevelInfo(int      _GameId, long _Index, object[] _Args = null);
        int      GetLevelsCount(int _GameId);
    }
    
    public abstract class LevelsLoader : InitBase, ILevelsLoader
    {
        #region nonpublic members
        
        protected readonly Dictionary<int, string[]>
            SerializedLevelsFromCacheDict  = new Dictionary<int, string[]>(), 
            SerializedLevelsFromRemoteDict = new Dictionary<int, string[]>();

        protected bool CachedLevelsLoaded, RemoteLevelsLoaded;

        #endregion

        #region inject

        protected IPrefabSetManager  PrefabSetManager  { get; }
        protected IMazeInfoValidator MazeInfoValidator { get; }

        protected LevelsLoader(
            IPrefabSetManager  _PrefabSetManager,
            IMazeInfoValidator _MazeInfoValidator)
        {
            PrefabSetManager  = _PrefabSetManager;
            MazeInfoValidator = _MazeInfoValidator;
        }

        #endregion

        #region api

        public override void Init()
        {
            PreloadLevels(CommonData.GameId, true);
            PreloadLevels(CommonData.GameId, false);
            Cor.Run(Cor.WaitWhile(
                () => !CachedLevelsLoaded || !RemoteLevelsLoaded,
                () => base.Init()));
        }

        public virtual MazeInfo GetLevelInfo(int _GameId, long _Index, object[] _Args = null)
        {
            PreloadLevelsIfWereNotLoaded(_GameId);
            MazeInfo Deserialize(IReadOnlyDictionary<int, string[]> _Dict) => JsonConvert.DeserializeObject<MazeInfo>(
                _Dict[_GameId][_Index]);
            var mazeInfo = Deserialize(SerializedLevelsFromRemoteDict);
            bool valid = MazeInfoValidator.Validate(mazeInfo);
            if (!valid)
                mazeInfo = Deserialize(SerializedLevelsFromCacheDict);
            valid = MazeInfoValidator.Validate(mazeInfo);
            if (valid)
                return mazeInfo;
            throw new Exception("Maze info is not valid!");
        }

        public int GetLevelsCount(int _GameId)
        {
            PreloadLevelsIfWereNotLoaded(_GameId);
            return Math.Min(
                SerializedLevelsFromRemoteDict[_GameId].Length,
                SerializedLevelsFromCacheDict[_GameId].Length);
        }

        #endregion

        #region nonpublic methods
        
        protected virtual void PreloadLevels(int _GameId, bool _Main)
        {
            int heapIndex = Application.isEditor ? SaveUtilsInEditor.GetValue(SaveKeysInEditor.StartHeapIndex) : 1;
            var asset = PrefabSetManager.GetObject<TextAsset>(PrefabSetName(_GameId),
                LevelsAssetName(heapIndex), _Main ? EPrefabSource.Bundle : EPrefabSource.Asset);
            string[] serializedLevels;
            var t = typeof(MazeInfo);
            var firstProp = t.GetProperties()[0];
            string levelsText = asset.text;
            Task.Run(() =>
            {
                levelsText = levelsText.Remove(levelsText.Length - 2, 2);
                string splitter = "{" + "\"" + firstProp.Name + "\"";
                serializedLevels = levelsText.Split(new[] {splitter, "," + splitter}, StringSplitOptions.None);
                serializedLevels = serializedLevels
                    .RemoveRange(new[] {serializedLevels[0]})
                    .Select(_MazeSerialized => splitter + _MazeSerialized).ToArray();
                if (_Main)
                {
                    SerializedLevelsFromRemoteDict.SetSafe(_GameId, serializedLevels);
                    RemoteLevelsLoaded = true;
                }
                else
                {
                    SerializedLevelsFromCacheDict.SetSafe(_GameId, serializedLevels);
                    CachedLevelsLoaded = true;
                }
            });
        }

        protected static string PrefabSetName(int _GameId)
        {
            return $"game_{_GameId}_levels";
        }

        protected virtual string LevelsAssetName(int _HeapIndex, object[] _Args = null)
        {
            string heapName = _HeapIndex <= 0 ? null : $"levels_{_HeapIndex}";
            return heapName ?? $"levels_{_HeapIndex}";
        }

        protected void PreloadLevelsIfWereNotLoaded(int _GameId)
        {
            if (SerializedLevelsFromRemoteDict.GetSafe(_GameId, out _) == null)
                PreloadLevels(_GameId, true);
            if (SerializedLevelsFromCacheDict.GetSafe(_GameId, out _) == null)
                PreloadLevels(_GameId, false);
        }

        #endregion
    }
}