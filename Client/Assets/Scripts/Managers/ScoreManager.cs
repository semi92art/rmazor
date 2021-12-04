using System.Collections.Generic;
using System.Linq;
using Constants;
using Entities;
using GameHelpers;
using UnityEngine;
using UnityEngine.Events;
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

    public interface IScoreManager : IInit
    {
        event ScoresEventHandler OnScoresChanged;
        ScoresEntity GetScore(ushort _Id);
        void SetScore(ushort _Id, int _Value);
        void ShowLeaderboard();
    }
    
    public class ScoreManager : IScoreManager
    {
        #region types

        private class ScoreArgs
        {
            public ushort Id         { get; }
            public string Key        { get; }
            public bool   OnlyCached { get; }

            public ScoreArgs(ushort _Id, string _Key, bool _OnlyCached)
            {
                Id = _Id;
                Key = _Key;
                OnlyCached = _OnlyCached;
            }
        }

        #endregion
        
        #region nonpublic members

        private readonly IReadOnlyList<ScoreArgs> m_ScoreArgsList = new List<ScoreArgs>
        {
            new ScoreArgs(DataFieldIds.Money, Application.platform == RuntimePlatform.Android ? GPGSIds.coins : "money", false),
            new ScoreArgs(DataFieldIds.Level, "level", true)
        };

        #endregion
        
        #region api

        public event ScoresEventHandler OnScoresChanged;
        
        public event UnityAction Initialized;
        public void Init()
        {
            AuthenticateSocial(() => Initialized?.Invoke());
        }

        public ScoresEntity GetScore(ushort _Id)
        {
            if (IsScoreOnlyCached(_Id))
                return GetScoreCached(_Id);
#if UNITY_EDITOR
            return GetScoreCached(_Id);
#elif UNITY_ANDROID
            return GetScoreAndroid(_Id);
#elif UNITY_IPHONE || UNITY_IOS
            return GetScoreIos(_Id);
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

        public void ShowLeaderboard()
        {
#if UNITY_EDITOR
            //do nothing
#elif UNITY_ANDROID
            ShowLeaderboardAndroid();
#elif UNITY_IPHONE || UNITY_IOS
            ShowLeaderboardIos();
#endif
        }

        #endregion

        #region nonpublic methods

        private void AuthenticateSocial(UnityAction _OnSucceed)
        {
            Social.localUser.Authenticate(_Success =>
            {
                if (_Success)
                {
                    Dbg.Log("Social authentication succeeded.");
                    string userInfo = "Username: " + Social.localUser.userName +
                                      "\nUser ID: " + Social.localUser.id +
                                      "\nIsUnderage: " + Social.localUser.underage;
                    Dbg.Log(userInfo);
                    _OnSucceed?.Invoke();
                }
                else
                {
                    Dbg.LogError("Social authentication failed.");
                }
            });
        }

        private static ScoresEntity GetScoreCached(ushort _Id)
        {
            var scores = new ScoresEntity{Result = EEntityResult.Pending};
            var gdff = new GameDataFieldFilter(GameClientUtils.AccountId, GameClientUtils.GameId,
                _Id) {OnlyLocal = true};
            gdff.Filter(_Fields =>
            {
                var scoreField = _Fields.First();
                scores.Value.Add(_Id, scoreField.ToInt());
                scores.Result = EEntityResult.Success;
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
            var args = m_ScoreArgsList.FirstOrDefault(_Args => _Args.Id == _Id);
            if (args != null) 
                return m_ScoreArgsList.FirstOrDefault(_Args => _Args.Id == _Id)?.Key;
            Dbg.LogError($"Score with id {_Id} does not exist.");
            return null;
        }

        private bool IsScoreOnlyCached(ushort _Id)
        {
            var args = m_ScoreArgsList.FirstOrDefault(_Args => _Args.Id == _Id);
            if (args != null) 
                return args.OnlyCached;
            Dbg.LogError($"Score with id {_Id} does not exist.");
            return true;
        }

#if UNITY_ANDROID && !UNITY_EDITOR

        private ScoresEntity GetScoreAndroid(ushort _Id)
        {
            var result = new ScoresEntity{Result = EEntityResult.Pending};
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
                        result.Value.Add(_Id, System.Convert.ToInt32(_Data.PlayerScore.value));
                        result.Result = _Data.Status == GooglePlayGames.BasicApi.ResponseStatus.Success
                            ? EEntityResult.Success
                            : EEntityResult.Fail;
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
            GooglePlayGames.PlayGamesPlatform.Instance.ShowLeaderboardUI(GPGSIds.coins);
        }

#elif (UNITY_IPHONE || UNITY_IOS) && !UNITY_EDITOR

        private ScoresEntity GetScoreIos(ushort _Id)
        {
            if (!Social.localUser.authenticated)
            {
                Dbg.LogWarning("User is not authenticated to Game Center");
                return GetScoreCached(_Id);
            }
            var score = new ScoresEntity{Result = EEntityResult.Pending};
            Social.LoadScores( "coins", _Scores =>
            {
                var cachedScore = GetScoreCached(_Id).GetScore(_Id);
                var socialScore = _Scores.FirstOrDefault(_S => _S.userID == Social.localUser.id);
                if (socialScore != null)
                {
                    if (cachedScore.HasValue && cachedScore > (int)socialScore.value)
                        SetScoreIos(_Id, cachedScore.Value);
                    else 
                        SetScoreCache(_Id, (int)socialScore.value);
                    score.Value.Add(_Id, (int)socialScore.value);
                    score.Result = EEntityResult.Success;
                }
                else
                {
                    score.Result = EEntityResult.Fail;
                    Dbg.LogWarning("Failed to get score from Game Center leaderboard");
                }
                score.Value = GetScoreCached(_Id).Value;
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