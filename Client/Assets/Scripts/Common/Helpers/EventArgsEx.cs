using System;

namespace Common.Helpers
{
    public class EventArgsEx : EventArgs
    {
        public string[] Args { get; set; } = new string[0];
    }
}