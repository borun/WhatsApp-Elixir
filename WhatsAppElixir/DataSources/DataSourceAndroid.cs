using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WhatsappViewer.DataSources
{
    class DataSourceAndroid : IDataSource
    {

        private string fileName;
        private SQLiteConnection sqlite_connection;

        private static DateTime unixDate = new DateTime(1970, 1, 1);

        private List<AndroidChatItem> Chats { get; set; }
        private List<AndroidMessageItem> Messages { get; set; }
        public string searchText { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string connectedDbFile => throw new NotImplementedException();

        public DataSourceAndroid(string DatabaseFilePath)
        {
            var bb = System.IO.File.ReadAllBytes(DatabaseFilePath);
            fileName = System.IO.Path.GetTempFileName();

            if (DatabaseFilePath.ToLower().EndsWith(".db"))
            {
                System.IO.File.WriteAllBytes(fileName, bb);
            }
            else if (DatabaseFilePath.ToLower().EndsWith(".crypt"))
            {
                System.IO.File.WriteAllBytes(fileName, Decrypt(bb));
            }
            else if (DatabaseFilePath.ToLower().EndsWith(".crypt7"))
            {
                System.IO.File.WriteAllBytes(fileName, Decrypt7(bb));
            }
            else if (DatabaseFilePath.ToLower().EndsWith(".crypt8"))
            {
                System.IO.File.WriteAllBytes(fileName, Decrypt8(bb));
            }
            else
            {
                throw new Exception("File not supported");
            }
            sqlite_connection = new SQLiteConnection("Data Source=" + fileName + ";Version=3;New=True;Compress=True;");
            sqlite_connection.Open();
        }

        ~DataSourceAndroid()
        {

            Dispose();

        }

        public IEnumerable<IMessageItem> getMessages(string ChatName)
        {
            using (var sqlite_command = sqlite_connection.CreateCommand())
            {

                sqlite_command.CommandText = "SELECT * FROM messages WHERE key_remote_jid = '" + ChatName + "'";
                SQLiteDataReader sqlite_datareader = sqlite_command.ExecuteReader();
                Messages = new List<AndroidMessageItem>();
                while (sqlite_datareader.Read())
                {
                    var item = new AndroidMessageItem();
                    item._id = int.Parse(sqlite_datareader["_id"] + "");
                    item.key_remote_jid = sqlite_datareader["key_remote_jid"] + "";
                    //item.key_from_me = int.Parse(sqlite_datareader["key_from_me"] + "");
                    //item.key_id = sqlite_datareader["key_id"] + "";
                    item.status = int.Parse(sqlite_datareader["status"] + "");
                    //item.needs_push = int.Parse(sqlite_datareader["needs_push"] + "");
                    item.data = sqlite_datareader["data"] + "";
                    item.timestamp = long.Parse(sqlite_datareader["timestamp"] + "");
                    item.media_url = sqlite_datareader["media_url"] + "";
                    //item.media_mime_type = sqlite_datareader["media_mime_type"] + "";
                    //item.media_wa_type = sqlite_datareader["media_wa_type"] + "";
                    //item.media_size = int.Parse(sqlite_datareader["media_size"] + "");
                    //item.media_name = sqlite_datareader["media_name"] + "";
                    //item.media_hash = sqlite_datareader["media_hash"] + "";
                    //item.latitude = double.Parse(sqlite_datareader["latitude"] + "");
                    //item.longitude = double.Parse(sqlite_datareader["longitude"] + "");
                    //item.thumb_image = sqlite_datareader["thumb_image"] + "";
                    item.remote_resource = sqlite_datareader["remote_resource"] + "";
                    //item.received_timestamp = long.Parse(sqlite_datareader["received_timestamp"] + "");
                    //item.send_timestamp = long.Parse(sqlite_datareader["send_timestamp"] + "");
                    //item.receipt_server_timestamp = long.Parse(sqlite_datareader["receipt_server_timestamp"] + "");
                    //item.receipt_device_timestamp = long.Parse(sqlite_datareader["receipt_device_timestamp"] + "");
                    //item.raw_data = sqlite_datareader["raw_data"] as byte[];
                    //item.recipient_count = int.Parse("0" + sqlite_datareader["recipient_count"]);
                    //item.media_duration = int.Parse("0" + sqlite_datareader["media_duration"] + "");
                    //item.origin = int.Parse("0" + sqlite_datareader["origin"] + "");
                    Messages.Add(item);
                }

                return Messages.OrderBy(o => o.timestamp).ToList();

            }
        }

        public static DateTime? TimeStampToDateTime(long ts)
        {
            if (ts > 0)
            {
                var t = ts.ToString();
                var xxx = t.Substring(0, t.Length - 3);
                ts = long.Parse(xxx);
                return unixDate.AddSeconds(ts);
            }
            return null;

        }

        public IEnumerable<ChatItem> getChats()
        {
            var list = new List<ChatItem>();

            using (var sqlite_command = sqlite_connection.CreateCommand())
            {
                sqlite_command.CommandText = "SELECT * FROM chat_list;";
                SQLiteDataReader sqlite_datareader = sqlite_command.ExecuteReader();
                Chats = new List<AndroidChatItem>();
                while (sqlite_datareader.Read())
                {
                    var fc = sqlite_datareader.FieldCount;

                    list.Add(new ChatItem()
                                        {
                                            id = int.Parse(sqlite_datareader["_id"].ToString()),
                                            name = sqlite_datareader["key_remote_jid"].ToString(),
                                            descr = fc > 3 ? sqlite_datareader["subject"] + "" : ""
                                        });
                }
            }
            return list;

        }

        #region encrypprion

        public static byte[] Decrypt8(byte[] toEncryptArray)
        {
            return DecompressGzip(Decrypt7(toEncryptArray));
        }

        private static byte[] DecompressGzip(byte[] gzip)
        {
            byte[] baRetVal = null;
            using (MemoryStream ByteStream = new MemoryStream(gzip))
            {
                using (GZipStream stream = new GZipStream(ByteStream, CompressionMode.Decompress))
                {
                    const int size = 4096;
                    byte[] buffer = new byte[size];
                    using (MemoryStream memory = new MemoryStream())
                    {
                        int count = 0;
                        count = stream.Read(buffer, 0, size);
                        while (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                            memory.Flush();
                            count = stream.Read(buffer, 0, size);
                        }

                        baRetVal = memory.ToArray();
                        memory.Close();
                    }
                    stream.Close();
                }
                ByteStream.Close();
            }
            return baRetVal;
        }


        public static byte[] Decrypt7(byte[] toEncryptArray)
        {

            MessageBox.Show("We need the 'key' file, locatad on the device folder '/data/data/com.whatsapp/files/key' ");

            OpenFileDialog openFileDialog1 = new OpenFileDialog() { Filter = "All Files (*.*)|*.*" };

            if (openFileDialog1.ShowDialog() != true)
                return null;

            var db7key = System.IO.File.ReadAllBytes(openFileDialog1.FileName);

            var header = toEncryptArray.Take(67).ToArray();

            toEncryptArray = toEncryptArray.Skip(67).ToArray();

            var db7key_iv = db7key.Skip(110).Take(16).ToArray();
            var db7key_aes = db7key.Skip(126).Take(32).ToArray();

            var db7key_iv2 = header.Skip(51).Take(16).ToArray();

            // iv deve essere presente nel header del file dal 0×34 -> 0×42.
            var b = Enumerable.SequenceEqual(db7key_iv, db7key_iv2);

            if (b == false)
            {
                throw new Exception("File 'key' not compatible.");
            }

            using (var rijndaelManaged = new RijndaelManaged())
            {
                rijndaelManaged.Key = db7key_aes;
                rijndaelManaged.IV = db7key_iv;
                rijndaelManaged.Mode = CipherMode.CBC;


                using (var memoryStream = new MemoryStream(toEncryptArray))
                using (var cryptoStream = new CryptoStream(memoryStream, rijndaelManaged.CreateDecryptor(db7key_aes, db7key_iv), CryptoStreamMode.Read))
                {
                    var dummy = new StreamReader(cryptoStream, Encoding.Default).ReadToEnd();
                    return System.Text.Encoding.Default.GetBytes(dummy);
                }
            }

        }


        public static byte[] Decrypt(byte[] toEncryptArray)
        {
            string key = "346a23652a46392b4d73257c67317e352e3372482177652c";
            byte[] keyArray = ParseHex(key);
            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;
            ICryptoTransform cTransform = rDel.CreateDecryptor();
            return cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
        }

        public static byte[] ParseHex(string hex)
        {
            int offset = hex.StartsWith("0x") ? 2 : 0;
            if ((hex.Length % 2) != 0)
            {
                throw new ArgumentException("Invalid length: " + hex.Length);
            }
            byte[] ret = new byte[(hex.Length - offset) / 2];

            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = (byte)((ParseNybble(hex[offset]) << 4)
                                 | ParseNybble(hex[offset + 1]));
                offset += 2;
            }
            return ret;
        }

        static int ParseNybble(char c)
        {
            if (c >= '0' && c <= '9')
            {
                return c - '0';
            }
            if (c >= 'A' && c <= 'F')
            {
                return c - 'A' + 10;
            }
            if (c >= 'a' && c <= 'f')
            {
                return c - 'a' + 10;
            }
            throw new ArgumentException("Invalid hex digit: " + c);
        }

        public void Dispose()
        {
            //if (sqlite_connection != null && sqlite_connection.State == System.Data.ConnectionState.Open)
            //    sqlite_connection.Close();
            try
            {
                sqlite_connection.Close();
                sqlite_connection.Dispose();
                System.IO.File.Delete(fileName);
            }
            catch (Exception) { }

            //if (!string.IsNullOrWhiteSpace(fileName) && System.IO.File.Exists(fileName))
            //    System.IO.File.Delete(fileName);
        }

        #endregion

    }
}
