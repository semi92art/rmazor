namespace Constants
{
    public static class DataFieldIds
    {
        //account field ids
        public const ushort FirstCurrency = 1;
        public const ushort SecondCurrency = 2;
        public const ushort ShowAds = 4;
        // game field ids
        public const ushort MainScore = 101;
        public static ushort LevelOpened(int _LevelIndex) => System.Convert.ToUInt16(102 + _LevelIndex);
    }
}