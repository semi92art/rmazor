using System.Globalization;
using System.Net;

namespace Common.Utils
{
    public static class NetworkUtils
    {
        public static bool IsPacketSuccess(long _ResponseCode)
        {
            return MathUtils.IsInRange(_ResponseCode, 200, 299);
        }

        public static bool IsInternetConnectionAvailable(int _TimeoutMs = 10000, string _Url = null)
        {
            try
            {
                _Url ??= CultureInfo.InstalledUICulture switch
                {
                    { Name: var n } when n.StartsWith("fa") => // Iran
                        "http://www.aparat.com",
                    { Name: var n } when n.StartsWith("zh") => // China
                        "http://www.baidu.com",
                    _ =>
                        "http://www.gstatic.com/generate_204"
                };
                var request = (HttpWebRequest)WebRequest.Create(_Url);
                request.KeepAlive = false;
                request.Timeout = _TimeoutMs;
                using ((HttpWebResponse)request.GetResponse()) 
                    return true;
            }
            catch
            {
                return false;
            }
        }
    }
}