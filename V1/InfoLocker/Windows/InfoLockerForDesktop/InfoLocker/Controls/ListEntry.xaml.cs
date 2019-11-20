using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace InfoLocker
{
    /// <summary>
    /// Interaction logic for ListEntry.xaml
    /// </summary>
    public partial class ListEntry : CommonEntry
    {
        #region PrivateMembers
        protected override string NewName
        {
            get { return NewNameText.Text; }
        }

        protected override bool IsEditing
        {
            get { return base.IsEditing; }
            set
            {
                if (base.IsEditing != value)
                {
                    if (value)
                    {
                        if ((ContextNode == null) || (ContextNode.Store == null) || ContextNode.Store.IsLocked)
                            return;

                        OldNameText.Visibility = System.Windows.Visibility.Hidden;
                        NewNameText.Visibility = System.Windows.Visibility.Visible;
                        NewNameText.Text = OldNameText.Text;
                        NewNameText.SelectAll();
                        NewNameText.Focus();

                        base.IsEditing = value;
                    }
                    else
                    {
                        OldNameText.Visibility = System.Windows.Visibility.Visible;
                        NewNameText.Visibility = System.Windows.Visibility.Hidden;

                        base.IsEditing = value;

                        if (base.ParentItem != null)
                            base.ParentItem.Focus();
                    }
                }
            }
        }
        #endregion

        public ListEntry()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(ListEntry_Loaded);
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(ListEntry_DataContextChanged);
        }

        #region OwnEventHandlers
        private void ListEntry_Loaded(object sender, RoutedEventArgs e)
        {
            if (base.ParentItem == null)
                base.ParentItem = CommonEntry.FindAnchestor<ListBoxItem>(this);
        }

        private void ListEntry_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (base.ParentItem == null)
                base.ParentItem = CommonEntry.FindAnchestor<ListBoxItem>(this);

            base.ContextNode = (e.NewValue as Node_Common);
        }
        #endregion
    }
}
