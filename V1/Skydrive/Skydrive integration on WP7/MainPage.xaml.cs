using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Live;

namespace InfoLocker
{
    public partial class MainPage : PhoneApplicationPage
    {
        private LiveConnectClient liveClient;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void signInButton1_SessionChanged(object sender, Microsoft.Live.Controls.LiveConnectSessionChangedEventArgs e)
        {
            if (e != null && e.Status == LiveConnectSessionStatus.Connected) 
            { 
                this.liveClient = new LiveConnectClient(e.Session);
                this.GetMe(); 
            }
            else
            { 
                this.liveClient = null; 
                this.tbError.Text = e.Error != null ? e.Error.ToString() : string.Empty; 
            }
        }

        private void GetMe() 
        { 
            this.liveClient.GetCompleted += OnGetMe; 
            this.liveClient.GetAsync("me", null); 
        }
        
        private void OnGetMe(object sender, LiveOperationCompletedEventArgs e) 
        { 
            this.liveClient.GetCompleted -= OnGetMe; 
            if (e.Error == null) 
            { 
                string firstName = e.Result.ContainsKey("first_name") ? e.Result["first_name"] as string : string.Empty; 
                string lastName = e.Result.ContainsKey("last_name") ? e.Result["last_name"] as string : string.Empty; 
                this.tbGreeting.Text = "Welcome " + firstName + " " + lastName; 
            } 
            else 
            { 
                this.tbError.Text = e.Error.ToString(); 
            } 
        }
    }
}