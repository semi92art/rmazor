using Common.Helpers;
using Common.Managers.PlatformGameServices.GameServiceAuth;
using Common.Network;

namespace Common.Managers.PlatformGameServices.Leaderboards
{
    public class LeaderboardProviderFake : LeaderboardProviderBase
    {
        public LeaderboardProviderFake(
            GlobalGameSettings                _GameSettings,
            ILocalizationManager              _LocalizationManager,
            IGameClient                       _GameClient,
            IPlatformGameServiceAuthenticator _Authenticator) 
            : base(
                _GameSettings,
                _LocalizationManager,
                _GameClient, 
                _Authenticator) { }
        
        public override bool ShowLeaderboard(ushort _Key)
        {
            Dbg.Log("Available only on device");
            return false;
        }
    }
}