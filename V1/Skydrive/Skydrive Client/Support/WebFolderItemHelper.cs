using System;
using System.Text.RegularExpressions;
using System.Web;

namespace HgCo.WindowsLive.SkyDrive.Support
{
    /// <summary>
    /// Provides methods for handling <see cref="WebFolderItemInfo"/> specific properties and tasks.
    /// </summary>
    internal static class WebFolderItemHelper
    {
        #region Fields
        
        /// <summary>
        /// The regular expression to parse a webfolderitem's path URL from HTML.
        /// </summary>
        private static readonly Regex RegexPathUrl = new Regex("(?i:https?://[^/]+/\\w+.aspx(?<Path>[^?]+))");

        /// <summary>
        /// The URL encoded representation of '+'.
        /// </summary>
        private static readonly string UrlEncodedPlusCharacter = HttpUtility.UrlEncode("+");

        #endregion

        #region Methods

        /// <summary>
        /// Parses the ShareType from a string.
        /// </summary>
        /// <param name="sharedWith">The ShareType text.</param>
        /// <returns>The parsed ShareType.</returns>
        public static WebFolderItemShareType ParseShareType(string sharedWith)
        {
            WebFolderItemShareType shareType = WebFolderItemShareType.None;
            if (!String.IsNullOrEmpty(sharedWith))
            {
                if (sharedWith.Equals("everyone (public)", StringComparison.OrdinalIgnoreCase))
                    shareType = WebFolderItemShareType.Public;
                else if (sharedWith.Equals("my network", StringComparison.OrdinalIgnoreCase))
                    shareType = WebFolderItemShareType.MyNetwork;
                else if (sharedWith.Equals("people i selected", StringComparison.OrdinalIgnoreCase))
                    shareType = WebFolderItemShareType.PeopleSelected;
                else if (sharedWith.Equals("just me", StringComparison.OrdinalIgnoreCase))
                    shareType = WebFolderItemShareType.Private;
            }
            return shareType;
        }
        
        /// <summary>
        /// Parses the PathUrl from an URL.
        /// </summary>
        /// <param name="url">The URL to be parsed.</param>
        /// <returns>The parsed PathUrl.</returns>
        public static string ParsePathUrl(string url)
        {
            string pathUrl = null;
            if (!String.IsNullOrEmpty(url))
            {
                string urlDecoded = HtmlDocumentHelper.DecodeUnicodeString(url);
                // Need to replace '+' to its URL encoded representation, 
                // otherwise UrlDecode method would decode it to ' '
                urlDecoded = urlDecoded.Replace("+", UrlEncodedPlusCharacter);
                urlDecoded = HttpUtility.UrlDecode(urlDecoded);
                if (RegexHelper.IsMatch(RegexPathUrl, urlDecoded))
                {
                    Match matchPathUrl = RegexHelper.Match(RegexPathUrl, urlDecoded);
                    pathUrl = matchPathUrl.Groups["Path"].Value.Replace("|", String.Empty);
                }
                else
                {
                    // In case of some folders (it seems which contain more unicode characters),
                    // path can be given in the query parameter "path"
                    var uriDecoded = new Uri(urlDecoded);
                    var queryParameters = HttpUtility.ParseQueryString(uriDecoded.Query);
                    var pathQueryParameter = queryParameters["path"];
                    if (!String.IsNullOrEmpty(pathQueryParameter))
                    {
                        pathUrl = pathQueryParameter;
                    }
                }
            }
            return pathUrl;
        }

        #endregion
    }
}
