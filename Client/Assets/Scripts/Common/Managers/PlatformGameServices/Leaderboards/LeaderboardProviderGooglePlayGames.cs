using Common.Helpers;
using Common.Managers.PlatformGameServices.GameServiceAuth;
using Common.Network;

namespace Common.Managers.PlatformGameServices.Leaderboards
{
    public class LeaderboardProviderGooglePlayGames : LeaderboardProviderBase
    {
        #region nonpublic members

        #endregion

        #region inject
        
        public LeaderboardProviderGooglePlayGames(
            CommonGameSettings                _Settings,
            ILocalizationManager              _LocalizationManager,
            IGameClient                       _GameClient,
            IPlatformGameServiceAuthenticator _Authenticator) 
            : base(
                _Settings,
                _LocalizationManager, 
                _GameClient, 
                _Authenticator) { }

        #endregion

        #region api

        #endregion

        #region nonpublic methods

        #endregion
    }
}