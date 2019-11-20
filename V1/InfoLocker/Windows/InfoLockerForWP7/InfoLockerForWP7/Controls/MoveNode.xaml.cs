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
    public partial class MoveNode : PhoneApplicationPage
    {
        public MoveNode()
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

            this.DataContext = StorageModel.Instance.Store.RootNode;
        }

        private void NodeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((e.AddedItems.Count > 0) && (e.AddedItems[0] != null))
                this.DataContext = (e.AddedItems[0] as Node_Folder);
        }

        private void OK_Clicked(object sender, RoutedEventArgs e)
        {
            Node_Folder selection = this.DataContext as Node_Folder;
            if (selection == null)
            {
                Error.Log(Error.Type.Error, "Invalid parent selected.");
                return;
            }

            if (!StorageModel.Instance.MoveNode(StorageModel.Instance.CurrentNode, selection))
                return;

            NavigationService.GoBack();
        }

        private void Cancel_Clicked(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void Up_Click(object sender, RoutedEventArgs e)
        {
            if ((this.DataContext as Node_Common).Parent != null)
                this.DataContext = (this.DataContext as Node_Common).Parent;
        }
    }
}