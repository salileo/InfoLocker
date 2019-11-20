using System;
using System.Linq;
using System.Diagnostics;
using System.Windows;

namespace InfoLocker
{
    public class Logger
    {
        public enum Type { Alert, Warning, Error, Critical }
        public static bool IsEnabled = true;

        static public void Log(Type type, string message)
        {
            Log(type, message, null);
        }

        static public void Log(Type type, string message, Exception exp)
        {
            if (IsEnabled)
            {
                bool debugMode = true;
                try
                {
                    debugMode = GlobalPreferences.Instance.DebugMode;
                }
                catch (Exception)
                {
                    debugMode = true;
                }

                string additionalData = string.Empty;
                if (exp != null)
                {
                    additionalData = "\n\nReason :\n";
                    additionalData += debugMode ? exp.ToString() : exp.Message;
                }

                if (type == Type.Alert)
                    MessageBox.Show(message + additionalData, "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
                else if (type == Type.Warning)
                    MessageBox.Show(message + additionalData, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                else if (type == Type.Error)
                    MessageBox.Show(message + additionalData, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else if (type == Type.Critical)
                    Debug.Assert(false, message + additionalData);
            }
        }
    }
}
