using System;
using System.Linq;
using System.Windows.Media.Imaging;

namespace InfoLocker
{
    public class Utils
    {
        static BitmapImage folder_bitmap = null;
        static BitmapImage note_bitmap = null;

        public static BitmapImage GetFolderIcon()
        {
            if (folder_bitmap == null)
            {
                string iconPath = "../icons/iconfolder.png";
                try
                {
                    if (!string.IsNullOrEmpty(iconPath))
                    {
                        folder_bitmap = new BitmapImage();
                        folder_bitmap.BeginInit();
                        folder_bitmap.UriSource = new Uri(iconPath, UriKind.Relative);
                        folder_bitmap.EndInit();
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
                string iconPath = "../icons/iconnote.png";
                try
                {
                    if (!string.IsNullOrEmpty(iconPath))
                    {
                        note_bitmap = new BitmapImage();
                        note_bitmap.BeginInit();
                        note_bitmap.UriSource = new Uri(iconPath, UriKind.Relative);
                        note_bitmap.EndInit();
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
