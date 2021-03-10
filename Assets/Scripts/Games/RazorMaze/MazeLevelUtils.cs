// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using Constants;
// using GameHelpers;
// using UnityEngine;
// using Newtonsoft.Json;
// using UnityEditor;
// using Utils;
//
// namespace Games.Maze.Utils
// {
//     public class MazeLevelsList
//     {
//         public List<MazeInfo> Levels { get; set; }
//     }
//     
//     public static class MazeLevelUtils
//     {
//         public static MazeInfo LoadLevel(MazeInfoLite _InfoLite)
//         {
//             var asset = PrefabUtilsEx.GetObject<TextAsset>("maze_levels", "all_levels");
//             var levelsListRaw = JsonConvert.DeserializeObject<MazeLevelsList>(asset.text);
//             var levelsList = levelsListRaw.Levels;
//             var result = levelsList.FirstOrDefault(_L => (MazeInfoLite)_L == _InfoLite);
//             return result;
//         }
//
//         public static List<MazeInfo> LoadLevels()
//         {
//             var asset = PrefabUtilsEx.GetObject<TextAsset>("maze_levels", "all_levels");
//             if (string.IsNullOrEmpty(asset.text))
//                 return new List<MazeInfo>();
//             var levelsListRaw = JsonConvert.DeserializeObject<MazeLevelsList>(asset.text);
//             return levelsListRaw.Levels;
//         }
//         
// #if UNITY_EDITOR
//         public static void SaveLevel(MazeInfo _Info)
//         {
//             CreateLevelsAssetIfNotExist();
//             var asset = PrefabUtilsEx.GetObject<TextAsset>("maze_levels", "all_levels");
//             var mazeLevelsList = new MazeLevelsList {Levels = new List<MazeInfo>()};
//             if (!string.IsNullOrEmpty(asset.text))
//                 mazeLevelsList = JsonConvert.DeserializeObject<MazeLevelsList>(asset.text);
//             var levelsList = mazeLevelsList.Levels;
//             var infoLite = (MazeInfoLite) _Info;
//             var existingInfo = levelsList.FirstOrDefault(_L => (MazeInfoLite) _L == infoLite);
//             if (existingInfo != null)
//                 levelsList.Remove(existingInfo);
//             levelsList.Add(_Info);
//             string serializedText = JsonConvert.SerializeObject(asset.text);
//             File.WriteAllText(LevelsAssetPath, serializedText);
//             EditorUtility.SetDirty(asset);
//             AssetDatabase.SaveAssets();
//         }
//
//         private static void CreateLevelsAssetIfNotExist()
//         {
//             if (ResLoader.PrefabSetExist(CommonPrefabSetNames.MazeLevels))
//                 return;
//             ResLoader.CreatePrefabSetIfNotExist(CommonPrefabSetNames.MazeLevels);
//             var fs = File.Create(LevelsAssetPath);
//             fs.Close();
//             AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
//             var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(LevelsAssetPath);
//             PrefabUtilsEx.SetPrefab(CommonPrefabSetNames.MazeLevels, "all_levels", asset);
//         }
//
//         private static string LevelsAssetPath => "Assets/Prefabs/Levels/Maze/all_levels.asset";
// #endif
//     }
// }