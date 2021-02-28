using System;
using System.Collections.Generic;
using System.Linq;
using Constants;
using Entities;
using GameHelpers;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using Network;
using UnityEngine;
using Utils;

namespace Managers
{
    public class ScoreManager : MonoBehaviour, ISingleton
    {
        #region singleton

        private static ScoreManager _instance;

        public static ScoreManager Instance = CommonUtils.MonoBehSingleton(ref _instance, "Score Manager");

        #endregion

        #region api

        public event ScoresEventHandler OnScoresChanged;

        public ScoresEntity GetMainScore()
        {
#if UNITY_EDITOR
            return GetMainScoreCached();
#elif UNITY_ANDROID
            return GetMainScoreAndroid();
#elif UNITY_IPHONE
            return GetMainScoreIos();
#endif
        }

        public void SetMainScore(int _Value)
        {
            SetMainScoreCache(_Value);
#if UNITY_ANDROID
            SetMainScoreAndroid(_Value);
#elif UNITY_IPHONE
            SetMainScoreIos(_Value);
#endif
        }
        
        public ScoresEntity GetScores(bool _ForcedFromServer = false)
        {
            var result = new ScoresEntity();
            result.Loaded = true;
            return result;
        }

        public void SetScore(ScoreType _ScoreType, int _Value)
        {
            var gff = new GameDataFieldFilter(GameClientUtils.AccountId,
                GameClientUtils.GameId,
                DataFieldIds.Main);
            gff.Filter(_Fields =>
            {
                if (_ScoreType == ScoreType.Main)
                {
                    _Fields.First(_F =>
                            _F.FieldId == DataFieldIds.Main)
                        .SetValue(_Value).Save();
                }
                OnScoresChanged?.Invoke(new ScoresEventArgs(GetScores()));
            });
        }
        
        #endregion
        
        #region nonpublic methods

        private ScoresEntity GetMainScoreCached()
        {
            int? score = SaveUtils.GetValue<int?>(SaveKey.MainScore);
            return new ScoresEntity{Loaded = true, Scores = new Dictionary<ScoreType, int>
            {
                {ScoreType.Main, score ?? 0}
            }};
        }
        
        private ScoresEntity GetMainScoreAndroid()
        {
            var result = new ScoresEntity();
            PlayGamesPlatform.Instance.LoadScores(
                GPGSIds.leaderboard_infinite_level,
                LeaderboardStart.PlayerCentered,
                100,
                LeaderboardCollection.Public,
                LeaderboardTimeSpan.AllTime,
                _Data =>
                {
                    if (_Data.Valid)
                    {
                        result.Scores.Add(ScoreType.Main, Convert.ToInt32(_Data.PlayerScore.value));
                        result.Loaded = true;
                    }
                    else result = GetMainScoreCached();
                });
            return result;
        }

        private ScoresEntity GetMainScoreIos()
        {
            //TODO
            return GetMainScoreCached();
        }

        private void SetMainScoreCache(int _Value)
        {
            int? val = _Value;
            SaveUtils.PutValue(SaveKey.MainScore, val);
        }

        private void SetMainScoreAndroid(int _Value)
        {
            //TODO
        }

        private void SetMainScoreIos(int _Value)
        {
            //TODO
        }
        
        #endregion
    }
    
    #region types

    public enum ScoreType
    {
        Main
    }

    public delegate void ScoresEventHandler(ScoresEventArgs _Args);

    public class ScoresEventArgs
    {
        public ScoresEntity ScoresEntity { get; }

        public ScoresEventArgs(ScoresEntity _ScoresEntity)
        {
            ScoresEntity = _ScoresEntity;
        }
    }
    
    #endregion
}