using Microsoft.Phone.Controls;

namespace InfoLockerForWP7
{
    public partial class PreferencesDialog : PhoneApplicationPage
    {
        public PreferencesDialog()
        {
            InitializeComponent();
            this.DataContext = GlobalPreferences.Instance;
        }
    }
}