﻿
namespace Entities
{
    public class Score
    {
        public int AccountId { get; set; }
        public int GameId { get; set; }
        public int Type { get; set; }
        public int Points { get; set; }
        public System.DateTime CreationTime { get; set; }
    }
}
