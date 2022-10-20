namespace Common.Ticker
{
    public static class TickerUtils
    {
        public static void PauseTickers(bool _Pause, params IUnityTicker[] _Tickers)
        {
            foreach (var ticker in _Tickers)
            {
                ticker.Pause = _Pause;
            }
        }
    }
}