using System;
using System.IO;
using System.Windows;
using Microsoft.Phone.Controls;
using System.IO.IsolatedStorage;

namespace InfoLockerForWP7
{
    public partial class NewStorage : PhoneApplicationPage
    {
        public NewStorage()
        {
            InitializeComponent();
            FileNameTextBox.Focus();
        }

        private void OK_Clicked(object sender, EventArgs args)
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
            if (files.FileExists(filename))
            {
                if (MessageBox.Show(filename + " already exists. Do you want to overwrite?", "Alert", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                    return;
            }

            if ((NewPassword1.Password.Length > 0) && (NewPassword1.Password.Length != 8))
            {
                Error.Log(Error.Type.Alert, "Password needs to be 8 characters long.");
                return;
            }

            if (NewPassword1.Password != NewPassword2.Password)
            {
                Error.Log(Error.Type.Alert, "The 2 passwords do not match.");
                return;
            }

            if (NewPassword1.Password.Length == 0)
            {
                if (MessageBox.Show("Are you sure you want to create a non-encrypted file?", "Alert", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                    return;
            }

            try
            {
                //At this point we are sure that the user intends to create a new storage
                if (!StorageModel.Instance.CloseStorage())
                    throw (new Exception("Could not close the currently opened storage"));

                StorageFile new_store = new StorageFile();
                new_store.Create(filename, NewPassword1.Password);
                StorageModel.Instance.Store = new_store;
                GlobalPreferences.Instance.LastUsedStorage = new_store.FileName;
            }
            catch (Exception e)
            {
                Error.Log(Error.Type.Error, "Could not create the new storage.", e);
                return;
            }

            NavigationService.GoBack();
        }
    }
}