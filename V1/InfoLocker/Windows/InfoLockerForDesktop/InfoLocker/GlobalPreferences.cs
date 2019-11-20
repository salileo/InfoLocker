using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Microsoft.Win32;

namespace InfoLocker
{
    public class GlobalPreferences : INotifyPropertyChanged
    {
        #region Instance
        private static GlobalPreferences sInstance;
        public static GlobalPreferences Instance
        {
            get
            {
                if (sInstance == null)
                    sInstance = new GlobalPreferences();

                return sInstance;
            }
        }
        #endregion

        #region PublicProperties
        public string Version = ProductVersion.VersionInfo.Version;

        public bool DebugMode
        {
            get { return m_debugMode; }
            set
            {
                m_debugMode = value;
                NotifyPropertyChanged("DebugMode");
                Save();
            }
        }

        public bool StartMinimized
        {
            get { return m_startMinimized; }
            set
            {
                m_startMinimized = value;
                NotifyPropertyChanged("StartMinimized");
                Save();
            }
        }

        public bool OpenLastUsedStorage
        {
            get { return m_openLastUsedStorage; }
            set
            {
                m_openLastUsedStorage = value;
                NotifyPropertyChanged("OpenLastUsedStorage");
                Save();
            }
        }

        public string LastUsedStorage
        {
            get { return m_lastUsedStorage; }
            set
            {
                m_lastUsedStorage = value;
                NotifyPropertyChanged("LastUsedStorage");
                Save();
            }
        }
        #endregion

        #region PrivateMembers
        private bool m_debugMode;
        private bool m_startMinimized;
        private bool m_openLastUsedStorage;
        private string m_lastUsedStorage;
        #endregion

        public GlobalPreferences()
        {
            m_debugMode = false;
            m_startMinimized = false;
            m_openLastUsedStorage = false;
            m_lastUsedStorage = string.Empty;

            RegistryKey key = null;
            try
            {
                key = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("InfoLocker");
                m_debugMode = bool.Parse(key.GetValue("DebugMode").ToString());
                m_startMinimized = bool.Parse(key.GetValue("StartMinimized").ToString());
                m_openLastUsedStorage = bool.Parse(key.GetValue("OpenLastUsedStorage").ToString());
                m_lastUsedStorage = key.GetValue("LastUsedStorage").ToString();
            }
            catch (Exception)
            {
                //This is probably the first time the preference is opened, so try to save it
                Save();
            }

            if (key != null)
                key.Close();
        }

        #region PrivateMethods
        private void Save()
        {
            RegistryKey key = null;

            try
            {
                key = Registry.CurrentUser.CreateSubKey("Software").CreateSubKey("InfoLocker");
                key.SetValue("DebugMode", DebugMode);
                key.SetValue("StartMinimized", StartMinimized);
                key.SetValue("OpenLastUsedStorage", OpenLastUsedStorage);
                key.SetValue("LastUsedStorage", LastUsedStorage);
            }
            catch (Exception e)
            {
                Logger.Log(Logger.Type.Warning, "Could not save global settings.", e);
            }

            if (key != null)
                key.Close();
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        #endregion
    }
}
