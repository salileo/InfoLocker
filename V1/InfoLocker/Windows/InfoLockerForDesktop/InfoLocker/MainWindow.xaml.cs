using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;

namespace InfoLocker
{
    public partial class MainWindow : Window
    {
        public List<Node_Common> PendingSelection { get; set; }

        #region PrivateMembers
        private TrayIcon m_trayIcon;
        private bool m_shouldExit;
        private bool m_startNewSearch;
        #endregion

        public MainWindow()
        {
            //GenericUpdater.Updater.DoUpdate("InfoLocker", GlobalPreferences.Instance.Version, "InfoLocker_Version.xml", "InfoLocker.msi", new GenericUpdater.Updater.UpdateFiredDelegate(UpdateFired));
            InitializeComponent();

            this.DataContext = StorageModel.Instance;
            StorageModel.Instance.PropertyChanged += new PropertyChangedEventHandler(StorageModel_PropertyChanged);

            m_trayIcon = new TrayIcon(this);
            m_shouldExit = false;
            PendingSelection = new List<Node_Common>();
            c_folderMenu.DataContext = StorageModel.Instance;

            if (GlobalPreferences.Instance.OpenLastUsedStorage && (GlobalPreferences.Instance.LastUsedStorage.Length > 0))
                StorageModel.Instance.OpenStorage(GlobalPreferences.Instance.LastUsedStorage);

            if (GlobalPreferences.Instance.StartMinimized)
                m_trayIcon.ShowTrayIcon(true);
        }

        void StorageModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Store":
                    c_treeview.Items.Clear();
                    c_folderGrid.DataContext = null;
                    c_noteGrid.DataContext = null;

                    if (StorageModel.Instance.Store != null)
                        c_treeview.Items.Add(StorageModel.Instance.Store.RootNode);

                    c_passwordGrid.DataContext = StorageModel.Instance.Store;
                    break;

                case "IsStorageUnlocked":
                    c_treeview.Items.Clear();

                    if (StorageModel.Instance.Store != null)
                    {
                        c_treeview.Items.Add(StorageModel.Instance.Store.RootNode);
                        StorageModel.Instance.CurrentNode = StorageModel.Instance.Store.RootNode;
                    }

                    break;

                case "CurrentNode":
                    Node_Common currentNode = StorageModel.Instance.CurrentNode;
                    Node_Common actualSelected = SetSelectedTreeNode(currentNode);
                    if (currentNode != actualSelected)
                        StorageModel.Instance.CurrentNode = actualSelected;

                    PendingSelection.Remove(actualSelected);

                    c_folderGrid.DataContext = null;
                    c_noteGrid.DataContext = null;

                    c_folderGrid.Visibility = Visibility.Hidden;
                    c_noteGrid.Visibility = Visibility.Hidden;

                    if (StorageModel.Instance.CurrentNode != null)
                    {
                        if (StorageModel.Instance.CurrentNode.NodeType == Node_Common.Type.Folder)
                        {
                            c_folderGrid.DataContext = StorageModel.Instance.CurrentNode;
                            c_folderGrid.Visibility = Visibility.Visible;
                            c_folderView.SelectedIndex = 0;
                        
                            c_noteGrid.Visibility = Visibility.Hidden;
                        }
                        else if (StorageModel.Instance.CurrentNode.NodeType == Node_Common.Type.Note)
                        {
                            c_noteGrid.DataContext = StorageModel.Instance.CurrentNode;
                            c_folderGrid.Visibility = Visibility.Hidden;
                            c_noteGrid.Visibility = Visibility.Visible;

                            if(SearchGrid.Visibility == System.Windows.Visibility.Visible)
                            {
                                Node_Note note = StorageModel.Instance.CurrentNode as Node_Note;
                                int index = note.Content.ToLower().IndexOf(SearchText.Text.ToLower());
                                if (index > 0)
                                {
                                    c_noteText.Focus();
                                    c_noteText.Select(index, SearchText.Text.Length);
                                }
                            }
                        }
                    }

                    break;
            }

            c_passwordGrid.Visibility = Visibility.Hidden;
            if ((StorageModel.Instance.Store != null) && StorageModel.Instance.Store.IsLocked)
            {
                SearchGrid.Visibility = System.Windows.Visibility.Collapsed;
                c_passwordBox.Password = string.Empty;
                c_passwordGrid.Visibility = Visibility.Visible;
                c_passwordBox.Focus();
            }
        }

        #region StorageManagement
        private void NewStorage(object sender, RoutedEventArgs e)
        {
            StorageModel.Instance.NewStorage();
        }

        private void OpenStorage(object sender, RoutedEventArgs e)
        {
            StorageModel.Instance.OpenStorage();
        }

        private void SaveStorage(object sender, RoutedEventArgs e)
        {
            StorageModel.Instance.SaveStorage();
        }

        private void CloseStorage(object sender, RoutedEventArgs e)
        {
            StorageModel.Instance.CloseStorage();
        }

        private void StorageProperties(object sender, RoutedEventArgs e)
        {
            StorageModel.Instance.OpenStorageProperties();
        }

        private void LockStorage(object sender, RoutedEventArgs e)
        {
            StorageModel.Instance.LockStorage();
        }

        private void UnLockStorage(object sender, RoutedEventArgs e)
        {
            StorageModel.Instance.UnLockStorage(c_passwordBox.Password);
        }
        #endregion

        #region ContentManagement
        private void AddFolder_Clicked(object sender, RoutedEventArgs e)
        {
            StorageModel.Instance.AddFolder(StorageModel.Instance.CurrentNode as Node_Folder);
        }

        private void AddTemplate_Clicked(object sender, RoutedEventArgs e)
        {
            StorageModel.Instance.AddTemplate(StorageModel.Instance.CurrentNode as Node_Folder);
        }

        private void AddNote_Clicked(object sender, RoutedEventArgs e)
        {
            StorageModel.Instance.AddNote(StorageModel.Instance.CurrentNode as Node_Folder);
        }
        #endregion

        #region PrivateMethods
        private Node_Common SetSelectedTreeNode(Node_Common node)
        {
            try
            {
                if (node == c_treeview.SelectedItem)
                    return node;

                List<Node_Common> nodes = new List<Node_Common>();
                while (node != null)
                {
                    nodes.Add(node);
                    node = node.Parent;
                }

                nodes.Reverse();

                TreeViewItem item = null;
                node = null;

                while (nodes.Count > 0)
                {
                    Node_Common current = nodes[0];
                    nodes.RemoveAt(0);

                    DependencyObject obj = null;
                    if (item == null)
                    {
                        obj = c_treeview.ItemContainerGenerator.ContainerFromItem(current);
                        if ((obj != null) && (obj is TreeViewItem))
                        {
                            item = obj as TreeViewItem;
                            node = current;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        obj = item.ItemContainerGenerator.ContainerFromItem(current);
                        if ((obj != null) && (obj is TreeViewItem))
                        {
                            item = obj as TreeViewItem;
                            node = current;
                        }
                        else
                        {
                            //the nodes are not found as the expanding is pending
                            nodes.Insert(0, current);
                            PendingSelection = nodes;
                            break;
                        }
                    }
                }

                if (item != null)
                {
                    //MethodInfo selectMethod = typeof(TreeViewItem).GetMethod("Select", BindingFlags.NonPublic | BindingFlags.Instance);
                    //selectMethod.Invoke(item, new object[] { true });

                    item.IsSelected = true;
                    item.IsExpanded = true;
                    item.Focus();
                }
            }
            catch(Exception)
            {
                node = null;
            }

            return node;
        }
        #endregion

        #region EventHandlers
        private void UpdateFired()
        {
            ExitApplication(null, null);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs args)
        {
            if (!m_shouldExit)
            {
                args.Cancel = true;
                m_trayIcon.ShowTrayIcon(true);
            }
            else
            {
                if (!StorageModel.Instance.CloseStorage())
                    args.Cancel = true;
            }
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.Escape)
            {
                if (SearchGrid.Visibility == System.Windows.Visibility.Visible)
                {
                    SearchGrid.Visibility = System.Windows.Visibility.Hidden;
                }
                else
                {
                    m_trayIcon.ShowTrayIcon(true);
                }

                args.Handled = true;
            }
            else if ((Keyboard.Modifiers == ModifierKeys.Control) && (args.Key == Key.N))
            {
                NewStorage(null, null);
                args.Handled = true;
            }
            else if ((Keyboard.Modifiers == ModifierKeys.Control) && (args.Key == Key.O))
            {
                OpenStorage(null, null);
                args.Handled = true;
            }
            else if ((Keyboard.Modifiers == ModifierKeys.Control) && (args.Key == Key.S))
            {
                SaveStorage(null, null);
                args.Handled = true;
            }
            else if ((Keyboard.Modifiers == ModifierKeys.Control) && (args.Key == Key.C))
            {
                CloseStorage(null, null);
                args.Handled = true;
            }
            else if ((Keyboard.Modifiers == ModifierKeys.Control) && (args.Key == Key.L))
            {
                LockStorage(null, null);
                args.Handled = true;
            }
            else if ((Keyboard.Modifiers == ModifierKeys.Control) && (args.Key == Key.F))
            {
                m_startNewSearch = true;
                SearchGrid.Visibility = System.Windows.Visibility.Visible;
                SearchText.Focus();
            }
        }

        private void PasswordBoxKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                UnLockStorage(null, null);
        }

        private void TreeSelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            StorageModel.Instance.CurrentNode = c_treeview.SelectedItem as Node_Common;
        }

        private void ListSelectionChanged(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                StorageModel.Instance.CurrentNode = c_folderView.SelectedItem as Node_Common;
        }

        private void Settings_Clicked(object sender, RoutedEventArgs e)
        {
            PreferencesDialog dlg = new PreferencesDialog();
            dlg.Owner = this;
            dlg.ShowDialog();
        }

        private void About_Clicked(object sender, RoutedEventArgs e)
        {
            AboutDialog dlg = new AboutDialog();
            dlg.Owner = this;
            dlg.ShowDialog();
        }

        public void ExitApplication(object sender, RoutedEventArgs e)
        {
            m_trayIcon.HideTrayIcon(false);

            if (!StorageModel.Instance.CloseStorage())
            {
                if (MessageBox.Show("The currently open storage was not able to close properly, this might lead to some data loss if you continue to exit.\nDo you want to close the application anyway?", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    //simply return without quitting
                    return;
                }
            }

            m_shouldExit = true;
            Close();
        }
        #endregion

        private void c_treeview_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Get the dragged ListViewItem
            System.Windows.Controls.TreeView treeView = sender as System.Windows.Controls.TreeView;
            TreeViewItem treeViewItem = CommonEntry.FindAnchestor<TreeViewItem>((DependencyObject)e.OriginalSource);

            // Find the data behind the TreeViewItem
            Node_Common node = null;
            if (treeViewItem != null)
            {
                if (treeViewItem.Header is Node_Common)
                    node = treeViewItem.Header as Node_Common;
                else if (treeViewItem.DataContext is Node_Common)
                    node = treeViewItem.DataContext as Node_Common;
            }

            if ((node != null) && (node.Parent != null))
            {
                // Initialize the drag & drop operation
                System.Windows.DataObject dragData = new System.Windows.DataObject("myNode", node);
                DragDrop.DoDragDrop(treeViewItem, dragData, System.Windows.DragDropEffects.All);
            }
        }

        private void c_folderView_DragEnter(object sender, DragEventArgs e)
        {
            c_folderView_DragOver(sender, e);
        }

        private void c_folderView_DragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;

            Node_Folder contextNode = c_folderView.DataContext as Node_Folder;
            Node_Common node = null;
            if (e.Data.GetDataPresent("myNode"))
                node = e.Data.GetData("myNode") as Node_Common;

            if (!StorageModel.Instance.CanMoveNode(node, contextNode))
            {
                e.Effects = System.Windows.DragDropEffects.None;
                return;
            }
        }

        private void c_folderView_Drop(object sender, DragEventArgs e)
        {
            e.Handled = true;

            Node_Folder contextNode = c_folderView.DataContext as Node_Folder;
            Node_Common node = null;
            if (e.Data.GetDataPresent("myNode"))
                node = e.Data.GetData("myNode") as Node_Common;

            StorageModel.Instance.MoveNode(node, contextNode);
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            if (m_startNewSearch)
            {
                Node_Common found = StorageModel.Instance.SearchStart(SearchText.Text);
                if (found != null)
                {
                    m_startNewSearch = false;
                    StorageModel.Instance.CurrentNode = found;
                }
            }
            else
            {
                Node_Common found = StorageModel.Instance.SearchNext(StorageModel.Instance.CurrentNode, SearchText.Text);
                if (found != null)
                {
                    StorageModel.Instance.CurrentNode = found;
                }
            }
        }

        private void SearchGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                SearchGrid.Visibility = System.Windows.Visibility.Collapsed;
                e.Handled = true;
            }
        }

        private void SearchText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                SearchGrid.Visibility = System.Windows.Visibility.Collapsed;
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                Search_Click(null, null);
                e.Handled = true;
            }
        }

        private void SearchText_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_startNewSearch = true;
        }
    }
}
