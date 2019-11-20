using System;
using System.Windows;
using Microsoft.Phone.Controls;

namespace InfoLockerForWP7
{
    public partial class StorageProperties : PhoneApplicationPage
    {
        private StorageFile m_store;

        public StorageProperties()
        {
            InitializeComponent();

            OldPassword.Password = String.Empty;
            NewPassword1.Password = String.Empty;
            NewPassword2.Password = String.Empty;
            OldPassword.Focus();

            m_store = StorageModel.Instance.Store;

            if (m_store != null)
                this.DataContext = m_store.FileInfo;

        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if ((m_store == null) || m_store.IsLocked)
                NavigationService.GoBack();
        }

        private void ChangePassword_Clicked(object sender, RoutedEventArgs e)
        {
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
                {
                    Error.Log(Error.Type.Alert, "Password not changed.");
                    return;
                }
            }

            if (m_store.TryUnLock(OldPassword.Password))
            {
                try
                {
                    m_store.SaveAs(m_store.FileName, NewPassword1.Password, true);
                }
                catch (Exception exp)
                {
                    Error.Log(Error.Type.Error, "Unable to change password.", exp);
                    return;
                }

                try
                {
                    m_store.Close(false);
                }
                catch (Exception)
                {
                    Error.Log(Error.Type.Warning, "Password was changed successfully, however the storage could not be reloaded. Please close the storage and open again.");
                    return;
                }

                m_store.Lock();
                Error.Log(Error.Type.Alert, "Password changed successfully. Please unlock the storage with the new password.");

                NavigationService.GoBack();
            }
            else
            {
                Error.Log(Error.Type.Error, "Incorrect old password.");
            }
        }
    }
}