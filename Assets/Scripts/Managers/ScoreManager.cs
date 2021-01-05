using System;
using System.Collections.Generic;
using Entities;
using Network;
using Network.PacketArgs;
using Network.Packets;
using UnityEngine;
using Utils;

namespace Managers
{
    public class ScoreManager : MonoBehaviour, ISingleton
    {
        #region singleton

        private static ScoreManager _instance;

        public static ScoreManager Instance = CommonUtils.Singleton(ref _instance, "Score Manager");

        #endregion
        
        #region nonpublic members

        private bool m_IsScoresLoadedLocal;

        #endregion
        
        #region api

        public event ScoresEventHandler OnScoresChanged;

        public ScoresEntity GetScores(bool _ForcedFromServer = false)
        {
            var result = new ScoresEntity();
            if ((!m_IsScoresLoadedLocal || _ForcedFromServer)
                && GameClient.Instance.LastConnectionSucceeded)
            {
                var scoresPacket = new GetScoresPacket(new AccIdGameId
                {
                    AccountId = GameClient.Instance.AccountId,
                    GameId = GameClient.Instance.GameId
                });
                scoresPacket.OnSuccess(() =>
                {
                    foreach (var score in scoresPacket.Response)
                        result.Scores.Add(score.Type, score.Points);
                    result.Loaded = true;
                    SetScoresLocal(result);
                }).OnFail(() =>
                {
                    Debug.LogError(scoresPacket.ErrorMessage);
                    result = GetScoresLocal(result);
                });

                GameClient.Instance.Send(scoresPacket);
            }
            else
                result = GetScoresLocal();
            return result;
        }

        public void SetScore(string _ScoreType, int _Value)
        {
            var scores = GetScoresLocal();
            if (GameClient.Instance.LastConnectionSucceeded)
            {
                var scoresPacket = new SetScorePacket(new SetScoreRequestArgs
                {
                    AccountId = GameClient.Instance.AccountId,
                    GameId = GameClient.Instance.GameId,
                    Type = _ScoreType,
                    Points = _Value,
                    LastUpdateTime = DateTime.Now
                });
                scoresPacket.OnFail(() => Debug.Log(scoresPacket.ErrorMessage));
                GameClient.Instance.Send(scoresPacket);
            }
            scores.Scores.SetEvenIfNotContainKey(_ScoreType, _Value);
            SetScoresLocal(scores);
        }
        
        #endregion
        
        #region nonpublic methods

        private ScoresEntity GetScoresLocal(ScoresEntity _ScoresEntity = null)
        {
            var currentScores = SaveUtils.GetValue<Dictionary<string, int>>(SaveKey.Scores);
            if (_ScoresEntity == null)
                return new ScoresEntity
                {
                    Scores = currentScores,
                    Loaded = true
                };
            _ScoresEntity.Scores = currentScores;
            _ScoresEntity.Loaded = true;
            return _ScoresEntity;
        }
        
        private void SetScoresLocal(ScoresEntity _ScoresEntity)
        {
            var currentScores = GetScoresLocal().Scores ??
                                new Dictionary<string, int>();
            foreach (var key in _ScoresEntity.Scores.Keys)
                currentScores.SetEvenIfNotContainKey(key, _ScoresEntity.Scores[key]);
            SaveUtils.PutValue(SaveKey.Scores, currentScores);
            m_IsScoresLoadedLocal = true;
            OnScoresChanged?.Invoke(new ScoresEventArgs(GetScoresLocal()));
        }

        #endregion
    }
    
    #region types

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