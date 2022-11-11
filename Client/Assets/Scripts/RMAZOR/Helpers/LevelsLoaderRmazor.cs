using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Extensions;
using Common.Managers;
using Common.Utils;
using Newtonsoft.Json;
using RMAZOR.Models.MazeInfos;
using UnityEngine;

namespace RMAZOR.Helpers
{
    public class LevelsLoaderRmazor : LevelsLoader
    {
        #region nonpublic members

        private readonly Dictionary<int, string[]> 
            m_SerializedBonusLevelsFromCacheDict  = new Dictionary<int, string[]>(),
            m_SerializedBonusLevelsFromRemoteDict = new Dictionary<int, string[]>();
        
        private bool m_CachedBonusLevelsLoaded, m_RemoteBonusLevelsLoaded;

        #endregion

        #region inject

        protected LevelsLoaderRmazor(
            IPrefabSetManager  _PrefabSetManager,
            IMazeInfoValidator _MazeInfoValidator) 
            : base(
                _PrefabSetManager,
                _MazeInfoValidator) { }

        #endregion

        #region api

        public override void Init()
        {
            PreloadLevels(CommonData.GameId, true);
            PreloadLevels(CommonData.GameId, false);
            bool InitializationPredicate()
            {
                bool mainLevelsLoaded  = CachedLevelsLoaded        || RemoteLevelsLoaded;
                bool bonusLevelsLoaded = m_CachedBonusLevelsLoaded || m_RemoteBonusLevelsLoaded;
                return mainLevelsLoaded && bonusLevelsLoaded;
            }
            Cor.Run(Cor.WaitWhile(
                InitializationPredicate,
                RaiseInitialization));
        }

        public override MazeInfo GetLevelInfo(int _GameId, long _Index, object[] _Args = null)
        {
            PreloadLevelsIfWereNotLoaded(_GameId);
            MazeInfo Deserialize(IReadOnlyDictionary<int, string[]> _Dict) => JsonConvert.DeserializeObject<MazeInfo>(
                _Dict[_GameId][_Index]);
            bool isBonusLevel = IsBonusLevel(_Args);
            var dicRemote = isBonusLevel ? 
                m_SerializedBonusLevelsFromRemoteDict
                : SerializedLevelsFromRemoteDict;
            var mazeInfo = Deserialize(dicRemote);
            bool valid = MazeInfoValidator.Validate(mazeInfo);
            if (!valid)
            {
                var dictCached = isBonusLevel
                    ? m_SerializedBonusLevelsFromCacheDict
                    : SerializedLevelsFromCacheDict;
                mazeInfo = Deserialize(dictCached);
            }
            valid = MazeInfoValidator.Validate(mazeInfo);
            if (valid)
                return mazeInfo;
            throw new Exception("Maze info is not valid!");
        }

        #endregion

        #region nonpublic methods

        protected override void PreloadLevels(int _GameId, bool _Main)
        {
            PreloadLevels(_GameId, _Main, true);
            PreloadLevels(_GameId, _Main, false);
        }
        
        private void PreloadLevels(int _GameId, bool _Main, bool _Bonus)
        {
            if (!_Bonus)
            {
                base.PreloadLevels(_GameId, _Main);
                return;
            }
            int heapIndex = Application.isEditor ? SaveUtilsInEditor.GetValue(SaveKeysInEditor.StartHeapIndex) : 1;
            var asset = PrefabSetManager.GetObject<TextAsset>(PrefabSetName(_GameId),
                LevelsAssetName(heapIndex, new object[] { "bonus" }),
                _Main ? EPrefabSource.Bundle : EPrefabSource.Asset);
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
                    m_SerializedBonusLevelsFromRemoteDict.SetSafe(_GameId, serializedLevels);
                    m_RemoteBonusLevelsLoaded = true;
                }
                else
                {
                    m_SerializedBonusLevelsFromCacheDict.SetSafe(_GameId, serializedLevels);
                    m_CachedBonusLevelsLoaded = true;
                }
            });
        }
        
        protected override string LevelsAssetName(int _HeapIndex, object[] _Args = null)
        {
            if (!IsBonusLevel(_Args)) 
                return base.LevelsAssetName(_HeapIndex, _Args);
            return base.LevelsAssetName(_HeapIndex, _Args) + "_bonus";
        }
        
        private static bool IsBonusLevel(object[] _Args)
        {
            return _Args != null && _Args.Contains("bonus");
        }

        #endregion

    }
}