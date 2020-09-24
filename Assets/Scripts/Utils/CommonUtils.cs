using System.Net;

namespace Clickers
{
    public static class CommonUtils
    {
    
        public static bool CheckForMainServerInternetConnection()
        {
            return CheckForConnection("77.37.152.15");
        }
    
        public static bool CheckForInternetConnection()
        {
            return CheckForConnection("http://google.com/generate_204");
        }

        private static bool CheckForConnection(string url)
        {
            int tryCount = 0;
            while (tryCount < 3)
            {
                try
                {
                    using (var client = new WebClient())
                    using (client.OpenRead(url)) 
                        return true; 
                }
                catch
                {
                    tryCount++;
                }
            }
            return false;
        }
    }
}
