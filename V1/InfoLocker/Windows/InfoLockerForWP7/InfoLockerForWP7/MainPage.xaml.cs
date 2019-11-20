using System;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace InfoLockerForWP7
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            StorageModel.Instance.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(StorageModel_PropertyChanged);
            UpdateView();
        }

        void StorageModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateView();
        }

        private void UpdateView()
        {
            this.DataContext = StorageModel.Instance.Store;

            c_folderGrid.DataContext = StorageModel.Instance.CurrentNode;
            c_noteGrid.DataContext = StorageModel.Instance.CurrentNode;

            c_folderGrid.Visibility = Visibility.Collapsed;
            c_noteGrid.Visibility = Visibility.Collapsed;
            c_passwordBox.Password = string.Empty;
            c_passwordGrid.Visibility = Visibility.Collapsed;

            if (StorageModel.Instance.CurrentNode != null)
            {
                if (StorageModel.Instance.CurrentNode.NodeType == Node_Common.Type.Folder)
                    c_folderGrid.Visibility = Visibility.Visible;
                else if (StorageModel.Instance.CurrentNode.NodeType == Node_Common.Type.Note)
                    c_noteGrid.Visibility = Visibility.Visible;
            }

            if ((StorageModel.Instance.Store != null) && StorageModel.Instance.Store.IsLocked)
                c_passwordGrid.Visibility = Visibility.Visible;

            UpdateApplicationBar();
        }

        private void UpdateApplicationBar()
        {
            ApplicationBar.MenuItems.Clear();

            if (StorageModel.Instance.Store != null)
            {
                if ((StorageModel.Instance.IsStorageUnlocked) && 
                    (StorageModel.Instance.CurrentNode != null))
                {
                    if (StorageModel.Instance.CurrentNode.NodeType == Node_Common.Type.Folder)
                    {
                        ApplicationBarMenuItem addFolder = new ApplicationBarMenuItem("Add Folder");
                        addFolder.Click += new EventHandler(addFolder_Click);
                        ApplicationBar.MenuItems.Add(addFolder);

                        ApplicationBarMenuItem addTemplate = new ApplicationBarMenuItem("Add Template");
                        addTemplate.Click += new EventHandler(addTemplate_Click);
                        ApplicationBar.MenuItems.Add(addTemplate);

                        ApplicationBarMenuItem addNote = new ApplicationBarMenuItem("Add Note");
                        addNote.Click += new EventHandler(addNote_Click);
                        ApplicationBar.MenuItems.Add(addNote);

                        ApplicationBarMenuItem separator1 = new ApplicationBarMenuItem("_________________");
                        separator1.IsEnabled = false;
                        ApplicationBar.MenuItems.Add(separator1);
                    }

                    ApplicationBarMenuItem renameNode = new ApplicationBarMenuItem("Rename");
                    renameNode.Click += new EventHandler(renameNode_Click);
                    ApplicationBar.MenuItems.Add(renameNode);

                    ApplicationBarMenuItem moveNode = new ApplicationBarMenuItem("Move");
                    moveNode.Click += new EventHandler(moveNode_Click);
                    ApplicationBar.MenuItems.Add(moveNode);

                    ApplicationBarMenuItem deleteNode = new ApplicationBarMenuItem("Delete");
                    deleteNode.Click += new EventHandler(deleteNode_Click);
                    ApplicationBar.MenuItems.Add(deleteNode);

                    ApplicationBarMenuItem separator2 = new ApplicationBarMenuItem("_________________");
                    separator2.IsEnabled = false;
                    ApplicationBar.MenuItems.Add(separator2);
                }

                ApplicationBarMenuItem closeStorage = new ApplicationBarMenuItem("Close Storage");
                closeStorage.Click += new EventHandler(closeStorage_Click);
                ApplicationBar.MenuItems.Add(closeStorage);

                if (StorageModel.Instance.IsStorageUnlocked)
                {
                    ApplicationBarMenuItem saveStorage = new ApplicationBarMenuItem("Save Storage");
                    saveStorage.Click += new EventHandler(saveStorage_Click);
                    ApplicationBar.MenuItems.Add(saveStorage);

                    ApplicationBarMenuItem lockStorage = new ApplicationBarMenuItem("Lock Storage");
                    lockStorage.Click += new EventHandler(lockStorage_Click);
                    ApplicationBar.MenuItems.Add(lockStorage);

                    ApplicationBarMenuItem storeProperties = new ApplicationBarMenuItem("Storage Properties");
                    storeProperties.Click += new EventHandler(storeProperties_Click);
                    ApplicationBar.MenuItems.Add(storeProperties);
                }

                ApplicationBarMenuItem separator3 = new ApplicationBarMenuItem("_________________");
                separator3.IsEnabled = false;
                ApplicationBar.MenuItems.Add(separator3);
            }

            ApplicationBarMenuItem newStorage = new ApplicationBarMenuItem("New Storage");
            newStorage.Click += new EventHandler(newStorage_Click);
            ApplicationBar.MenuItems.Add(newStorage);

            ApplicationBarMenuItem openStorage = new ApplicationBarMenuItem("Open Storage");
            openStorage.Click += new EventHandler(openStorage_Click);
            ApplicationBar.MenuItems.Add(openStorage);

            ApplicationBarMenuItem preferences = new ApplicationBarMenuItem("Preferences");
            preferences.Click += new EventHandler(preferences_Click);
            ApplicationBar.MenuItems.Add(preferences);

            ApplicationBarMenuItem about = new ApplicationBarMenuItem("About");
            about.Click += new EventHandler(about_Click);
            ApplicationBar.MenuItems.Add(about);
        }

        void addFolder_Click(object sender, EventArgs e)
        {
            StorageModel.Instance.AddFolder(StorageModel.Instance.CurrentNode as Node_Folder);
        }

        void addTemplate_Click(object sender, EventArgs e)
        {
            StorageModel.Instance.AddTemplate(StorageModel.Instance.CurrentNode as Node_Folder);
        }

        void addNote_Click(object sender, EventArgs e)
        {
            StorageModel.Instance.AddNote(StorageModel.Instance.CurrentNode as Node_Folder);
        }

        void renameNode_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Controls/RenameNode.xaml", UriKind.Relative));
        }

        void moveNode_Click(object sender, EventArgs e)
        {
            StorageModel.Instance.MoveNode(StorageModel.Instance.CurrentNode, null);
        }

        void deleteNode_Click(object sender, EventArgs e)
        {
            StorageModel.Instance.DeleteNode(StorageModel.Instance.CurrentNode);
        }

        void newStorage_Click(object sender, EventArgs e)
        {
            StorageModel.Instance.NewStorage();
        }

        void openStorage_Click(object sender, EventArgs e)
        {
            StorageModel.Instance.OpenStorage();
        }

        void closeStorage_Click(object sender, EventArgs e)
        {
            StorageModel.Instance.CloseStorage();
        }

        void saveStorage_Click(object sender, EventArgs e)
        {
            StorageModel.Instance.SaveStorage();
        }

        void lockStorage_Click(object sender, EventArgs e)
        {
            StorageModel.Instance.LockStorage();
            c_passwordBox.Password = string.Empty;
        }

        private void storeProperties_Click(object sender, EventArgs e)
        {
            StorageModel.Instance.OpenStorageProperties();
        }

        private void preferences_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Controls/PreferencesDialog.xaml", UriKind.Relative));
        }

        private void about_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Controls/AboutDialog.xaml", UriKind.Relative));
        }

        private void unlock_Clicked(object sender, RoutedEventArgs e)
        {
            StorageModel.Instance.UnLockStorage(c_passwordBox.Password);
            c_passwordBox.Password = string.Empty;
        }

        private void Main_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if ((StorageModel.Instance.CurrentNode != null) &&
                (StorageModel.Instance.CurrentNode.Parent != null))
            {
                StorageModel.Instance.CurrentNode = StorageModel.Instance.CurrentNode.Parent; 
                e.Cancel = true;
            }
        }

        private void folderView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                Node_Common node = e.AddedItems[0] as Node_Common;
                if (node != null)
                    StorageModel.Instance.CurrentNode = node;
            }
        }
    }
}
