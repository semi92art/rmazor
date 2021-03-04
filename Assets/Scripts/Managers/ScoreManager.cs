using System.Collections.Generic;
using System.Linq;
using Constants;
using Entities;
using GameHelpers;
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
#if UNITY_ANDROID && !UNITY_EDITOR
            SetMainScoreAndroid(_Value);
#elif UNITY_IPHONE && !UNITY_EDITOR
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
        
        public void ShowLeaderboard()
        {
#if UNITY_EDITOR
            //do nothing
#elif UNITY_ANDROID
            ShowLeaderboardAndroid();
#elif UNITY_IPHONE
            ShowLeaderboardIos();
#endif
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
        
        private void SetMainScoreCache(int _Value)
        {
            int? val = _Value;
            SaveUtils.PutValue(SaveKey.MainScore, val);
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private ScoresEntity GetMainScoreAndroid()
        {
            var result = new ScoresEntity();
            GooglePlayGames.PlayGamesPlatform.Instance.LoadScores(
                GPGSIds.leaderboard_infinite_level,
                GooglePlayGames.BasicApi.LeaderboardStart.PlayerCentered,
                100,
                GooglePlayGames.BasicApi.LeaderboardCollection.Public,
                GooglePlayGames.BasicApi.LeaderboardTimeSpan.AllTime,
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
        
        private void SetMainScoreAndroid(int _Value)
        {
            Social.ReportScore(_Value, GPGSIds.leaderboard_infinite_level, _Success => 
            {
                if (!_Success)
                    Debug.LogError("Failed to post leaderboard score");
            });
        }
        
        private static void ShowLeaderboardAndroid()
        {
            GooglePlayGames.PlayGamesPlatform.Instance.ShowLeaderboardUI(GPGSIds.leaderboard_infinite_level);
        }

#elif UNITY_IPHONE && !UNITY_EDITOR
        
        private ScoresEntity GetMainScoreIos()
        {
            if (!Social.localUser.authenticated)
            {
                Debug.LogWarning("User is not authenticated to Game Center");
                return GetMainScoreCached();
            }
            var score = new ScoresEntity();
            Social.LoadScores( "mazes_infinite_level_score", _Scores =>
            {
                var cachedScore = GetMainScoreCached().Scores[ScoreType.Main];
                var socialScore = _Scores.FirstOrDefault(_S => _S.userID == Social.localUser.id);
                if (socialScore != null)
                {
                    if (cachedScore > (int)socialScore.value)
                        SetMainScoreIos(cachedScore);
                    else 
                        SetMainScoreCache((int)socialScore.value);
                    score.Scores.Add(ScoreType.Main, (int)socialScore.value);
                }
                else
                    Debug.LogWarning("Failed to get score from Game Center leaderboard");
                score.Scores = GetMainScoreCached().Scores;
                score.Loaded = true;
            });
            return score;
        }
        
        private void SetMainScoreIos(int _Value)
        {
            if (!Social.localUser.authenticated)
            {
                Debug.LogWarning("User is not authenticated to Game Center");
                return;
            }
            Social.LoadScores( "mazes_infinite_level_score", _Scores =>
            {
                var socialScore = _Scores.FirstOrDefault(_S => _S.userID == Social.localUser.id);
                if (socialScore == null)
                {
                    Debug.LogWarning("Failed to set score to Game Center leaderboard");
                    return;
                }
                socialScore.value = _Value;
            });
        }
        
        private static void ShowLeaderboardIos()
        {
            Social.ShowLeaderboardUI();
        }
        
#endif
        
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