#if YANDEX_GAMES
using Common.Entities;
using Common.Helpers;
using Common.Managers.PlatformGameServices.GameServiceAuth;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Network;

namespace Common.Managers.PlatformGameServices.Leaderboards
{
    public class LeaderboardProviderYandexGames : LeaderboardProviderBase
    {
        private IYandexGameFacade YandexGame { get; }

        public LeaderboardProviderYandexGames(
            IYandexGameFacade                 _YandexGame,
            GlobalGameSettings                _GameSettings,
            ILocalizationManager              _LocalizationManager,
            IGameClient                       _GameClient,
            IPlatformGameServiceAuthenticator _Authenticator)
            : base(
                _GameSettings,
                _LocalizationManager,
                _GameClient,
                _Authenticator)
        {
            YandexGame = _YandexGame;
        }

        public override bool ShowLeaderboard(ushort _Key)
        {
            return base.ShowLeaderboard(_Key);
        }

        public override ScoresEntity GetScoreFromLeaderboard(ushort _Key, bool _FromCache)
        {
            return base.GetScoreFromLeaderboard(_Key, _FromCache);
        }

        public override bool SetScoreToLeaderboard(ushort _Key, long _Value, bool _OnlyToCache)
        {
            base.SetScoreToLeaderboard(_Key, _Value, _OnlyToCache);
            if (_OnlyToCache)
                return true;
            string leaderboardId = GetScoreId(_Key);
            YandexGame.NewLeaderboardScoresDec(leaderboardId, (int)_Value);
            return true;
        }
    }
}
#endif