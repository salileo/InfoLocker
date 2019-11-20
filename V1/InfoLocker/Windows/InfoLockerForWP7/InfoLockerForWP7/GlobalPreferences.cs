using System;
using System.ComponentModel;
using System.IO.IsolatedStorage;

namespace InfoLockerForWP7
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
        public String Version = "1.0";

        public Boolean DebugMode
        {
            get { return m_debugMode; }
            set
            {
                m_debugMode = value;
                NotifyPropertyChanged("DebugMode");
                Save();
            }
        }

        public Boolean StartMinimized
        {
            get { return m_startMinimized; }
            set
            {
                m_startMinimized = value;
                NotifyPropertyChanged("StartMinimized");
                Save();
            }
        }

        public Boolean OpenLastUsedStorage
        {
            get { return m_openLastUsedStorage; }
            set
            {
                m_openLastUsedStorage = value;
                NotifyPropertyChanged("OpenLastUsedStorage");
                Save();
            }
        }

        public String LastUsedStorage
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
        private Boolean m_debugMode;
        private Boolean m_startMinimized;
        private Boolean m_openLastUsedStorage;
        private String m_lastUsedStorage;
        #endregion

        public GlobalPreferences()
        {
            m_debugMode = false;
            m_startMinimized = false;
            m_openLastUsedStorage = false;
            m_lastUsedStorage = String.Empty;

            try
            {
                IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
                m_debugMode = Boolean.Parse(settings["DebugMode"].ToString());
                m_startMinimized = Boolean.Parse(settings["StartMinimized"].ToString());
                m_openLastUsedStorage = Boolean.Parse(settings["OpenLastUsedStorage"].ToString());
                m_lastUsedStorage = settings["LastUsedStorage"].ToString();
            }
            catch (Exception)
            {
                //This is probably the first time the preference is opened, so try to save it
                Save();
            }
        }

        #region PrivateMethods
        private void Save()
        {
            try
            {
                IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
                settings.Clear();
                settings.Add("DebugMode", DebugMode);
                settings.Add("StartMinimized", StartMinimized);
                settings.Add("OpenLastUsedStorage", OpenLastUsedStorage);
                settings.Add("LastUsedStorage", LastUsedStorage);
                settings.Save();
            }
            catch (Exception e)
            {
                Error.Log(Error.Type.Warning, "Could not save global settings.", e);
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        #endregion
    }
}
