using System;
using Games;
using System.IO;
using Newtonsoft.Json;
#if UNITY_EDITOR
using System.Collections.Generic;
#endif

namespace GameHelpers
{
    public static class LevelUtils
    {
        public static TLevelProps LoadLevel<TLevelProps>(GameMode _GameMode, int _Index)
        {
            //TODO
            string levelName = LevelName(_Index);
            //sring seriazidedText = 
            //return JsonConvert.DeserializeObject<TLevelProps>(seriazidedText);
            throw new NotImplementedException();
        }

#if UNITY_EDITOR

        public static void SaveLevel(GameMode _GameMode, int _Index, object _LevelProps)
        {
            //TODO
            string relativeLevelAssetPath = LevelPath(_GameMode, _Index);
            string fullLevelAssetPath = Path.Combine(Directory.GetCurrentDirectory(), relativeLevelAssetPath);
            string serializedLevel = JsonConvert.SerializeObject(_LevelProps);
            throw new NotImplementedException();
        }

        public static List<int> GetLevelIndexes(GameMode _GameMode)
        {
            //TODO
            throw new NotImplementedException();
        }

        public static void SaveLevelsToDatabase()
        {
            
        }

#endif
        
        public static void LoadLevelsFromDatabase()
        {
            
        }
        
        private static string LevelName(int _Index) => "level_" + _Index;
        private static string LevelPath(GameMode _GameMode, int _Index)
        {
            return Path.Combine(
                Directory.GetCurrentDirectory(), 
                "Levels", 
                GameModeNames.Names[_GameMode], 
                LevelName(_Index));
        }
    }
}