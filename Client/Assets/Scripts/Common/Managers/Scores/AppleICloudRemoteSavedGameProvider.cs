using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Common.Entities;
using Common.Helpers;
using SA.iOS.GameKit;

namespace Common.Managers.Scores
{
    public class AppleICloudRemoteSavedGameProvider : InitBase, IRemoteSavedGameProvider
    {
        #region nonpublic members

        private EEntityResult         m_FetchSavedGamesResult = EEntityResult.Pending;
        private List<ISN_GKSavedGame> m_FetchedSavedGames     = new List<ISN_GKSavedGame>();

        #endregion

        public override void Init()
        {
            ISN_GKLocalPlayerListener.DidModifySavedGame.AddListener(DidModifySavedGame);
            ISN_GKLocalPlayerListener.HasConflictingSavedGames.AddListener(HasConflictingSavedGames);
            base.Init();
        }

        public Entity<object> GetSavedGame(string    _FileName)
        {
            var entity = new Entity<object>();
            var savedGame = m_FetchedSavedGames.FirstOrDefault(_G => _G.Name == _FileName);
            if (savedGame == null)
            {
                Dbg.LogWarning($"Saved game with {_FileName} does not exist in fetched saved games.");
                entity.Result = EEntityResult.Fail;
                return entity;
            }
            savedGame.Load(_Result =>
            {
                if (_Result.IsSucceeded)
                {
                    entity.Value = ScoreManagerUtils.FromByteArray<object>(_Result.BytesArrayData);
                    entity.Result = EEntityResult.Success;
                }
                else
                {
                    Dbg.LogWarning($"Failed to load data from fetched saved game with file name {_FileName}");
                    entity.Result = EEntityResult.Fail;
                }
            });
            return entity;
        }

        public void SaveGame<T>(T _Data) where T : FileNameArgs
        {
            var data = ScoreManagerUtils.ToByteArray(_Data);
            if (data == null)
            {
                Dbg.LogError("Saved data cannot be null");
                return;
            }
            ISN_GKLocalPlayer.SavedGame(
                _Data.FileName,
                data,
                _Result =>
                {
                    if (_Result.IsSucceeded) 
                    {
                        if (!m_FetchedSavedGames.Any())
                            FetchSavedGames();
                        Dbg.Log($"Saved game name: {_Result.SavedGame.Name}");
                        Dbg.Log($"Saved game device name: {_Result.SavedGame.DeviceName}");
                        Dbg.Log($"Saved game modification date: {_Result.SavedGame.ModificationDate}");
                    } 
                    else
                    {
                        Dbg.LogError("SavedGame is failed! With:" +
                                     $" {_Result.Error.Code} and description: {_Result.Error.Message}");
                    }
                });
        }

        public void DeleteSavedGame(string _FileName)
        {
            if (m_FetchSavedGamesResult != EEntityResult.Success
                || m_FetchedSavedGames.All(_G => _G == null || _G.Name != _FileName))
            {
                Dbg.LogWarning("Failed to delete saved game");
                return;
            }
            var savedGame = m_FetchedSavedGames.First(_G => _G.Name == _FileName);
            ISN_GKLocalPlayer.DeleteSavedGame(savedGame, _Result =>
            {
                if (_Result.IsSucceeded)
                    Dbg.Log($"Saved game with file name {_FileName} deleted.");
                else
                    Dbg.LogWarning($"Failed to delete saved game with file name {_FileName}: {_Result.Error}");
            });
        }
        
        public void FetchSavedGames()
        {
            ISN_GKLocalPlayer.FetchSavedGames(_Result => 
            {
                if (_Result.IsSucceeded) 
                {
                    m_FetchedSavedGames = _Result.SavedGames;
                    m_FetchSavedGamesResult = EEntityResult.Success;
                    Dbg.Log($"Loaded {_Result.SavedGames.Count} saved games");
                    var sb = new StringBuilder();
                    foreach (var game in _Result.SavedGames)
                    {
                        sb.Clear();
                        sb.AppendLine($"saved game name: {game.Name}");
                        sb.AppendLine($"saved game DeviceName: {game.DeviceName}");
                        sb.AppendLine($"saved game ModificationDate: {game.ModificationDate}");
                        Dbg.Log(sb.ToString());
                    }
                }
                else
                {
                    m_FetchSavedGamesResult = EEntityResult.Fail;
                    Dbg.LogError("Fetching saved games is failed! With:" +
                                 $" {_Result.Error.Code} and description: {_Result.Error.Message}");
                }
            });
        }
        
        private static void DidModifySavedGame(ISN_GKSavedGameSaveResult _Result) {
            Dbg.Log($"DidModifySavedGame! Device name = {_Result.SavedGame.DeviceName} " +
                    $"| game name = {_Result.SavedGame.Name} | modification Date = " +
                    $"{_Result.SavedGame.ModificationDate.ToString(CultureInfo.InvariantCulture)}");
        }
        
        private static void HasConflictingSavedGames(ISN_GKSavedGameFetchResult _Result)
        {
            foreach (var game in _Result.SavedGames)
            {
                Dbg.Log($"HasConflictingSavedGames! Device name = {game.DeviceName} " +
                        $"| game name = {game.Name} | modification Date = " +
                        $"{game.ModificationDate.ToString(CultureInfo.InvariantCulture)}");
            }
            // var conflictedGameIds = _Result.SavedGames.Select(game => game.Id).ToList();
            // ISN_GKLocalPlayer.ResolveConflictingSavedGames(conflictedGameIds, null, null);
        }
    }
}