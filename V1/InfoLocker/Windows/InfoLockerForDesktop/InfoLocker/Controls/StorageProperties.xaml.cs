using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace InfoLocker
{
    public partial class StorageProperties : Window
    {
        private StorageFile Store;

        public StorageProperties()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(StorageProperties_Loaded);

            OldPassword.Password = string.Empty;
            NewPassword1.Password = string.Empty;
            NewPassword2.Password = string.Empty;
            OldPassword.Focus();

            Store = StorageModel.Instance.Store;

            if (Store != null)
                this.DataContext = Store.FileInfo;
        }

        void StorageProperties_Loaded(object sender, RoutedEventArgs e)
        {
            if ((Store == null) || Store.IsLocked)
            {
                DialogResult = false;
                return;
            }
        }

        private void Close_Clicked(object sender, RoutedEventArgs args)
        {
            DialogResult = true;
        }

        private void KeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                DialogResult = false;
        }

        private void ChangePassword_Clicked(object sender, RoutedEventArgs e)
        {
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
                {
                    Logger.Log(Logger.Type.Alert, "Password not changed.");
                    return;
                }
            }

            if (Store.TryUnLock(OldPassword.Password))
            {
                try
                {
                    Store.SaveAs(Store.FileName, NewPassword1.Password, true);
                }
                catch (Exception exp)
                {
                    Logger.Log(Logger.Type.Error, "Unable to change password.", exp);
                    return;
                }

                try
                {
                    Store.Close(false);
                }
                catch (Exception)
                {
                    Logger.Log(Logger.Type.Warning, "Password was changed successfully, however the storage could not be reloaded. Please close the storage and open again.");
                    return;
                }

                Store.Lock();
                DialogResult = true;
                Logger.Log(Logger.Type.Alert, "Password changed successfully. Please unlock the storage with the new password.");
            }
            else
            {
                Logger.Log(Logger.Type.Error, "Incorrect old password.");
            }
        }
    }
}