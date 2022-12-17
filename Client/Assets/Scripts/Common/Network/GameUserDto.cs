using System;
using System.Collections.Generic;

namespace Common.Network
{
    [Serializable]
    public class GameUserDto
    {
        public string                      Idfa       { get; set; }
        public string                      Action     { get; set; }
        public string                      Country    { get; set; }
        public string                      Language   { get; set; }
        public string                      Platform   { get; set; }
        public string                      AppVersion { get; set; }
        public IDictionary<string, object> EventData  { get; set; }
    }
}