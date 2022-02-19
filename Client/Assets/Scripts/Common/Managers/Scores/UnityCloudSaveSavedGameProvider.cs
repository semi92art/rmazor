// using System.Collections.Generic;
// using System.Threading.Tasks;
// using Common.Entities;
// using Common.Helpers;
// using Unity.Services.Authentication;
// using Unity.Services.Authentication.Models;
// using Unity.Services.CloudSave;
// using Unity.Services.Core;
//
// namespace Common.Managers.Scores
// {
//     public class UnityCloudSaveSavedGameProvider : InitBase, IRemoteSavedGameProvider
//     {
//         #region nonpublic members
//
//         private Dictionary<string, string> m_FetchedSavedData = new Dictionary<string, string>();
//
//         #endregion
//         
//         #region api
//
//         public override async void Init()
//         {
//             await UnityServices.InitializeAsync();
//             await SignInAnonymously();
//             FetchSavedGames();
//             base.Init();
//         }
//
//         public Entity<object> GetSavedGame(string _FileName)
//         {
//             var entity = new Entity<object>();
//             if (!m_FetchedSavedData.ContainsKey(_FileName))
//             {
//                 entity.Result = EEntityResult.Fail;
//                 Dbg.LogWarning($"Fetched saved games do not contain value with key {_FileName}");
//             }
//             else
//             {
//                 string resultString = m_FetchedSavedData[_FileName];
//                 entity.Value = ScoreManagerUtils.FromString<object>(resultString);
//                 entity.Result = EEntityResult.Success;
//             }
//             return entity;
//         }
//
//         public async void SaveGame<T>(T _Data) where T : FileNameArgs
//         {
//             string key = _Data.FileName;
//             string value = ScoreManagerUtils.ToString(_Data);
//             Dbg.Log($"Start saving data to unity cloud save: {value}");
//             var data = new Dictionary<string, object>{{key, value}};
//             await SaveData.ForceSaveAsync(data);
//             Dbg.Log("Data saved to unity cloud save");
//             FetchSavedGames();
//         }
//
//         public async void DeleteSavedGame(string _FileName)
//         {
//             await SaveData.ForceDeleteAsync("key");
//             FetchSavedGames();
//         }
//
//         public async void FetchSavedGames()
//         {
//             m_FetchedSavedData = await SaveData.LoadAllAsync();
//         }
//
//         #endregion
//
//         private static async Task SignInAnonymously()
//         {
//             
//             AuthenticationService.Instance.SignedIn += () =>
//             {
//                 string playerId = AuthenticationService.Instance.PlayerId;
//                 Dbg.Log("Signed in as: " + playerId);
//                 Dbg.Log("Unity Services Player ID: " + AuthenticationService.Instance.PlayerId);
//             };
//             AuthenticationService.Instance.SignInFailed += Dbg.Log;
//             await AuthenticationService.Instance.SignInAnonymouslyAsync();
//         }
//     }
// }