using System;

namespace HgCo.WindowsLive.SkyDrive.Support
{
    /// <summary>
    /// Provides methods for handling URI.
    /// </summary>
    internal static class UriHelper
    {
        /// <summary>
        /// Gets an URI from an URL string. It also decodes the URL string.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>The URI.</returns>
        public static Uri GetUri(string url)
        {
            string urlFormatted = FormatUrl(url);
            if (!String.IsNullOrEmpty(urlFormatted))
                return new Uri(urlFormatted);
            else return null;
        }

        /// <summary>
        /// Formats (decodes) an URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>The formatted URL.</returns>
        public static string FormatUrl(string url)
        {
            string urlFormatted = HtmlDocumentHelper.DecodeUnicodeString(url);
            if (!String.IsNullOrEmpty(urlFormatted))
                return urlFormatted;
            else return null;
        }
    }
}
