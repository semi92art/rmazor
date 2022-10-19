using System;
using System.Collections.Generic;
using Common.Enums;

namespace Common.Managers.Notifications
{
    public class NotificationInfoEx
    {
        public Dictionary<ELanguage, string> Title { get; set; }
        public Dictionary<ELanguage, string> Body  { get; set; }
        public TimeSpan                      Span  { get; set; }
    }
}