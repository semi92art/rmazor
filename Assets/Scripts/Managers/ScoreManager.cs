using System;
using System.Collections.Generic;
using System.Linq;
using Constants;
using Entities;
using GameHelpers;
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

        public ScoresEntity GetScores(bool _ForcedFromServer = false)
        {
            var result = new ScoresEntity();

            var gdf = new GameDataFieldFilter(
                GameClientUtils.AccountId,
                GameClientUtils.GameId,
                DataFieldIds.InfiniteLevelScore);
            gdf.Filter(_DataFields =>
            {
                int mainScore = _DataFields.First(_F =>
                    _F.FieldId == DataFieldIds.InfiniteLevelScore).ToInt();
                result.Scores.Add(ScoreType.Main, mainScore);
                result.Loaded = true;
                OnScoresChanged?.Invoke(new ScoresEventArgs(result));
            }, _ForcedFromServer);
            return result;
        }

        public void SetScore(ScoreType _ScoreType, int _Value)
        {
            var gff = new GameDataFieldFilter(GameClientUtils.AccountId,
                GameClientUtils.GameId,
                DataFieldIds.InfiniteLevelScore);
            gff.Filter(_Fields =>
            {
                if (_ScoreType == ScoreType.Main)
                {
                    _Fields.First(_F =>
                            _F.FieldId == DataFieldIds.InfiniteLevelScore)
                        .SetValue(_Value).Save();
                }
                OnScoresChanged?.Invoke(new ScoresEventArgs(GetScores()));
            });
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