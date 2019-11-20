using System;
using System.Windows;
using System.ComponentModel;
using System.Collections.Generic;

namespace InfoLockerForWP7
{
    public class StorageModel : INotifyPropertyChanged
    {
        #region Instance
        private static StorageModel sInstance;
        public static StorageModel Instance
        {
            get
            {
                if (sInstance == null)
                    sInstance = new StorageModel();

                return sInstance;
            }
        }
        #endregion

        #region PrivateMembers
        private StorageFile m_store;
        private Node_Common m_currentNode;
        #endregion

        #region PublicProperties
        public Boolean IsStorageUnlocked
        {
            get
            {
                if ((m_store == null) || (m_store.IsLocked))
                    return false;
                else
                    return true;
            }
        }

        public StorageFile Store
        {
            get { return m_store; }
            set
            {
                if (m_store != null)
                {
                    m_store.PropertyChanged -= Store_PropertyChanged;
                    m_store = null;
                }

                m_store = value;
                CurrentNode = null;

                if (m_store != null)
                {
                    m_store.PropertyChanged += new PropertyChangedEventHandler(Store_PropertyChanged);
                    CurrentNode = m_store.RootNode;
                }

                NotifyPropertyChanged("Store");
                NotifyPropertyChanged("IsStorageUnlocked");
            }
        }

        public Node_Common CurrentNode
        {
            get { return m_currentNode; }
            set
            {
                m_currentNode = value;
                NotifyPropertyChanged("CurrentNode");
            }
        }
        #endregion

        public StorageModel()
        {
            m_store = null;
            m_currentNode = null;
        }

        #region StorageManagement
        public void NewStorage()
        {
            (App.Current as App).RootFrame.Navigate(new Uri("/Controls/NewStorage.xaml", UriKind.Relative));
        }

        public void OpenStorage(String filename)
        {
            try
            {
                //At this point we are sure that the user intends to open another storage
                if (!CloseStorage())
                    throw (new Exception("Could not close the currently opened storage"));

                StorageFile new_store = new StorageFile(filename);
                Store = new_store;
                
                GlobalPreferences.Instance.LastUsedStorage = new_store.FileName;
            }
            catch (Exception e)
            {
                Error.Log(Error.Type.Error, "Could not open storage.", e);

                if (GlobalPreferences.Instance.LastUsedStorage.ToLower() == filename.ToLower())
                    GlobalPreferences.Instance.LastUsedStorage = String.Empty;
            }
        }

        public void OpenStorage()
        {
            (App.Current as App).RootFrame.Navigate(new Uri("/Controls/OpenStorage.xaml", UriKind.Relative));
        }

        public Boolean CloseStorage()
        {
            if (Store == null)
                return true;

            if (Store.IsDirty)
            {
                if (MessageBox.Show("Do you want to save the storage before closing it?", "Alert", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    if (!SaveStorage())
                    {
                        if (MessageBox.Show("Do you want to continue closing without saving?", "Warning", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                        {
                            return false;
                        }
                    }
                }
            }

            Boolean success = true;

            try
            {
                Store.Close(false);
                Store = null;
            }
            catch (Exception exp)
            {
                Error.Log(Error.Type.Error, "Could not close the storage.", exp);
                success = false;
            }

            return success;
        }

        public Boolean SaveStorage()
        {
            if (Store == null)
                return true;

            if (Store.IsLocked)
            {
                Error.Log(Error.Type.Error, "Please unlock the storage in order to save it.");
                return false;
            }

            if (Store.IsDirty && !Store.IsInSync())
            {
                if (MessageBox.Show("The storage has changes on disk as well as in the opened state. Are you sure you want to overwrite the disk changes?", "Alert", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                {
                    if (MessageBox.Show("Do you want to discard the open changes and reload from file?", "Alert", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        try
                        {
                            Store.Close(false);
                        }
                        catch (Exception exp)
                        {
                            Error.Log(Error.Type.Error, "Could not refresh the storage.", exp);
                        }
                    }

                    return false;
                }
            }

            try
            {
                Store.Save();
            }
            catch (Exception exp)
            {
                Error.Log(Error.Type.Error, "Could not save the storage.", exp);
                return false;
            }

            return true;
        }

        public void OpenStorageProperties()
        {
            if ((Store == null) || Store.IsLocked)
                return;

            if (Store.IsDirty)
            {
                if (MessageBox.Show("The storage needs to be saved to open the properties. Do you want to save the storage now?", "Alert", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                    return;

                if (!SaveStorage())
                {
                    Error.Log(Error.Type.Error, "Cannot open properties till the storage is saved.");
                    return;
                }
            }
            else
            {
                if (!Store.IsInSync())
                {
                    Error.Log(Error.Type.Alert, "The storage has changed on disk. Please lock and unlock the storage and try opening the properties again.");
                    return;
                }
            }

            (App.Current as App).RootFrame.Navigate(new Uri("/Controls/StorageProperties.xaml", UriKind.Relative));
        }

        public Boolean LockStorage()
        {
            if (Store == null)
                return true;

            try
            {
                Store.Lock();
            }
            catch (Exception exp)
            {
                Error.Log(Error.Type.Error, "Could not lock the storage.", exp);
                return false;
            }

            return true;
        }

        public Boolean UnLockStorage(String password)
        {
            if (Store == null)
                return true;

            try
            {
                Boolean shouldSync = false;
                if (!Store.IsInSync())
                {
                    if (MessageBox.Show("The storage has changed on disk. Do you want to reload it?", "Alert", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        shouldSync = true;
                }

                if (shouldSync)
                {
                    if (!SaveStorage())
                    {
                        Error.Log(Error.Type.Error, "Could not save storage during the sync.");
                    }
                    else if (!Store.Sync())
                    {
                        Error.Log(Error.Type.Error, "Could not sync the storage.");
                    }
                }

                Store.UnLock(password);
            }
            catch (Exception exp)
            {
                Error.Log(Error.Type.Error, "Could not unlock the storage.", exp);
                return false;
            }

            return true;
        }

        private void Store_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsLocked":
                    NotifyPropertyChanged("IsStorageUnlocked");
                    break;

                case "RootNode":
                    CurrentNode = Store.RootNode;
                    break;
            }
        }
        #endregion

        #region ContentManagement
        public void AddFolder(Node_Folder parent)
        {
            if ((parent == null) ||
                (parent.NodeType != Node_Common.Type.Folder) ||
                (parent.Store == null) ||
                (parent.Store.IsLocked))
                return;

            CurrentNode = parent;
            (App.Current as App).RootFrame.Navigate(new Uri("/Controls/AddContent.xaml?contentType=" + Node_Common.Type.Folder.ToString(), UriKind.Relative));
        }

        public void AddTemplate(Node_Folder parent)
        {
            if ((parent == null) ||
                (parent.NodeType != Node_Common.Type.Folder) ||
                (parent.Store == null) ||
                (parent.Store.IsLocked))
                return;

            CurrentNode = parent;
            (App.Current as App).RootFrame.Navigate(new Uri("/Controls/AddTemplate.xaml", UriKind.Relative));
        }

        public void AddNote(Node_Folder parent)
        {
            if ((parent == null) ||
                (parent.NodeType != Node_Common.Type.Folder) ||
                (parent.Store == null) ||
                (parent.Store.IsLocked))
                return;

            CurrentNode = parent;
            (App.Current as App).RootFrame.Navigate(new Uri("/Controls/AddContent.xaml?contentType=" + Node_Common.Type.Note.ToString(), UriKind.Relative));
        }

        public Boolean DeleteNode(Node_Common node)
        {
            try
            {
                if ((node == null) ||
                    (node.Store == null) ||
                    (node.Store.IsLocked))
                {
                    throw (new Exception("Invalid state."));
                }

                if ((node.NodeType == Node_Common.Type.Folder) &&
                    (node.Parent == null))
                {
                    throw (new Exception("Cannot delete the root folder."));
                }

                if (MessageBox.Show("Are you sure you want to delete \"" + node.Name + "\"?", "Warning", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    if (CurrentNode == node)
                        CurrentNode = node.Parent;

                    if (!node.Parent.RemoveNode(node))
                        throw (new Exception("Error in deletion."));
                }
            }
            catch (Exception e)
            {
                Error.Log(Error.Type.Error, "Unable to delete the selected node.", e);
                return false;
            }

            return true;
        }

        public Boolean CanMoveNode(Node_Common nodeToMove, Node_Folder newParent)
        {
            if ((nodeToMove == null) ||
                (newParent == null) ||
                (newParent.Store == null) ||
                (newParent.Store.IsLocked) ||
                (nodeToMove == newParent))
            {
                return false;
            }

            List<Node_Common> parents = new List<Node_Common>();
            Node_Common node = newParent;
            while (node != null)
            {
                parents.Add(node);
                node = node.Parent;
            }

            if (parents.Contains(nodeToMove))
                return false;

            return true;
        }

        public Boolean MoveNode(Node_Common node, Node_Folder newParent)
        {
            try
            {
                if ((node.NodeType == Node_Common.Type.Folder) &&
                    (node.Parent == null))
                {
                    throw (new Exception("Cannot move the root folder."));
                }

                if (newParent == null)
                {
                    CurrentNode = node;
                    (App.Current as App).RootFrame.Navigate(new Uri("/Controls/MoveNode.xaml", UriKind.Relative));
                }
                else if (CanMoveNode(node, newParent))
                {
                    if (!newParent.AddNode(node))
                    {
                        throw (new Exception("Error in adding node to new parent."));
                    }
                }
                else
                {
                    throw (new Exception("Invalid parent selected."));
                }
            }
            catch (Exception e)
            {
                Error.Log(Error.Type.Error, "Unable to move the selected node.", e);
                return false;
            }

            return true;
        }

        public Boolean SortNode(Node_Common node)
        {
            try
            {
                if ((node == null) ||
                    (node.Store == null) ||
                    (node.Store.IsLocked))
                {
                    throw (new Exception("Invalid state."));
                }

                Boolean success = false;
                if (node.NodeType == Node_Common.Type.Folder)
                    success = (node as Node_Folder).SortNodes(-1);
                else if (node.NodeType == Node_Common.Type.Note)
                    success = node.Parent.SortNodes(-1);

                if (!success)
                    throw (new Exception("Error in sorting."));

                StorageModel.Instance.CurrentNode = StorageModel.Instance.Store.RootNode;
            }
            catch (Exception e)
            {
                Error.Log(Error.Type.Error, "Unable to sort the selected node.", e);
                return false;
            }

            return true;
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        #endregion
    }
}
