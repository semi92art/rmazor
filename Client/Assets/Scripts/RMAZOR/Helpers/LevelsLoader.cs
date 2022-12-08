using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Utils;
using RMAZOR.Models.MazeInfos;
using UnityEngine;

namespace RMAZOR.Helpers
{
    public interface ILevelsLoader : IInit
    {
        string   GetLevelInfoRaw(int _GameId, long _Index, Dictionary<string, object> _Args);
        MazeInfo GetLevelInfo(int    _GameId, long            _Index, Dictionary<string, object> _Args);
        int      GetLevelsCount(int  _GameId, Dictionary<string, object> _Args);
    }
    
    public abstract class LevelsLoader : InitBase, ILevelsLoader
    {
        #region nonpublic members

        protected string[]
            SerializedLevelsFromCache  = new string[0],
            SerializedLevelsFromRemote = new string[0];

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
        
        public abstract MazeInfo GetLevelInfo(int    _GameId, long _Index, Dictionary<string, object> _Args);
        public abstract string GetLevelInfoRaw(int _GameId, long _Index, Dictionary<string, object> _Args);

        public virtual int GetLevelsCount(int _GameId, Dictionary<string, object> _Args = null)
        {
            PreloadLevelsIfWereNotLoaded(_GameId);
            return SerializedLevelsFromRemote.Length > 0
                ? SerializedLevelsFromRemote.Length
                : SerializedLevelsFromCache.Length;
        }

        #endregion

        #region nonpublic methods
        
        protected virtual void PreloadLevels(int _GameId, bool _Bundle)
        {
            int heapIndex = Application.isEditor ? SaveUtilsInEditor.GetValue(SaveKeysInEditor.StartHeapIndex) : 1;
            if (heapIndex <= 0)
                heapIndex = 1;
            var asset = PrefabSetManager.GetObject<TextAsset>(PrefabSetName(_GameId),
                LevelsAssetName(heapIndex), _Bundle ? EPrefabSource.Bundle : EPrefabSource.Asset);
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
                    SerializedLevelsFromRemote = serializedLevels;
                    RemoteLevelsLoaded = true;
                }
                else
                {
                    SerializedLevelsFromCache = serializedLevels;
                    CachedLevelsLoaded = true;
                }
            });
        }

        protected static string PrefabSetName(int _GameId)
        {
            return $"game_{_GameId}_levels";
        }

        protected virtual string LevelsAssetName(int _HeapIndex, Dictionary<string, object> _Args = null)
        {
            string heapName = _HeapIndex <= 0 ? null : $"levels_{_HeapIndex}";
            return heapName ?? $"levels_{_HeapIndex}";
        }

        protected void PreloadLevelsIfWereNotLoaded(int _GameId)
        {
            if (SerializedLevelsFromRemote == null)
                PreloadLevels(_GameId, true);
            if (SerializedLevelsFromCache == null)
                PreloadLevels(_GameId, false);
        }

        #endregion
    }
}