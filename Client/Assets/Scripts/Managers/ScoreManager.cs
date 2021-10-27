using System.Linq;
using Constants;
using Entities;
using GameHelpers;
using UnityEngine;
using Utils;

namespace Managers
{
    public delegate void ScoresEventHandler(ScoresEventArgs _Args);

    public class ScoresEventArgs
    {
        public ScoresEntity ScoresEntity { get; }

        public ScoresEventArgs(ScoresEntity _ScoresEntity)
        {
            ScoresEntity = _ScoresEntity;
        }
    }

    public interface IScoreManager
    {
        event ScoresEventHandler OnScoresChanged;
        ScoresEntity GetScore(ushort _Id);
        void SetScore(ushort _Id, int _Value);
        void ShowLeaderboard();
    }
    
    public class ScoreManager : IScoreManager
    {
        #region api

        public event ScoresEventHandler OnScoresChanged;

        public ScoresEntity GetScore(ushort _Id)
        {
            if (IsScoreOnlyCached(_Id))
                return GetScoreCached(_Id);
#if UNITY_EDITOR
            return GetScoreCached(_Id);
#elif UNITY_ANDROID
            return GetScoreAndroid(_Id);
#elif UNITY_IPHONE
            return GetMainScoreIos(_Id);
#endif
        }

        public void SetScore(ushort _Id, int _Value)
        {
            SetScoreCache(_Id, _Value);
            if (IsScoreOnlyCached(_Id))
                return;
#if UNITY_ANDROID && !UNITY_EDITOR
            SetScoreAndroid(_Id, _Value);
#elif UNITY_IPHONE && !UNITY_EDITOR
            SetScoreIos(_Id, _Value);
#endif
        }

        // public ScoresEntity GetScores(bool _ForcedFromServer = false)
        // {
        //     var result = new ScoresEntity();
        //     result.Loaded = true;
        //     return result;
        // }

        // public void SetScore(ScoreType _ScoreType, int _Value)
        // {
        //     var gff = new GameDataFieldFilter(GameClientUtils.AccountId,
        //         GameClientUtils.GameId,
        //         DataFieldIds.MainScore);
        //     gff.Filter(_Fields =>
        //     {
        //         if (_ScoreType == ScoreType.Coins)
        //         {
        //             _Fields.First(_F =>
        //                     _F.FieldId == DataFieldIds.MainScore)
        //                 .SetValue(_Value).Save();
        //         }
        //
        //         OnScoresChanged?.Invoke(new ScoresEventArgs(GetScores()));
        //     });
        // }

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

        private static ScoresEntity GetScoreCached(ushort _Id)
        {
            var scores = new ScoresEntity();
            var gdff = new GameDataFieldFilter(GameClientUtils.AccountId, GameClientUtils.GameId,
                _Id) {OnlyLocal = true};
            gdff.Filter(_Fields =>
            {
                var scoreField = _Fields.First();
                scores.Scores.Add(_Id, scoreField.ToInt());
                scores.Loaded = true;
            });
            return scores;
        }

        private static void SetScoreCache(ushort _Id, int _Value)
        {
            var gdff = new GameDataFieldFilter(GameClientUtils.AccountId, GameClientUtils.GameId,
                _Id) {OnlyLocal = true};
            gdff.Filter(_Fields =>
            {
                var scoreField = _Fields.First();
                scoreField.SetValue(_Value).Save(true);
            });
        }
        
        private string GetScoreKey(ushort _Id)
        {
            return null; // TODO
        }

        private bool IsScoreOnlyCached(ushort _Id)
        {
            return true; // TODO
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private ScoresEntity GetScoreAndroid(ushort _Id)
        {
            var result = new ScoresEntity();
            GooglePlayGames.PlayGamesPlatform.Instance.LoadScores(
                GetScoreKey(_Id),
                GooglePlayGames.BasicApi.LeaderboardStart.PlayerCentered,
                100,
                GooglePlayGames.BasicApi.LeaderboardCollection.Public,
                GooglePlayGames.BasicApi.LeaderboardTimeSpan.AllTime,
                _Data =>
                { 
                    if (_Data.Valid)
                    {
                        result.Scores.Add(_Id, System.Convert.ToInt32(_Data.PlayerScore.value));
                        result.Loaded = true;
                    }
                    else result = GetScoreCached(_Id);
                });
            return result;
        }
        
        private void SetScoreAndroid(ushort _Id, int _Value)
        {
            Social.ReportScore(_Value, GetScoreKey(_Id), _Success => 
            {
                if (!_Success)
                    Dbg.LogError("Failed to post leaderboard score");
            });
        }
        
        private static void ShowLeaderboardAndroid()
        {
            GooglePlayGames.PlayGamesPlatform.Instance.ShowLeaderboardUI(GPGSIds.leaderboard_infinite_level);
        }

#elif UNITY_IPHONE && !UNITY_EDITOR

        private ScoresEntity GetMainScoreIos(ushort _Id)
        {
            if (!Social.localUser.authenticated)
            {
                Dbg.LogWarning("User is not authenticated to Game Center");
                return GetScoreCached(_Id);
            }
            var score = new ScoresEntity();
            Social.LoadScores( "mazes_infinite_level_score", _Scores =>
            {
                var cachedScore = GetScoreCached(_Id).GetScore(_Id);
                var socialScore = _Scores.FirstOrDefault(_S => _S.userID == Social.localUser.id);
                if (socialScore != null)
                {
                    if (cachedScore.HasValue && cachedScore > (int)socialScore.value)
                        SetScoreIos(_Id, cachedScore.Value);
                    else 
                        SetScoreCache(_Id, (int)socialScore.value);
                    score.Scores.Add(_Id, (int)socialScore.value);
                }
                else
                    Dbg.LogWarning("Failed to get score from Game Center leaderboard");
                score.Scores = GetScoreCached(_Id).Scores;
                score.Loaded = true;
            });
            return score;
        }
        
        private void SetScoreIos(ushort _Id, int _Value)
        {
            if (!Social.localUser.authenticated)
            {
                Dbg.LogWarning("User is not authenticated to Game Center");
                return;
            }
            Social.LoadScores( GetScoreKey(_Id), _Scores =>
            {
                var socialScore = _Scores.FirstOrDefault(_S => _S.userID == Social.localUser.id);
                if (socialScore == null)
                {
                    Dbg.LogWarning("Failed to set score to Game Center leaderboard");
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
}