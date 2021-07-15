using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace MazeCreator
{
    public static class CookiesHelper
    {
        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetGetCookieEx(
            string _Url, 
            string _CookieName, 
            StringBuilder _CookieData, 
            ref int _Size,
            Int32  _DwFlags,
            IntPtr  _LpReserved);

        private const Int32 InternetCookieHttponly = 0x2000;

        /// <summary>
        /// Gets the URI cookie container.
        /// </summary>
        /// <param name="_Uri">The URI.</param>
        /// <returns></returns>
        public static CookieContainer GetUriCookieContainer(Uri _Uri)
        {
            CookieContainer cookies = null;
            // Determine the size of the cookie
            int datasize = 8192 * 16;
            StringBuilder cookieData = new StringBuilder(datasize);
            if (!InternetGetCookieEx(_Uri.ToString(), null, cookieData, ref datasize, InternetCookieHttponly, IntPtr.Zero))
            {
                if (datasize < 0)
                    return null;
                // Allocate stringbuilder large enough to hold the cookie
                cookieData = new StringBuilder(datasize);
                if (!InternetGetCookieEx(
                    _Uri.ToString(),
                    null, cookieData, 
                    ref datasize, 
                    InternetCookieHttponly, 
                    IntPtr.Zero))
                    return null;
            }
            if (cookieData.Length > 0)
            {
                cookies = new CookieContainer();
                cookies.SetCookies(_Uri, cookieData.ToString().Replace(';', ','));
            }
            return cookies;
        }
    }
}