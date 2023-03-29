using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Common.Constants;
using mazing.common.Runtime;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Utils;
using Newtonsoft.Json;
using RMAZOR.Enitities;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views;
using UnityEngine;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.Helpers
{
    public class LevelsLoaderRmazor : LevelsLoader
    {
        #region types

        private class GameModeAndLevelTypePair
        {
            private readonly string m_GameMode;
            private readonly string m_LevelType;

            public GameModeAndLevelTypePair(string _GameMode, string _LevelType)
            {
                m_GameMode  = _GameMode;
                m_LevelType = _LevelType;
            }

            public override bool Equals(object _Obj)
            {
                return Equals(_Obj as GameModeAndLevelTypePair);
            }

            private bool Equals(GameModeAndLevelTypePair _Obj)
            {
                return m_GameMode == _Obj.m_GameMode && m_LevelType == _Obj.m_LevelType;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 23 + m_GameMode.GetHashCode();
                    hash = hash * 23 + m_LevelType.GetHashCode();
                    return hash;
                }
            }

            public override string ToString()
            {
                return "Game Mode: " + m_GameMode + ", Level Type: " + m_LevelType;
            }
        }

        private class SerializedLevelsAndLoadedPair
        {
            public string[] SerializedLevelsBundle { get; set; }
            public string[] SerializedLevelsLocal  { get; set; }
            public bool     BundleLevelsLoaded     { get; set; }
            public bool     LocalLevelsLoaded      { get; set; }
        }

        #endregion
        
        #region nonpublic members
        
        private readonly Dictionary<GameModeAndLevelTypePair, SerializedLevelsAndLoadedPair> m_SerializedLevelsDict
            = new Dictionary<GameModeAndLevelTypePair,SerializedLevelsAndLoadedPair>();

        private IList<LevelGenerationParams> m_GenerationParamsForRandomLevels;
        
        private readonly object m_PreloadLevelLock = new object();

        #endregion

        #region inject
        
        private ILevelGeneratorRmazor LevelGeneratorRmazor { get; }

        protected LevelsLoaderRmazor(
            IPrefabSetManager     _PrefabSetManager,
            IMazeInfoValidator    _MazeInfoValidator,
            ILevelGeneratorRmazor _LevelGeneratorRmazor) 
            : base(
                _PrefabSetManager,
                _MazeInfoValidator)
        {
            LevelGeneratorRmazor = _LevelGeneratorRmazor;
        }

        #endregion

        #region api

        public override void Init()
        {
            PreloadLevelsIfWereNotLoaded();
            Cor.Run(Cor.WaitWhile(InitializationPredicate, RaiseInitialization));
            LoadGenerationParamsForRandomLevels();
        }

        public override Entity<MazeInfo> GetLevelInfo(LevelInfoArgs _Args)
        {
            var entyty = new Entity<MazeInfo>();
            if (_Args.GameMode == ParameterGameModeRandom)
            {
                int levelSize = (int) _Args.Arguments.GetSafe(KeyRandomLevelSize, out _);
                var genParams = GetLevelGenerationParams(levelSize);
                entyty = LevelGeneratorRmazor.GetLevelInfoRandomAsync(genParams);
                return entyty;
            }
            PreloadLevelsIfWereNotLoaded();
            string[] bundleLevels = GetLevelsCollection(_Args, false);
            string[] localLevels  = GetLevelsCollection(_Args, true);
            var mazeInfo = GetLevelInfoCore(_Args, bundleLevels);
            bool valid = MazeInfoValidator.Validate(mazeInfo, out string error);
            if (!valid)
            {
                Dbg.LogError("Remote maze info is not valid: " + error);
                mazeInfo = GetLevelInfoCore(_Args, localLevels);
            }
            valid = MazeInfoValidator.Validate(mazeInfo, out error);
            if (!valid) 
                throw GetLevelLoadException(_Args, error, bundleLevels, localLevels);
            entyty.Value = mazeInfo;
            entyty.Result = EEntityResult.Success;
            return entyty;
        }

        public override string GetLevelInfoRaw(LevelInfoArgs _Args)
        {
            string[] bundleLevels = GetLevelsCollection(_Args, false);
            string[] localLevels  = GetLevelsCollection(_Args, true);
            string GetInfo(IReadOnlyList<string> _Levels) => _Levels[(int)_Args.LevelIndex];
            string mazeInfoRaw = GetInfo(bundleLevels);
            bool valid = MazeInfoValidator.ValidateRaw(mazeInfoRaw, out string error);
            if (!valid)
            {
                Dbg.LogError("Remote maze info is not valid: " + error);
                mazeInfoRaw = GetInfo(localLevels);
            }
            if (valid)
                return mazeInfoRaw;
            throw GetLevelLoadException(_Args, error, bundleLevels, localLevels);
        }

        public override int GetLevelsCount(LevelInfoArgs _Args)
        {
            PreloadLevelsIfWereNotLoaded();
            var key = new GameModeAndLevelTypePair(_Args.GameMode, _Args.LevelType);
            var val = m_SerializedLevelsDict[key];
            var levels =  val.BundleLevelsLoaded ? val.SerializedLevelsBundle : val.SerializedLevelsLocal;
            return levels.Length;
        }

        #endregion

        #region nonpublic methods

        private bool InitializationPredicate()
        {
            var dictKeys = new[]
            {
                new GameModeAndLevelTypePair(ParameterGameModeMain,    ParameterLevelTypeDefault),
                new GameModeAndLevelTypePair(ParameterGameModeMain,    ParameterLevelTypeBonus),
                new GameModeAndLevelTypePair(ParameterGameModePuzzles, ParameterLevelTypeDefault),
            };
            bool allLevelsLoaded;
            lock (m_PreloadLevelLock)
            {
                allLevelsLoaded = dictKeys.All(_Key =>
                {
                    if (m_SerializedLevelsDict == null)
                        return false;
                    if (!m_SerializedLevelsDict.ContainsKey(_Key))
                        return false;
                    var value = m_SerializedLevelsDict[_Key];
                    if (value == null)
                        return false;
                    return value.BundleLevelsLoaded || value.LocalLevelsLoaded;
                });
            }
            return !allLevelsLoaded;
        }

        private string[] GetLevelsCollection(LevelInfoArgs _Args, bool _Local)
        {
            if (_Args.GameMode == ParameterGameModeDailyChallenge)
                _Args.GameMode = ParameterGameModeMain;
            var key = new GameModeAndLevelTypePair(_Args.GameMode, _Args.LevelType);
            var val = m_SerializedLevelsDict[key];
            var levels = _Local ? val.SerializedLevelsLocal : val.SerializedLevelsBundle;
            return levels;
        }

        private void PreloadLevels(bool _Local)
        {
            PreloadLevels(_Local, ParameterGameModeMain,    ParameterLevelTypeDefault);
            PreloadLevels(_Local, ParameterGameModeMain,    ParameterLevelTypeBonus);
            PreloadLevels(_Local, ParameterGameModePuzzles, ParameterLevelTypeDefault);
        }
        
        private void PreloadLevels(bool _Local, string _GameMode, string _LevelType)
        {
            var key = new GameModeAndLevelTypePair(_GameMode, _LevelType);
            if (m_SerializedLevelsDict.ContainsKey(key))
            {
                if (_Local && m_SerializedLevelsDict[key].LocalLevelsLoaded)
                    return;
                if (!_Local && m_SerializedLevelsDict[key].BundleLevelsLoaded)
                    return;
            }
            var asset = PrefabSetManager.GetObject<TextAsset>(PrefabSetName,
                LevelsAssetName(_GameMode, _LevelType),
                _Local ? EPrefabSource.Asset : EPrefabSource.Bundle);
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
                lock (m_PreloadLevelLock)
                {
                    if (!m_SerializedLevelsDict.ContainsKey(key))
                    {
                        var newVal = new SerializedLevelsAndLoadedPair();
                        m_SerializedLevelsDict.Add(key, newVal);
                    }
                    var val = m_SerializedLevelsDict[key];
                    if (!_Local)
                        (val.SerializedLevelsBundle, val.BundleLevelsLoaded) = (serializedLevels, true);
                    else
                        (val.SerializedLevelsLocal, val.LocalLevelsLoaded) = (serializedLevels, true);
                }
            });
        }

        private static string LevelsAssetName(
            string _GameMode,
            string _LevelType,
            int?   _HeapIndexForced = null)
        {
            if (_HeapIndexForced != null)
                return LevelsAssetName(_HeapIndexForced.Value);
            int heapIndex = _GameMode switch
            {
                ParameterGameModeMain => _LevelType switch
                {
                    ParameterLevelTypeDefault  => 1,
                    ParameterLevelTypeBonus    => 2,
                    _                          => throw new SwitchExpressionException(_LevelType)
                },
                ParameterGameModePuzzles => 3,
                _                        => throw new SwitchExpressionException(_GameMode)
            };
            return LevelsAssetName(heapIndex);
        }

        private static Exception GetLevelLoadException(
            LevelInfoArgs               _Args,
            string                      _Error,
            IReadOnlyCollection<string> _BundleLevelsCollection,
            IReadOnlyCollection<string> _LocalLevelsCollection)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Local maze info is not valid: " + _Error);
            sb.AppendLine("Level index: "                  + _Args.LevelIndex);
            sb.AppendLine("Game mode: "                    + _Args.GameMode);
            sb.AppendLine("Level type: "                   + _Args.LevelType);
            sb.AppendLine("Bundle levels list count: "     + _BundleLevelsCollection?.Count);
            sb.AppendLine("Local levels list count: "      + _LocalLevelsCollection?.Count);
            return new Exception(sb.ToString());
        }

        private LevelGenerationParams GetLevelGenerationParams(int _LevelSizeArg)
        {
            if (_LevelSizeArg != -1) 
                return m_GenerationParamsForRandomLevels[_LevelSizeArg];
            static float Formula(float _V) => _V * _V; 
            float randVal = Formula(UnityEngine.Random.value) * (1f - MathUtils.Epsilon);
            _LevelSizeArg = Mathf.FloorToInt(m_GenerationParamsForRandomLevels.Count * randVal);
            return m_GenerationParamsForRandomLevels[_LevelSizeArg];
        }

        private void LoadGenerationParamsForRandomLevels()
        {
            var genParamsScrObj = PrefabSetManager.GetObject<LevelGenerationParamsSetScriptableObject>(
                CommonPrefabSetNames.Configs, 
                "random_levels_gen_params");
            m_GenerationParamsForRandomLevels = genParamsScrObj.set
                .Where(_Item => _Item.inUse)
                .ToList();
        }
        
        private static MazeInfo GetLevelInfoCore(LevelInfoArgs _Args, IReadOnlyList<string> _Levels)
        {
            string level = _Levels[(int)_Args.LevelIndex];
            if (_Args.LevelIndex >= _Levels.Count)
                return null;
            var levelInfo = JsonConvert.DeserializeObject<MazeInfo>(level);
            if (_Args.Arguments.ContainsKey(KeyRemoveTrapsFromLevel))
                RemoveTrapsFromLevelInfo(levelInfo);
            return levelInfo;
        }

        private static void RemoveTrapsFromLevelInfo(MazeInfo _LevelInfo)
        {
            var trapTypesToRemoveWithBlock = new[]
            {
                EMazeItemType.Hammer,
                EMazeItemType.Turret,
                EMazeItemType.TrapIncreasing,
                EMazeItemType.TrapReact
            };
            var trapTypesToRemoveWithPath = new[]
            {
                EMazeItemType.Spear,
                EMazeItemType.GravityTrap,
                EMazeItemType.TrapMoving
            };
            foreach (var mazeItem in _LevelInfo.MazeItems.ToList())
            {
                if (trapTypesToRemoveWithBlock.Contains(mazeItem.Type))
                    mazeItem.Type = EMazeItemType.Block;
                else if (trapTypesToRemoveWithPath.Contains(mazeItem.Type))
                {
                    _LevelInfo.MazeItems.Remove(mazeItem);
                    var pathItem = new PathItem {Blank = false, Position = mazeItem.Position};
                    _LevelInfo.PathItems.Add(pathItem);
                }
            }
        }
        
        private void PreloadLevelsIfWereNotLoaded()
        {
            PreloadLevels(true);
            PreloadLevels(false);
        }

        #endregion
    }
}