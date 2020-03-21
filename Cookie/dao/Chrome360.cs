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
    class Chrome360 : IReader
    {
        public string BrowserName { get { return "360Chrome"; } }

        private readonly BaseChrome Model;

        public Chrome360()
        {
            string LOCAL_PATH = BaseChrome.GetAppDataLocalPath();
            string userDataPath = Path.Combine(LOCAL_PATH, @"360Chrome\Chrome\User Data");
            Model = new BaseChrome(userDataPath);
        }

        public IEnumerable<PassModel> Passwords(string host = null)
        {
            return Model.ReadPassword(host);
        }

        public IEnumerable<Cookie> Cookies(string host = null)
        {
            return Model.ReadCookie(host);
        }
    }



}
