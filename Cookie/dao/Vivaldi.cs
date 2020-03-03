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
    class Vivaldi : BaseChrome, IReader
    {
        public string BrowserName { get { return "Vivaldi"; } }

        public IEnumerable<Cookie> Cookies(string host = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PassModel> Passwords()
        {
            String LOCAL_PATH = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return base.Reads(Path.Combine(LOCAL_PATH, @"Vivaldi\User Data"));
        }

    }

}
