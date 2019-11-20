using System;
using System.Windows;
using System.Windows.Input;

namespace InfoLocker
{
    /// <summary>
    /// Interaction logic for AddContent.xaml
    /// </summary>
    public partial class AddContent : Window
    {
        private string NodeName { get; set; }
        private Node_Common Node { get; set; }

        public AddContent(string content_type)
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(AddContent_Loaded);

            if (!string.IsNullOrEmpty(content_type))
            {
                Title = "Add " + content_type + " ...";
                c_label.Text = "Please enter name of the new " + content_type.ToLower() + " -";
                c_name.Focus();

                if (content_type == Node_Common.Type.Folder.ToString())
                    Node = new Node_Folder();
                else if (content_type == Node_Common.Type.Note.ToString())
                    Node = new Node_Note();
            }
        }

        void AddContent_Loaded(object sender, RoutedEventArgs e)
        {
            if ((StorageModel.Instance.CurrentNode == null) || (StorageModel.Instance.CurrentNode.NodeType != Node_Common.Type.Folder))
            {
                Cancel_Clicked(null, null);
                return;
            }

            if (Node == null)
            {
                Cancel_Clicked(null, null);
                return;
            }
        }

        private void OK_Clicked(object sender, RoutedEventArgs e)
        {
            NodeName = c_name.Text;
            if (NodeName.Length == 0)
            {
                Logger.Log(Logger.Type.Alert, "Please enter a valid name.");
                return;
            }

            Node.Name = NodeName;
            (StorageModel.Instance.CurrentNode as Node_Folder).AddNode(Node);
            StorageModel.Instance.CurrentNode = Node;

            DialogResult = true;
        }

        private void Cancel_Clicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void KeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Cancel_Clicked(null, null);
            else if (e.Key == Key.Enter)
                OK_Clicked(null, null);
        }
    }
}