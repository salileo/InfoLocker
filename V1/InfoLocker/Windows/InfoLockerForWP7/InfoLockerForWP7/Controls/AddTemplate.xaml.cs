using System;
using System.Windows;
using Microsoft.Phone.Controls;

namespace InfoLockerForWP7
{
    public partial class AddTemplate : PhoneApplicationPage
    {
        private String m_nodeContent { get; set; }

        public AddTemplate()
        {
            InitializeComponent();
            c_name.Focus();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if ((StorageModel.Instance.CurrentNode == null) || (StorageModel.Instance.CurrentNode.NodeType != Node_Common.Type.Folder))
            {
                Cancel_Clicked(null, null);
                return;
            }
        }

        private void Radio_Clicked(object sender, RoutedEventArgs e)
        {
            ScrollView.ScrollToVerticalOffset(0);

            c_grid_bankdetails.Visibility = Visibility.Collapsed;
            c_grid_creditcarddetails.Visibility = Visibility.Collapsed;
            c_grid_websitedetails.Visibility = Visibility.Collapsed;
            c_grid_emaildetails.Visibility = Visibility.Collapsed;
            c_grid_computerdetails.Visibility = Visibility.Collapsed;

            if (c_radio_bankaccount.IsChecked == true)
                HandleBankAccount(false);
            else if (c_radio_creditcardaccount.IsChecked == true)
                HandleCreditCardAccount(false);
            else if (c_radio_websiteaccount.IsChecked == true)
                HandleWebsiteAccount(false);
            else if (c_radio_emailaccount.IsChecked == true)
                HandleEmailAccount(false);
            else if (c_radio_computeraccount.IsChecked == true)
                HandleComputerAccount(false);
        }

        private void OK_Clicked(object sender, RoutedEventArgs e)
        {
            string nodeName = c_name.Text;
            if (nodeName.Length == 0)
            {
                Error.Log(Error.Type.Alert, "Please enter a valid name.");
                return;
            }

            if (c_radio_bankaccount.IsChecked == true)
                HandleBankAccount(true);
            else if (c_radio_creditcardaccount.IsChecked == true)
                HandleCreditCardAccount(true);
            else if (c_radio_websiteaccount.IsChecked == true)
                HandleWebsiteAccount(true);
            else if (c_radio_emailaccount.IsChecked == true)
                HandleEmailAccount(true);
            else if (c_radio_computeraccount.IsChecked == true)
                HandleComputerAccount(true);

            Node_Note node = new Node_Note();
            node.Name = nodeName;
            node.Content = m_nodeContent;
            (StorageModel.Instance.CurrentNode as Node_Folder).AddNode(node);
            StorageModel.Instance.CurrentNode = node;

            NavigationService.GoBack();
        }

        private void Cancel_Clicked(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void HandleBankAccount(Boolean save)
        {
            if (save)
            {
                String content = String.Empty;

                if (c_bank_bankname.Text.Length > 0)
                    content += c_bank_bankname.Text + "\n";
                if (c_bank_bankbranch.Text.Length > 0)
                    content += "Branch : " + c_bank_bankbranch.Text + "\n";
                if (c_bank_bankaddress.Text.Length > 0)
                    content += "Address : " + c_bank_bankaddress.Text + "\n";

                if (c_bank_customercare.Text.Length > 0)
                    content += "\nCustomer Care Numbers :\n" + c_bank_customercare.Text + "\n";

                content += "\nAccount Details :-\n";
                if (c_bank_accountnumber.Text.Length > 0)
                    content += "Account Number : " + c_bank_accountnumber.Text + "\n";
                if (c_bank_mobilepin.Text.Length > 0)
                    content += "Mobile PIN : " + c_bank_mobilepin.Text + "\n";
                if (c_bank_phonepin.Text.Length > 0)
                    content += "Bank-by-Phone PIN (TPIN) : " + c_bank_phonepin.Text + "\n";

                content += "\nOnline Details :-\n";
                if (c_bank_onlinewebsite.Text.Length > 0)
                    content += c_bank_onlinewebsite.Text + "\n";
                if (c_bank_onlineaccountid.Text.Length > 0)
                    content += "Login ID : " + c_bank_onlineaccountid.Text + "\n";
                if (c_bank_onlineloginpassword.Text.Length > 0)
                    content += "Login Password : " + c_bank_onlineloginpassword.Text + "\n";
                if (c_bank_onlinetranspassword.Text.Length > 0)
                    content += "Transaction Password : " + c_bank_onlinetranspassword.Text + "\n";

                content += "\nATM/Debit Card Details :-\n";
                if (c_bank_atmcardtype.Text.Length > 0)
                    content += c_bank_atmcardtype.Text + "\n";
                if (c_bank_atmcardnumber.Text.Length > 0)
                    content += "Card Number : " + c_bank_atmcardnumber.Text + "\n";
                if (c_bank_atmvalidity.Text.Length > 0)
                    content += "Validity : " + c_bank_atmvalidity.Text + "\n";
                if (c_bank_atmcvv.Text.Length > 0)
                    content += "CVV : " + c_bank_atmcvv.Text + "\n";
                if (c_bank_atmpin.Text.Length > 0)
                    content += "PIN : " + c_bank_atmpin.Text + "\n";

                m_nodeContent = content;
            }
            else
            {
                c_grid_bankdetails.Visibility = Visibility.Visible;
                c_bank_bankname.Text = String.Empty;
                c_bank_bankbranch.Text = String.Empty;
                c_bank_bankaddress.Text = String.Empty;
                c_bank_customercare.Text = String.Empty;
                c_bank_accountnumber.Text = String.Empty;
                c_bank_mobilepin.Text = String.Empty;
                c_bank_phonepin.Text = String.Empty;
                c_bank_onlinewebsite.Text = String.Empty;
                c_bank_onlineaccountid.Text = String.Empty;
                c_bank_onlineloginpassword.Text = String.Empty;
                c_bank_onlinetranspassword.Text = String.Empty;
                c_bank_atmcardnumber.Text = String.Empty;
                c_bank_atmcardtype.Text = String.Empty;
                c_bank_atmvalidity.Text = String.Empty;
                c_bank_atmcvv.Text = String.Empty;
                c_bank_atmpin.Text = String.Empty;
            }
        }

        private void HandleCreditCardAccount(Boolean save)
        {
            if (save)
            {
                String content = String.Empty;

                if (c_creditcard_bankname.Text.Length > 0)
                    content += c_creditcard_bankname.Text + "\n";
                if (c_creditcard_bankaddress.Text.Length > 0)
                    content += "Address : " + c_creditcard_bankaddress.Text + "\n";

                if (c_creditcard_customercare.Text.Length > 0)
                    content += "\nCustomer Care Numbers :\n" + c_creditcard_customercare.Text + "\n";

                if (c_creditcard_phonepin.Text.Length > 0)
                    content += "\nBank-by-Phone PIN (TPIN) : " + c_creditcard_phonepin.Text + "\n";

                content += "\nCard Details :-\n";
                if (c_creditcard_cardnumber.Text.Length > 0)
                    content += "Card Number : " + c_creditcard_cardnumber.Text + "\n";
                if (c_creditcard_cardvalidity.Text.Length > 0)
                    content += "Validity : " + c_creditcard_cardvalidity.Text + "\n";
                if (c_creditcard_cardcvv.Text.Length > 0)
                    content += "CVV : " + c_creditcard_cardcvv.Text + "\n";
                if (c_creditcard_cardpin.Text.Length > 0)
                    content += "PIN : " + c_creditcard_cardpin.Text + "\n";

                content += "\nOnline Details :-\n";
                if (c_creditcard_onlinewebsite.Text.Length > 0)
                    content += c_creditcard_onlinewebsite.Text + "\n";
                if (c_creditcard_onlineaccountid.Text.Length > 0)
                    content += "Login ID : " + c_creditcard_onlineaccountid.Text + "\n";
                if (c_creditcard_onlineloginpassword.Text.Length > 0)
                    content += "Login Password : " + c_creditcard_onlineloginpassword.Text + "\n";
                if (c_creditcard_onlinetranspassword.Text.Length > 0)
                    content += "Transaction Password : " + c_creditcard_onlinetranspassword.Text + "\n";
                if (c_creditcard_onlinesecurecode.Text.Length > 0)
                    content += "Secure Code : " + c_creditcard_onlinesecurecode.Text + "\n";

                content += "\nOther Details :-\n";
                if (c_creditcard_billingdate.Text.Length > 0)
                    content += "Billing Date : " + c_creditcard_billingdate.Text + "\n";
                if (c_creditcard_duedate.Text.Length > 0)
                    content += "Due Date : " + c_creditcard_duedate.Text + "\n";

                m_nodeContent = content;
            }
            else
            {
                c_grid_creditcarddetails.Visibility = Visibility.Visible;
                c_creditcard_bankname.Text = String.Empty;
                c_creditcard_bankaddress.Text = String.Empty;
                c_creditcard_customercare.Text = String.Empty;
                c_creditcard_phonepin.Text = String.Empty;
                c_creditcard_cardnumber.Text = String.Empty;
                c_creditcard_cardvalidity.Text = String.Empty;
                c_creditcard_cardcvv.Text = String.Empty;
                c_creditcard_cardpin.Text = String.Empty;
                c_creditcard_onlinewebsite.Text = String.Empty;
                c_creditcard_onlineaccountid.Text = String.Empty;
                c_creditcard_onlineloginpassword.Text = String.Empty;
                c_creditcard_onlinetranspassword.Text = String.Empty;
                c_creditcard_onlinesecurecode.Text = String.Empty;
                c_creditcard_billingdate.Text = String.Empty;
                c_creditcard_duedate.Text = String.Empty;
            }
        }

        private void HandleWebsiteAccount(Boolean save)
        {
            if (save)
            {
                String content = String.Empty;

                if (c_website_onlinewebsite.Text.Length > 0)
                    content += c_website_onlinewebsite.Text + "\n";
                if (c_website_onlineusername.Text.Length > 0)
                    content += "Username : " + c_website_onlineusername.Text + "\n";
                if (c_website_onlinepassword.Text.Length > 0)
                    content += "Password : " + c_website_onlinepassword.Text + "\n";

                m_nodeContent = content;
            }
            else
            {
                c_grid_websitedetails.Visibility = Visibility.Visible;
                c_website_onlinewebsite.Text = String.Empty;
                c_website_onlineusername.Text = String.Empty;
                c_website_onlinepassword.Text = String.Empty;
            }
        }

        private void HandleEmailAccount(Boolean save)
        {
            if (save)
            {
                String content = String.Empty;

                if (c_email_address.Text.Length > 0)
                    content += c_email_address.Text + "\n";
                if (c_email_onlinewebsite.Text.Length > 0)
                    content += "Website : " + c_email_onlinewebsite.Text + "\n";
                if (c_email_onlinepassword.Text.Length > 0)
                    content += "Password : " + c_email_onlinepassword.Text + "\n";

                m_nodeContent = content;
            }
            else
            {
                c_grid_emaildetails.Visibility = Visibility.Visible;
                c_email_address.Text = String.Empty;
                c_email_onlinewebsite.Text = String.Empty;
                c_email_onlinepassword.Text = String.Empty;
            }
        }

        private void HandleComputerAccount(Boolean save)
        {
            if (save)
            {
                String content = String.Empty;

                if (c_computer_name.Text.Length > 0)
                    content += "Computer Name : " + c_computer_name.Text + "\n";
                if (c_computer_username.Text.Length > 0)
                    content += "Username : " + c_computer_username.Text + "\n";
                if (c_computer_password.Text.Length > 0)
                    content += "Password : " + c_computer_password.Text + "\n";

                m_nodeContent = content;
            }
            else
            {
                c_grid_computerdetails.Visibility = Visibility.Visible;
                c_computer_name.Text = String.Empty;
                c_computer_username.Text = String.Empty;
                c_computer_password.Text = String.Empty;
            }
        }
    }
}