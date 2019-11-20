using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage;

namespace InfoLocker
{
    public class Settings
    {
        public static string AppName = "InfoLocker";

        private static Settings instance;
        private static object lockObj = new object();

        public static Settings Instance
        {
            get
            {
                lock(lockObj)
                {
                    if (instance == null)
                    {
                        instance = new Settings();
                        instance.container = ApplicationData.Current.LocalSettings;
                    }

                    return instance;
                }
            }
        }

        private ApplicationDataContainer container = null;

        public string CabinetPath
        {
            get
            {
                string value = this.container.Values["cabinetPath"] as string;
                return value;
            }

            set
            {
                this.container.Values["cabinetPath"] = value;
            }
        }
    }
}
