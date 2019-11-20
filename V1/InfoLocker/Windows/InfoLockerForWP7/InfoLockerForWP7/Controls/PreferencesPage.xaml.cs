using Microsoft.Phone.Controls;

namespace InfoLockerForWP7
{
    public partial class PreferencesPage : PhoneApplicationPage
    {
        public PreferencesPage()
        {
            InitializeComponent();
            this.DataContext = GlobalPreferences.Instance;
        }
    }
}