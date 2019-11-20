using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media;

namespace InfoLocker
{
    /// <summary>
    /// Interaction logic for MoveNode.xaml
    /// </summary>
    public partial class MoveNode : Window
    {
        private StorageFile m_store;
        private Node_Common m_node;

        public MoveNode(Node_Common node)
        {
            InitializeComponent();
            m_store = StorageModel.Instance.Store;
            m_node = node;

            if(m_node != null)
                c_label.Text = "Please choose the new folder for '" + m_node.Name + "' -";

            if(m_store != null)
                c_treeview.Items.Add(m_store.RootNode);

            this.Loaded += new RoutedEventHandler(MoveNode_Loaded);
        }

        void MoveNode_Loaded(object sender, RoutedEventArgs e)
        {
            if ((m_node == null) || (m_store == null))
            {
                DialogResult = false;
                return;
            }

            TreeViewItem item = null;
            DependencyObject obj = c_treeview.ItemContainerGenerator.ContainerFromItem(m_store.RootNode);
            if ((obj != null) && (obj is TreeViewItem))
                item = obj as TreeViewItem;

            if (item != null)
            {
                //MethodInfo selectMethod = typeof(TreeViewItem).GetMethod("Select", BindingFlags.NonPublic | BindingFlags.Instance);
                //selectMethod.Invoke(item, new object[] { true });

                item.IsSelected = true;
                item.IsExpanded = true;
                item.Focus();
            }
        }

        private void OK_Clicked(object sender, RoutedEventArgs e)
        {
            Node_Folder parent = c_treeview.SelectedItem as Node_Folder;
            if(parent == null)
            {
                Logger.Log(Logger.Type.Error, "Invalid parent selected.");
                return;
            }

            if (!StorageModel.Instance.MoveNode(m_node, parent))
                    return;
            
            DialogResult = true;
        }

        private void Cancel_Clicked(object sender, RoutedEventArgs e)
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
    }
}
