using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WhatsappViewer.DataSources
{
    class DataSourceIOS : IDataSource
    {

        private string fManifestDB;
        private string fChatDB;
        private string fLocalDB;

        private string backupDirectory;

        private SQLiteConnection sqlite_connect;
        private List<IOSChatItem> Chats { get; set; }
        private List<IOSMessageItem> Messages { get; set; }
        private Dictionary<int, List<IOSMessageItem>> Msg { get; set; }
        public string searchText { get;  set; }

        private bool savedDB = false;
        public string connectedDbFile => this.fLocalDB;

        public static DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public DataSourceIOS(string backupDir, bool savedDB = false)
        {
            this.fLocalDB = !savedDB? System.IO.Path.GetTempFileName() : backupDir + Path.DirectorySeparatorChar + "chatStorage.db";
            this.savedDB = savedDB;

            sqlite_connect = new SQLiteConnection("Data Source=" + fLocalDB + ";Version=3;New=true;Compress=true;", true);
            sqlite_connect.Open();

            this.backupDirectory = backupDir;
            
            extractChatStorageDB();

        }

        ~DataSourceIOS()
        {
            Dispose();
        }

        private void extractChatStorageDB()
        {

            if (!this.savedDB)
            {
                var openDB = this.backupDirectory + Path.DirectorySeparatorChar + "Manifest.db";
                fManifestDB = System.IO.Path.GetTempFileName();
                System.IO.File.WriteAllBytes(fManifestDB, System.IO.File.ReadAllBytes(openDB));
                var trans = sqlite_connect.BeginTransaction();
                using (var cmd = sqlite_connect.CreateCommand())
                {

                    try
                    {
                        cmd.CommandText = " ATTACH '" + fManifestDB + "' AS manifestDB;";
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show("Error while attaching chatDB or savedDB:\n" + ex.Message);
                        trans.Rollback();
                    }

                }
                //trans.Commit();

                var fileID = string.Empty;
                using (var sqlite_command = sqlite_connect.CreateCommand())
                {
                    sqlite_command.CommandText = "SELECT fileID FROM manifestDB.Files WHERE domain = 'AppDomainGroup-group.net.whatsapp.WhatsApp.shared' AND relativePath = 'ChatStorage.sqlite'";
                    SQLiteDataReader sqlite_datareader = sqlite_command.ExecuteReader();
                    sqlite_datareader.Read();
                    var values = sqlite_datareader.GetValues();
                    fileID = values["fileID"] + "";

                }

                var dir = fileID.Substring(0, 2);
                var chatStorage = this.backupDirectory + "\\" + dir + "\\" + fileID;
                fChatDB = System.IO.Path.GetTempFileName();
                System.IO.File.WriteAllBytes(fChatDB, System.IO.File.ReadAllBytes(chatStorage));
                //trans = sqlite_connect.BeginTransaction();
                using (var cmd = sqlite_connect.CreateCommand())
                {

                    try
                    {
                        cmd.CommandText = " ATTACH '" + fChatDB + "' AS chatDB;";
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show("Error while attaching chatDB or savedDB:\n" + ex.Message);
                        trans.Rollback();
                    }

                }

                using (var cmd = sqlite_connect.CreateCommand())
                {
                    cmd.CommandText = "CREATE TABLE IF NOT EXISTS ZSOURCEINFO (Z_PK INTEGER PRIMARY KEY, ZOS VARCHAR, ZBACKUPPATH VARCHAR)";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "INSERT INTO ZSOURCEINFO ('ZOS', 'ZBACKUPPATH' ) VALUES ('ios', '" + this.backupDirectory + "')";
                    cmd.ExecuteNonQuery();
                }
                trans.Commit();
            }
            else
            {
                using (var cmd = sqlite_connect.CreateCommand())
                {
                    cmd.CommandText = "SELECT ZBACKUPPATH FROM ZSOURCEINFO WHERE ZOS ='ios'";
                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        var values = reader.GetValues();
                        this.backupDirectory = values["ZBACKUPPATH"] + "";
                    }
                }
            }

        }
        public IEnumerable<IMessageItem> getMessages(string ChatName)
        {
            
            using (var sqlite_command = sqlite_connect.CreateCommand())
            {
                if (Msg == null)
                {
                    if (!this.savedDB)
                    {
                        var trans = sqlite_connect.BeginTransaction();
                        try
                        {
                            sqlite_command.CommandText = "DROP TABLE IF EXISTS ZTMPMESSAGEVIEW";
                            sqlite_command.ExecuteNonQuery();
                            sqlite_command.CommandText = "CREATE TABLE ZTMPMESSAGEVIEW AS SELECT M.Z_PK," +
                                                        "       M.ZMESSAGESTATUS," +
                                                        "       M.ZMESSAGETYPE," +
                                                        "       M.ZMESSAGEDATE," +
                                                        "       M.ZPUSHNAME," +
                                                        "       M.ZGROUPMEMBER," +
                                                        "       '' AS ZTITLE," +
                                                        "       '' AS ZMEDIALOCALPATH," +
                                                        "       M.ZTEXT," +
                                                        "       M.ZTOJID," +
                                                        "       M.ZFROMJID," +
                                                        "       O.ZCONTACTNAME," +
                                                        "       O.ZMEMBERJID," +
                                                        "       M.ZCHATSESSION," +
                                                        "       M.ZMEDIAITEM," +
                                                        "       '' AS fileID" +
                                                        " FROM " +
                                                        "  (SELECT Z_PK," +
                                                        "          ZMESSAGESTATUS," +
                                                        "          ZMESSAGETYPE," +
                                                        "          ZMESSAGEDATE," +
                                                        "          ZPUSHNAME," +
                                                        "          ZTEXT," +
                                                        "          ZTOJID," +
                                                        "          ZFROMJID," +
                                                        "          ZCHATSESSION," +
                                                        "          ZGROUPMEMBER," +
                                                        "          '' AS ZMEDIAITEM" +
                                                        "   FROM chatDB.ZWAMESSAGE" +
                                                        "   WHERE ZMESSAGETYPE = 0 ) AS M" +
                                                        " LEFT JOIN chatDB.ZWAGROUPMEMBER AS O ON M.ZGROUPMEMBER = O.Z_PK" +
                                                        " UNION" +
                                                        " SELECT X.*," +
                                                        "       Y.fileID" +
                                                        " FROM" +
                                                        "  (SELECT M.Z_PK," +
                                                        "          M.ZMESSAGESTATUS," +
                                                        "          M.ZMESSAGETYPE," +
                                                        "          M.ZMESSAGEDATE," +
                                                        "          M.ZPUSHNAME," +
                                                        "          M.ZGROUPMEMBER," +
                                                        "          N.ZTITLE," +
                                                        "          'Message/' || N.ZMEDIALOCALPATH AS ZMEDIALOCALPATH," +
                                                        "          M.ZTEXT," +
                                                        "          M.ZTOJID," +
                                                        "          M.ZFROMJID," +
                                                        "          O.ZCONTACTNAME," +
                                                        "          O.ZMEMBERJID," +
                                                        "          M.ZCHATSESSION," +
                                                        "          M.ZMEDIAITEM" +
                                                        "   FROM" +
                                                        "     (SELECT Z_PK," +
                                                        "             ZMESSAGESTATUS," +
                                                        "             ZMESSAGETYPE," +
                                                        "             ZMESSAGEDATE," +
                                                        "             ZPUSHNAME," +
                                                        "             ZTEXT," +
                                                        "             ZTOJID," +
                                                        "             ZFROMJID," +
                                                        "             ZCHATSESSION," +
                                                        "             ZGROUPMEMBER," +
                                                        "             ZMEDIAITEM" +
                                                        "      FROM chatDB.ZWAMESSAGE" +
                                                        "      WHERE ZMEDIAITEM IS NOT NULL ) AS M" +
                                                        "   LEFT JOIN chatDB.ZWAGROUPMEMBER AS O ON M.ZGROUPMEMBER = O.Z_PK" +
                                                        "   LEFT JOIN chatDB.ZWAMEDIAITEM AS N ON M.ZMEDIAITEM = N.Z_PK" +
                                                        "   WHERE ZMESSAGETYPE IN (1, 2, 4, 8, 11,15)" +
                                                        "   ORDER BY ZMEDIALOCALPATH) AS X" +
                                                        " LEFT JOIN" +
                                                        "  (SELECT fileID," +
                                                        "          relativePath" +
                                                        "   FROM manifestDB.Files" +
                                                        "   WHERE flags = 1" +
                                                        "     AND DOMAIN LIKE 'AppDomainGroup-group.net.whatsapp%'" +
                                                        "     AND relativePath LIKE 'Message/%'" +
                                                        "     AND relativePath NOT LIKE '%thumb'" +
                                                        "   ORDER BY relativePath) AS Y ON Y.relativePath = X.ZMEDIALOCALPATH";

                            sqlite_command.ExecuteNonQuery();
                            trans.Commit();

                        }
                        catch (Exception)
                        {
                            trans.Rollback();
                        }

                    }

                    sqlite_command.CommandText = "SELECT * FROM ZTMPMESSAGEVIEW";
                    
                    SQLiteDataReader sqlite_datareader = sqlite_command.ExecuteReader();
                    Msg = new Dictionary<int, List<IOSMessageItem>>();
                    while (sqlite_datareader.Read())
                    {
                        var values = sqlite_datareader.GetValues();

                        var item = new IOSMessageItem();
                        item.Z_PK = int.Parse("0" + values["Z_PK"] + "");
                        item.ZMESSAGESTATUS = int.Parse("0" + values["ZMESSAGESTATUS"] + "");
                        item.ZMESSAGETYPE = int.Parse("0" + values["ZMESSAGETYPE"] + "");
                        item.ZMESSAGEDATE = TimeStampToDateTime(double.Parse("0" + values["ZMESSAGEDATE"], CultureInfo.InvariantCulture));
                        item.ZFROMJID = values["ZFROMJID"] + "";
                        item.ZPUSHNAME = values["ZPUSHNAME"] + "";
                        item.ZTEXT = values["ZTEXT"] + "";
                        item.ZTOJID = values["ZTOJID"] + "";
                        item.ZCONTACTNAME = values["ZCONTACTNAME"] + "";
                        item.ZMEMBERJID = values["ZMEMBERJID"] + "";
                        item.CHATSESSIONID = int.Parse("0" + values["ZCHATSESSION"] + "");
                        item.ZTITLE = values["ZTITLE"] +"";
                        item.ZMEDIALOCALPATH = values["ZMEDIALOCALPATH"] + "";

                        //var relativePath = values["ZMEDIALOCALPATH"] + "";
                        var fileID = values["fileID"] + "";
                        if (!string.IsNullOrWhiteSpace(fileID))
                        {
                            var dir = fileID.Substring(0, 2);
                            item.localFilePath = this.backupDirectory + "\\" + dir + "\\" + fileID;
                        }

                        List<IOSMessageItem> message;
                        if (Msg.TryGetValue(item.CHATSESSIONID,out message))
                        {
                            message.Add(item);
                        }
                        else
                        {
                            message = new List<IOSMessageItem>();
                            message.Add(item);
                            Msg.Add(item.CHATSESSIONID, message);

                        }
                       
                    }
                }
                List<IOSMessageItem> message1;
                if (Msg.TryGetValue(int.Parse("0" + ChatName), out message1))
                {
                    if(!string.IsNullOrWhiteSpace(searchText))
                        return message1.Where(o => o.message.ToLower().Contains(searchText.ToLower())).OrderBy(o => o.ZMESSAGEDATE).ToList();
                    else
                        return message1.OrderBy(o => o.ZMESSAGEDATE).ToList();
                }
                else
                    return null;

            }
        }

        public static DateTime? TimeStampToDateTime(double ts)
        {
            if (ts > 0)
            {
                return unixEpoch + TimeSpan.FromSeconds(978307200 + ts);
            }
            return null;
        }

        public IEnumerable<ChatItem> getChats()
        {

            var list = new List<ChatItem>();
            if (!this.savedDB)
            {
                var trans = sqlite_connect.BeginTransaction();
                try
                {
                    using (var sqlite_command = sqlite_connect.CreateCommand())
                    {
                        sqlite_command.CommandText = "DROP TABLE IF EXISTS ZTMPSESSIONVIEW";
                        sqlite_command.ExecuteNonQuery();
                        sqlite_command.CommandText = "CREATE TABLE ZTMPSESSIONVIEW AS SELECT Z_PK,ZCONTACTJID,ZPARTNERNAME,ZLASTMESSAGEDATE FROM chatDB.ZWACHATSESSION WHERE ZCONTACTJID NOT LIKE '%@status' AND ZHIDDEN = 0;";
                        sqlite_command.ExecuteNonQuery();

                    }
                    trans.Commit();
                }
                catch (Exception)
                {
                    trans.Rollback();
                }
            }

            using (var sqlite_command = sqlite_connect.CreateCommand())
            {

                sqlite_command.CommandText = "SELECT * FROM ZTMPSESSIONVIEW";

                SQLiteDataReader sqlite_datareader = sqlite_command.ExecuteReader();
                Chats = new List<IOSChatItem>();
                while (sqlite_datareader.Read())
                {
                    var item = new IOSChatItem();

                    var values = sqlite_datareader.GetValues();

                    item.Z_PK = int.Parse("0" + values["Z_PK"].ToString());
                    item.ZLASTMESSAGEDATE = TimeStampToDateTime(double.Parse("0" + values["ZLASTMESSAGEDATE"], CultureInfo.InvariantCulture));
                    item.ZCONTACTJID = values["ZCONTACTJID"].ToString();

                    Chats.Add(item);

                    list.Add(new ChatItem()
                    {
                        id = int.Parse("0" + values["Z_PK"].ToString()),
                        name = values["ZCONTACTJID"].ToString(),
                        descr = values["ZPARTNERNAME"].ToString(),
                        lastmessage = TimeStampToDateTime(double.Parse("0" + values["ZLASTMESSAGEDATE"], CultureInfo.InvariantCulture))
                    });

                }

            }
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                List<int> ids = new List<int>();
                foreach (KeyValuePair<int, List<IOSMessageItem>> entry in Msg)
                {
                    if (entry.Value.Where(o => o.message.ToLower().Contains(searchText.ToLower())).ToList().Count > 0)
                    {
                        ids.Add(entry.Key);
                    }
                }
                return list.Where(o => ids.Contains(o.id)).OrderByDescending(o => o.lastmessage).ToList();
            }
            return list.OrderByDescending(o => o.lastmessage);
        }

        public void Dispose()
        {
            try
            {
                sqlite_connect.Close();
                sqlite_connect.Dispose();
                System.IO.File.Delete(fManifestDB);
                System.IO.File.Delete(fChatDB);
                System.IO.File.Delete(fLocalDB);
            }
            catch (Exception) { }
        }
    }
}
