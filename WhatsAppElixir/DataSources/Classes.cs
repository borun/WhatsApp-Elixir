using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WhatsappViewer.DataSources
{

    public class ChatItem
    {
        public int id { get; set; }
        public string name { get; set; }
        public string descr { get; set; }
        public DateTime? lastmessage { get; set; }

    }
    interface IMessageItem
    {
        int id { get; set; }
        string sender { get; set; }
        DateTime? datetime { get; set; }
        string message { get; set; }
        string media { get; set; }
        int? status { get; set; }
        string StatusToString { get; set; }
        string imageFilePath { get; set; }
        string imageFileName { get; set; }
        Visibility hasImage { get; set; }
    }

    public class FileInfos
    {
        public string sha1 { get; set; }
        public string md5 { get; set; }
        public int size { get; set; }
    }

    #region IOS

    class IOSChatItem
    {
        public int Z_PK { get; set; }
        public int Z_ENT { get; set; }
        public int Z_OPT { get; set; }
        public int ZCONTACTABID { get; set; }
        public int ZFLAGS { get; set; }
        public int ZMESSAGECOUNTER { get; set; }
        public int ZUNREADCOUNT { get; set; }
        public int ZGROUPINFO { get; set; }
        public int ZLASTMESSAGE { get; set; }
        public int ZPROPERTIES { get; set; }
        public DateTime? ZLASTMESSAGEDATE { get; set; }
        public string ZCONTACTJID { get; set; }
        public string ZLASTMESSAGETEXT { get; set; }
        public string ZPARTNERNAME { get; set; }
        public string ZSAVEDINPUT { get; set; }

    }

    class IOSMessageItem : IMessageItem
    {
        public int Z_PK { get; set; }

        public int ZISFROMME { get; set; }
        public int ZMESSAGESTATUS { get; set; }
        public int ZMESSAGETYPE { get; set; }
        public DateTime? ZMESSAGEDATE { get; set; }
        public string ZFROMJID { get; set; }
        public string ZPUSHNAME { get; set; }
        public string ZTEXT { get; set; }
        public string ZTOJID { get; set; }
        public string ZCONTACTNAME { get; set; }
        public string ZMEMBERJID { get; set; }
        public int CHATSESSIONID { get; set; }
        public string ZTITLE { get; set; }
        public string ZMEDIALOCALPATH { get; set; }
        public string localFilePath { get; set; }

        public int id
        {
            get { return Z_PK; }
            set { }
        }

        public string sender
        {
            get {
                string fromNumber = ZFROMJID;
                if (!string.IsNullOrWhiteSpace(ZFROMJID))
                {
                    string[] x = ZFROMJID.Split(new char[] { '-', '@' });
                    if (x.Length > 1)
                        fromNumber = x[0];
                    if (string.IsNullOrWhiteSpace(ZPUSHNAME))
                        return fromNumber;
                    else
                         return string.Format("{0} ({1})", fromNumber, ZPUSHNAME);

                }
                return "";
            }
            set { }
        }

        public DateTime? datetime
        {
            get { return ZMESSAGEDATE; }
            set { }
        }

        public string message
        {
            get { return (string.IsNullOrWhiteSpace(localFilePath)) ? ZTEXT : ZTITLE; }
            set { }
        }

        public string media
        {
            get { return null; }
            set { }
        }

        public int? status
        {
            get { return ZMESSAGESTATUS; }
            set { }
        }

        public string StatusToString
        {
            get
            {
                if (ZMESSAGESTATUS == 2)
                {
                    if (!string.IsNullOrWhiteSpace(ZMEMBERJID))
                    {
                        return Utils.getNumberOnly(ZMEMBERJID) + " " + ZCONTACTNAME;
                    }
                    if (!string.IsNullOrWhiteSpace(ZFROMJID))
                    {
                        return Utils.getNumberOnly(ZFROMJID) + " " + ZPUSHNAME;
                    }
                }
                return "";
            }
            set { }
        }

        public string imageFilePath {
            get { return !string.IsNullOrWhiteSpace(localFilePath ) ? localFilePath : null; }
            set { } }

        public Visibility hasImage
        {
            get { return !string.IsNullOrWhiteSpace(localFilePath)? Visibility.Visible : Visibility.Collapsed; }
            set { }
        }

        public string imageFileName
        {
            get { return !string.IsNullOrWhiteSpace(localFilePath) ? System.IO.Path.GetFileName(ZMEDIALOCALPATH) : String.Empty;  }
            set { }
        }
    }

    class IOSMediaItem
    {
        public int Z_PK { get; set; }
        public int Z_ENT { get; set; }
        public int Z_OPT { get; set; }
        public int ZFILESIZE { get; set; }
        public int ZMEDIAORIGIN { get; set; }
        public int ZMEDIASAVED { get; set; }
        public int ZMOVIEDURATION { get; set; }
        public int ZMESSAGE { get; set; }
        public float ZASPECTRATIO { get; set; }
        public float ZHACCURACY { get; set; }
        public float ZLATITUDE { get; set; }
        public float ZLONGITUDE { get; set; }
        public float ZAUTHORNAME { get; set; }
        public float ZCOLLECTIONNAME { get; set; }
        public float ZMEDIALOCALPATH { get; set; }
        public float ZMEDIAURL { get; set; }
        public float ZTHUMBNAILLOCALPATH { get; set; }
        public float ZTITLE { get; set; }
        public float ZVCARDNAME { get; set; }
        public float ZVCARDSTRING { get; set; }
        public float ZXMPPTHUMBPATH { get; set; }
        public byte[] ZTHUMBNAILDATA { get; set; }

    }

    #endregion

    #region android

    class AndroidChatItem
    {
        public int _id { get; set; }
        public string key_remote_jid { get; set; }
        public int message_table_id { get; set; }
    }

    class AndroidMessageItem : IMessageItem
    {
        public int _id { get; set; }
        public string key_remote_jid { get; set; }
        public int key_from_me { get; set; }
        public string key_id { get; set; }
        public int status { get; set; }
        public int needs_push { get; set; }
        public string data { get; set; }
        public long timestamp { get; set; }
        public string media_url { get; set; }
        public string media_mime_type { get; set; }
        public string media_wa_type { get; set; }
        public int media_size { get; set; }
        public string media_name { get; set; }
        public string media_hash { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string thumb_image { get; set; }
        public string remote_resource { get; set; }
        public long received_timestamp { get; set; }
        public long send_timestamp { get; set; }
        public long receipt_server_timestamp { get; set; }
        public long receipt_device_timestamp { get; set; }
        public byte[] raw_data { get; set; }
        public int recipient_count { get; set; }
        public int media_duration { get; set; }
        public int origin { get; set; }

        public int id
        {
            get { return _id; }
            set { }
        }

        public string sender
        {
            get { return remote_resource; }
            set { }
        }

        public DateTime? datetime
        {
            get { return DataSourceAndroid.TimeStampToDateTime(timestamp); }
            set { }
        }

        public string message
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(media_url))
                    return "<media>: " + media_url;
                else
                    return data;
            }
            set { }
        }

        public string media
        {
            get { return null; }
            set { }
        }

        int? IMessageItem.status
        {
            get { return status; }
            set { }
        }
        public string StatusToString
        {
            get
            {
                // item on goups
                if (status == 6)
                {

                    if (Utils.getNumberOnly(sender) == Utils.getNumberOnly(key_remote_jid))
                    {
                        if (string.IsNullOrWhiteSpace(message))
                            return Utils.getNumberOnly(sender) + " create the group";
                        else
                            return Utils.getNumberOnly(sender) + " change group name";
                    }

                    if (string.IsNullOrWhiteSpace(message))
                        return Utils.getNumberOnly(sender) + " join the group";
                    else
                        return Utils.getNumberOnly(sender);

                }

                if (status == 0)
                {
                    return Utils.getNumberOnly(sender);
                }

                return string.Format("?? {0}", status) + " " + Utils.getNumberOnly(sender);
            }
            set { }
        }

        public string imageFilePath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Visibility hasImage { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string imageFileName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

    #endregion




}
