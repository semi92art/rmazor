using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Games
{
    public class LevelsCommonInfo
    {
        public List<LevelCommonInfo> Infos { get; set; }

        public void SetInfo(LevelCommonInfo _Info)
        {
            if (_Info == null)
                throw new ArgumentException("Level info is null");
            if (!Infos.Any())
            {
                Infos.Add(_Info);
                return;
            }
            LevelCommonInfo existing = Infos.FirstOrDefault(_I => _I.Index == _Info.Index);
            if (existing != null)
                existing.Available = _Info.Available;
            else 
                Infos.Add(_Info);
        }

        public static LevelsCommonInfo GetFromString(string _S)
        {
            return JsonConvert.DeserializeObject<LevelsCommonInfo>(_S);
        }
    }

    public class LevelCommonInfo
    {
        public int Index { get; set; }
        public bool Available { get; set; }
    }
}