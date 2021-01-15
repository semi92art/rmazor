namespace Constants
{
    public static class DataFieldIds
    {
        //account field ids
        public const ushort FirstCurrency = 1;
        public const ushort SecondCurrency = 2;
        public const ushort Lifes = 3;
        public const ushort ShowAds = 4;
        // game field ids
        public const ushort MainScore = 101;

        public static ushort[] AccountIds => new[]
        {
            FirstCurrency,
            SecondCurrency,
            Lifes, 
            ShowAds
        };

        public static ushort[] GameIds => new[]
        {
            MainScore
        };
    }
}