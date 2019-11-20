using System;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Resources;

namespace InfoLocker
{
    public class TrayIcon
    {
        private MainWindow m_parent;
        private NotifyIcon m_trayIcon;

        public TrayIcon(MainWindow obj)
        {
            try
            {
                //create the tray icon and set the icon file
                m_trayIcon = new NotifyIcon();

                Uri uri = new Uri("/Icons/main.ico", UriKind.Relative);
                StreamResourceInfo info = App.GetResourceStream(uri);
                m_trayIcon.Icon = new Icon(info.Stream);
                m_trayIcon.Text = "InfoLocker";
                m_trayIcon.Visible = false;

                //set the mouse handlers
                m_trayIcon.Click += new System.EventHandler(TrayIcon_SingleClick);

                //setup the context menu
                MenuItem menuItem1 = new MenuItem("Open main window");
                menuItem1.Click += new System.EventHandler(TrayIcon_SingleClick);
                MenuItem menuItem2 = new MenuItem("Exit Application");
                menuItem2.Click += new System.EventHandler(TrayIcon_Exit);
                ContextMenu menu = new ContextMenu();
                menu.MenuItems.Add(menuItem1);
                menu.MenuItems.Add(menuItem2);
                m_trayIcon.ContextMenu = menu;

                //set the parent in the end after all initialization has succeeded
                m_parent = obj;
            }
            catch (Exception e)
            {
                Logger.Log(Logger.Type.Error, "Tray icon creation failed. " + e.ToString());
            }
        }

        public void ShowTrayIcon(bool hideParent)
        {
            if (m_parent == null)
                return;

            //always lock the storage whenever we are being minimized or restored
            StorageModel.Instance.LockStorage();

            try
            {
                m_trayIcon.Visible = true;

                if (hideParent)
                    m_parent.Hide();
            }
            catch (Exception e)
            {
                Logger.Log(Logger.Type.Error, "Tray icon show failed. " + e.ToString());
            }
        }

        public void HideTrayIcon(bool showParent)
        {
            if (m_parent == null)
                return;

            //always lock the storage whenever we are being minimized or restored
            StorageModel.Instance.LockStorage();

            try
            {
                m_trayIcon.Visible = false;

                if (showParent)
                {
                    m_parent.Show();
                    m_parent.WindowState = WindowState.Normal;
                    m_parent.Focus();
                }
            }
            catch (Exception e)
            {
                Logger.Log(Logger.Type.Error, "Tray icon hide failed. " + e.ToString());
            }
        }

        public void SetText(string text)
        {
            m_trayIcon.Text = text;
        }

        private void TrayIcon_SingleClick(object sender, EventArgs args)
        {
            HideTrayIcon(true);
        }

        private void TrayIcon_Exit(object sender, EventArgs args)
        {
            if (m_parent != null)
            {
                m_parent.ExitApplication(null, null);
            }
        }
    }
}
