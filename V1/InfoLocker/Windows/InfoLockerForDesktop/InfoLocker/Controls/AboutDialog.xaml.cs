using System;
using System.Windows;
using System.Windows.Input;

namespace InfoLocker
{
    /// <summary>
    /// Interaction logic for AboutDialog.xaml
    /// </summary>
    public partial class AboutDialog : Window
    {
        public AboutDialog()
        {
            InitializeComponent();

            c_text.Text += "InfoLocker v" + GlobalPreferences.Instance.Version.ToString() + "\n";
            c_text.Text += "Created by Salil Kapoor\n";
            c_text.Text += "\n";
            c_text.Text += "Used for storing private/personal information in an easy and secure way. This software makes the information available at your finger tips.\n";
        }

        private void OK_Clicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void KeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                DialogResult = false;
        }
    }
}