using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Extensions;
using Common.Managers;
using Common.Utils;
using mazing.common.Runtime;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Utils;
using Newtonsoft.Json;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using UnityEngine;

namespace RMAZOR.Helpers
{
    public class LevelsLoaderRmazor : LevelsLoader
    {
        #region nonpublic members

        private string[]
            m_SerializedBonusLevelsFromCache  = new string[0],
            m_SerializedBonusLevelsFromRemote = new string[0];

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
                return !mainLevelsLoaded || !bonusLevelsLoaded;
            }
            Cor.Run(Cor.WaitWhile(
                InitializationPredicate,
                RaiseInitialization));
        }

        public override MazeInfo GetLevelInfo(int _GameId, long _Index, bool _IsBonus)
        {
            PreloadLevelsIfWereNotLoaded(_GameId);
            MazeInfo GetInfo(IReadOnlyList<string> _Levels)
            {
                string level = _Levels[(int)_Index];
                return _Index < _Levels.Count ? 
                    JsonConvert.DeserializeObject<MazeInfo>(level) : null;
            }
            var dicRemote  = _IsBonus ? m_SerializedBonusLevelsFromRemote : SerializedLevelsFromRemote;
            var dictCached = _IsBonus ? m_SerializedBonusLevelsFromCache  : SerializedLevelsFromCache;
            var mazeInfo = GetInfo(dicRemote);
            bool valid = MazeInfoValidator.Validate(mazeInfo, out string error);
            if (!valid)
            {
                Dbg.LogError("Remote maze info is not valid: " + error);
                mazeInfo = GetInfo(dictCached);
            }
            valid = MazeInfoValidator.Validate(mazeInfo, out error);
            if (valid)
                return mazeInfo;
            var sb = new StringBuilder();
            sb.AppendLine("Local maze info is not valid: " + error);
            sb.AppendLine("Game Id: " + _GameId);
            sb.AppendLine("Level index: " + _Index);
            sb.AppendLine("Is bonus level: " + _IsBonus);
            sb.AppendLine("Remote levels list count: " + dicRemote.Length);
            sb.AppendLine("Local levels list count: " + dictCached.Length);
            throw new Exception(sb.ToString());
        }

        public override string GetLevelInfoRaw(int _GameId, long _Index, bool _IsBonus)
        {
            var dicRemote  = _IsBonus ? m_SerializedBonusLevelsFromRemote : SerializedLevelsFromRemote;
            var dictCached = _IsBonus ? m_SerializedBonusLevelsFromCache  : SerializedLevelsFromCache;
            string GetInfo(IReadOnlyList<string> _Levels) => _Levels[(int)_Index];
            string mazeInfoRaw = GetInfo(dicRemote);
            bool valid = MazeInfoValidator.ValidateRaw(mazeInfoRaw, out string error);
            if (!valid)
            {
                Dbg.LogError("Remote maze info is not valid: " + error);
                mazeInfoRaw = GetInfo(dictCached);
            }
            if (valid)
                return mazeInfoRaw;
            var sb = new StringBuilder();
            sb.AppendLine("Local maze info is not valid: " + error);
            sb.AppendLine("Game Id: " + _GameId);
            sb.AppendLine("Level index: " + _Index);
            sb.AppendLine("Is bonus level: " + _IsBonus);
            sb.AppendLine("Remote levels list count: " + dicRemote.Length);
            sb.AppendLine("Local levels list count: " + dictCached.Length);
            throw new Exception(sb.ToString());
        }

        public override int GetLevelsCount(int _GameId, bool _IsBonus)
        {
            PreloadLevelsIfWereNotLoaded(_GameId);
            if (_IsBonus)
            {
                return m_SerializedBonusLevelsFromRemote.Length > 0
                    ? m_SerializedBonusLevelsFromRemote.Length
                    : m_SerializedBonusLevelsFromCache.Length;
            }
            return SerializedLevelsFromRemote.Length > 0
                ? SerializedLevelsFromRemote.Length
                : SerializedLevelsFromCache.Length;
        }

        #endregion

        #region nonpublic methods

        protected override void PreloadLevels(int _GameId, bool _Bundle)
        {
            PreloadLevels(_GameId, _Bundle, true);
            PreloadLevels(_GameId, _Bundle, false);
        }
        
        private void PreloadLevels(int _GameId, bool _Bundle, bool _Bonus)
        {
            if (!_Bonus)
            {
                base.PreloadLevels(_GameId, _Bundle);
                return;
            }
            int heapIndex = Application.isEditor ? SaveUtilsInEditor.GetValue(SaveKeysInEditor.StartHeapIndex) : 1;
            var args = new Dictionary<string, object>
            {
                {CommonInputCommandArg.KeyNextLevelType, CommonInputCommandArg.ParameterLevelTypeBonus}
            };
            var asset = PrefabSetManager.GetObject<TextAsset>(PrefabSetName(_GameId),
                LevelsAssetName(heapIndex, true),
                _Bundle ? EPrefabSource.Bundle : EPrefabSource.Asset);
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
                if (_Bundle)
                {
                    m_SerializedBonusLevelsFromRemote = serializedLevels;
                    m_RemoteBonusLevelsLoaded = true;
                }
                else
                {
                    m_SerializedBonusLevelsFromCache = serializedLevels;
                    m_CachedBonusLevelsLoaded = true;
                }
            });
        }
        
        protected override string LevelsAssetName(int _HeapIndex, bool _IsBonus)
        {
            return base.LevelsAssetName(_HeapIndex, false) + (!_IsBonus ? string.Empty : "_bonus");
        }

        #endregion

    }
}