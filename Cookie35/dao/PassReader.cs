using System;
using System.Collections.Generic;

namespace dao
{
    interface IReader
    {
        IEnumerable<PassModel> Passwords();
        IEnumerable<Cookie> Cookies(String host = null);
        string BrowserName { get; }
    }
}