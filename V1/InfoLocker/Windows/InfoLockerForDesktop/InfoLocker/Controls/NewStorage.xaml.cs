using System;
using System.Windows;
using System.Windows.Input;
using System.IO;

namespace InfoLocker
{
    public partial class NewStorage : Window
    {
        private string FileName { get; set; }
        private string PasswordText { get; set; }

        public NewStorage()
        {
            InitializeComponent();
            FileName = string.Empty;
            PasswordText = string.Empty;
            FileNameTextBox.Focus();
        }

        private void OK_Clicked(object sender, RoutedEventArgs args)
        {
            string filename = Path.GetFileName(FileNameTextBox.Text);
            if (string.IsNullOrEmpty(filename))
            {
                Logger.Log(Logger.Type.Alert, "No filename specified.");
                return;
            }

            string filename_noext = Path.GetFileNameWithoutExtension(filename);
            if (string.IsNullOrEmpty(filename_noext))
            {
                Logger.Log(Logger.Type.Alert, "No filename specified.");
                return;
            }

            if (!filename.ToLower().EndsWith(".stg"))
                filename += ".stg";

            string directory = Path.GetDirectoryName(FileNameTextBox.Text);
            if (string.IsNullOrEmpty(directory))
            {
                Logger.Log(Logger.Type.Alert, "No folder specified.");
                return;
            }

            try
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
            }
            catch(Exception)
            {
                Logger.Log(Logger.Type.Error, "Could not create containing folder.");
                return;
            }

            if ((NewPassword1.Password.Length > 0) && (NewPassword1.Password.Length != 8))
            {
                Logger.Log(Logger.Type.Alert, "Password needs to be 8 characters long.");
                return;
            }

            if (NewPassword1.Password != NewPassword2.Password)
            {
                Logger.Log(Logger.Type.Alert, "The 2 passwords do not match.");
                return;
            }

            if (NewPassword1.Password.Length == 0)
            {
                if (MessageBox.Show("Are you sure you want to create a non-encrypted file?", "Alert", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return;
            }

            FileName = directory + "\\" + filename;
            PasswordText = NewPassword1.Password;

            try
            {
                //At this point we are sure that the user intends to create a new storage
                if (!StorageModel.Instance.CloseStorage())
                    throw (new Exception("Could not close the currently opened storage"));

                StorageFile new_store = new StorageFile();
                new_store.Create(FileName, PasswordText);
                StorageModel.Instance.Store = new_store;
                GlobalPreferences.Instance.LastUsedStorage = new_store.FileName;
            }
            catch (Exception e)
            {
                Logger.Log(Logger.Type.Error, "Could not create the new storage.", e);
                return;
            }

            DialogResult = true;
        }

        private void Cancel_Clicked(object sender, RoutedEventArgs args)
        {
            DialogResult = false;
        }

        private void KeyPressed(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.Escape)
                Cancel_Clicked(null, null);
            else if (args.Key == Key.Enter)
                OK_Clicked(null, null);
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog fileDialog = new System.Windows.Forms.SaveFileDialog();
            fileDialog.Title = "Select Storage File";
            fileDialog.Filter = "Storage File(*.STG)|*.STG";
            fileDialog.FilterIndex = 1;
            fileDialog.RestoreDirectory = true;
            System.Windows.Forms.DialogResult result = fileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                FileNameTextBox.Text = fileDialog.FileName;
            }
        }
    }
}