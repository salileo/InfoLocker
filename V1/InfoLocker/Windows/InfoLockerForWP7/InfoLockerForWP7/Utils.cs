using System;
using System.Windows;
using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace InfoLockerForWP7
{
    public class Error
    {
        public enum Type { Alert, Warning, Error, Critical }
        public static Boolean IsDisabled { get; set; }

        static public void Log(Type type, String message)
        {
            Log(type, message, null);
        }

        static public void Log(Type type, String message, Exception exp)
        {
            if (!IsDisabled)
            {
                Boolean debugMode = true;
                try
                {
                    debugMode = GlobalPreferences.Instance.DebugMode;
                }
                catch(Exception)
                {
                    debugMode = true;
                }

                String additionalData = String.Empty;
                if (exp != null)
                {
                    additionalData = "\n\nReason :\n";
                    additionalData += debugMode ? exp.ToString() : exp.Message;
                }

                if (type == Type.Alert)
                    MessageBox.Show(message + additionalData, "Alert", MessageBoxButton.OK);
                else if (type == Type.Warning)
                    MessageBox.Show(message + additionalData, "Warning", MessageBoxButton.OK);
                else if (type == Type.Error)
                    MessageBox.Show(message + additionalData, "Error", MessageBoxButton.OK);
                else if (type == Type.Critical)
                    Debug.Assert(false, message + additionalData);
            }
        }
    }

    public class Utils
    {
        static BitmapImage folder_bitmap = null;
        static BitmapImage note_bitmap = null;

        public static BitmapImage GetFolderIcon()
        {
            if (folder_bitmap == null)
            {
                String iconPath = "../icons/iconfolder.png";
                try
                {
                    if (!String.IsNullOrEmpty(iconPath))
                    {
                        folder_bitmap = new BitmapImage();
                        //folder_bitmap.BeginInit();
                        folder_bitmap.UriSource = new Uri(iconPath, UriKind.Relative);
                        //folder_bitmap.EndInit();
                    }
                }
                catch (Exception)
                {
                    folder_bitmap = null;
                }
            }

            return folder_bitmap;
        }

        public static BitmapImage GetNoteIcon()
        {
            if (note_bitmap == null)
            {
                String iconPath = "../icons/iconnote.png";
                try
                {
                    if (!String.IsNullOrEmpty(iconPath))
                    {
                        note_bitmap = new BitmapImage();
                        //note_bitmap.BeginInit();
                        note_bitmap.UriSource = new Uri(iconPath, UriKind.Relative);
                        //note_bitmap.EndInit();
                    }
                }
                catch (Exception)
                {
                    note_bitmap = null;
                }
            }

            return note_bitmap;
        }
    }
}
