using System;
using System.Windows;
using System.Windows.Input;

namespace InfoLocker
{
    /// <summary>
    /// Interaction logic for AddTemplate.xaml
    /// </summary>
    public partial class AddTemplate : Window
    {
        private string NodeName { get; set; }
        private string NodeContent { get; set; }
        private Double m_MinHeight;
        private Double m_FixedHeight;
        private Double m_PerEntryHeight;

        public AddTemplate()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(AddTemplate_Loaded);

            m_MinHeight = this.Height;
            m_FixedHeight = 60;
            m_PerEntryHeight = 26;

            c_radio_bankaccount.IsChecked = true;
            c_name.Focus();
        }

        void AddTemplate_Loaded(object sender, RoutedEventArgs e)
        {
            if ((StorageModel.Instance.CurrentNode == null) || (StorageModel.Instance.CurrentNode.NodeType != Node_Common.Type.Folder))
            {
                Cancel_Clicked(null, null);
                return;
            }
        }

        private void Radio_Clicked(object sender, RoutedEventArgs e)
        {
            c_grid_bankdetails.Visibility = Visibility.Hidden;
            c_grid_creditcarddetails.Visibility = Visibility.Hidden;
            c_grid_websitedetails.Visibility = Visibility.Hidden;
            c_grid_emaildetails.Visibility = Visibility.Hidden;
            c_grid_computerdetails.Visibility = Visibility.Hidden;

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
            NodeName = c_name.Text;
            if (NodeName.Length == 0)
            {
                Logger.Log(Logger.Type.Alert, "Please enter a valid name.");
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
            node.Name = NodeName;
            node.Content = NodeContent;
            (StorageModel.Instance.CurrentNode as Node_Folder).AddNode(node);
            StorageModel.Instance.CurrentNode = node;

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
            else if(e.Key == Key.Enter)
                OK_Clicked(null, null);
        }

        private void HandleBankAccount(bool save)
        {
            if (save)
            {
                string content = string.Empty;

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

                NodeContent = content;
            }
            else
            {
                c_grid_bankdetails.Visibility = Visibility.Visible;
                this.Height = Math.Max(m_FixedHeight + (m_PerEntryHeight * 16), m_MinHeight);

                c_bank_bankname.Text = string.Empty;
                c_bank_bankbranch.Text = string.Empty;
                c_bank_bankaddress.Text = string.Empty;
                c_bank_customercare.Text = string.Empty;
                c_bank_accountnumber.Text = string.Empty;
                c_bank_mobilepin.Text = string.Empty;
                c_bank_phonepin.Text = string.Empty;
                c_bank_onlinewebsite.Text = string.Empty;
                c_bank_onlineaccountid.Text = string.Empty;
                c_bank_onlineloginpassword.Text = string.Empty;
                c_bank_onlinetranspassword.Text = string.Empty;
                c_bank_atmcardnumber.Text = string.Empty;
                c_bank_atmcardtype.Text = string.Empty;
                c_bank_atmvalidity.Text = string.Empty;
                c_bank_atmcvv.Text = string.Empty;
                c_bank_atmpin.Text = string.Empty;
            }
        }

        private void HandleCreditCardAccount(bool save)
        {
            if (save)
            {
                string content = string.Empty;

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

                NodeContent = content;
            }
            else
            {
                c_grid_creditcarddetails.Visibility = Visibility.Visible;
                this.Height = Math.Max(m_FixedHeight + (m_PerEntryHeight * 15), m_MinHeight);

                c_creditcard_bankname.Text = string.Empty;
                c_creditcard_bankaddress.Text = string.Empty;
                c_creditcard_customercare.Text = string.Empty;
                c_creditcard_phonepin.Text = string.Empty;
                c_creditcard_cardnumber.Text = string.Empty;
                c_creditcard_cardvalidity.Text = string.Empty;
                c_creditcard_cardcvv.Text = string.Empty;
                c_creditcard_cardpin.Text = string.Empty;
                c_creditcard_onlinewebsite.Text = string.Empty;
                c_creditcard_onlineaccountid.Text = string.Empty;
                c_creditcard_onlineloginpassword.Text = string.Empty;
                c_creditcard_onlinetranspassword.Text = string.Empty;
                c_creditcard_onlinesecurecode.Text = string.Empty;
                c_creditcard_billingdate.Text = string.Empty;
                c_creditcard_duedate.Text = string.Empty;
            }
        }

        private void HandleWebsiteAccount(bool save)
        {
            if (save)
            {
                string content = string.Empty;

                if (c_website_onlinewebsite.Text.Length > 0)
                    content += c_website_onlinewebsite.Text + "\n";
                if (c_website_onlineusername.Text.Length > 0)
                    content += "Username : " + c_website_onlineusername.Text + "\n";
                if (c_website_onlinepassword.Text.Length > 0)
                    content += "Password : " + c_website_onlinepassword.Text + "\n";

                NodeContent = content;
            }
            else
            {
                c_grid_websitedetails.Visibility = Visibility.Visible;
                this.Height = Math.Max(m_FixedHeight + (m_PerEntryHeight * 3), m_MinHeight);

                c_website_onlinewebsite.Text = string.Empty;
                c_website_onlineusername.Text = string.Empty;
                c_website_onlinepassword.Text = string.Empty;
            }
        }

        private void HandleEmailAccount(bool save)
        {
            if (save)
            {
                string content = string.Empty;

                if (c_email_address.Text.Length > 0)
                    content += c_email_address.Text + "\n";
                if (c_email_onlinewebsite.Text.Length > 0)
                    content += "Website : " + c_email_onlinewebsite.Text + "\n";
                if (c_email_onlinepassword.Text.Length > 0)
                    content += "Password : " + c_email_onlinepassword.Text + "\n";

                NodeContent = content;
            }
            else
            {
                c_grid_emaildetails.Visibility = Visibility.Visible;
                this.Height = Math.Max(m_FixedHeight + (m_PerEntryHeight * 3), m_MinHeight);

                c_email_address.Text = string.Empty;
                c_email_onlinewebsite.Text = string.Empty;
                c_email_onlinepassword.Text = string.Empty;
            }
        }

        private void HandleComputerAccount(bool save)
        {
            if (save)
            {
                string content = string.Empty;

                if (c_computer_name.Text.Length > 0)
                    content += "Computer Name : " + c_computer_name.Text + "\n";
                if (c_computer_username.Text.Length > 0)
                    content += "Username : " + c_computer_username.Text + "\n";
                if (c_computer_password.Text.Length > 0)
                    content += "Password : " + c_computer_password.Text + "\n";

                NodeContent = content;
            }
            else
            {
                c_grid_computerdetails.Visibility = Visibility.Visible;
                this.Height = Math.Max(m_FixedHeight + (m_PerEntryHeight * 3), m_MinHeight);

                c_computer_name.Text = string.Empty;
                c_computer_username.Text = string.Empty;
                c_computer_password.Text = string.Empty;
            }
        }
    }
}