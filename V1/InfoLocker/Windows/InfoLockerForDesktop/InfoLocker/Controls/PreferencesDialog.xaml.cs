using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using System.ComponentModel;

namespace InfoLocker
{
    public partial class PreferencesDialog : Window
    {
        public PreferencesDialog()
        {
            InitializeComponent();
            this.DataContext = GlobalPreferences.Instance;
        }

        private void OK_Clicked(object sender, RoutedEventArgs args)
        {
            Close();
        }

        private void KeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}