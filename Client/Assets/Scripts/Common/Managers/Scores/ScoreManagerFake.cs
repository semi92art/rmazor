using Common.Entities;
using Common.Helpers;
using Common.Network;
using Common.Ticker;
using UnityEngine.Events;
// ReSharper disable ClassNeverInstantiated.Global

namespace Common.Managers.Scores
{
    public class ScoreManagerFake : ScoreManagerBase
    {
        public ScoreManagerFake(
            CommonGameSettings       _Settings,
            IGameClient              _GameClient,
            ILocalizationManager     _LocalizationManager,
            ICommonTicker            _Ticker,
            IRemoteSavedGameProvider _RemoteSavedGameProvider)
            : base(
                _Settings,
                _GameClient,
                _LocalizationManager,
                _Ticker,
                _RemoteSavedGameProvider) { }

        public override ScoresEntity GetScoreFromLeaderboard(ushort _Key, bool _FromCache)
        {
            return base.GetScoreFromLeaderboard(_Key, true);
        }

        public override bool SetScoreToLeaderboard(ushort _Key, long _Value, bool _OnlyToCache)
        {
            return base.SetScoreToLeaderboard(_Key, _Value, true);
        }

        public override bool ShowLeaderboard(ushort _Key)
        {
            Dbg.Log("Available only on device");
            return false;
        }

        public override void SaveGameProgress<T>(T _Data, bool _OnlyToCache)
        {
            SaveGameProgressToCache(_Data);
        }

        public override void DeleteSavedGame(string _FileName)
        {
            Dbg.Log("Available only on device");
        }

        public override Entity<object> GetSavedGameProgress(string _FileName, bool _FromCache)
        {
            return GetSavedGameProgressFromCache(_FileName);
        }

        protected override bool IsAuthenticatedInPlatformGameService()
        {
            return false;
        }

        protected override void AuthenticatePlatformGameService(UnityAction _OnFinish)
        {
            // do nothing
        }
    }
}