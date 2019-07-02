using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using WhatsappViewer.DataSources;

namespace WhatsappViewer
{
    public class EntryToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string stringValue = value as string;
            if (string.IsNullOrEmpty(stringValue) || string.IsNullOrEmpty(stringValue.Trim()))
                return System.Windows.Media.Brushes.Lavender;
            return System.Windows.Media.Brushes.Beige;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    class Utils
    {

        public static string downloadMedia(string url)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "POST";
                WebResponse resp = req.GetResponse();
                return new StreamReader(resp.GetResponseStream(), Encoding.Default).ReadToEnd();
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public static FileInfos getFileInfo(string fileName)
        {
            var sha1 = "";
            var md5 = "";
            var size = -1;
            byte[] data = System.IO.File.ReadAllBytes(fileName);
            size = data.Length;
            using (SHA256 shaM = new SHA256Managed())
            {
                sha1 = getString(shaM.ComputeHash(data));
            }
            using (MD5 md5M = MD5.Create())
            {
                md5 = getString(md5M.ComputeHash(data));
            }

            return new FileInfos()
            {
                sha1 = sha1,
                md5 = md5,
                size = size
            };

        }

        public static string getString(byte[] data)
        {
            var sBuilder = new StringBuilder();
            foreach (var c in data)
                sBuilder.Append(c.ToString("x2"));
            return sBuilder.ToString();
        }

        public static string getNumberOnly(string number)
        {
            return (number + "").Split(new char[] { '-', '@' })[0];
        }

    }
}
