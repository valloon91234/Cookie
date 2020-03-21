using dao;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Cookie
{
    class Program
    {
        private static void PrintConsole(String text = null, ConsoleColor color = ConsoleColor.White)
        {
            if (text == null)
            {
                Console.WriteLine();
                return;
            }
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.White;
        }

        static void Main(string[] args)
        {
            PrintConsole("Loading...");
            //dao.Main.Start();
            GetPasswords(args);
            //GetCookies(args);
        }

        static void GetPasswords(string[] args)
        {
            List<IReader> readers = new List<IReader>
            {
                new Chrome(),
                new Firefox(),
                //new Chrome360se6(),
                //new ChromeSogou(),
                new Chrome360(),
                new ChromeCore(),
                new ChromeUC(),
                new ChromeTW(),
                new ChromeDC()
            };
            String host = null;
            if (args.Length > 0) host = args[0];
            foreach (var reader in readers)
            {
                PrintConsole();
                var data = reader.Passwords(host);
                if (data.Count() == 0)
                {
                    PrintConsole($"Cannot find password store from {reader.BrowserName}.\r\n");
                    continue;
                }
                foreach (var one in data)
                {
                    PrintConsole($"{one.Url}    <{reader.BrowserName} {one.Profile}>", ConsoleColor.Green);
                    PrintConsole($"{one.Username}  :  {one.Password}");
                    PrintConsole();
                }
            }
            PrintConsole();
        }

        static void GetCookies(string[] args)
        {
            List<IReader> readers = new List<IReader>
            {
                new Chrome(),
                new Firefox(),
                new Chrome360se6(),
                new ChromeSogou(),
                new Chrome360(),
                new ChromeCore(),
                new ChromeUC(),
                new ChromeTW(),
                new ChromeDC()
            };
            String host = null;
            if (args.Length > 0) host = args[0];
            foreach (var reader in readers)
            {
                PrintConsole();
                var data = reader.Cookies(host);
                if (data.Count() == 0)
                {
                    PrintConsole($"Cannot find cookie store from {reader.BrowserName}.\r\n");
                    continue;
                }
                foreach (var one in data)
                {
                    PrintConsole($"{one.Url}    <{reader.BrowserName} {one.Profile}>", ConsoleColor.Green);
                    PrintConsole($"{one.Name} = {one.Value}");
                    PrintConsole();
                }
            }
            if (host != null)
            {
                PrintConsole();
                try
                {
                    PrintConsole("Getting cookies from IE...\r\n", ConsoleColor.Green);
                    String result = IE.GetCookies(host);
                    result = HttpUtility.UrlDecode(result.Replace("; ", Environment.NewLine));
                    PrintConsole(result);
                }
                catch
                {
                    PrintConsole($"Cannot find cookie store from IE.");
                }
            }
            PrintConsole();
        }

    }
}
