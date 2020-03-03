using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace dao
{
    /// <summary>
    /// http://raidersec.blogspot.com/2013/06/how-browsers-store-your-passwords-and.html#chrome_decryption
    /// </summary>
    class Chrome360se6 : BaseChrome, IReader
    {
        public string BrowserName { get { return "360se6"; } }

        public IEnumerable<PassModel> Passwords()
        {
            String LOCAL_PATH = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return base.Reads(Path.Combine(LOCAL_PATH, @"Google\Chrome\User Data"));
        }

        public IEnumerable<Cookie> Cookies(String host = null)
        {
            String userData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\360se6\User Data";
            return base.ReadsCookie(userData, host);
        }

    }



}
