using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Data;

namespace InfoLocker
{
    public class CommonEntry : UserControl
    {
        #region PrivateMembers
        private Node_Common contextNode;
        private Control parentItem;
        #endregion

        #region PublicProperties
        public Node_Common ContextNode
        {
            get { return contextNode; }
            set
            {
                contextNode = value;
                if (ParentItem != null)
                {
                    ContextMenu menu = GetContextMenu();
                    if (menu != null)
                    {
                        ParentItem.ContextMenu = menu;
                        ParentItem.ContextMenu.Opened += new RoutedEventHandler(ContextMenu_Opened);
                    }

                    if (ParentItem is TreeViewItem)
                    {
                        //workaround for selection of the node
                        MainWindow wnd = (App.Current.MainWindow as MainWindow);
                        if (wnd.PendingSelection.Contains(contextNode))
                            StorageModel.Instance.CurrentNode = contextNode;
                    }
                }
            }
        }

        public Control ParentItem
        {
            get { return parentItem; }
            set
            {
                if (parentItem != null)
                {
                    parentItem.ContextMenu = null;
                    parentItem.KeyDown -= Parent_KeyDown;
                    parentItem.LostFocus -= Parent_LostFocus;

                    parentItem.DragEnter -= Parent_DragEnter;
                    parentItem.DragOver -= Parent_DragOver;
                    parentItem.Drop -= Parent_Drop;

                    parentItem.PreviewMouseLeftButtonDown -= Parent_PreviewMouseLeftButtonDown;
                }

                parentItem = value;
                if (parentItem != null)
                {
                    parentItem.KeyDown += new KeyEventHandler(Parent_KeyDown);
                    parentItem.LostFocus += new RoutedEventHandler(Parent_LostFocus);

                    if (ContextNode != null)
                    {
                        ContextMenu menu = GetContextMenu();
                        if (menu != null)
                        {
                            parentItem.ContextMenu = menu;
                            parentItem.ContextMenu.Opened += new RoutedEventHandler(ContextMenu_Opened);
                        }
                    }

                    if (parentItem is TreeViewItem)
                    {
                        parentItem.AllowDrop = true;
                        parentItem.DragEnter += new DragEventHandler(Parent_DragEnter);
                        parentItem.DragOver += new DragEventHandler(Parent_DragOver);
                        parentItem.Drop += new DragEventHandler(Parent_Drop);

                        //workaround for selection of the node
                        MainWindow wnd = (App.Current.MainWindow as MainWindow);
                        if (wnd.PendingSelection.Contains(contextNode))
                            StorageModel.Instance.CurrentNode = contextNode;
                    }
                    else if (parentItem is ListBoxItem)
                    {
                        parentItem.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(Parent_PreviewMouseLeftButtonDown);
                    }
                }
            }
        }

        void Parent_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((ContextNode != null) && (ContextNode.Parent != null))
            {
                // Initialize the drag & drop operation
                System.Windows.DataObject dragData = new System.Windows.DataObject("myNode", ContextNode);
                DragDrop.DoDragDrop(ParentItem, dragData, System.Windows.DragDropEffects.All);
            }
        }

        void Parent_DragEnter(object sender, DragEventArgs e)
        {
            Parent_DragOver(sender, e);
        }

        void Parent_DragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;

            Node_Common node = null;
            if (e.Data.GetDataPresent("myNode"))
                node = e.Data.GetData("myNode") as Node_Common;

            if (!StorageModel.Instance.CanMoveNode(node, ContextNode as Node_Folder))
            {
                e.Effects = System.Windows.DragDropEffects.None;
                return;
            }
        }

        void Parent_Drop(object sender, DragEventArgs e)
        {
            e.Handled = true;

            Node_Common node = null;
            if (e.Data.GetDataPresent("myNode"))
                node = e.Data.GetData("myNode") as Node_Common;

            StorageModel.Instance.MoveNode(node, ContextNode as Node_Folder);
        }

        protected virtual bool IsEditing { get; set; }
        protected virtual string NewName { get; set;  }
        #endregion

        #region ParentEventHandlers
        private void Parent_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                IsEditing = true;
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                if (IsEditing)
                {
                    DoRename(true);
                    e.Handled = true;
                }
                else
                {
                    if(ContextNode != null)
                        StorageModel.Instance.CurrentNode = ContextNode;

                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Escape)
            {
                if (IsEditing)
                {
                    IsEditing = false;
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Delete)
            {
                delete_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.F3)
            {
                move_Click(null, null);
                e.Handled = true;
            }
            else if (ParentItem is TreeViewItem)
            {
                if (e.Key == Key.F4)
                {
                    sort_Click(null, null);
                    e.Handled = true;
                }
                else if ((Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Alt)) && (e.Key == Key.F))
                {
                    addFolder_Click(null, null);
                    e.Handled = true;
                }
                else if ((Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Alt)) && (e.Key == Key.T))
                {
                    addTemplate_Click(null, null);
                    e.Handled = true;
                }
                else if ((Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Alt)) && (e.Key == Key.N))
                {
                    addNote_Click(null, null);
                    e.Handled = true;
                }
            }
        }

        private void Parent_LostFocus(object sender, RoutedEventArgs e)
        {
            if (IsEditing)
                DoRename(false);
        }
        #endregion

        #region ContextMenu
        private ContextMenu GetContextMenu()
        {
            if ((ContextNode == null) || (ContextNode.Store == null))
                return null;

            ContextMenu menu = new ContextMenu();
            menu.DataContext = ContextNode;

            if (ContextNode.NodeType == Node_Common.Type.Folder)
            {
                bool fullmenu = (ParentItem != null) ? (ParentItem is TreeViewItem) : false;
                PopulateFolderContextMenuItems(menu.Items, fullmenu);
            }
            else if (ContextNode.NodeType == Node_Common.Type.Note)
            {
                PopulateNoteContextMenuItems(menu.Items);
            }

            return menu;
        }

        private void PopulateFolderContextMenuItems(ItemCollection items, bool fullmenu)
        {
            items.Clear();
            MenuItem item = null;

            Binding bnd = null;
            try
            {
                bnd = new Binding("IsLocked");
                bnd.Source = ContextNode.Store;
                bnd.Converter = new BoolInverseConverter();
            }
            catch (Exception)
            {
                bnd = null;
            }

            if (fullmenu)
            {
                item = new MenuItem();
                item.Header = "Add _folder";
                item.ToolTip = "Add a folder under the current node";
                item.InputGestureText = "Ctrl+Alt+F";
                item.SetBinding(MenuItem.IsEnabledProperty, bnd);
                item.Click += new System.Windows.RoutedEventHandler(addFolder_Click);
                items.Add(item);

                item = new MenuItem();
                item.Header = "Add note (from _template)";
                item.ToolTip = "Add a note using a predefined template";
                item.InputGestureText = "Ctrl+Alt+T";
                item.SetBinding(MenuItem.IsEnabledProperty, bnd);
                item.Click += new System.Windows.RoutedEventHandler(addTemplate_Click);
                items.Add(item);

                item = new MenuItem();
                item.Header = "Add _note (raw)";
                item.ToolTip = "Add a simple note under the current node";
                item.InputGestureText = "Ctrl+Alt+N";
                item.SetBinding(MenuItem.IsEnabledProperty, bnd);
                item.Click += new System.Windows.RoutedEventHandler(addNote_Click);
                items.Add(item);

                items.Add(new Separator());
            }

            item = new MenuItem();
            item.Header = "_Rename";
            item.ToolTip = "Rename the currently selected item";
            item.InputGestureText = "F2";
            item.SetBinding(MenuItem.IsEnabledProperty, bnd);
            item.Click += new System.Windows.RoutedEventHandler(rename_Click);
            items.Add(item);

            item = new MenuItem();
            item.Header = "_Move";
            item.ToolTip = "Move the currently selected item";
            item.InputGestureText = "F3";
            item.SetBinding(MenuItem.IsEnabledProperty, bnd);
            item.Click += new System.Windows.RoutedEventHandler(move_Click);
            items.Add(item);

            item = new MenuItem();
            item.Header = "_Delete";
            item.ToolTip = "Delete the currently selected item";
            item.InputGestureText = "Del";
            item.SetBinding(MenuItem.IsEnabledProperty, bnd);
            item.Click += new System.Windows.RoutedEventHandler(delete_Click);
            items.Add(item);

            if (fullmenu)
            {
                item = new MenuItem();
                item.Header = "_Sort";
                item.ToolTip = "Sort the currently selected item";
                item.InputGestureText = "F4";
                item.SetBinding(MenuItem.IsEnabledProperty, bnd);
                item.Click += new System.Windows.RoutedEventHandler(sort_Click);
                items.Add(item);
            }
        }

        private void PopulateNoteContextMenuItems(ItemCollection items)
        {
            items.Clear();
            MenuItem item = null;

            Binding bnd = null;
            try
            {
                bnd = new Binding("IsLocked");
                bnd.Source = ContextNode.Store;
                bnd.Converter = new BoolInverseConverter();
            }
            catch (Exception)
            {
                bnd = null;
            }

            item = new MenuItem();
            item.Header = "_Rename";
            item.ToolTip = "Rename the currently selected item";
            item.InputGestureText = "F2";
            item.SetBinding(MenuItem.IsEnabledProperty, bnd);
            item.Click += new System.Windows.RoutedEventHandler(rename_Click);
            items.Add(item);

            item = new MenuItem();
            item.Header = "_Move";
            item.ToolTip = "Move the currently selected item";
            item.InputGestureText = "F3";
            item.SetBinding(MenuItem.IsEnabledProperty, bnd);
            item.Click += new System.Windows.RoutedEventHandler(move_Click);
            items.Add(item);

            item = new MenuItem();
            item.Header = "_Delete";
            item.ToolTip = "Delete the currently selected item";
            item.InputGestureText = "Del";
            item.SetBinding(MenuItem.IsEnabledProperty, bnd);
            item.Click += new System.Windows.RoutedEventHandler(delete_Click);
            items.Add(item);
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (ParentItem is TreeViewItem)
                (ParentItem as TreeViewItem).IsSelected = true;
        }

        private void rename_Click(object sender, System.Windows.RoutedEventArgs args)
        {
            IsEditing = true;
        }

        private void move_Click(object sender, System.Windows.RoutedEventArgs args)
        {
            if (ContextNode == null)
                return;

            StorageModel.Instance.MoveNode(ContextNode, null);
        }

        private void delete_Click(object sender, System.Windows.RoutedEventArgs args)
        {
            if (ContextNode == null)
                return;

            StorageModel.Instance.DeleteNode(ContextNode);
        }

        private void sort_Click(object sender, System.Windows.RoutedEventArgs args)
        {
            if (ContextNode == null)
                return;

            StorageModel.Instance.SortNode(ContextNode);
        }

        private void addFolder_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ContextNode == null)
                return;

            StorageModel.Instance.AddFolder(ContextNode as Node_Folder);
        }

        private void addTemplate_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ContextNode == null)
                return;

            StorageModel.Instance.AddTemplate(ContextNode as Node_Folder);
        }

        private void addNote_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ContextNode == null)
                return;

            StorageModel.Instance.AddNote(ContextNode as Node_Folder);
        }
        #endregion

        #region PrivateMethods
        private void DoRename(bool displayError)
        {
            if ((ContextNode == null) || (ContextNode.Store == null) || ContextNode.Store.IsLocked)
                return;

            string newName = NewName;
            if (newName.Length <= 0)
            {
                if (displayError)
                {
                    Logger.Log(Logger.Type.Alert, "Please enter a valid name.");
                    return;
                }
                else
                {
                    IsEditing = false;
                    return;
                }
            }

            try
            {
                ContextNode.Name = newName;
                IsEditing = false;
            }
            catch (Exception e)
            {
                if (displayError)
                    Logger.Log(Logger.Type.Error, "Unable to rename the selected node.", e);
                else
                    IsEditing = false;

                return;
            }
        }
        #endregion

        public static T FindAnchestor<T>(DependencyObject current)
            where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }

                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }
    }
}
