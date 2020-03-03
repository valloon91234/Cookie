using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Cookie
{
    internal static class IE
    {
        #region Static Methods

        public static string GetCookies(Uri uri)
        {
            return GetCookies(uri.OriginalString);
        }

        public static string GetCookies(string uri)
        {
            return GetCookies(uri, CookieRetrievalFlags.None);
        }

        public static string GetCookies(Uri uri, CookieRetrievalFlags flags)
        {
            return GetCookies(uri.OriginalString, flags);
        }

        public static string GetCookies(string uri, CookieRetrievalFlags flags)
        {
            StringBuilder buffer;
            string result;
            int bufferLength;

            bufferLength = 1024;
            buffer = new StringBuilder(bufferLength);

            if (NativeMethods.InternetGetCookieEx(uri, null, buffer, ref bufferLength, (int)flags, IntPtr.Zero))
            {
                result = buffer.ToString();
            }
            else
            {
                result = null;

                if (Marshal.GetLastWin32Error() == NativeMethods.ERROR_INSUFFICIENT_BUFFER)
                {
                    buffer.Length = bufferLength;

                    if (NativeMethods.InternetGetCookieEx(uri, null, buffer, ref bufferLength, (int)flags, IntPtr.Zero))
                    {
                        result = buffer.ToString();
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
