﻿using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace dao
{
    /// <summary>
    /// http://raidersec.blogspot.com/2013/06/how-browsers-store-your-passwords-and.html#chrome_decryption
    /// </summary>
    class Brave : IReader
    {
        public string BrowserName { get { return "Brave"; } }

        private readonly ChromeModel Model;

        public Brave()
        {
            string LOCAL_PATH = ChromeModel.GetAppDataLocalPath();
            string userDataPath = Path.Combine(LOCAL_PATH, @"BraveSoftware\Brave-Browse\User Data");
            Model = new ChromeModel(userDataPath);
        }

        public IEnumerable<PassModel> Passwords()
        {
            return Model.ReadPassword();
        }

        public IEnumerable<Cookie> Cookies(string host = null)
        {
            throw new NotImplementedException();
        }
    }

}
