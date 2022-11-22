using System.Text;

namespace Common.Ticker
{
    public static class TickerUtils
    {
        public static void PauseTickers(bool _Pause, params IUnityTicker[] _Tickers)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Pause tickers = {_Pause}:");
            foreach (var ticker in _Tickers)
            {
                sb.AppendLine(ticker.GetType().Name);
                ticker.Pause = _Pause;
            }
            Dbg.Log(sb.ToString());
        }
    }
}