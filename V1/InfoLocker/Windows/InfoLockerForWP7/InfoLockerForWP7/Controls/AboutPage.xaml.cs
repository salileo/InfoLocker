using Microsoft.Phone.Controls;

namespace InfoLockerForWP7
{
    public partial class AboutPage : PhoneApplicationPage
    {
        public AboutPage()
        {
            InitializeComponent();

            c_text.Text += "InfoLocker v" + GlobalPreferences.Instance.Version.ToString() + "\n";
            c_text.Text += "Created by Salil Kapoor\n";
            c_text.Text += "\n";
            c_text.Text += "Used for storing private/personal information in an easy and secure way. This software makes the information available at your finger tips.\n";
        }
    }
}