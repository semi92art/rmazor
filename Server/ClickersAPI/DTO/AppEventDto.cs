using System.Collections.Generic;

namespace ClickersAPI.DTO
{
    public class AppEventDto
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