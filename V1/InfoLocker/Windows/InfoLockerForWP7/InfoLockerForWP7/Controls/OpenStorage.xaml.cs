using System;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using System.IO;

namespace InfoLockerForWP7.Controls
{
    public partial class OpenStorage : PhoneApplicationPage
    {
        public OpenStorage()
        {
            InitializeComponent();

            IsolatedStorageFile files = IsolatedStorageFile.GetUserStoreForApplication();
            String[] filelist = files.GetFileNames();
            FileList.ItemsSource = filelist;
        }

        private void FileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((e.AddedItems.Count > 0) && (e.AddedItems[0] != null))
                FileNameTextBox.Text = e.AddedItems[0].ToString();
        }

        private void OK_Clicked(object sender, RoutedEventArgs e)
        {
            String filename = Path.GetFileName(FileNameTextBox.Text);
            if (String.IsNullOrEmpty(filename))
            {
                Error.Log(Error.Type.Alert, "No filename specified.");
                return;
            }
            else if (filename != FileNameTextBox.Text)
            {
                Error.Log(Error.Type.Alert, "Please do no specify any folder in the name.");
                return;
            }

            String filename_noext = Path.GetFileNameWithoutExtension(filename);
            if (String.IsNullOrEmpty(filename_noext))
            {
                Error.Log(Error.Type.Alert, "No filename specified.");
                return;
            }

            if (!filename.ToLower().EndsWith(".stg"))
                filename += ".stg";

            IsolatedStorageFile files = IsolatedStorageFile.GetUserStoreForApplication();
            if (!files.FileExists(filename))
            {
                Error.Log(Error.Type.Error, "File not found.");
                return;
            }

            StorageModel.Instance.OpenStorage(filename);
            NavigationService.GoBack();
        }
    }
}