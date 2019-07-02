using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SQLite;
using System.Collections.ObjectModel;
using WhatsappViewer.DataSources;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace WhatsappViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        IDataSource data;
        ChatItem selectedChatItem;
        CommonOpenFileDialog commonOpenFileDialog = new CommonOpenFileDialog("Select iTunes/Android Backup directory of WhatsApp") { IsFolderPicker = true, Multiselect = false };
        OpenFileDialog openFileDialog1 = new OpenFileDialog() { Filter = "All Supported files|*.crypt8;*.crypt7;*.crypt;*.db;*.sqlite;|Android Crypted (.crypt8)|*.crypt8|Android Crypted (.crypt7)|*.crypt7|Android Crypted (.crypt)|*.crypt|Android (.db)|*.db|IOS (.sqlite)|*.sqlite|All Files (*.*)|*.*" };
        bool isMessageLoading = false;

        private async void ButtonSelectFile_Click(object sender, RoutedEventArgs e)
        {

            var iOSManifestDB = System.IO.Path.DirectorySeparatorChar + "Manifest.db";
            try
            {
                commonOpenFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                if (commonOpenFileDialog.ShowDialog() != CommonFileDialogResult.Ok)
                    return;
                isMessageLoading = true;
                if (data != null)
                {
                    txtStatus.Text = "Clearing previous connection....";
                    data.Dispose();
                }

                ListView1.ItemsSource = null;
                ListView1.ItemsSource = null;

                
                txtStatus.Text = "Loading chat sessions....";
                var filename = commonOpenFileDialog.FileName.ToLower() + iOSManifestDB;
                if (File.Exists(filename))
                {
                    data = new DataSourceIOS(commonOpenFileDialog.FileName.ToLower());
                }
                else
                {
                    txtStatus.Text = "WhatsApp backup file is not found in the selected directory!";
                    MessageBox.Show("WhatsApp backup file is not found in the selected directory!");
                    return;
                }

                setFileInfo(commonOpenFileDialog.FileName + iOSManifestDB);

                TreeView1.ItemsSource = await Task.Run(() => data.getChats());
                txtStatus.Text = "Loading messages for chat sessions.It may take several minutes...";
                await Task.Run(() => data.getMessages(String.Empty));
                txtStatus.Text = "Chats are loaded";
                isMessageLoading = false;
                return;
            }
            catch (Exception ex)
            {
                txtStatus.Text = "Error while opening file!";
                MessageBox.Show("Error while opening file!\nMessage:\n" + ex.Message);
            }

        }


        private void TreeView1_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (isMessageLoading)
            {
                e.Handled = true;
                return;
            }
                
            this.selectedChatItem = TreeView1.SelectedValue as ChatItem;
            if (this.selectedChatItem == null)
                return;

            if (data is DataSourceAndroid)
                ListView1.ItemsSource = data.getMessages(this.selectedChatItem.name);
            else if (data is DataSourceIOS)
                ListView1.ItemsSource = data.getMessages(this.selectedChatItem.id + "");
            if (ListView1.Items.Count > 0)
            {
                ListView1.ScrollIntoView(ListView1.Items[ListView1.Items.Count -1 ]);
            }
        }

        private void MediaHyperlink_Click(object sender, RoutedEventArgs e)
        {
            var data = (ListView1.ItemsSource as List<IMessageItem>)[ListView1.SelectedIndex];

            if (data == null)
                return;
            try
            {
                var x = Utils.downloadMedia(data.media);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void setFileInfo(string fileName)
        {
            //groupBoxInfo.DataContext = Utils.getFileInfo(fileName);
        }


        private void export_Click(object sender, RoutedEventArgs e)
        {

            if (isMessageLoading)
                return;
            // Create an instance of the open file dialog box.
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog1.Filter = "Text Files (.txt)|*.txt|All Files (*.*)|*.*";
            openFileDialog1.CheckFileExists = false;


            var sel = TreeView1.SelectedValue as ChatItem;

            // Call the ShowDialog method to show the dialog box.
            bool? userClickedOK = openFileDialog1.ShowDialog();

            // Process input if the user clicked OK.
            if (userClickedOK == true)
            {
                foreach (IMessageItem item in ListView1.SelectedItems)
                {
                    System.IO.File.AppendAllText(openFileDialog1.FileName, string.Format("{0}\t{1}\t{2}\t{3}\t{4}\n", item.datetime, sel.name, sel.descr, item.sender, item.message));
                }
            }

        }

        private void TextBoxMessage_TextChanged(object sender, TextChangedEventArgs e)
        {
            var search = (sender as TextBox).Text;
            if (string.IsNullOrWhiteSpace(search))
            {
                if (data is DataSourceAndroid)
                    ListView1.ItemsSource = data.getMessages(this.selectedChatItem.name);
                else if (data is DataSourceIOS)
                    ListView1.ItemsSource = data.getMessages(this.selectedChatItem.id + "");
            }
            else
            {

            }
        }

        private void TextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 3)
            {
                ((TextBox)sender).SelectAll();
            }
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            if (isMessageLoading)
                return;
            var fSavedDB = Directory.GetCurrentDirectory() + System.IO.Path.DirectorySeparatorChar +"chatStorage.db";

            try
            {
                if(File.Exists(fSavedDB)) File.Delete(fSavedDB);
                // You can not copy/read a locked file using normal File's methods. It requires to open as FileShare.ReadWrite.
                using (var inputFile = new FileStream (this.data.connectedDbFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)){
                    using (var outputFile = new FileStream(fSavedDB, FileMode.Create))
                    {
                        inputFile.CopyTo(outputFile);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can not save the databse!");
            }

            
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var filename = Directory.GetCurrentDirectory() + System.IO.Path.DirectorySeparatorChar + "chatStorage.db";
            if (File.Exists(filename))
            {
                isMessageLoading = true;
                txtStatus.Text = "Loading chat sessions....";
                data = new DataSourceIOS(Directory.GetCurrentDirectory(), true);
                TreeView1.ItemsSource = await Task.Run(() => data.getChats());
                txtStatus.Text = "Loading messages for chat sessions.It may take several minutes...";
                await Task.Run(() => data.getMessages(String.Empty));
                txtStatus.Text = "Chats are loaded";
                isMessageLoading = false;
            }
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(e.ClickCount == 1)
            {
                var fileName = System.IO.Path.GetTempPath() + ((System.Windows.Controls.Image)sender).Tag.ToString();
                Uri uri = new Uri(((System.Windows.Controls.Image)sender).Source.ToString());
                var sourceFile = uri.LocalPath;
                File.Copy(sourceFile, fileName, true);
                System.Diagnostics.Process.Start(fileName);

            }
        }

        private void search_Click(object sender, RoutedEventArgs e)
        {
            if (isMessageLoading)
                return;
            if (string.IsNullOrEmpty(txtSearch.Text))
                return;

            data.searchText = txtSearch.Text;
            TreeView1.ItemsSource = data.getChats();
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;
            if (isMessageLoading) 
                return;
            if (string.IsNullOrEmpty(txtSearch.Text))
                return;

            data.searchText = txtSearch.Text;
            TreeView1.ItemsSource = data.getChats();
        }
    }
}
