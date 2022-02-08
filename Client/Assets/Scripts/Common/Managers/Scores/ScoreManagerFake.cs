using Common;
using Common.Entities;
using Common.Network;
using Common.Ticker;
using UnityEngine.Events;

namespace Managers.Scores
{
    public class ScoreManagerFake : ScoreManagerBase

    {
        public ScoreManagerFake(
            IGameClient _GameClient, 
            ILocalizationManager _LocalizationManager, 
            ICommonTicker _Ticker) 
            : base(_GameClient, _LocalizationManager, _Ticker) { }

        public override ScoresEntity GetScoreFromLeaderboard(ushort _Id, bool _FromCache)
        {
            return base.GetScoreFromLeaderboard(_Id, true);
        }

        public override bool SetScoreToLeaderboard(ushort _Id, long _Value, bool _OnlyToCache)
        {
            return base.SetScoreToLeaderboard(_Id, _Value, true);
        }

        public override bool ShowLeaderboard(ushort _Id)
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