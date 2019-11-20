using System;

namespace HgCo.WindowsLive.SkyDrive.Support
{
    /// <summary>
    /// Provides methods for handling <see cref="WebFolderInfo"/> specific properties and tasks.
    /// </summary>
    internal static class WebFolderHelper
    {
        /// <summary>
        /// Parses the ContentType from a string.
        /// </summary>
        /// <param name="contentTypeText">The ContentType text.</param>
        /// <returns>The parsed ContentType.</returns>
        public static WebFolderContentType ParseContentType(string contentTypeText)
        {
            WebFolderContentType contentType = WebFolderContentType.None;
            if (!String.IsNullOrEmpty(contentTypeText))
            {
                contentTypeText = contentTypeText.ToUpperInvariant();
                if (contentTypeText.Contains("DOCUMENTS"))
                    contentType = WebFolderContentType.Documents;
                else if (contentTypeText.Contains("FAVORITES"))
                    contentType = WebFolderContentType.Favorites;
                else if (contentTypeText.Contains("PHOTOS"))
                    contentType = WebFolderContentType.Photos;
            }
            return contentType;
        }

        /// <summary>
        /// Parses the ViewType from a string.
        /// </summary>
        /// <param name="viewTypeText">The ViewType text.</param>
        /// <returns>The parsed ViewType.</returns>
        public static WebFolderViewType ParseViewType(string viewTypeText)
        {
            WebFolderViewType viewType = WebFolderViewType.None;
            if (!String.IsNullOrEmpty(viewTypeText))
            {
                viewTypeText = viewTypeText.ToUpperInvariant();
                if (viewTypeText.Contains("DETAILS"))
                    viewType = WebFolderViewType.Details;
                else if (viewTypeText.Contains("ICONS"))
                    viewType = WebFolderViewType.Icons;
                else if (viewTypeText.Contains("THUMBNAILS"))
                    viewType = WebFolderViewType.Thumbnails;
            }
            return viewType;
        }

        /// <summary>
        /// Gets a string representing the specified ViewType for operation postback.
        /// </summary>
        /// <param name="viewType">The ViewType.</param>
        /// <returns>The ViewType string.</returns>
        public static string GetViewTypeForPostback(WebFolderViewType viewType)
        {
            string viewTypeText = String.Empty;
            switch(viewType)
            {
                case WebFolderViewType.Thumbnails:
                    viewTypeText = "Thumbs";
                    break;
                default:
                    viewTypeText = viewType.ToString();
                    break;
            }
            return viewTypeText;
        }
    }
}
