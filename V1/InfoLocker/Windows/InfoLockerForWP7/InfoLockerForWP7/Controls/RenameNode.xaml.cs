using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace InfoLockerForWP7.Controls
{
    public partial class RenameNode : PhoneApplicationPage
    {
        public RenameNode()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!StorageModel.Instance.IsStorageUnlocked)
            {
                Cancel_Clicked(null, null);
                return;
            }

            NewName.Text = StorageModel.Instance.CurrentNode.Name;
        }

        private void OK_Clicked(object sender, RoutedEventArgs e)
        {
            Node_Common current = StorageModel.Instance.CurrentNode;
            if ((current == null) || (current.Store == null) || current.Store.IsLocked)
                return;

            String newName = NewName.Text;
            if (newName.Length <= 0)
            {
                Error.Log(Error.Type.Alert, "Please enter a valid name.");
                return;
            }

            try
            {
                current.Name = newName;
            }
            catch (Exception exp)
            {
                Error.Log(Error.Type.Error, "Unable to rename the selected node.", exp);
                return;
            }

            NavigationService.GoBack();
        }

        private void Cancel_Clicked(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}