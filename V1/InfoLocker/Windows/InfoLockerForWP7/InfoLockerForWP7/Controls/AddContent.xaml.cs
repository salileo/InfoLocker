using System;
using Microsoft.Phone.Controls;

namespace InfoLockerForWP7
{
    public partial class AddContent : PhoneApplicationPage
    {
        private Node_Common m_newNode { get; set; }

        public AddContent()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if ((StorageModel.Instance.CurrentNode == null) || (StorageModel.Instance.CurrentNode.NodeType != Node_Common.Type.Folder))
            {
                Cancel_Clicked(null, null);
                return;
            }

            String content_type = "";
            if (NavigationContext.QueryString.TryGetValue("contentType", out content_type))
            {
                PageTitle.Text = "Add " + content_type + " ...";
                c_label.Text = "Please enter name of the new " + content_type.ToLower() + " -";
                c_name.Focus();

                if (content_type == Node_Common.Type.Folder.ToString())
                    m_newNode = new Node_Folder();
                else if (content_type == Node_Common.Type.Note.ToString())
                    m_newNode = new Node_Note();
                else
                    Cancel_Clicked(null, null);
            }
            else
            {
                Cancel_Clicked(null, null);
            }
        }

        private void OK_Clicked(object sender, EventArgs e)
        {
            string nodeName = c_name.Text;
            if (nodeName.Length == 0)
            {
                Error.Log(Error.Type.Alert, "Please enter a valid name.");
                return;
            }

            m_newNode.Name = nodeName;
            (StorageModel.Instance.CurrentNode as Node_Folder).AddNode(m_newNode);
            StorageModel.Instance.CurrentNode = m_newNode;

            NavigationService.GoBack();
        }

        private void Cancel_Clicked(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}