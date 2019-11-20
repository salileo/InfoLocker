using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using HgCo.WindowsLive.SkyDrive.Support;
using HtmlAgilityPack;

namespace HgCo.WindowsLive.SkyDrive
{
    /// <summary>
    /// Provides methods (API) for interacting with SkyDrive online storage.
    /// </summary>
    public class SkyDriveWebClient
    {
        #region Constants and Fields

        /// <summary>
        /// The regular expression to parse a user name.
        /// </summary>
        private static readonly Regex RegexUserName = new Regex("(?<Id>[^@]+)@(?<Domain>.+)");

        /// <summary>
        /// The regular expression to parse a postback URL from JavaScript to log on.
        /// </summary>
        private static readonly Regex RegexLogOnPostBackUrl = new Regex("var\\s+srf_uPost\\s*=\\s*'(?<URL>[^']+)';");

        /// <summary>
        /// The regular expression to parse a preload URL from JavaScript to log on.
        /// </summary>
        private static readonly Regex RegexPreloadUrl = new Regex("var\\s+srf_uPreload\\s*=\\s*'(?<URL>[^']+)';");

        /// <summary>
        /// The regular expression to parse a blob from JavaScript to log on.
        /// </summary>
        private static readonly Regex RegexPPSXBlob = new Regex("var\\s+srf_sRBlob\\s*=\\s*'(?<BLOB>[^']+)';");

        /// <summary>
        /// The regular expression to parse a webfolderitem's title attribute from HTML.
        /// </summary>
        private static readonly Regex RegexWebFolderItemTitle = new Regex("(?i:(?<Name>[^\n]+)(\nShared with:\\s*(?<SharedWith>[^\n]+))?(\nDate created:\\s*(?<DateAdded>[^\n]+))?(\nDate modified:\\s*(?<DateModified>[^\n]+))?(\nType:\\s*(?<Type>[^\n]+))?(\nSize:\\s*(?<Size>[^\n]+))?(\n(?<Description>.*))?)");

        /// <summary>
        /// The regular expression to parse a webfile's download URL from JavaScript.
        /// </summary>
        private static readonly Regex RegexWebFileDownloadUrl = new Regex("downloadUrl\\s*:\\s*'(?<URL>[^']+)'");

        /// <summary>
        /// The regular expression to parse a webfolderitem's name from HTML TITLE tag.
        /// </summary>
        private static readonly Regex RegexWebFolderItemName = new Regex("(?<Name>[^\\-]+)");

        /// <summary>
        /// The regular expression to parse a webfolderitem's description from JavaScript.
        /// </summary>
        private static readonly Regex RegexWebFolderItemDescription = new Regex("caption:\\s*'(?<Description>[^']*)'");

        /// <summary>
        /// The regular expression to parse a webfolderitemimage's class attribute.
        /// </summary>
        private static readonly Regex RegexWebFolderItemImageClass = new Regex("(?<LocationX>\\-?\\d+)_(?<LocationY>\\-?\\d+)_(?<Width>\\d+)_(?<Height>\\d+)");

        ///// <summary>
        ///// The URI of SkyDrive Live page.
        ///// </summary>
        //private static readonly Uri SkyDriveLiveUri = new Uri("http://skydrive.live.com");
        
        /// <summary>
        /// The URI of SkyDrive log on page.
        /// </summary>
        private static readonly Uri SkyDriveLogOnUri = new Uri("https://login.live.com/login.srf?wa=wsignin1.0&rpsnv=11&ct=1297063040&rver=6.1.6206.0&wp=MBI&wreply=http:%2F%2Fskydrive.live.com%2Fhome.aspx&lc=1033&id=250206&cbcxt=sky");

        /// <summary>
        /// The Cache used to accelerate web communication to SkyDrive.
        /// </summary>
        private static readonly WebCache Cache = new WebCache();

        /// <summary>
        /// The HttpWebClient used to communicate to SkyDrive's web server.
        /// </summary>
        private readonly HttpWebClient webClient;
        
        #endregion

        #region Delegates

        /// <summary>
        /// Represents the method that handles asynchronous calls.
        /// </summary>
        private delegate void AsyncDelegate();

        /// <summary>
        /// Represents the method that handles asynchronous calls.
        /// </summary>
        private delegate void AsyncDelegate<T>(T param);

        /// <summary>
        /// Represents the method that handles asynchronous calls.
        /// </summary>
        private delegate void AsyncDelegate<T1, T2>(T1 param1, T2 param2);

        /// <summary>
        /// Represents the method that handles asynchronous calls.
        /// </summary>
        private delegate void AsyncDelegate<T1, T2, T3>(T1 param1, T2 param2, T3 param3);

        /// <summary>
        /// Represents the method that handles asynchronous calls.
        /// </summary>
        private delegate void AsyncDelegate<T1, T2, T3, T4>(T1 param1, T2 param2, T3 param3, T4 param4);

        /// <summary>
        /// Represents the method that handles asynchronous calls.
        /// </summary>
        private delegate R AsyncReturnDelegate<R>();

        /// <summary>
        /// Represents the method that handles asynchronous calls.
        /// </summary>
        private delegate R AsyncReturnDelegate<R, T>(T param);

        /// <summary>
        /// Represents the method that handles asynchronous calls.
        /// </summary>
        private delegate R AsyncReturnDelegate<R, T1, T2>(T1 param1, T2 param2);

        /// <summary>
        /// Represents the method that handles asynchronous calls.
        /// </summary>
        private delegate R AsyncReturnDelegate<R, T1, T2, T3>(T1 param1, T2 param2, T3 param3);

        /// <summary>
        /// Represents the method that handles asynchronous calls.
        /// </summary>
        private delegate R AsyncReturnDelegate<R, T1, T2, T3, T4>(T1 param1, T2 param2, T3 param3, T4 param4);

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a webfile upload operation successfully transfers some or all of the data.
        /// </summary>
        /// <remarks>
        /// This event is raised each time a webfile upload make progress.
        /// This event is raised when uploads are started by calling UploadWebFile(string, WebFolderInfo) method.
        /// </remarks>
        public event EventHandler<UploadWebFileProgressChangedEventArgs> UploadWebFileProgressChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the session used for SkyDrive communication.
        /// </summary>
        /// <value>The session.</value>
        public WebSession Session { get { return webClient.Session; } }

        /// <summary>
        /// Gets or sets the time-out value in milliseconds.
        /// </summary>
        /// <value>The number of milliseconds to wait before a request times out. The default is 100,000 milliseconds (100 seconds).</value>
        public int Timeout 
        {
            get { return webClient.Timeout; }
            set { webClient.Timeout = value; } 
        }

        /// <summary>
        /// Gets or sets the proxy information used for SkyDrive communication.
        /// </summary>
        /// <value>The <see cref="IWebProxy"/> object to use to proxy the SkyDrive communication.</value>
        public IWebProxy Proxy
        {
            get { return webClient.Proxy; }
            set { webClient.Proxy = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SkyDriveWebClient"/> class.
        /// </summary>
        public SkyDriveWebClient() : this(null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SkyDriveWebClient"/> class.
        /// </summary>
        /// <param name="session">The session to use for SkyDrive communication.</param>
        public SkyDriveWebClient(WebSession session)
        {
            webClient = session != null ? new HttpWebClient(session) : new HttpWebClient();
            webClient.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 5.1; Trident/4.0; .NET CLR 2.0.50727; .NET CLR 1.1.4322; .NET CLR 3.0.04506.648; .NET CLR 3.5.21022; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729)";
            //webClient.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 2.0.50727; .NET CLR 3.0.04506.30; InfoPath.2; .NET CLR 3.0.04506.648; .NET CLR 3.5.21022; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729; .NET CLR 1.1.4322)";
            //webClient.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; .NET CLR 2.0.50727; .NET CLR 3.0.04506.30; InfoPath.2; .NET CLR 3.0.04506.648; .NET CLR 3.5.21022; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729; .NET CLR 1.1.4322)";
            //webClient.UserAgent = "Opera/9.64 (Windows NT 5.1; U; en) Presto/2.1.1";
            webClient.Timeout = 100000;
            
            // This lines of code remove form element from ElementsFalgs dictionary in order to:
            // - do not allow overlapping forms, and
            // - let form have child nodes!!
            if (HtmlNode.ElementsFlags.ContainsKey("form"))
                HtmlNode.ElementsFlags.Remove("form");
        }

        #endregion

        #region Public Methods

        #region Synchronous Methods

        #region Session Related Methods

        /// <summary>
        /// Logs on to a specified user account.
        /// </summary>
        /// <param name="userName">The name of the user.</param>
        /// <param name="userPassword">The user password.</param>
        public void LogOn(string userName, string userPassword)
        {
            Match matchUserName = RegexHelper.Match(RegexUserName, userName);
            string responseString = null;
            HtmlDocument responseDocument = new HtmlDocument();
            NameValueCollection parameters = new NameValueCollection();
            Uri uriRefresh = null;

            try
            {
                string finalURL = string.Empty;

                // Request-Response No. 1
                responseString = webClient.DownloadString(SkyDriveLogOnUri);

                NameValueCollection tagAttributesPPFT = HtmlDocumentHelper.GetTagAttributes(
                    HtmlDocumentHelper.GetTagByName(responseString, "PPFT"));

                Match matchPPSXBlob = RegexHelper.Match(RegexPPSXBlob, responseString);
                string ppsxBlob = matchPPSXBlob.Groups["BLOB"].Value;

                Match matchPreloadUrl = RegexHelper.Match(RegexPreloadUrl, responseString);
                Uri uriPreload = UriHelper.GetUri(HtmlDocumentHelper.DecodeJavascriptString(
                    matchPreloadUrl.Groups["URL"].Value));

                Match matchLogOnPostBackUrl = RegexHelper.Match(RegexLogOnPostBackUrl, responseString);
                Uri uriLogOnPostBack = UriHelper.GetUri(HtmlDocumentHelper.DecodeJavascriptString(
                    matchLogOnPostBackUrl.Groups["URL"].Value));

                // Request-Response No. 2
                responseString = webClient.DownloadString(uriPreload, SkyDriveLogOnUri.OriginalString, ref finalURL);

                //Session.AddCookie(
                //    "idsbho",
                //    "7.250.4225.0$7.250.4225.0$9.0.8027.6000$6.1.0.0.7600()",
                //    "login.live.com");
                Session.AddCookie(
                    "CkTst",
                    String.Format(CultureInfo.InvariantCulture, "G{0}000", UnixDateTimeHelper.Parse(DateTime.UtcNow)),
                    "login.live.com");
                Session.AddCookie(
                    "WLOpt",
                    "nrme=1",
                    "login.live.com");
                Session.AddCookie(
                    "wlidperf",
                    String.Format(CultureInfo.InvariantCulture, "throughput=29&latency=100&FR=L&ST={0}120", UnixDateTimeHelper.Parse(DateTime.UtcNow)),
                    ".live.com");
                Session.AddCookie(
                    "wl_preperf",
                    "req=17&com=17&cache=3",
                    ".live.com");

                parameters["login"] = userName;
                parameters["passwd"] = userPassword;
                parameters["type"] = "11";
                parameters["LoginOptions"] = "3";
                parameters["NewUser"] = "1";
                parameters["MEST"] = "";
                parameters["PPSX"] = ppsxBlob;
                parameters["PPFT"] = tagAttributesPPFT["value"];
                parameters["idsbho"] = "1";
                parameters["PwdPad"] = "";
                parameters["SSO"] = "";
                parameters["i1"] = "";
                parameters["i2"] = "1";
                parameters["i3"] = "35312";
                parameters["i4"] = "";
                parameters["i12"] = "1";

                // Request-Response No. 3
                responseString = webClient.UploadValuesUrlEncoded(uriLogOnPostBack, parameters, SkyDriveLogOnUri.OriginalString);
                uriRefresh = HtmlDocumentHelper.GetMetaTagRefreshUri(responseString);

                // Request-Response No. 4
                responseString = webClient.DownloadString(uriRefresh, ref finalURL);

                responseDocument.LoadHtml(responseString);
                HtmlNode nodeForm = responseDocument.GetElementbyId("fmHF");
                Uri uriFormAction = UriHelper.GetUri(nodeForm.Attributes["action"].Value);
                parameters = ParseFormPostBackParameters(responseDocument, nodeForm.Attributes["name"].Value);

                // Request-Response No. 5
                responseString = webClient.UploadValuesUrlEncoded(uriFormAction, parameters, finalURL);
            }
            catch (Exception ex)
            {
                throw new LogOnFailedException(ex.Message, ex);
            }
        }
        
        #endregion

        #region WebDrive Related Methods

        /// <summary>
        /// Gets the SkyDrive storage info.
        /// </summary>
        /// <returns>The SkyDrive storage info.</returns>
        public WebDriveInfo GetWebDriveInfo()
        {
            WebDriveInfo webDriveInfo = null;

            try
            {
                Uri uriRootWebFolderBrowse = GetRootWebFolderBrowseUri(Session);
                string responseString = webClient.DownloadString(uriRootWebFolderBrowse);

                HtmlDocument responseDocument = new HtmlDocument();
                responseDocument.LoadHtml(responseString);

                HtmlNode nodeDiskUsedImage = responseDocument.GetElementbyId("fillingImg");
                HtmlNode nodeDiskFreeImage = responseDocument.GetElementbyId("remainingImg");

                webDriveInfo = new WebDriveInfo
                {
                    Cid = Session.Cid,
                    UsedDiskSpace = nodeDiskUsedImage != null ?
                        HtmlDocumentHelper.DecodeUnicodeString(nodeDiskUsedImage.Attributes["title"].Value)
                            .Replace("used", String.Empty).Trim() : String.Empty,
                    FreeDiskSpace = nodeDiskFreeImage != null ? 
                        HtmlDocumentHelper.DecodeUnicodeString(nodeDiskFreeImage.Attributes["title"].Value)
                            .Replace("available", String.Empty).Trim() : String.Empty
                };
            }
            catch (Exception ex)
            {
                throw new OperationFailedException(ex.Message, ex);
            }

            return webDriveInfo;
        }
        #endregion

        #region WebFolderItem Related Methods

        /// <summary>
        /// Lists webfolderitems located in SkyDrive's root.
        /// </summary>
        /// <returns>The list of webfolderitems in SkyDrive's root.</returns>
        public WebFolderItemInfo[] ListRootWebFolderItems()
        {
            List<WebFolderItemInfo> lWebFolderItem = new List<WebFolderItemInfo>();

            try
            {
                Uri uriRootWebFolderBrowse = GetRootWebFolderBrowseUri(Session);
                string responseString = webClient.DownloadString(uriRootWebFolderBrowse);

                HtmlDocument responseDocument = new HtmlDocument();
                responseDocument.LoadHtml(responseString);

                HtmlNodeCollection nodeItemGroups = responseDocument.DocumentNode
                    .SelectNodes("//div[contains(@class, 'tvContainer')]");
                foreach (HtmlNode nodeItemGroup in nodeItemGroups)
                {
                    HtmlNodeCollection nodeItems = nodeItemGroup
                        .SelectNodes(".//div[contains(@class, 'tvItemContainer')]");
                    if (nodeItems != null)
                        foreach (HtmlNode nodeItem in nodeItems)
                        {
                            HtmlNode nodeItemLink = nodeItem.SelectSingleNode(".//a[@class='tvLink']");
                            Match matchWebFolderItemTitle = RegexHelper.Match(RegexWebFolderItemTitle, 
                                HtmlDocumentHelper.DecodeUnicodeString(nodeItemLink.Attributes["title"].Value));
                            HtmlNodeCollection nodeItemImages = nodeItem.SelectNodes(".//div[@class='tvItemSpriteWrapper']//img");

                            WebFolderInfo webFolder = new WebFolderInfo
                            {
                                Name = HtmlDocumentHelper.DecodeUnicodeString(nodeItemLink.InnerText),
                                Description = matchWebFolderItemTitle.Groups["Description"].Value,
                                ShareType = WebFolderItemHelper.ParseShareType(
                                    matchWebFolderItemTitle.Groups["SharedWith"].Value),
                                DateAdded = !String.IsNullOrEmpty(matchWebFolderItemTitle.Groups["DateAdded"].Value) ?
                                    DateTime.Parse(
                                        HtmlDocumentHelper.DecodeUnicodeString(matchWebFolderItemTitle.Groups["DateAdded"].Value),
                                        CultureInfo.InvariantCulture) : (DateTime?)null,
                                DateModified = !String.IsNullOrEmpty(matchWebFolderItemTitle.Groups["DateModified"].Value) ?
                                    DateTime.Parse(
                                        HtmlDocumentHelper.DecodeUnicodeString(matchWebFolderItemTitle.Groups["DateModified"].Value),
                                        CultureInfo.InvariantCulture) : (DateTime?)null,
                                PathUrl = WebFolderItemHelper.ParsePathUrl(nodeItemLink.Attributes["href"].Value),

                                ContentType = WebFolderHelper.ParseContentType(nodeItemGroup.Id)
                            };
                            webFolder.ViewUrl = GetWebFolderItemViewUrl(Session, webFolder);

                            if (nodeItemImages != null && nodeItemImages.Count > 1)
                            {
                                webFolder.WebIcon = ParseWebFolderItemIcon(
                                    nodeItemImages[0],
                                    nodeItemImages.Count > 2 ? nodeItemImages[1] : null,
                                    nodeItemImages[nodeItemImages.Count - 1]);
                                webFolder.WebIcon.ContentWebImageOffsetX = 0;
                                webFolder.WebIcon.ContentWebImageOffsetY = 8;
                                webFolder.WebIcon.ShareTypeWebImageOffsetX = 32;
                                webFolder.WebIcon.ShareTypeWebImageOffsetY = 32;
                            }

                            lWebFolderItem.Add(webFolder);
                        }
                }
            }
            catch (Exception ex)
            {
                throw new OperationFailedException(ex.Message, ex);
            }

            return lWebFolderItem.ToArray();
        }

        /// <summary>
        /// Lists webfolderitems located in a sub webfolder.
        /// </summary>
        /// <param name="webFolderParent">The webfolder of which webfolderitems are to be listed.</param>
        /// <returns>The list of webfolderitems in the sub webfolder.</returns>
        public WebFolderItemInfo[] ListSubWebFolderItems(WebFolderInfo webFolderParent)
        {
            return ListSubWebFolderItems(webFolderParent, WebFolderViewType.Details);
        }

        /// <summary>
        /// Lists webfolderitems located in a sub webfolder.
        /// </summary>
        /// <param name="webFolderParent">The webfolder which webfolderitems are to be listed.</param>
        /// <param name="viewType">The ViewType how webfolderitems are desired to be listed.</param>
        /// <returns>The list of webfolderitems in the sub webfolder.</returns>
        /// <remarks>The selected ViewType highly affects the available information of a webfolderitem.</remarks>
        public WebFolderItemInfo[] ListSubWebFolderItems(WebFolderInfo webFolderParent, WebFolderViewType viewType)
        {
            if (webFolderParent == null)
                throw new ArgumentNullException("webFolderParent");

            WebFolderItemInfo[] webFolderItems = new WebFolderItemInfo[0];

            try
            {
                Uri uriSubWebFolderBrowse = GetSubWebFolderBrowseUri(Session, webFolderParent);
                string responseString = webClient.DownloadString(uriSubWebFolderBrowse);

                HtmlDocument responseDocument = new HtmlDocument();
                responseDocument.LoadHtml(responseString);

                HtmlNode nodeViewTypeOriginal = responseDocument.GetElementbyId("browseViewMenu");
                WebFolderViewType viewTypeOriginal =
                    WebFolderHelper.ParseViewType(nodeViewTypeOriginal.InnerText);

                // Change ViewType of parent webfolder as requested.
                if (viewTypeOriginal != viewType)
                {
                    NameValueCollection parameters = ParseFormPostBackParameters(responseDocument, "aspnetForm");
                    parameters["postVerb"] = "Update";
                    parameters["postVerbData"] = String.Format(
                        CultureInfo.InvariantCulture,
                        "{0}:Alpha",
                        WebFolderHelper.GetViewTypeForPostback(viewType));
                    webClient.UploadValuesUrlEncoded(uriSubWebFolderBrowse, parameters, false);

                    responseString = webClient.DownloadString(uriSubWebFolderBrowse);
                    responseDocument.LoadHtml(responseString);

                    HtmlNode nodeViewType = responseDocument.GetElementbyId("browseViewMenu");
                    viewType = WebFolderHelper.ParseViewType(nodeViewType.InnerText);
                }

                switch (viewType)
                {
                    case WebFolderViewType.Details:
                        webFolderItems = ParseSubWebFolderViewDetails(webFolderParent, responseDocument);
                        break;
                    case WebFolderViewType.Icons:
                        webFolderItems = ParseSubWebFolderViewIcons(webFolderParent, responseDocument);
                        break;
                    case WebFolderViewType.Thumbnails:
                        webFolderItems = ParseSubWebFolderViewThumbnails(webFolderParent, responseDocument);
                        break;
                    default:
                        throw new NotSupportedException(viewType.ToString());
                }

                // Change back ViewType of parent webfolder to original.
                if (viewTypeOriginal != viewType)
                {
                    NameValueCollection parameters = ParseFormPostBackParameters(responseDocument, "aspnetForm");
                    parameters["postVerb"] = "Update";
                    parameters["postVerbData"] = String.Format(
                        CultureInfo.InvariantCulture, 
                        "{0}:Alpha", 
                        WebFolderHelper.GetViewTypeForPostback(viewTypeOriginal));
                    webClient.UploadValuesUrlEncoded(uriSubWebFolderBrowse, parameters, false);
                }
            }
            catch (Exception ex)
            {
                throw new OperationFailedException(ex.Message, ex);
            }

            return webFolderItems;
        }

        /// <summary>
        /// Gets a webfolderitem with all data.
        /// </summary>
        /// <param name="webFolderItem">The webfolderitem.</param>
        /// <returns>The webfolderitem with all data.</returns>
        public WebFolderItemInfo GetWebFolderItem(WebFolderItemInfo webFolderItem)
        {
            try
            {
                Uri uriWebFolderItemView = GetWebFolderItemViewUri(Session, webFolderItem);
                string responseString = webClient.DownloadString(uriWebFolderItemView);

                HtmlDocument responseDocument = new HtmlDocument();
                responseDocument.LoadHtml(responseString);

                HtmlNode nodeName = responseDocument.GetElementbyId("c_PageTitle");
                var webItemName = HttpUtility.HtmlDecode(nodeName.InnerText).TrimEnd();
                if (webFolderItem.ItemType == WebFolderItemType.File)
                {
                    WebFileInfo webFile = (WebFileInfo)webFolderItem;
                    webFile.Name = System.IO.Path.GetFileNameWithoutExtension(webItemName);
                    webFile.Extension = System.IO.Path.GetExtension(webItemName);
                    if (String.IsNullOrEmpty(webFile.Extension))
                        webFile.Extension = System.IO.Path.GetExtension(webFile.PathUrl);
                }
                else webFolderItem.Name = webItemName;
                
                if (RegexHelper.IsMatch(RegexWebFolderItemDescription, responseString))
                    webFolderItem.Description = HtmlDocumentHelper.DecodeJavascriptString(RegexHelper.Match(RegexWebFolderItemDescription, responseString)
                        .Groups["Description"].Value);

                HtmlNode nodeImageContentType = responseDocument.GetElementbyId("spPreviewImage");
                HtmlNode nodeImageContent = webFolderItem.ItemType == WebFolderItemType.File ? 
                    responseDocument.GetElementbyId("spPicturePreview") : 
                    responseDocument.GetElementbyId("spPreviewWrapper")
                        .SelectSingleNode(".//img[@class='spPreviewCoverImage']");
                HtmlNode nodeImageShareType = responseDocument.GetElementbyId("spPreviewWrapper")
                    .SelectSingleNode(".//div[contains(@class, 'spPreviewSecIcon')]//img");

                HtmlNode nodeInformation = responseDocument.GetElementbyId("spProperties")
                    .SelectSingleNode("./table");
                HtmlNodeCollection nodeProperties = nodeInformation.SelectNodes("./tr");

                foreach (HtmlNode nodeProperty in nodeProperties)
                {
                    HtmlNode nodePropertyName = nodeProperty
                        .SelectSingleNode("./td[@class='spLabel']//div[@class='spLabelDiv']");
                    HtmlNode nodePropertyValue = nodeProperty
                        .SelectSingleNode("./td[@class='spValue']//span");

                    switch (HtmlDocumentHelper.DecodeUnicodeString(nodePropertyName.InnerText.ToUpperInvariant()))
                    {
                        case "ADDED BY:":
                            HtmlNode nodeCreator = nodePropertyValue.SelectSingleNode(".//a");
                            webFolderItem.CreatorName = HtmlDocumentHelper.DecodeUnicodeString(nodeCreator.InnerText);
                            webFolderItem.CreatorUrl = UriHelper.FormatUrl(nodeCreator.Attributes["href"].Value);
                            break;
                        case "SHARED WITH:":
                            webFolderItem.ShareType = WebFolderItemHelper.ParseShareType(
                                HtmlDocumentHelper.DecodeUnicodeString(nodePropertyValue.InnerText));
                            break;
                        case "TYPE:":
                            switch (webFolderItem.ItemType)
                            {
                                case WebFolderItemType.File:
                                    ((WebFileInfo)webFolderItem).ContentType = HtmlDocumentHelper.DecodeUnicodeString(nodePropertyValue.InnerText);
                                    break;
                                case WebFolderItemType.Folder:
                                    ((WebFolderInfo)webFolderItem).ContentType = WebFolderHelper.ParseContentType(
                                        HtmlDocumentHelper.DecodeUnicodeString(nodePropertyValue.InnerText));
                                    break;
                            }
                            break;
                        case "SIZE:":
                            webFolderItem.Size = HtmlDocumentHelper.DecodeUnicodeString(nodePropertyValue.InnerText);
                            break;
                        case "DATE ADDED:":
                            webFolderItem.DateAdded = DateTime.Parse(
                                HtmlDocumentHelper.DecodeUnicodeString(nodePropertyValue.InnerText),
                                CultureInfo.InvariantCulture);
                            break;
                        case "DATE MODIFIED:":
                            webFolderItem.DateModified = DateTime.Parse(
                                HtmlDocumentHelper.DecodeUnicodeString(nodePropertyValue.InnerText),
                                CultureInfo.InvariantCulture);
                            break;
                    }
                }

                webFolderItem.ViewUrl = GetWebFolderItemViewUrl(Session, webFolderItem);
                webFolderItem.DownloadUrl = ParseWebFolderItemDownloadUrl(responseString, webFolderItem);

                webFolderItem.WebIcon = ParseWebFolderItemIcon(
                    nodeImageContentType,
                    nodeImageContent,
                    nodeImageShareType);
                if (webFolderItem.WebIcon != null)
                {
                    if (webFolderItem.WebIcon.ContentWebImage != null)
                    {
                        webFolderItem.WebIcon.ContentWebImageOffsetX = 2;
                        webFolderItem.WebIcon.ContentWebImageOffsetY = 8;
                    }
                    if (webFolderItem.ItemType == WebFolderItemType.Folder &&
                        ((WebFolderInfo)webFolderItem).ContentType == WebFolderContentType.Photos)
                    {
                        webFolderItem.WebIcon.ShareTypeWebImageOffsetX = 104;
                        webFolderItem.WebIcon.ShareTypeWebImageOffsetY = 99;
                    }
                    else
                    {
                        webFolderItem.WebIcon.ShareTypeWebImageOffsetX = 55;
                        webFolderItem.WebIcon.ShareTypeWebImageOffsetY = 65;
                    }
                }

                WebFavoriteInfo webFavorite = webFolderItem as WebFavoriteInfo;
                if (webFavorite != null)
                {
                    HtmlNode nodeWebAddress = responseDocument.DocumentNode
                        .SelectSingleNode("//div[@class='spFavoriteUrl']//a");
                    if (nodeWebAddress != null)
                        webFavorite.WebAddress = UriHelper.FormatUrl(
                            nodeWebAddress.Attributes["href"].Value);
                }
            }
            catch (Exception ex)
            {
                throw new OperationFailedException(ex.Message, ex);
            }
            
            return webFolderItem;
        }

        /// <summary>
        /// Determines whether the specified webfolderitem exists.
        /// </summary>
        /// <param name="webFolderItem">The webfolderitem to look for.</param>
        /// <returns><c>true</c> if the specified webfolderitem exists; otherwise, <c>false</c>.</returns>
        public bool IsWebFolderItemExists(WebFolderItemInfo webFolderItem)
        {
            bool isExists = false;
            
            try
            {
                Uri uriWebFolderItemView = GetWebFolderItemViewUri(Session, webFolderItem);
                webClient.DownloadString(uriWebFolderItemView);
                
                isExists = true;
            }
            catch (WebException ex)
            {
                if (((HttpWebResponse)ex.Response).StatusCode != HttpStatusCode.NotFound)
                    throw new OperationFailedException(ex.Message, ex);
            }

            return isExists;
        }

        /// <summary>
        /// Renames a webfolderitem.
        /// </summary>
        /// <param name="webFolderItem">The webfolderitem to be renamed.</param>
        /// <param name="newName">The new name.</param>
        public void RenameWebFolderItem(WebFolderItemInfo webFolderItem, string newName)
        {
            Uri uriWebFolderItemRename = GetWebFolderItemRenameUri(Session, webFolderItem);
            
            try
            {
                string responseString = webClient.DownloadString(uriWebFolderItemRename);

                NameValueCollection parameters = ParseFormPostBackParameters(responseString, "aspnetForm");
                parameters["itemName"] = newName;

                webClient.UploadValuesUrlEncoded(uriWebFolderItemRename, parameters, false);
            }
            catch (Exception ex)
            {
                throw new OperationFailedException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Changes the description of a webfolderitem.
        /// </summary>
        /// <param name="webFolderItem">The webfolderitem to be changed.</param>
        /// <param name="newDescription">The new description.</param>
        public void ChangeWebFolderItemDescription(WebFolderItemInfo webFolderItem, string newDescription)
        {
            try
            {
                Uri uriWebFolderItemView = GetWebFolderItemViewUri(Session, webFolderItem);
                string responseString = webClient.DownloadString(uriWebFolderItemView);

                NameValueCollection parameters = ParseFormPostBackParameters(responseString, "aspnetForm");
                string canaryValue = parameters["canary"];
                parameters.Clear();

                parameters["actionVerb"] = "updateCaption";
                parameters["actionValue"] = newDescription;
                parameters["canary"] = canaryValue;

                Uri uriWebFolderItemChangeDescription = GetWebFolderItemChangeDescriptionUri(Session, webFolderItem);
                webClient.UploadValuesUrlEncoded(uriWebFolderItemChangeDescription, parameters, false);
            }
            catch (Exception ex)
            {
                throw new OperationFailedException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Moves a sub webfolderitem into the specified webfolder.
        /// </summary>
        /// <param name="webFolderItem">The sub webfolderitem to be moved.</param>
        /// <param name="webFolderDestination">The destination webfolder where webfolderitem is being moved.</param>
        public void MoveSubWebFolderItem(WebFolderItemInfo webFolderItem, WebFolderInfo webFolderDestination)
        {
            Uri uriWebFolderItemView = GetWebFolderItemViewUri(Session, webFolderItem);
            string responseString = webClient.DownloadString(uriWebFolderItemView);

            HtmlDocument responseDocument = new HtmlDocument();
            responseDocument.LoadHtml(responseString);

            HtmlNode nodeLinkMove = responseDocument.GetElementbyId("move");
            Uri uriWebFolderItemMoveCopyHome = UriHelper.GetUri(nodeLinkMove.Attributes["href"].Value);

            Uri uriWebFolderItemMoveCopy = GetWebFolderDestinationMoveCopyUri(Session, webFolderDestination, uriWebFolderItemMoveCopyHome);
            responseString = webClient.DownloadString(uriWebFolderItemMoveCopy);

            NameValueCollection parameters = ParseFormPostBackParameters(responseString, "aspnetForm");
            parameters["linkClicked"] = "true";

            webClient.UploadValuesUrlEncoded(uriWebFolderItemMoveCopy, parameters, false);
        }

        /// <summary>
        /// Deletes a webfolderitem.
        /// </summary>
        /// <param name="webFolderItem">The webfolderitem to be deleted.</param>
        public void DeleteWebFolderItem(WebFolderItemInfo webFolderItem)
        {
            try
            {
                Uri uriWebFolderItemView = GetWebFolderItemViewUri(Session, webFolderItem);
                string responseString = webClient.DownloadString(uriWebFolderItemView);

                NameValueCollection parameters = ParseFormPostBackParameters(responseString, "aspnetForm");
                parameters["postVerb"] = "deleteItem";
                parameters["postVerbData"] = String.Empty;

                webClient.UploadValuesUrlEncoded(uriWebFolderItemView, parameters, false);
            }
            catch (Exception ex)
            {
                throw new OperationFailedException(ex.Message, ex);
            }
        }

        #endregion

        #region WebFolderItemIcon Related Methods

        /// <summary>
        /// Downloads a webfolderitemicon as an Image.
        /// </summary>
        /// <param name="webFolderItemIcon">The webfolderitemicon to download.</param>
        /// <returns>The webfolderitemicon as an Image.</returns>
        /// <remarks>It's cached, in a short time period a given icon is downloaded only once from the server.</remarks>
        public Image DownloadWebFolderItemIcon(WebFolderItemIconInfo webFolderItemIcon)
        {
            Image imgWebFolderItemIcon = null;

            if (webFolderItemIcon != null)
                try
                {
                    Image imgContentType = null;
                    Image imgContent = null;
                    Image imgShareType = null;

                    if (webFolderItemIcon.ContentTypeWebImage != null)
                        imgContentType = DownloadWebFolderItemImage(webFolderItemIcon.ContentTypeWebImage);
                    if (webFolderItemIcon.ContentWebImage != null)
                        imgContent = DownloadWebFolderItemImage(webFolderItemIcon.ContentWebImage);
                    if (webFolderItemIcon.ShareTypeWebImage != null)
                        imgShareType = DownloadWebFolderItemImage(webFolderItemIcon.ShareTypeWebImage);

                    // Draw images on each other to get the icon.
                    if (imgContentType != null || imgContent != null)
                    {
                        Bitmap bmp = new Bitmap(
                            Math.Max(
                                imgContentType != null ? imgContentType.Width : 0,
                                imgContent != null ? imgContent.Width : 0),
                            Math.Max(
                                imgContentType != null ? imgContentType.Height : 0,
                                imgContent != null ? imgContent.Height : 0));
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            if (imgContentType != null)
                                g.DrawImage(imgContentType, 0, 0);
                            if (imgContent != null)
                                g.DrawImage(imgContent,
                                    webFolderItemIcon.ContentWebImageOffsetX ?? 0,
                                    webFolderItemIcon.ContentWebImageOffsetY ?? 0);
                            if (imgShareType != null)
                                g.DrawImage(
                                    imgShareType,
                                    webFolderItemIcon.ShareTypeWebImageOffsetX ?? 0,
                                    webFolderItemIcon.ShareTypeWebImageOffsetY ?? 0);
                        }
                        imgWebFolderItemIcon = bmp;
                    }
                }
                catch (Exception ex)
                {
                    throw new OperationFailedException(ex.Message, ex);
                }
            
            return imgWebFolderItemIcon;
        }

        /// <summary>
        /// Downloads a webfolderitemimage as an Image.
        /// </summary>
        /// <param name="webFolderItemImage">The webfolderitemimage to download.</param>
        /// <returns>The webfolderitemimage as an Image.</returns>
        /// <remarks>It's cached, in a short time period a given image is downloaded only once from the server.</remarks>
        public Image DownloadWebFolderItemImage(WebFolderItemImageInfo webFolderItemImage)
        {
            Image imgWebFolderItemImage = null;

            if (webFolderItemImage != null)
                try
                {
                    byte[] bytesDownloaded = Cache[webFolderItemImage.WebAddress] as byte[];
                    if (bytesDownloaded == null)
                    {
                        bytesDownloaded = webClient.DownloadData(UriHelper.GetUri(webFolderItemImage.WebAddress));
                        Cache[webFolderItemImage.WebAddress] = bytesDownloaded;
                    }

                    Image imgDownloaded = null;
                    using (MemoryStream msr = new MemoryStream(bytesDownloaded))
                        imgDownloaded = Image.FromStream(msr);

                    // Extract image, if it's in a striped image.
                    if (webFolderItemImage.LocationX.HasValue || webFolderItemImage.LocationY.HasValue)
                    {
                        Bitmap bmp = new Bitmap(imgDownloaded.Width, imgDownloaded.Width);
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            g.DrawImage(
                                imgDownloaded,
                                0, 0,
                                new Rectangle(
                                    webFolderItemImage.LocationX ?? 0,
                                    webFolderItemImage.LocationY ?? 0,
                                    imgDownloaded.Width,
                                    imgDownloaded.Width),
                                GraphicsUnit.Pixel);
                        }
                        imgWebFolderItemImage = bmp;
                    }
                    else imgWebFolderItemImage = imgDownloaded;
                }
                catch (Exception ex)
                {
                    throw new OperationFailedException(ex.Message, ex);
                }
            
            return imgWebFolderItemImage;
        }

        #endregion

        #region WebFolder Related Methods

        /// <summary>
        /// Creates a webfolder in SkyDrive's root.
        /// </summary>
        /// <param name="name">The name of the webfolder.</param>
        /// <param name="shareType">The ShareType of the webfolder.</param>
        public void CreateRootWebFolder(string name, WebFolderItemShareType shareType)
        {
            try
            {
                Uri uriRootWebFolderCreate = GetRootWebFolderCreateUri(Session);
                string responseString = webClient.DownloadString(uriRootWebFolderCreate);

                NameValueCollection parameters = ParseFormPostBackParameters(responseString, "aspnetForm");
                parameters["LiveFolderName"] = name;
                // TODO: Implement full featered permissions settings!
                switch (shareType)
                {
                    case WebFolderItemShareType.Public:
                    case WebFolderItemShareType.Private:
                        parameters["PC_DropDownSelect"] = shareType.ToString().ToLowerInvariant();
                        break;
                    case WebFolderItemShareType.MyNetwork:
                        parameters["PC_DropDownSelect"] = "cn";
                        break;
                    case WebFolderItemShareType.PeopleSelected:
                        parameters["PC_DropDownSelect"] = "custom";
                        break;
                    default:
                        throw new NotSupportedException(shareType.ToString());
                }

                webClient.UploadValuesUrlEncoded(uriRootWebFolderCreate, parameters, false);
            }
            catch (Exception ex)
            {
                throw new OperationFailedException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Creates a webfolder in a sub webfolder.
        /// </summary>
        /// <param name="name">The name of the webfolder.</param>
        /// <param name="webFolderParent">The webfolder where the new webfolder is to be created.</param>
        public void CreateSubWebFolder(string name, WebFolderInfo webFolderParent)
        {
            try
            {
                Uri uriSubWebFolderCreate = GetSubWebFolderCreateUri(Session, webFolderParent);
                string responseString = webClient.DownloadString(uriSubWebFolderCreate);

                NameValueCollection parameters = ParseFormPostBackParameters(responseString, "aspnetForm");
                parameters["itemName"] = name;

                webClient.UploadValuesUrlEncoded(uriSubWebFolderCreate, parameters, false);
            }
            catch (Exception ex)
            {
                throw new OperationFailedException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Lists webfolders located in SkyDrive's root.
        /// </summary>
        /// <returns>The list of webfolders in SkyDrive's root.</returns>
        public WebFolderInfo[] ListRootWebFolders()
        {
            List<WebFolderInfo> lRootWebFolder = new List<WebFolderInfo>();

            WebFolderItemInfo[] webFolderItems = ListRootWebFolderItems();
            foreach (WebFolderItemInfo webFolderItem in webFolderItems)
                if (webFolderItem.ItemType == WebFolderItemType.Folder)
                {
                    lRootWebFolder.Add(webFolderItem as WebFolderInfo);
                }

            return lRootWebFolder.ToArray();
        }

        /// <summary>
        /// Lists webfolders located in a sub webfolder.
        /// </summary>
        /// <param name="webFolderParent">The webfolder which webfolders are to be listed.</param>
        /// <returns>The list of webfolders in the sub webfolder.</returns>
        public WebFolderInfo[] ListSubWebFolders(WebFolderInfo webFolderParent)
        {
            return ListSubWebFolders(webFolderParent, WebFolderViewType.Details);
        }

        /// <summary>
        /// Lists webfolders located in a sub webfolder.
        /// </summary>
        /// <param name="webFolderParent">The webfolder which webfolders are to be listed.</param>
        /// <param name="viewType">The ViewType how webfolders are desired to be listed.</param>
        /// <returns>The list of webfolders in the sub webfolder.</returns>
        /// <remarks>The selected ViewType affects the available information of a webfolder.</remarks>
        public WebFolderInfo[] ListSubWebFolders(WebFolderInfo webFolderParent, WebFolderViewType viewType)
        {
            List<WebFolderInfo> lSubWebFolder = new List<WebFolderInfo>();

            WebFolderItemInfo[] webFolderItems = ListSubWebFolderItems(webFolderParent, viewType);
            foreach (WebFolderItemInfo webFolderItem in webFolderItems)
                if (webFolderItem.ItemType == WebFolderItemType.Folder)
                {
                    lSubWebFolder.Add(webFolderItem as WebFolderInfo);
                }

            return lSubWebFolder.ToArray();
        }

        /// <summary>
        /// Gets a webfolder with all data.
        /// </summary>
        /// <param name="webFolder">The webfolder.</param>
        /// <returns>The webfolder with all data.</returns>
        public WebFolderInfo GetWebFolder(WebFolderInfo webFolder)
        {
            return (WebFolderInfo)GetWebFolderItem(webFolder);
        }

        /// <summary>
        /// Downloads a webfolder's content as a .zip package.
        /// </summary>
        /// <param name="webFolder">The webfolder to be downloaded.</param>
        /// <returns>A readable stream that contains the webfolder as a .zip package.</returns>
        public Stream DownloadWebFolder(WebFolderInfo webFolder)
        {
            Stream stream = null;

            try
            {
                Uri uriWebFolderItemView = GetWebFolderItemViewUri(Session, webFolder);
                string responseString = webClient.DownloadString(uriWebFolderItemView);

                Uri uriDownload = ParseWebFolderItemDownloadUri(responseString, webFolder);
                HttpWebRequest webreq = webClient.GetHttpWebRequest(uriDownload);
                WebResponse webresp = webreq.GetResponse();
                stream = webresp.GetResponseStream();
            }
            catch (Exception ex)
            {
                throw new OperationFailedException(ex.Message, ex);
            }
            
            return stream;
        }

        /// <summary>
        /// Determines whether the specified webfolder exists.
        /// </summary>
        /// <param name="webFolder">The webfolder to look for.</param>
        /// <returns><c>true</c> if the specified webfolder exists; otherwise, <c>false</c>.</returns>
        public bool IsWebFolderExists(WebFolderInfo webFolder)
        {
            return IsWebFolderItemExists(webFolder);
        }


        /// <summary>
        /// Renames a webfolder.
        /// </summary>
        /// <param name="webFolder">The webfolder to be renamed.</param>
        /// <param name="newName">The new name.</param>
        public void RenameWebFolder(WebFolderInfo webFolder, string newName)
        {
            RenameWebFolderItem(webFolder, newName);
        }

        /// <summary>
        /// Changes the description of a webfolder.
        /// </summary>
        /// <param name="webFolder">The webfolder to be changed.</param>
        /// <param name="newDescription">The new description.</param>
        public void ChangeWebFolderDescription(WebFolderInfo webFolder, string newDescription)
        {
            ChangeWebFolderItemDescription(webFolder, newDescription);
        }

        /// <summary>
        /// Changes the ContentType of a webfolder in SkyDrive's root.
        /// </summary>
        /// <param name="webFolder">The webfolder.</param>
        /// <param name="newContentType">The new ContentType of the webfolder.</param>
        public void ChangeRootWebFolderContentType(WebFolderInfo webFolder, WebFolderContentType newContentType)
        {
            try
            {
                Uri uriWebFolderChangeContentType = GetRootWebFolderChangeContentTypeUri(Session, webFolder);
                string responseString = webClient.DownloadString(uriWebFolderChangeContentType);

                NameValueCollection parameters = ParseFormPostBackParameters(responseString, "aspnetForm");
                switch (newContentType)
                {
                    case WebFolderContentType.Documents:
                        parameters["folderCategory"] = newContentType.ToString().Replace("s", String.Empty);
                        break;
                    default:
                        parameters["folderCategory"] = newContentType.ToString();
                        break;
                }

                webClient.UploadValuesUrlEncoded(uriWebFolderChangeContentType, parameters, false);
            }
            catch (Exception ex)
            {
                throw new OperationFailedException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Moves a sub webfolder into the specified webfolder.
        /// </summary>
        /// <param name="webFolder">The sub webfolder to be moved.</param>
        /// <param name="webFolderDestination">The destination webfolder where sub webfolder is being moved.</param>
        public void MoveSubWebFolder(WebFolderInfo webFolder, WebFolderInfo webFolderDestination)
        {
            MoveSubWebFolderItem(webFolder, webFolderDestination);
        }

        /// <summary>
        /// Deletes a webfolder.
        /// </summary>
        /// <param name="webFolder">The webfolder to be deleted.</param>
        public void DeleteWebFolder(WebFolderInfo webFolder)
        {
            DeleteWebFolderItem(webFolder);
        }

        #endregion

        #region WebFile Related Methods

        /// <summary>
        /// Uploads a webfile to the specified webfolder.
        /// </summary>
        /// <param name="fileName">The name of the file (including path) to upload.</param>
        /// <param name="webFolderParent">The webfolder where webfile is to be uploaded.</param>
        public void UploadWebFile(string fileName, WebFolderInfo webFolderParent)
        {
            FileInfo fiWebFile = new FileInfo(fileName);
            if (!fiWebFile.Exists)
                throw new FileNotFoundException("WebFile to upload cannot be found!", fiWebFile.FullName);

            string userAgentOriginal = webClient.UserAgent;
            try
            {
                Uri uriUpload = GetWebFileUploadUri(Session, webFolderParent);
                webClient.UserAgent = null;
                string responseString = webClient.DownloadString(uriUpload);

                NameValueCollection parameters = ParseFormPostBackParameters(responseString, "aspnetForm");
                Dictionary<string, object> dicParameter = new Dictionary<string, object>(parameters.Count);
                for (int idxParameter = 0; idxParameter < parameters.Count; idxParameter++)
                    dicParameter.Add(
                        parameters.GetKey(idxParameter),
                        parameters[idxParameter]);

                dicParameter["photoSize"] = "0";
                dicParameter["fileUpload1"] = fiWebFile;

                webClient.UploadValuesProgressChanged += new EventHandler<UploadValuesProgressChangedEventArgs>(delegate(object sender, UploadValuesProgressChangedEventArgs e)
                {
                    OnUploadValuesProgressChanged(new UploadWebFileProgressChangedEventArgs(
                        e.BytesSent,
                        e.TotalBytesToSent));
                });
                webClient.UploadValuesMultipartEncoded(uriUpload, dicParameter, false);
            }
            catch (Exception ex)
            {
                throw new OperationFailedException(ex.Message, ex);
            }
            finally
            {
                webClient.UserAgent = userAgentOriginal;
            }
        }

        /// <summary>
        /// Lists webfiles located in a sub webfolder.
        /// </summary>
        /// <param name="webFolderParent">The webfolder which webfiles are to be listed.</param>
        /// <returns>The list of webfiles in the sub webfolder.</returns>
        public WebFileInfo[] ListSubWebFolderFiles(WebFolderInfo webFolderParent)
        {
            return ListSubWebFolderFiles(webFolderParent, WebFolderViewType.Details);
        }

        /// <summary>
        /// Lists webfiles located in a sub webfolder.
        /// </summary>
        /// <param name="webFolderParent">The webfolder which webfiles are to be listed.</param>
        /// <param name="viewType">The ViewType how webfiles are desired to be listed.</param>
        /// <returns>The list of webfiles in the sub webfolder.</returns>
        /// <remarks>The selected ViewType highly affects the available information of a webfile.</remarks>
        public WebFileInfo[] ListSubWebFolderFiles(WebFolderInfo webFolderParent, WebFolderViewType viewType)
        {
            List<WebFileInfo> lSubWebFile = new List<WebFileInfo>();

            WebFolderItemInfo[] webFolderItems = ListSubWebFolderItems(webFolderParent, viewType);
            foreach (WebFolderItemInfo webFolderItem in webFolderItems)
                if (webFolderItem.ItemType == WebFolderItemType.File)
                {
                    lSubWebFile.Add(webFolderItem as WebFileInfo); 
                }

            return lSubWebFile.ToArray();
        }

        /// <summary>
        /// Gets a webfile with all data.
        /// </summary>
        /// <param name="webFile">The webfile.</param>
        /// <returns>The webfile with all data.</returns>
        public WebFileInfo GetWebFile(WebFileInfo webFile)
        {
            return (WebFileInfo)GetWebFolderItem(webFile);
        }

        /// <summary>
        /// Downloads a webfile.
        /// </summary>
        /// <param name="webFile">The webfile to download.</param>
        /// <returns>A readable stream that contains the webfile's content.</returns>
        public Stream DownloadWebFile(WebFileInfo webFile)
        {
            Stream stream = null;

            try
            {
                Uri uriWebFolderItemView = GetWebFolderItemViewUri(Session, webFile);
                string responseString = webClient.DownloadString(uriWebFolderItemView);

                Uri uriDownload = ParseWebFolderItemDownloadUri(responseString, webFile);
                if (uriDownload == null)
                    throw new WebResponseFormatException(String.Format(
                        CultureInfo.InvariantCulture,
                        "Could not find download URL of '{0}'.",
                        webFile.FullName));

                HttpWebRequest webreq = webClient.GetHttpWebRequest(uriDownload);
                WebResponse webresp = webreq.GetResponse();
                stream = webresp.GetResponseStream();
            }
            catch (Exception ex)
            {
                throw new OperationFailedException(ex.Message, ex);
            }

            return stream;
        }

        /// <summary>
        /// Determines whether the specified webfile exists.
        /// </summary>
        /// <param name="webFile">The webfile to look for.</param>
        /// <returns><c>true</c> if the specified webfile exists; otherwise, <c>false</c>.</returns>
        public bool IsWebFileExists(WebFileInfo webFile)
        {
            return IsWebFolderItemExists(webFile);
        }

        /// <summary>
        /// Renames a webfile.
        /// </summary>
        /// <param name="webFile">The webfile to be renamed.</param>
        /// <param name="newName">The new name.</param>
        public void RenameWebFile(WebFileInfo webFile, string newName)
        {
            RenameWebFolderItem(webFile, newName);
        }

        /// <summary>
        /// Changes the description of a webfile.
        /// </summary>
        /// <param name="webFile">The webfile to be changed.</param>
        /// <param name="newDescription">The new description.</param>
        public void ChangeWebFileDescription(WebFileInfo webFile, string newDescription)
        {
            ChangeWebFolderItemDescription(webFile, newDescription);
        }

        /// <summary>
        /// Copies a webfile into the specified webfolder.
        /// </summary>
        /// <param name="webFile">The webfile to be copied.</param>
        /// <param name="webFolderDestination">The destination webfolder where webfile is being copied.</param>
        public void CopyWebFile(WebFileInfo webFile, WebFolderInfo webFolderDestination)
        {
            Uri uriWebFolderItemView = GetWebFolderItemViewUri(Session, webFile);
            string responseString = webClient.DownloadString(uriWebFolderItemView);

            HtmlDocument responseDocument = new HtmlDocument();
            responseDocument.LoadHtml(responseString);

            HtmlNode nodeLinkCopy = responseDocument.GetElementbyId("copy");
            Uri uriWebFolderItemMoveCopyHome = UriHelper.GetUri(nodeLinkCopy.Attributes["href"].Value);

            Uri uriWebFolderItemMoveCopy = GetWebFolderDestinationMoveCopyUri(Session, webFolderDestination, uriWebFolderItemMoveCopyHome);
            responseString = webClient.DownloadString(uriWebFolderItemMoveCopy);

            NameValueCollection parameters = ParseFormPostBackParameters(responseString, "aspnetForm");
            parameters["linkClicked"] = "true";

            webClient.UploadValuesUrlEncoded(uriWebFolderItemMoveCopy, parameters, false);
        }

        /// <summary>
        /// Moves a webfile into the specified webfolder.
        /// </summary>
        /// <param name="webFile">The webfile to be moved.</param>
        /// <param name="webFolderDestination">The destination webfolder where webfile is being moved.</param>
        public void MoveWebFile(WebFileInfo webFile, WebFolderInfo webFolderDestination)
        {
            MoveSubWebFolderItem(webFile, webFolderDestination);
        }

        /// <summary>
        /// Deletes a webfile.
        /// </summary>
        /// <param name="webFile">The webFile to be deleted.</param>
        public void DeleteWebFile(WebFileInfo webFile)
        {
            DeleteWebFolderItem(webFile);
        }

        #endregion

        #region WebFavorite Related Methods

        /// <summary>
        /// Creates a webfavorite in a sub webfolder.
        /// </summary>
        /// <param name="name">The name of the webfavorite.</param>
        /// <param name="webAddress">The web address of the webfavorite.</param>
        /// <param name="webFolderParent">The webfolder where the webfavorite is to be created.</param>
        public void CreateWebFavorite(string name, Uri webAddress, WebFolderInfo webFolderParent)
        {
            CreateWebFavorite(name, webAddress, null, webFolderParent);
        }

        /// <summary>
        /// Creates a webfavorite in a sub webfolder.
        /// </summary>
        /// <param name="name">The name of the webfavorite.</param>
        /// <param name="webAddress">The web address of the webfavorite.</param>
        /// <param name="description">The description of the webfavorite.</param>
        /// <param name="webFolderParent">The webfolder where the webfavorite is to be created.</param>
        public void CreateWebFavorite(string name, Uri webAddress, string description, WebFolderInfo webFolderParent)
        {
            try
            {
                Uri uriWebFavoriteCreate = GetWebFavoriteCreateUri(Session, webFolderParent);
                string responseString = webClient.DownloadString(uriWebFavoriteCreate);

                NameValueCollection parameters = ParseFormPostBackParameters(responseString, "aspnetForm");
                parameters["itemUrl"] = webAddress != null ? webAddress.AbsoluteUri : String.Empty;
                parameters["itemName"] = name;
                parameters["itemCaption"] = description;

                webClient.UploadValuesUrlEncoded(uriWebFavoriteCreate, parameters, false);
            }
            catch (Exception ex)
            {
                throw new OperationFailedException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Gets a webfavorite with all data.
        /// </summary>
        /// <param name="webFavorite">The webfavorite.</param>
        /// <returns>The webfavorite with all data.</returns>
        public WebFavoriteInfo GetWebFavorite(WebFavoriteInfo webFavorite)
        {
            return (WebFavoriteInfo)GetWebFolderItem(webFavorite);
        }

        /// <summary>
        /// Renames a webfavorite.
        /// </summary>
        /// <param name="webFavorite">The webfavorite to be renamed.</param>
        /// <param name="newName">The new name.</param>
        public void RenameWebFavorite(WebFavoriteInfo webFavorite, string newName)
        {
            RenameWebFolderItem(webFavorite, newName);
        }

        /// <summary>
        /// Downloads a webfavorite.
        /// </summary>
        /// <param name="webFavorite">The webfavorite to download.</param>
        /// <returns>A readable stream that contains the webfavorite's content.</returns>
        public Stream DownloadWebFavorite(WebFavoriteInfo webFavorite)
        {
            return DownloadWebFile(webFavorite);
        }

        /// <summary>
        /// Deletes a webfavorite.
        /// </summary>
        /// <param name="webFavorite">The webfavorite to be deleted.</param>
        public void DeleteWebFavorite(WebFavoriteInfo webFavorite)
        {
            DeleteWebFolderItem(webFavorite);
        }

        #endregion

        #endregion

        #region Asynchronous Methods

        #region Session Related Methods

        /// <summary>
        /// Begins an asynchronous operation to log on to a specified user account.
        /// </summary>
        /// <param name="userName">The name of the user.</param>
        /// <param name="userPassword">The user password.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginLogOn(string userName, string userPassword, AsyncCallback callback)
        {
            var delegateLogOn = new AsyncDelegate<string, string>(LogOn);
            IAsyncResult asyncResult = delegateLogOn.BeginInvoke(userName, userPassword, callback, delegateLogOn);
            return asyncResult;
        }

        /// <summary>
        /// Ends an asynchronous operation to log on to a specified user account.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        public void EndLogOn(IAsyncResult asyncResult)
        {
            var delegateLogOn = (AsyncDelegate<string, string>)asyncResult.AsyncState;
            delegateLogOn.EndInvoke(asyncResult);
        }

        #endregion

        #region WebFolderItem Related Methods

        /// <summary>
        /// Begins an asynchronous operation to list webfolderitems located in SkyDrive's root.
        /// </summary>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginListRootWebFolderItems(AsyncCallback callback)
        {
            var delegateListRootItems = new AsyncReturnDelegate<WebFolderItemInfo[]>(ListRootWebFolderItems);
            IAsyncResult asyncResult = delegateListRootItems.BeginInvoke(callback, delegateListRootItems);
            return asyncResult;
        }

        /// <summary>
        /// Ends an asynchronous operation to list webfolderitems located in SkyDrive's root.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        /// <returns>The list of webfolderitems in SkyDrive's root.</returns>
        public WebFolderItemInfo[] EndListRootWebFolderItems(IAsyncResult asyncResult)
        {
            var delegateListRootItems = (AsyncReturnDelegate<WebFolderItemInfo[]>)asyncResult.AsyncState;
            WebFolderItemInfo[] webFolderItems = delegateListRootItems.EndInvoke(asyncResult);
            return webFolderItems;
        }

        /// <summary>
        /// Begins an asynchronous operation to list webfolderitems located in a sub webfolder.
        /// </summary>
        /// <param name="webFolderParent">The webfolder which webfolderitems are to be listed.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginListSubWebFolderItems(WebFolderInfo webFolderParent, AsyncCallback callback)
        {
            var delegateListSubItems = new AsyncReturnDelegate<WebFolderItemInfo[], WebFolderInfo>(ListSubWebFolderItems);
            IAsyncResult asyncResult = delegateListSubItems.BeginInvoke(webFolderParent, callback, delegateListSubItems);
            return asyncResult;
        }

        /// <summary>
        /// Ends an asynchronous operation to list webfolderitems located in a sub webfolder.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        /// <returns>The list of webfolderitems in the sub webfolder.</returns>
        public WebFolderItemInfo[] EndListSubWebFolderItems(IAsyncResult asyncResult)
        {
            var delegateListSubItems = (AsyncReturnDelegate<WebFolderItemInfo[], WebFolderInfo>)asyncResult.AsyncState;
            WebFolderItemInfo[] webFolderItems = delegateListSubItems.EndInvoke(asyncResult);
            return webFolderItems;
        }

        /// <summary>
        /// Begins an asynchronous operation to get a webfolderitem with all data.
        /// </summary>
        /// <param name="webFolderItem">The webfolderitem.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginGetWebFolderItem(WebFolderItemInfo webFolderItem, AsyncCallback callback)
        {
            var delegateGetItem = new AsyncReturnDelegate<WebFolderItemInfo, WebFolderItemInfo>(GetWebFolderItem);
            IAsyncResult asyncResult = delegateGetItem.BeginInvoke(webFolderItem, callback, delegateGetItem);
            return asyncResult;
        }

        /// <summary>
        /// Ends an asynchronous operation to get a webfolderitem with all data.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        /// <returns>The webfolderitem with all data.</returns>
        public WebFolderItemInfo EndGetWebFolderItem(IAsyncResult asyncResult)
        {
            var delegateGetItem = (AsyncReturnDelegate<WebFolderItemInfo, WebFolderItemInfo>)asyncResult.AsyncState;
            WebFolderItemInfo webFolderItem = delegateGetItem.EndInvoke(asyncResult);
            return webFolderItem;
        }

        /// <summary>
        /// Begins an asynchronous operation to determine whether the specified webfolderitem exists.
        /// </summary>
        /// <param name="webFolderItem">The webfolderitem to look for.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginIsWebFolderItemExists(WebFolderItemInfo webFolderItem, AsyncCallback callback)
        {
            var delegateIsItemExists = new AsyncReturnDelegate<bool, WebFolderItemInfo>(IsWebFolderItemExists);
            IAsyncResult asyncResult = delegateIsItemExists.BeginInvoke(webFolderItem, callback, delegateIsItemExists);
            return asyncResult;
        }

        /// <summary>
        /// Ends an asynchronous operation to determine whether the specified webfolderitem exists.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        /// <returns><c>true</c> if the specified webfolderitem exists; otherwise, <c>false</c>.</returns>
        public bool EndIsWebFolderItemExists(IAsyncResult asyncResult)
        {
            var delegateIsItemExists = (AsyncReturnDelegate<bool, WebFolderItemInfo>)asyncResult.AsyncState;
            bool isItemExists = delegateIsItemExists.EndInvoke(asyncResult);
            return isItemExists;
        }

        /// <summary>
        /// Begins an asynchronous operation to rename a webfolderitem.
        /// </summary>
        /// <param name="webFolderItem">The webfolderitem to be renamed.</param>
        /// <param name="newName">The new name.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginRenameWebFolderItem(WebFolderItemInfo webFolderItem, string newName, AsyncCallback callback)
        {
            var delegateChangeItem = new AsyncDelegate<WebFolderItemInfo, string>(RenameWebFolderItem);
            IAsyncResult asyncResult = delegateChangeItem.BeginInvoke(webFolderItem, newName, callback, delegateChangeItem);
            return asyncResult;
        }

        /// <summary>
        /// Ends an asynchronous operation to rename a webfolderitem.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        public void EndRenameWebFolderItem(IAsyncResult asyncResult)
        {
            var delegateChangeItem = (AsyncDelegate<WebFolderItemInfo, string>)asyncResult.AsyncState;
            delegateChangeItem.EndInvoke(asyncResult);
        }

        /// <summary>
        /// Begins an asynchronous operation to change the description of a webfolderitem.
        /// </summary>
        /// <param name="webFolderItem">The webfolderitem to be changed.</param>
        /// <param name="newDescription">The new description.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginChangeWebFolderItemDescription(WebFolderItemInfo webFolderItem, string newDescription, AsyncCallback callback)
        {
            var delegateChangeItem = new AsyncDelegate<WebFolderItemInfo, string>(ChangeWebFolderItemDescription);
            IAsyncResult asyncResult = delegateChangeItem.BeginInvoke(webFolderItem, newDescription, callback, delegateChangeItem);
            return asyncResult;
        }

        /// <summary>
        /// Ends an asynchronous operation to change the description of a webfolderitem.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        public void EndChangeWebFolderItemDescription(IAsyncResult asyncResult)
        {
            var delegateChangeItem = (AsyncDelegate<WebFolderItemInfo, string>)asyncResult.AsyncState;
            delegateChangeItem.EndInvoke(asyncResult);
        }

        /// <summary>
        /// Begins an asynchronous operation to move a sub webfolderitem into the specified webfolder.
        /// </summary>
        /// <param name="webFolderItem">The sub webfolderitem to be moved.</param>
        /// <param name="webFolderDestination">The destination webfolder where webfolderitem is being moved.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginMoveSubWebFolderItem(WebFolderItemInfo webFolderItem, WebFolderInfo webFolderDestination, AsyncCallback callback)
        {
            var delegateMoveItem = new AsyncDelegate<WebFolderItemInfo, WebFolderInfo>(MoveSubWebFolderItem);
            IAsyncResult asyncResult = delegateMoveItem.BeginInvoke(webFolderItem, webFolderDestination, callback, delegateMoveItem);
            return asyncResult;
        }

        /// <summary>
        /// Ends an asynchronous operation to move a sub webfolderitem into the specified webfolder.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        public void EndMoveSubWebFolderItem(IAsyncResult asyncResult)
        {
            var delegateMoveItem = (AsyncDelegate<WebFolderItemInfo, WebFolderInfo>)asyncResult.AsyncState;
            delegateMoveItem.EndInvoke(asyncResult);
        }

        /// <summary>
        /// Begins an asynchronous operation to delete a webfolderitem.
        /// </summary>
        /// <param name="webFolderItem">The webfolderitem to be deleted.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginDeleteWebFolderItem(WebFolderItemInfo webFolderItem, AsyncCallback callback)
        {
            var delegateDeleteItem = new AsyncDelegate<WebFolderItemInfo>(DeleteWebFolderItem);
            IAsyncResult asyncResult = delegateDeleteItem.BeginInvoke(webFolderItem, callback, delegateDeleteItem);
            return asyncResult;
        }

        /// <summary>
        /// Ends an asynchronous operation to delete a webfolderitem.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        public void EndDeleteWebFolderItem(IAsyncResult asyncResult)
        {
            var delegateDeleteItem = (AsyncDelegate<WebFolderItemInfo>)asyncResult.AsyncState;
            delegateDeleteItem.EndInvoke(asyncResult);
        }

        #endregion

        #region WebFolderItemIcon Related Methods
        
        /// <summary>
        /// Begins an asynchronous operation to download webfolderitemicon as an Image.
        /// </summary>
        /// <param name="webFolderItemIcon">The webfolderitemicon to download.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginDownloadWebFolderItemIcon(WebFolderItemIconInfo webFolderItemIcon, AsyncCallback callback)
        {
            var delegateDownloadItemIcon = new AsyncReturnDelegate<Image, WebFolderItemIconInfo>(DownloadWebFolderItemIcon);
            IAsyncResult asyncResult = delegateDownloadItemIcon.BeginInvoke(webFolderItemIcon, callback, delegateDownloadItemIcon);
            return asyncResult;
        }

        /// <summary>
        /// Ends an asynchronous operation to download webfolderitemicon as an Image.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        /// <returns>The webfolderitemicon as an Image.</returns>
        public Image EndDownloadWebFolderItemIcon(IAsyncResult asyncResult)
        {
            var delegateDownloadItemIcon = (AsyncReturnDelegate<Image, WebFolderItemIconInfo>)asyncResult.AsyncState;
            Image imgItemIcon = delegateDownloadItemIcon.EndInvoke(asyncResult);
            return imgItemIcon;
        }

        /// <summary>
        /// Begins an asynchronous operation to download webfolderitemimage as an Image.
        /// </summary>
        /// <param name="webFolderItemImage">The webfolderitemimage to download.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginDownloadWebFolderItemImage(WebFolderItemImageInfo webFolderItemImage, AsyncCallback callback)
        {
            var delegateDownloadItemImage = new AsyncReturnDelegate<Image, WebFolderItemImageInfo>(DownloadWebFolderItemImage);
            IAsyncResult asyncResult = delegateDownloadItemImage.BeginInvoke(webFolderItemImage, callback, delegateDownloadItemImage);
            return asyncResult;
        }

        /// <summary>
        /// Ends an asynchronous operation to download webfolderitemimage as an Image.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        /// <returns>The webfolderitemimage as an Image.</returns>
        public Image EndDownloadWebFolderItemImage(IAsyncResult asyncResult)
        {
            var delegateDownloadItemImage = (AsyncReturnDelegate<Image, WebFolderItemImageInfo>)asyncResult.AsyncState;
            Image imgItemImage = delegateDownloadItemImage.EndInvoke(asyncResult);
            return imgItemImage;
        }

        #endregion

        #region WebFolder Related Methods

        /// <summary>
        /// Begins an asynchronous operation to create a webfolder in SkyDrive's root.
        /// </summary>
        /// <param name="name">The name of the webfolder.</param>
        /// <param name="shareType">The ShareType of the webfolder.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginCreateRootWebFolder(string name, WebFolderItemShareType shareType, AsyncCallback callback)
        {
            var delegateCreateRootItem = new AsyncDelegate<string, WebFolderItemShareType>(CreateRootWebFolder);
            IAsyncResult asyncResult = delegateCreateRootItem.BeginInvoke(name, shareType, callback, delegateCreateRootItem);
            return asyncResult;
        }

        /// <summary>
        /// Ends an asynchronous operation to create a webfolder in SkyDrive's root.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        public void EndCreateRootWebFolder(IAsyncResult asyncResult)
        {
            var delegateCreateRootItem = (AsyncDelegate<string, WebFolderItemShareType>)asyncResult.AsyncState;
            delegateCreateRootItem.EndInvoke(asyncResult);
        }

        /// <summary>
        /// Begins an asynchronous operation to create a webfolder in a sub webfolder.
        /// </summary>
        /// <param name="name">The name of the webfolder.</param>
        /// <param name="webFolderParent">The webfolder where the new webfolder is to be created.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginCreateSubWebFolder(string name, WebFolderInfo webFolderParent, AsyncCallback callback)
        {
            var delegateCreateSubItem = new AsyncDelegate<string, WebFolderInfo>(CreateSubWebFolder);
            IAsyncResult asyncResult = delegateCreateSubItem.BeginInvoke(name, webFolderParent, callback, delegateCreateSubItem);
            return asyncResult;
        }

        /// <summary>
        /// Ends an asynchronous operation to create a webfolder in a sub webfolder.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        public void EndCreateSubWebFolder(IAsyncResult asyncResult)
        {
            var delegateCreateSubItem = (AsyncDelegate<string, WebFolderInfo>)asyncResult.AsyncState;
            delegateCreateSubItem.EndInvoke(asyncResult);
        }

        /// <summary>
        /// Begins an asynchronous operation to list webfolders located in SkyDrive's root.
        /// </summary>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginListRootWebFolders(AsyncCallback callback)
        {
            var delegateListRootItems = new AsyncReturnDelegate<WebFolderInfo[]>(ListRootWebFolders);
            IAsyncResult asyncResult = delegateListRootItems.BeginInvoke(callback, delegateListRootItems);
            return asyncResult;
        }

        /// <summary>
        /// Ends an asynchronous operation to list webfolders located in SkyDrive's root.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        /// <returns>The list of webfolders in SkyDrive's root.</returns>
        public WebFolderInfo[] EndListRootWebFolders(IAsyncResult asyncResult)
        {
            var delegateListRootItems = (AsyncReturnDelegate<WebFolderInfo[]>)asyncResult.AsyncState;
            WebFolderInfo[] webFolders = delegateListRootItems.EndInvoke(asyncResult);
            return webFolders;
        }

        /// <summary>
        /// Begins an asynchronous operation to list webfolders located in a sub webfolder.
        /// </summary>
        /// <param name="webFolderParent">The webfolder which webfolders are to be listed.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginListSubWebFolders(WebFolderInfo webFolderParent, AsyncCallback callback)
        {
            var delegateListSubItems = new AsyncReturnDelegate<WebFolderInfo[], WebFolderInfo>(ListSubWebFolders);
            IAsyncResult asyncResult = delegateListSubItems.BeginInvoke(webFolderParent, callback, delegateListSubItems);
            return asyncResult;
        }

        /// <summary>
        /// Ends an asynchronous operation to list webfolders located in a sub webfolder.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        /// <returns>The list of webfolders in the sub webfolder.</returns>
        public WebFolderInfo[] EndListSubWebFolders(IAsyncResult asyncResult)
        {
            var delegateListSubItems = (AsyncReturnDelegate<WebFolderInfo[], WebFolderInfo>)asyncResult.AsyncState;
            WebFolderInfo[] webFolders = delegateListSubItems.EndInvoke(asyncResult);
            return webFolders;
        }


        /// <summary>
        /// Begins an asynchronous operation to get a webfolder with all data.
        /// </summary>
        /// <param name="webFolder">The webfolder.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginGetWebFolder(WebFolderInfo webFolder, AsyncCallback callback)
        {
            return BeginGetWebFolderItem(webFolder, callback);
        }

        /// <summary>
        /// Ends an asynchronous operation to get a webfolder with all data.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        /// <returns>The webfolder with all data.</returns>
        public WebFolderInfo EndGetWebFolder(IAsyncResult asyncResult)
        {
            return (WebFolderInfo)EndGetWebFolderItem(asyncResult);
        }

        /// <summary>
        /// Begins an asynchronous operation to download a webfolder's content as a .zip package.
        /// </summary>
        /// <param name="webFolder">The webfolder to be downloaded.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginDownloadWebFolder(WebFolderInfo webFolder, AsyncCallback callback)
        {
            var delegateDownloadItem = new AsyncReturnDelegate<Stream, WebFolderInfo>(DownloadWebFolder);
            IAsyncResult asyncResult = delegateDownloadItem.BeginInvoke(webFolder, callback, delegateDownloadItem);
            return asyncResult;
        }

        /// <summary>
        /// Ends an asynchronous operation to download a webfolder's content as a .zip package.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        /// <returns>A readable stream that contains the webfolder as a .zip package.</returns>
        public Stream EndDownloadWebFolder(IAsyncResult asyncResult)
        {
            var delegateDownloadItem = (AsyncReturnDelegate<Stream, WebFolderInfo>)asyncResult.AsyncState;
            Stream sr = (Stream)delegateDownloadItem.EndInvoke(asyncResult);
            return sr;
        }

        /// <summary>
        /// Begins an asynchronous operation to determine whether the specified webfolder exists.
        /// </summary>
        /// <param name="webFolder">The webfolder to look for.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginIsWebFolderExists(WebFolderInfo webFolder, AsyncCallback callback)
        {
            return BeginIsWebFolderItemExists(webFolder, callback);
        }

        /// <summary>
        /// Ends an asynchronous operation to determine whether the specified webfolder exists.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        /// <returns><c>true</c> if the specified webfolder exists; otherwise, <c>false</c>.</returns>
        public bool EndIsWebFolderExists(IAsyncResult asyncResult)
        {
            return EndIsWebFolderItemExists(asyncResult);
        }

        /// <summary>
        /// Begins an asynchronous operation to rename a webfolder.
        /// </summary>
        /// <param name="webFolder">The webfolder to be renamed.</param>
        /// <param name="newName">The new name.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginRenameWebFolder(WebFolderInfo webFolder, string newName, AsyncCallback callback)
        {
            return BeginRenameWebFolderItem(webFolder, newName, callback);
        }

        /// <summary>
        /// Ends an asynchronous operation to rename a webfolder.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        public void EndRenameWebFolder(IAsyncResult asyncResult)
        {
            EndRenameWebFolderItem(asyncResult);
        }

        /// <summary>
        /// Begins an asynchronous operation to change the description of a webfolder.
        /// </summary>
        /// <param name="webFolder">The webfolder to be changed.</param>
        /// <param name="newDescription">The new description.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginChangeWebFolderDescription(WebFolderInfo webFolder, string newDescription, AsyncCallback callback)
        {
            return BeginChangeWebFolderItemDescription(webFolder, newDescription, callback);
        }

        /// <summary>
        /// Ends an asynchronous operation to change the description of a webfolder.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        public void EndChangeWebFolderDescription(IAsyncResult asyncResult)
        {
            EndChangeWebFolderItemDescription(asyncResult);
        }

        /// <summary>
        /// Begins an asynchronous operation to change the ContentType of a webfolder in SkyDrive's root.
        /// </summary>
        /// <param name="webFolder">The webfolder.</param>
        /// <param name="newContentType">The new ContentType of the webfolder.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginChangeRootWebFolderContentType(WebFolderInfo webFolder, WebFolderContentType newContentType, AsyncCallback callback)
        {
            var delegateChangeItem = new AsyncDelegate<WebFolderInfo, WebFolderContentType>(ChangeRootWebFolderContentType);
            IAsyncResult asyncResult = delegateChangeItem.BeginInvoke(webFolder, newContentType, callback, delegateChangeItem);
            return asyncResult;
        }

        /// <summary>
        /// Ends an asynchronous operation to change the ContentType of a webfolder in SkyDrive's root.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        public void EndChangeRootWebFolderContentType(IAsyncResult asyncResult)
        {
            var delegateChangeItem = (AsyncDelegate<WebFolderInfo, WebFolderContentType>)asyncResult.AsyncState;
            delegateChangeItem.EndInvoke(asyncResult);
        }

        /// <summary>
        /// Begins an asynchronous operation to move a sub webfolder into the specified webfolder.
        /// </summary>
        /// <param name="webFolder">The sub webfolder to be moved.</param>
        /// <param name="webFolderDestination">The destination webfolder where sub webfolder is being moved.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginMoveSubWebFolder(WebFolderInfo webFolder, WebFolderInfo webFolderDestination, AsyncCallback callback)
        {
            return BeginMoveSubWebFolderItem(webFolder, webFolderDestination, callback);
        }

        /// <summary>
        /// Ends an asynchronous operation to move a sub webfolder into the specified webfolder.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        public void EndMoveSubWebFolder(IAsyncResult asyncResult)
        {
            EndMoveSubWebFolderItem(asyncResult);
        }

        /// <summary>
        /// Begins an asynchronous operation to delete a webfolder.
        /// </summary>
        /// <param name="webFolder">The webfolder to be deleted.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginDeleteWebFolder(WebFolderInfo webFolder, AsyncCallback callback)
        {
            return BeginDeleteWebFolderItem(webFolder, callback);
        }

        /// <summary>
        /// Ends an asynchronous operation to delete a webfolder.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        public void EndDeleteWebFolder(IAsyncResult asyncResult)
        {
            EndDeleteWebFolderItem(asyncResult);
        }

        #endregion

        #region WebFile Related Methods
        
        /// <summary>
        /// Begins an asynchronous operation to upload a webfile to the specified webfolder.
        /// </summary>
        /// <param name="fileName">The name of the file (including path) to upload.</param>
        /// <param name="webFolderParent">The webfolder where webfile is to be uploaded.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginUploadWebFile(string fileName, WebFolderInfo webFolderParent, AsyncCallback callback)
        {
            var delegateUploadItem = new AsyncDelegate<string, WebFolderInfo>(UploadWebFile);
            IAsyncResult result = delegateUploadItem.BeginInvoke(fileName, webFolderParent, callback, delegateUploadItem);

            return result;
        }

        /// <summary>
        /// Ends an asynchronous operation to upload a webfile to the specified webfolder.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        public void EndUploadWebFile(IAsyncResult asyncResult)
        {
            var delegateUploadItem = (AsyncDelegate<string, WebFolderInfo>)asyncResult.AsyncState;
            delegateUploadItem.EndInvoke(asyncResult);
        }

        /// <summary>
        /// Begins an asynchronous operation to list webfiles located in a sub webfolder.
        /// </summary>
        /// <param name="webFolderParent">The webfolder which webfiles are to be listed.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginListSubWebFolderFiles(WebFolderInfo webFolderParent, AsyncCallback callback)
        {
            var delegateListSubItems = new AsyncReturnDelegate<WebFileInfo[], WebFolderInfo>(ListSubWebFolderFiles);
            IAsyncResult asyncResult = delegateListSubItems.BeginInvoke(webFolderParent, callback, delegateListSubItems);
            return asyncResult;
        }

        /// <summary>
        /// Ends an asynchronous operation to list webfiles located in a sub webfolder.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        /// <returns>The list of webfiles in the sub webfolder.</returns>
        public WebFileInfo[] EndListSubWebFolderFiles(IAsyncResult asyncResult)
        {
            var delegateListSubItems = (AsyncReturnDelegate<WebFileInfo[], WebFolderInfo>)asyncResult.AsyncState;
            WebFileInfo[] webFiles = delegateListSubItems.EndInvoke(asyncResult);
            return webFiles;
        }

        /// <summary>
        /// Begins an asynchronous operation to get a webfile with all data.
        /// </summary>
        /// <param name="webFile">The webfile.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginGetWebFile(WebFileInfo webFile, AsyncCallback callback)
        {
            return BeginGetWebFolderItem(webFile, callback);
        }

        /// <summary>
        /// Ends an asynchronous operation to get a webfile with all data.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        /// <returns>The webfile with all data.</returns>
        public WebFileInfo EndGetWebFile(IAsyncResult asyncResult)
        {
            return (WebFileInfo)EndGetWebFolderItem(asyncResult);
        }

        /// <summary>
        /// Begins an asynchronous operation to download a webfile.
        /// </summary>
        /// <param name="webFile">The webfile to download.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginDownloadWebFile(WebFileInfo webFile, AsyncCallback callback)
        {
            var delegateDownloadItem = new AsyncReturnDelegate<Stream, WebFileInfo>(DownloadWebFile);
            IAsyncResult asyncResult = delegateDownloadItem.BeginInvoke(webFile, callback, delegateDownloadItem);
            return asyncResult;
        }

        /// <summary>
        /// Ends an asynchronous operation to download a webfile.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        /// <returns>A readable stream that contains the webfile's content.</returns>
        public Stream EndDownloadWebFile(IAsyncResult asyncResult)
        {
            var delegateDownloadItem = (AsyncReturnDelegate<Stream, WebFileInfo>)asyncResult.AsyncState;
            Stream sr = (Stream)delegateDownloadItem.EndInvoke(asyncResult);
            return sr;
        }

        /// <summary>
        /// Begins an asynchronous operation to determine whether the specified webfile exists.
        /// </summary>
        /// <param name="webFile">The webfile to look for.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginIsWebFileExists(WebFileInfo webFile, AsyncCallback callback)
        {
            return BeginIsWebFolderItemExists(webFile, callback);
        }

        /// <summary>
        /// Ends an asynchronous operation to determine whether the specified webfile exists.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        /// <returns><c>true</c> if the specified webfile exists; otherwise, <c>false</c>.</returns>
        public bool EndIsWebFileExists(IAsyncResult asyncResult)
        {
            return EndIsWebFolderItemExists(asyncResult);
        }

        /// <summary>
        /// Begins an asynchronous operation to rename a webfile.
        /// </summary>
        /// <param name="webFile">The webfile to be renamed.</param>
        /// <param name="newName">The new name.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginRenameWebFile(WebFileInfo webFile, string newName, AsyncCallback callback)
        {
            return BeginRenameWebFolderItem(webFile, newName, callback);
        }

        /// <summary>
        /// Ends an asynchronous operation to rename a webfile.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        public void EndRenameWebFile(IAsyncResult asyncResult)
        {
            EndRenameWebFolderItem(asyncResult);
        }

        /// <summary>
        /// Begins an asynchronous operation to change the description of a webfile.
        /// </summary>
        /// <param name="webFile">The webfile to be changed.</param>
        /// <param name="newDescription">The new description.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginChangeWebFileDescription(WebFileInfo webFile, string newDescription, AsyncCallback callback)
        {
            return BeginChangeWebFolderItemDescription(webFile, newDescription, callback);
        }

        /// <summary>
        /// Ends an asynchronous operation to change the description of a webfile.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        public void EndChangeWebFileDescription(IAsyncResult asyncResult)
        {
            EndChangeWebFolderItemDescription(asyncResult);
        }

        /// <summary>
        /// Begins an asynchronous operation to copy a webfile into the specified webfolder.
        /// </summary>
        /// <param name="webFile">The webfile to be copied.</param>
        /// <param name="webFolderDestination">The destination webfolder where webfile is being copied.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginCopyWebFile(WebFileInfo webFile, WebFolderInfo webFolderDestination, AsyncCallback callback)
        {
            var delegateCopyItem = new AsyncDelegate<WebFileInfo, WebFolderInfo>(CopyWebFile);
            IAsyncResult asyncResult = delegateCopyItem.BeginInvoke(webFile, webFolderDestination, callback, delegateCopyItem);
            return asyncResult;
        }

        /// <summary>
        /// Ends an asynchronous operation to copy a webfile into the specified webfolder.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        public void EndCopyWebFile(IAsyncResult asyncResult)
        {
            var delegateCopyItem = (AsyncDelegate<WebFileInfo, WebFolderInfo>)asyncResult.AsyncState;
            delegateCopyItem.EndInvoke(asyncResult);
        }

        /// <summary>
        /// Begins an asynchronous operation to move a webfile into the specified webfolder.
        /// </summary>
        /// <param name="webFile">The webfile to be moved.</param>
        /// <param name="webFolderDestination">The destination webfolder where webfile is being moved.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginMoveWebFile(WebFileInfo webFile, WebFolderInfo webFolderDestination, AsyncCallback callback)
        {
            return BeginMoveSubWebFolderItem(webFile, webFolderDestination, callback);
        }

        /// <summary>
        /// Ends an asynchronous operation to move a webfile into the specified webfolder.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        public void EndMoveWebFile(IAsyncResult asyncResult)
        {
            EndMoveSubWebFolderItem(asyncResult);
        }

        /// <summary>
        /// Begins an asynchronous operation to delete a webfile.
        /// </summary>
        /// <param name="webFile">The webFile to be deleted.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginDeleteWebFile(WebFileInfo webFile, AsyncCallback callback)
        {
            return BeginDeleteWebFolderItem(webFile, callback);
        }

        /// <summary>
        /// Ends an asynchronous operation to delete a webfile.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        public void EndDeleteWebFile(IAsyncResult asyncResult)
        {
            EndDeleteWebFolderItem(asyncResult);
        }
        #endregion

        #region WebFavorite Related Methods

        /// <summary>
        /// Begins an asynchronous operation to create a webfavorite in a sub webfolder.
        /// </summary>
        /// <param name="name">The name of the webfavorite.</param>
        /// <param name="webAddress">The web address of the webfavorite.</param>
        /// <param name="webFolderParent">The webfolder where the webfavorite is to be created.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginCreateFavorite(string name, Uri webAddress, WebFolderInfo webFolderParent, AsyncCallback callback)
        {
            var delegateCreateFavorite = new AsyncDelegate<string, Uri, WebFolderInfo>(CreateWebFavorite);
            IAsyncResult asyncResult = delegateCreateFavorite.BeginInvoke(name, webAddress, webFolderParent, callback, delegateCreateFavorite);
            return asyncResult;
        }

        /// <summary>
        /// Begins an asynchronous operation to create a webfavorite in a sub webfolder.
        /// </summary>
        /// <param name="name">The name of the webfavorite.</param>
        /// <param name="webAddress">The web address of the webfavorite.</param>
        /// <param name="description">The description of the webfavorite.</param>
        /// <param name="webFolderParent">The webfolder where the webfavorite is to be created.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginCreateFavorite(string name, Uri webAddress, string description, WebFolderInfo webFolderParent, AsyncCallback callback)
        {
            var delegateCreateFavorite = new AsyncDelegate<string, Uri, string, WebFolderInfo>(CreateWebFavorite);
            IAsyncResult asyncResult = delegateCreateFavorite.BeginInvoke(name, webAddress, description, webFolderParent, callback, delegateCreateFavorite);
            return asyncResult;
        }

        /// <summary>
        /// Ends an asynchronous operation to create a webfavorite in a sub webfolder.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        public void EndCreateFavorite(IAsyncResult asyncResult)
        {
            var delegateCreateFavorite1 = asyncResult.AsyncState as AsyncDelegate<string, Uri, WebFolderInfo>;
            if (delegateCreateFavorite1 != null)
            {
                delegateCreateFavorite1.EndInvoke(asyncResult);
                return;
            }
            var delegateCreateFavorite2 = asyncResult.AsyncState as AsyncDelegate<string, Uri, string, WebFolderInfo>;
            if (delegateCreateFavorite2 != null)
            {
                delegateCreateFavorite2.EndInvoke(asyncResult);
                return;
            }
        }

        /// <summary>
        /// Begins an asynchronous operation to get a webfavorite with all data.
        /// </summary>
        /// <param name="webFavorite">The webfavorite.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginGetWebFavorite(WebFavoriteInfo webFavorite, AsyncCallback callback)
        {
            return BeginGetWebFolderItem(webFavorite, callback);
        }

        /// <summary>
        /// Ends an asynchronous operation to get a webfavorite with all data.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        /// <returns>The webfavorite with all data.</returns>
        public WebFavoriteInfo EndGetWebFavorite(IAsyncResult asyncResult)
        {
            return (WebFavoriteInfo)EndGetWebFolderItem(asyncResult);
        }

        /// <summary>
        /// Begins an asynchronous operation to rename a webfavorite.
        /// </summary>
        /// <param name="webFavorite">The webfavorite to be renamed.</param>
        /// <param name="newName">The new name.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginRenameWebFavorite(WebFavoriteInfo webFavorite, string newName, AsyncCallback callback)
        {
            return BeginRenameWebFolderItem(webFavorite, newName, callback);
        }

        /// <summary>
        /// Ends an asynchronous operation to rename a webfavorite.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        public void EndRenameWebFavorite(IAsyncResult asyncResult)
        {
            EndRenameWebFolderItem(asyncResult);
        }

        /// <summary>
        /// Begins an asynchronous operation to download a webfavorite.
        /// </summary>
        /// <param name="webFavorite">The webfavorite to download.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginDownloadWebFavorite(WebFavoriteInfo webFavorite, AsyncCallback callback)
        {
            return BeginDownloadWebFile(webFavorite, callback);
        }

        /// <summary>
        /// Ends an asynchronous operation to download a webfavorite.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        /// <returns>A readable stream that contains the webfavorite's content.</returns>
        public Stream EndDownloadWebFavorite(IAsyncResult asyncResult)
        {
            return EndDownloadWebFile(asyncResult);
        }

        /// <summary>
        /// Begins an asynchronous operation to delete a webfavorite.
        /// </summary>
        /// <param name="webFavorite">The webfavorite to be deleted.</param>
        /// <param name="callback">The <see cref="AsyncCallback" /> delegate.</param>
        /// <returns>An <see cref="IAsyncResult" /> that references the asynchronous operation.</returns>
        public IAsyncResult BeginDeleteWebFavorite(WebFavoriteInfo webFavorite, AsyncCallback callback)
        {
            return BeginDeleteWebFolderItem(webFavorite, callback);
        }

        /// <summary>
        /// Ends an asynchronous operation to delete a webfavorite.
        /// </summary>
        /// <param name="asyncResult">The pending result of the asynchronous operation.</param>
        public void EndDeleteWebFavorite(IAsyncResult asyncResult)
        {
            EndDeleteWebFolderItem(asyncResult);
        }
        #endregion

        #endregion

        #endregion

        #region Protected Methods

        /// <summary>
        /// Raises the <see cref="E:UploadWebFileProgressChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="UploadWebFileProgressChangedEventArgs"/> instance containing the event data.</param>
        protected void OnUploadValuesProgressChanged(UploadWebFileProgressChangedEventArgs e)
        {
            if (UploadWebFileProgressChanged != null)
                UploadWebFileProgressChanged(this, e);
        }

        #endregion

        #region Private Methods

        #region Uri Related Methods

        /// <summary>
        /// Gets the URI for creating a webfolder in SkyDrive's root.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns>The URI for creating a webfolder in SkyDrive's root.</returns>
        private static Uri GetRootWebFolderCreateUri(WebSession session)
        {
            Uri uriCreate = null;
            if (session != null && !String.IsNullOrEmpty(session.Cid))
                uriCreate = new Uri(String.Format(
                    CultureInfo.InvariantCulture, 
                    "https://{0}.skydrive.live.com/newlivefolder.aspx",
                    session.Cid));
            return uriCreate;
        }

        /// <summary>
        /// Gets the URI for creating a webfolder in a sub webfolder.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="webFolderParent">The webfolder where the new webfolder is to be created.</param>
        /// <returns>The URI for creating a webfolder in a sub webfolder.</returns>
        private static Uri GetSubWebFolderCreateUri(WebSession session, WebFolderInfo webFolderParent)
        {
            Uri uriCreate = null;
            if (session != null && !String.IsNullOrEmpty(session.Cid) && webFolderParent != null)
                uriCreate = new Uri(String.Format(
                    CultureInfo.InvariantCulture, 
                    "{0}://{1}.skydrive.live.com/newfolder.aspx{2}?ct=skydrive",
                    webFolderParent.ShareType != WebFolderItemShareType.Public ? "https" : "http",
                    session.Cid,
                    webFolderParent.PathUrl));
            return uriCreate;
        }

        /// <summary>
        /// Gets the URI for changing description of a webfolderitem.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="webFolderItem">The webfolderitem to be changed.</param>
        /// <returns>The URI for changing description of a webfolderitem.</returns>
        private static Uri GetWebFolderItemChangeDescriptionUri(WebSession session, WebFolderItemInfo webFolderItem)
        {
            Uri uriCreate = null;
            if (session != null && !String.IsNullOrEmpty(session.Cid) && webFolderItem != null)
                uriCreate = new Uri(String.Format(
                    CultureInfo.InvariantCulture, 
                    "{0}://{1}.skydrive.live.com/inlineedit.ashx{2}",
                    webFolderItem.ShareType != WebFolderItemShareType.Public ? "https" : "http",
                    session.Cid,
                    webFolderItem.PathUrl));
            return uriCreate;
        }

        /// <summary>
        /// Gets the URI for creating a webfavorite in a sub webfolder.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="webFolderParent">The webfolder where the webfavorite is to be created.</param>
        /// <returns>The URI for creating a webfavorite in a sub webfolder.</returns>
        private static Uri GetWebFavoriteCreateUri(WebSession session, WebFolderInfo webFolderParent)
        {
            Uri uriCreate = null;
            if (session != null && !String.IsNullOrEmpty(session.Cid) && webFolderParent != null)
                uriCreate = new Uri(String.Format(
                    CultureInfo.InvariantCulture, 
                    "{0}://{1}.skydrive.live.com/createfavorite.aspx{2}?ref=1",
                    webFolderParent.ShareType != WebFolderItemShareType.Public ? "https" : "http",
                    session.Cid,
                    webFolderParent.PathUrl));
            return uriCreate;
        }

        /// <summary>
        /// Gets the URI for uploading a webfile to a sub webfolder.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="webFolderParent">The webfolder where webfile is to be uploaded.</param>
        /// <returns>The URI for uploading a webfile to a sub webfolder.</returns>
        private static Uri GetWebFileUploadUri(WebSession session, WebFolderInfo webFolderParent)
        {
            Uri uriUpload = null;
            if (session != null && !String.IsNullOrEmpty(session.Cid) && webFolderParent != null)
                uriUpload = new Uri(String.Format(
                    CultureInfo.InvariantCulture, 
                    "{0}://{1}.skydrive.live.com/upload.aspx{2}",
                    webFolderParent.ShareType != WebFolderItemShareType.Public ? "https" : "http",
                    session.Cid,
                    webFolderParent.PathUrl));
            return uriUpload;
        }

        /// <summary>
        /// Gets the URI for browsing a webfolder in SkyDrive's root.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns>The URI for browsing a webfolder in SkyDrive's root.</returns>
        private static Uri GetRootWebFolderBrowseUri(WebSession session)
        {
            Uri uriBrowse = null;
            if (session != null && !String.IsNullOrEmpty(session.Cid))
                uriBrowse = new Uri(String.Format(
                    CultureInfo.InvariantCulture, 
                    "http://{0}.skydrive.live.com/home.aspx", 
                    session.Cid));
            return uriBrowse;
        }

        /// <summary>
        /// Gets the URI for browsing a webfolder in a sub webfolder.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="webFolderParent">The webfolder where sub webfolder is to be browsed.</param>
        /// <returns>The URI for browsing a webfolder in a sub webfolder.</returns>
        private static Uri GetSubWebFolderBrowseUri(WebSession session, WebFolderInfo webFolderParent)
        {
            Uri uriBrowse = null;
            if (session != null && !String.IsNullOrEmpty(session.Cid) && webFolderParent != null)
                uriBrowse = new Uri(String.Format(
                    CultureInfo.InvariantCulture, 
                    "{0}://{1}.skydrive.live.com/browse.aspx{2}",
                    webFolderParent.ShareType != WebFolderItemShareType.Public ? "https" : "http",
                    session.Cid,
                    webFolderParent.PathUrl));
            return uriBrowse;
        }

        /// <summary>
        /// Gets the URI for viewing a webfolderitem.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="webFolderItem">The webfolderitem to view.</param>
        /// <returns>The URI for viewing a webfolderitem.</returns>
        private static Uri GetWebFolderItemViewUri(WebSession session, WebFolderItemInfo webFolderItem)
        {
            string urlView = GetWebFolderItemViewUrl(session, webFolderItem);
            return urlView != null ? new Uri(urlView) : null;
        }

        /// <summary>
        /// Gets the URL for viewing a webfolderitem.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="webFolderItem">The webfolderitem to view.</param>
        /// <returns>The URL for viewing a webfolderitem.</returns>
        private static string GetWebFolderItemViewUrl(WebSession session, WebFolderItemInfo webFolderItem)
        {
            string urlView = null;
            if (session != null && !String.IsNullOrEmpty(session.Cid) && webFolderItem != null)
                urlView = String.Format(
                    CultureInfo.InvariantCulture,
                    "{0}://{1}.skydrive.live.com/self.aspx{2}",
                    webFolderItem.ShareType != WebFolderItemShareType.Public ? "https" : "http",
                    session.Cid,
                    webFolderItem.PathUrl);
            return urlView;
        }

        /// <summary>
        /// Gets the URI for renaming a webfolderitem.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="webFolderItem">The webfolderitem to rename.</param>
        /// <returns>The URI for renaming a webfolderitem.</returns>
        private static Uri GetWebFolderItemRenameUri(WebSession session, WebFolderItemInfo webFolderItem)
        {
            Uri uriRename = null;
            if (session != null && !String.IsNullOrEmpty(session.Cid) && webFolderItem != null)
                uriRename = new Uri(String.Format(
                    CultureInfo.InvariantCulture, 
                    "{0}://{1}.skydrive.live.com/rename.aspx{2}?ref={3}",
                    webFolderItem.ShareType != WebFolderItemShareType.Public ? "https" : "http",
                    session.Cid,
                    webFolderItem.PathUrl,
                    webFolderItem.ItemType == WebFolderItemType.Folder ? "1" : "2"));
            return uriRename;
        }

        /// <summary>
        /// Gets the URI for moving/copying a webfolderitem into a webfolder.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="webFolderDestination">The destination webfolder where the webfolderitem is being moved/copied.</param>
        /// <param name="uriMoveCopyHome">The home URI for moving/copying a webfolderitem.</param>
        /// <returns>The URI for moving/copying a webfolderitem into a webfolder.</returns>
        private static Uri GetWebFolderDestinationMoveCopyUri(WebSession session, WebFolderInfo webFolderDestination, Uri uriMoveCopyHome)
        {
            Uri uriMoveCopy = null;
            if (session != null && !String.IsNullOrEmpty(session.Cid) && webFolderDestination != null && uriMoveCopyHome != null)
                uriMoveCopy = new Uri(String.Format(
                    CultureInfo.InvariantCulture,
                    "https://{0}.skydrive.live.com/movecopy.aspx{1}{2}",
                    session.Cid,
                    webFolderDestination.PathUrl,
                    uriMoveCopyHome.Query));
            return uriMoveCopy;
        }

        /// <summary>
        /// Gets the URI for changing a webfolder's ContentType in SkyDrive's root.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="webFolder">The webfolder to change.</param>
        /// <returns>The URI for changing a webfolder's ContentType in SkyDrive's root</returns>
        private static Uri GetRootWebFolderChangeContentTypeUri(WebSession session, WebFolderInfo webFolder)
        {
            Uri uriChange = null;
            if (session != null && !String.IsNullOrEmpty(session.Cid) && webFolder != null)
                uriChange = new Uri(String.Format(
                    CultureInfo.InvariantCulture,
                    "{0}://{1}.skydrive.live.com/changefoldertype.aspx{2}",
                    webFolder.ShareType != WebFolderItemShareType.Public ? "https" : "http",
                    session.Cid,
                    webFolder.PathUrl));
            return uriChange;
        }

        /// <summary>
        /// Parses the URI for downloading a webfolderitem.
        /// </summary>
        /// <param name="html">The HTML to parse.</param>
        /// <param name="webFolderItem">The webfolderitem to get the download URI for.</param>
        /// <returns>The webfolderitem's direct download URI.</returns>
        private static Uri ParseWebFolderItemDownloadUri(string html, WebFolderItemInfo webFolderItem)
        {
            string urlDownload = ParseWebFolderItemDownloadUrl(html, webFolderItem);
            return urlDownload != null ? new Uri(urlDownload) : null;
        }

        /// <summary>
        /// Parses the URL for downloading a webfolderitem.
        /// </summary>
        /// <param name="html">The HTML to parse.</param>
        /// <param name="webFolderItem">The webfolderitem to get the download URL for.</param>
        /// <returns>The webfolderitem's direct download URL.</returns>
        private static string ParseWebFolderItemDownloadUrl(string html, WebFolderItemInfo webFolderItem)
        {
            string urlDownload = null;

            switch (webFolderItem.ItemType)
            {
                case WebFolderItemType.Folder:
                    //var webFolder = (WebFolderInfo)webFolderItem;
                    HtmlDocument htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(html);

                    HtmlNode nodeDownloadLink = htmlDocument.GetElementbyId("downloadFolder");
                    // If webfolder has no sub webfolderitems, download link is not accessable.
                    if (nodeDownloadLink != null)
                        urlDownload = UriHelper.FormatUrl(HtmlDocumentHelper.DecodeUnicodeString(
                            nodeDownloadLink.Attributes["href"].Value));
                    break;
                case WebFolderItemType.File:
                    var webFile = (WebFileInfo)webFolderItem;
                    MatchCollection matchDownloadUrls = RegexHelper.Matches(RegexWebFileDownloadUrl, html);
                    foreach (Match matchDownloadUrl in matchDownloadUrls)
                    {
                        Uri uri = UriHelper.GetUri(HtmlDocumentHelper.DecodeJavascriptString(
                            matchDownloadUrl.Groups["URL"].Value));
                        if (uri.LocalPath.EndsWith(webFile.FullName, StringComparison.OrdinalIgnoreCase))
                        {
                            urlDownload = uri.AbsoluteUri;
                            break;
                        }
                    }
                    break;
            }
            return urlDownload;
        }

        #endregion

        #region ViewType Related List Parsing Methods

        /// <summary>
        /// Parses a sub webfolder if ViewType is Details.
        /// </summary>
        /// <param name="webFolderParent">The webfolder of which webfolderitems are to be parsed.</param>
        /// <param name="responseDocument">The HTML document of an HTTP response.</param>
        /// <returns>The list of webfolderitems.</returns>
        private WebFolderItemInfo[] ParseSubWebFolderViewDetails(WebFolderInfo webFolderParent, HtmlDocument responseDocument)
        {
            List<WebFolderItemInfo> lWebFolderItem = new List<WebFolderItemInfo>();

            HtmlNode nodeContainer = responseDocument.GetElementbyId("dvContainer");

            if (nodeContainer != null)
            {
                HtmlNodeCollection nodeItems = nodeContainer
                     .SelectNodes(".//table[@class='dvTable']");
                if (nodeItems != null)
                    foreach (HtmlNode nodeItem in nodeItems)
                    {
                        WebFolderItemInfo webFolderItem = null;

                        HtmlNode nodeItemImageContentType = nodeItem.SelectSingleNode(".//span[contains(@class, 'spriteWrapper')]//img");
                        HtmlNode nodeItemLink = nodeItem.SelectSingleNode(".//a[@class='dvName']");
                        HtmlNode nodeItemCreatorName = nodeItem.SelectSingleNode(".//div[@class='dvPerson']");
                        HtmlNode nodeItemWebAddress = nodeItem.SelectSingleNode(".//div[@class='dvCommands']/div/a");

                        Match matchWebFolderItemTitle = RegexHelper.Match(RegexWebFolderItemTitle,
                            HtmlDocumentHelper.DecodeUnicodeString(nodeItemLink.Attributes["title"].Value));

                        WebFolderItemType itemType = nodeItemLink.Attributes["href"].Value.Contains("browse.aspx") ?
                            WebFolderItemType.Folder : WebFolderItemType.File;

                        switch (itemType)
                        {
                            case WebFolderItemType.Folder:
                                webFolderItem = new WebFolderInfo
                                {
                                    Name = HtmlDocumentHelper.DecodeUnicodeString(nodeItemLink.InnerText),
                                    ShareType = webFolderParent.ShareType,
                                    CreatorName = HtmlDocumentHelper.DecodeUnicodeString(nodeItemCreatorName.InnerText),
                                    //TODO: Parse date info from dvTime div
                                    //DateModified = DateTime.Parse(
                                    //    matchWebFolderItemTitle.Groups["DateModified"].Value,
                                    //    CultureInfo.InvariantCulture),
                                    PathUrl = WebFolderItemHelper.ParsePathUrl(nodeItemLink.Attributes["href"].Value),

                                    ContentType = webFolderParent.ContentType
                                };
                                break;
                            case WebFolderItemType.File:
                                string itemContentType = matchWebFolderItemTitle.Groups["Type"].Value ?? String.Empty;
                                if (itemContentType.StartsWith(WebFavoriteInfo.WebFavoriteContentType, StringComparison.OrdinalIgnoreCase))
                                    webFolderItem = new WebFavoriteInfo
                                    {
                                        Name = HtmlDocumentHelper.DecodeUnicodeString(nodeItemLink.InnerText),
                                        ShareType = webFolderParent.ShareType,
                                        CreatorName = HtmlDocumentHelper.DecodeUnicodeString(nodeItemCreatorName.InnerText),
                                        DateAdded = !String.IsNullOrEmpty(matchWebFolderItemTitle.Groups["DateAdded"].Value) ?
                                            DateTime.Parse(
                                                HtmlDocumentHelper.DecodeUnicodeString(matchWebFolderItemTitle.Groups["DateAdded"].Value),
                                                CultureInfo.InvariantCulture) : (DateTime?)null,
                                        DateModified = !String.IsNullOrEmpty(matchWebFolderItemTitle.Groups["DateModified"].Value) ?
                                            DateTime.Parse(
                                                HtmlDocumentHelper.DecodeUnicodeString(matchWebFolderItemTitle.Groups["DateModified"].Value),
                                                CultureInfo.InvariantCulture) : (DateTime?)null,
                                        PathUrl = WebFolderItemHelper.ParsePathUrl(nodeItemLink.Attributes["href"].Value),

                                        Extension = System.IO.Path.GetExtension(matchWebFolderItemTitle.Groups["Name"].Value),
                                        ContentType = itemContentType,
                                        Size = matchWebFolderItemTitle.Groups["Size"].Value,

                                        WebAddress = nodeItemWebAddress != null ?
                                            HtmlDocumentHelper.DecodeUnicodeString(nodeItemWebAddress.Attributes["href"].Value) :
                                            null
                                    };
                                else
                                    webFolderItem = new WebFileInfo
                                    {
                                        Name = HtmlDocumentHelper.DecodeUnicodeString(nodeItemLink.InnerText),
                                        ShareType = webFolderParent.ShareType,
                                        CreatorName = HtmlDocumentHelper.DecodeUnicodeString(nodeItemCreatorName.InnerText),
                                        DateAdded = !String.IsNullOrEmpty(matchWebFolderItemTitle.Groups["DateAdded"].Value) ?
                                            DateTime.Parse(
                                                HtmlDocumentHelper.DecodeUnicodeString(matchWebFolderItemTitle.Groups["DateAdded"].Value),
                                                CultureInfo.InvariantCulture) : (DateTime?)null,
                                        DateModified = !String.IsNullOrEmpty(matchWebFolderItemTitle.Groups["DateModified"].Value) ?
                                            DateTime.Parse(
                                                HtmlDocumentHelper.DecodeUnicodeString(matchWebFolderItemTitle.Groups["DateModified"].Value),
                                                CultureInfo.InvariantCulture) : (DateTime?)null,
                                        PathUrl = WebFolderItemHelper.ParsePathUrl(nodeItemLink.Attributes["href"].Value),

                                        Extension = System.IO.Path.GetExtension(matchWebFolderItemTitle.Groups["Name"].Value),
                                        ContentType = itemContentType,
                                        Size = matchWebFolderItemTitle.Groups["Size"].Value,
                                    };
                                break;
                            default:
                                throw new NotSupportedException(itemType.ToString());
                        }
                        webFolderItem.WebIcon = ParseWebFolderItemIcon(nodeItemImageContentType, null, null);

                        if (webFolderItem != null)
                        {
                            webFolderItem.ViewUrl = GetWebFolderItemViewUrl(Session, webFolderItem);

                            lWebFolderItem.Add(webFolderItem);
                        }
                    }
            }

            return lWebFolderItem.ToArray();
        }

        /// <summary>
        /// Parses a sub webfolder if ViewType is Icons.
        /// </summary>
        /// <param name="webFolderParent">The webfolder of which webfolderitems are to be parsed.</param>
        /// <param name="responseDocument">The HTML document of an HTTP response.</param>
        /// <returns>The list of webfolderitems.</returns>
        private WebFolderItemInfo[] ParseSubWebFolderViewIcons(WebFolderInfo webFolderParent, HtmlDocument responseDocument)
        {
            List<WebFolderItemInfo> lWebFolderItem = new List<WebFolderItemInfo>();

            HtmlNodeCollection nodeItems = responseDocument.DocumentNode
                .SelectNodes("//div[contains(@class, 'tvItemContainer')]");
            if (nodeItems != null)
                foreach (HtmlNode nodeItem in nodeItems)
                {
                    HtmlNode nodeItemImageContentType = nodeItem.SelectSingleNode(".//img[contains(@class, 'tvFileTypeImage')]");
                    HtmlNode nodeItemImageContent = nodeItem.SelectSingleNode(".//img[@class='tvItemImagePlaceholder']");
                    HtmlNode nodeItemLink = nodeItem.SelectSingleNode(".//a[@class='tvLink']");
                    
                    Match matchWebFolderItemTitle = RegexHelper.Match(RegexWebFolderItemTitle, 
                        HtmlDocumentHelper.DecodeUnicodeString(nodeItemLink.Attributes["title"].Value));
                    WebFolderItemType itemType = nodeItemLink.Attributes["href"].Value.Contains("browse.aspx") ?
                        WebFolderItemType.Folder : WebFolderItemType.File;

                    WebFolderItemInfo webFolderItem = null;
                    switch (itemType)
                    {
                        case WebFolderItemType.Folder:
                            webFolderItem = new WebFolderInfo
                            {
                                Name = HtmlDocumentHelper.DecodeUnicodeString(nodeItemLink.InnerText),
                                ShareType = webFolderParent.ShareType,
                                PathUrl = WebFolderItemHelper.ParsePathUrl(nodeItemLink.Attributes["href"].Value),
                            };
                            break;
                        case WebFolderItemType.File:
                            string itemContentType = matchWebFolderItemTitle.Groups["Type"].Value ?? String.Empty;
                            if (itemContentType.StartsWith(WebFavoriteInfo.WebFavoriteContentType, StringComparison.OrdinalIgnoreCase))
                                webFolderItem = new WebFavoriteInfo
                                {
                                    Name = HtmlDocumentHelper.DecodeUnicodeString(nodeItemLink.InnerText),
                                    ShareType = webFolderParent.ShareType,
                                    DateAdded = !String.IsNullOrEmpty(matchWebFolderItemTitle.Groups["DateAdded"].Value) ?
                                        DateTime.Parse(
                                            HtmlDocumentHelper.DecodeUnicodeString(matchWebFolderItemTitle.Groups["DateAdded"].Value),
                                            CultureInfo.InvariantCulture) : (DateTime?)null,
                                    DateModified = !String.IsNullOrEmpty(matchWebFolderItemTitle.Groups["DateModified"].Value) ?
                                        DateTime.Parse(
                                            HtmlDocumentHelper.DecodeUnicodeString(matchWebFolderItemTitle.Groups["DateModified"].Value),
                                            CultureInfo.InvariantCulture) : (DateTime?)null,

                                    Extension = System.IO.Path.GetExtension(matchWebFolderItemTitle.Groups["Name"].Value),
                                    ContentType = matchWebFolderItemTitle.Groups["Type"].Value,
                                    Size = matchWebFolderItemTitle.Groups["Size"].Value
                                };
                            else
                                webFolderItem = new WebFileInfo
                                {
                                    Name = HtmlDocumentHelper.DecodeUnicodeString(nodeItemLink.InnerText),
                                    ShareType = webFolderParent.ShareType,
                                    DateAdded = !String.IsNullOrEmpty(matchWebFolderItemTitle.Groups["DateAdded"].Value) ?
                                        DateTime.Parse(
                                            HtmlDocumentHelper.DecodeUnicodeString(matchWebFolderItemTitle.Groups["DateAdded"].Value),
                                            CultureInfo.InvariantCulture) : (DateTime?)null,
                                    DateModified = !String.IsNullOrEmpty(matchWebFolderItemTitle.Groups["DateModified"].Value) ?
                                        DateTime.Parse(
                                            HtmlDocumentHelper.DecodeUnicodeString(matchWebFolderItemTitle.Groups["DateModified"].Value),
                                            CultureInfo.InvariantCulture) : (DateTime?)null,
                                    PathUrl = WebFolderItemHelper.ParsePathUrl(nodeItemLink.Attributes["href"].Value),

                                    Extension = System.IO.Path.GetExtension(matchWebFolderItemTitle.Groups["Name"].Value),
                                    ContentType = matchWebFolderItemTitle.Groups["Type"].Value,
                                    Size = matchWebFolderItemTitle.Groups["Size"].Value
                                };
                            break;
                        default:
                            throw new NotSupportedException(itemType.ToString());
                    }

                    webFolderItem.ViewUrl = GetWebFolderItemViewUrl(Session, webFolderItem);
                    webFolderItem.WebIcon = ParseWebFolderItemIcon(
                        nodeItemImageContentType,
                        nodeItemImageContent,
                        null);
                    if (webFolderItem.WebIcon != null)
                    {
                        if (webFolderItem.WebIcon.ContentTypeWebImage != null &&
                            webFolderItem.WebIcon.ContentTypeWebImage != null)
                        {
                            webFolderItem.WebIcon.ContentWebImageOffsetX = 0;
                            webFolderItem.WebIcon.ContentWebImageOffsetY = 8;
                        }
                    }

                    lWebFolderItem.Add(webFolderItem);
                }

            return lWebFolderItem.ToArray();
        }

        /// <summary>
        /// Parses a sub webfolder if ViewType is Thumbnails.
        /// </summary>
        /// <param name="webFolderParent">The webfolder of which webfolderitems are to be parsed.</param>
        /// <param name="responseDocument">The HTML document of an HTTP response.</param>
        /// <returns>The list of webfolderitems.</returns>
        private WebFolderItemInfo[] ParseSubWebFolderViewThumbnails(WebFolderInfo webFolderParent, HtmlDocument responseDocument)
        {
            List<WebFolderItemInfo> lWebFolderItem = new List<WebFolderItemInfo>();

            HtmlNodeCollection nodeItems = responseDocument.GetElementbyId("thumbnailWrapper")
                 .SelectNodes("//div[contains(@class, 'tibk')]");
            if (nodeItems != null)
                foreach (HtmlNode nodeItem in nodeItems)
                {
                    HtmlNode nodeItemImageContentType = nodeItem.SelectSingleNode(".//img[contains(@class, 'tvFileTypeImage')]");
                    HtmlNode nodeItemImageContent = nodeItem.SelectSingleNode(".//img[@class='']");
                    HtmlNode nodeItemLink = nodeItem.SelectSingleNode(".//a[contains(@class, 'tLink')]");
                    HtmlNode nodeItemImageTransparent = nodeItemLink.SelectSingleNode(".//img");
                    
                    Match matchWebFolderItemTitle = RegexHelper.Match(RegexWebFolderItemTitle, 
                        HtmlDocumentHelper.DecodeUnicodeString(nodeItemImageTransparent.Attributes["title"].Value));
                    WebFolderItemType itemType = nodeItemLink.Attributes["href"].Value
                        .Contains("browse.aspx") ? WebFolderItemType.Folder : WebFolderItemType.File;

                    WebFolderItemInfo webFolderItem = null;
                    switch (itemType)
                    {
                        case WebFolderItemType.Folder:
                            webFolderItem = new WebFolderInfo
                            {
                                Name = matchWebFolderItemTitle.Groups["Name"].Value,
                                ShareType = webFolderParent.ShareType,
                                PathUrl = WebFolderItemHelper.ParsePathUrl(nodeItemLink.Attributes["href"].Value),
                            };
                            break;
                        case WebFolderItemType.File:
                            string itemContentType = matchWebFolderItemTitle.Groups["Type"].Value ?? String.Empty;
                            if (itemContentType.StartsWith(WebFavoriteInfo.WebFavoriteContentType, StringComparison.OrdinalIgnoreCase))
                                webFolderItem = new WebFavoriteInfo
                                {
                                    Name = Path.GetFileNameWithoutExtension(matchWebFolderItemTitle.Groups["Name"].Value),
                                    ShareType = webFolderParent.ShareType,
                                    DateAdded = !String.IsNullOrEmpty(matchWebFolderItemTitle.Groups["DateAdded"].Value) ?
                                        DateTime.Parse(
                                            HtmlDocumentHelper.DecodeUnicodeString(matchWebFolderItemTitle.Groups["DateAdded"].Value),
                                            CultureInfo.InvariantCulture) : (DateTime?)null,
                                    DateModified = !String.IsNullOrEmpty(matchWebFolderItemTitle.Groups["DateModified"].Value) ?
                                        DateTime.Parse(
                                            HtmlDocumentHelper.DecodeUnicodeString(matchWebFolderItemTitle.Groups["DateModified"].Value),
                                            CultureInfo.InvariantCulture) : (DateTime?)null,
                                    PathUrl = WebFolderItemHelper.ParsePathUrl(nodeItemLink.Attributes["href"].Value),

                                    Extension = System.IO.Path.GetExtension(matchWebFolderItemTitle.Groups["Name"].Value),
                                    ContentType = matchWebFolderItemTitle.Groups["Type"].Value,
                                    Size = matchWebFolderItemTitle.Groups["Size"].Value
                                };
                            else
                                webFolderItem = new WebFileInfo
                                {
                                    Name = Path.GetFileNameWithoutExtension(matchWebFolderItemTitle.Groups["Name"].Value),
                                    ShareType = webFolderParent.ShareType,
                                    DateAdded = !String.IsNullOrEmpty(matchWebFolderItemTitle.Groups["DateAdded"].Value) ?
                                        DateTime.Parse(
                                            HtmlDocumentHelper.DecodeUnicodeString(matchWebFolderItemTitle.Groups["DateAdded"].Value),
                                            CultureInfo.InvariantCulture) : (DateTime?)null,
                                    DateModified = !String.IsNullOrEmpty(matchWebFolderItemTitle.Groups["DateModified"].Value) ?
                                        DateTime.Parse(
                                            HtmlDocumentHelper.DecodeUnicodeString(matchWebFolderItemTitle.Groups["DateModified"].Value),
                                            CultureInfo.InvariantCulture) : (DateTime?)null,
                                    PathUrl = WebFolderItemHelper.ParsePathUrl(nodeItemLink.Attributes["href"].Value),

                                    Extension = System.IO.Path.GetExtension(matchWebFolderItemTitle.Groups["Name"].Value),
                                    ContentType = matchWebFolderItemTitle.Groups["Type"].Value,
                                    Size = matchWebFolderItemTitle.Groups["Size"].Value
                                };
                            break;
                        default:
                            throw new NotSupportedException(itemType.ToString());
                    }

                    webFolderItem.ViewUrl = GetWebFolderItemViewUrl(Session, webFolderItem);
                    webFolderItem.WebIcon = ParseWebFolderItemIcon(
                        nodeItemImageContentType,
                        nodeItemImageContent,
                        null);
                    if (webFolderItem.WebIcon != null)
                    {
                        if (webFolderItem.WebIcon.ContentTypeWebImage != null &&
                            webFolderItem.WebIcon.ContentTypeWebImage != null)
                        {
                            webFolderItem.WebIcon.ContentWebImageOffsetX = 0;
                            webFolderItem.WebIcon.ContentWebImageOffsetY = 8;
                        }
                    }

                    lWebFolderItem.Add(webFolderItem);
                }

            return lWebFolderItem.ToArray();
        }

        #endregion

        #region WebFolderItemIcon Related Parsing Methods

        /// <summary>
        /// Parses a webfolderitemicon from HTML.
        /// </summary>
        /// <param name="nodeImageContentType">The HTML image of ContentType to parse.</param>
        /// <param name="nodeImageContent">The HTML image of content to parse.</param>
        /// <param name="nodeImageShareType">The HTML image of ShareType to parse.</param>
        /// <returns>The webfolderitemicon.</returns>
        private static WebFolderItemIconInfo ParseWebFolderItemIcon(HtmlNode nodeImageContentType, HtmlNode nodeImageContent, HtmlNode nodeImageShareType)
        {
            WebFolderItemIconInfo webFolderItemIcon = null;

            if (nodeImageContentType != null || nodeImageContent != null || nodeImageShareType != null)
            {
                webFolderItemIcon = new WebFolderItemIconInfo();

                webFolderItemIcon.ContentTypeWebImage = ParseWebFolderItemImage(nodeImageContentType);
                webFolderItemIcon.ContentWebImage = ParseWebFolderItemImage(nodeImageContent);
                webFolderItemIcon.ShareTypeWebImage = ParseWebFolderItemImage(nodeImageShareType);
            }

            return webFolderItemIcon;
        }

        /// <summary>
        /// Parses a webfolderitemimage from HTML.
        /// </summary>
        /// <param name="nodeImage">The HTML image to parse.</param>
        /// <returns>The webfolderitemimage.</returns>
        private static WebFolderItemImageInfo ParseWebFolderItemImage(HtmlNode nodeImage)
        {
            WebFolderItemImageInfo webFolderItemImage = null;

            if (nodeImage != null)
            {
                webFolderItemImage = new WebFolderItemImageInfo
                {
                    WebAddress = UriHelper.FormatUrl(nodeImage.Attributes["src"].Value)
                };

                if (nodeImage.Attributes["class"] != null &&
                    RegexHelper.IsMatch(RegexWebFolderItemImageClass, nodeImage.Attributes["class"].Value))
                {
                    Match matchWebFolderItemImageClass = RegexHelper.Match(
                        RegexWebFolderItemImageClass,
                        nodeImage.Attributes["class"].Value);

                    webFolderItemImage.Width = Int32.Parse(matchWebFolderItemImageClass.Groups["Width"].Value, CultureInfo.InvariantCulture);
                    webFolderItemImage.Height = Int32.Parse(matchWebFolderItemImageClass.Groups["Height"].Value, CultureInfo.InvariantCulture);
                    webFolderItemImage.LocationX = -Int32.Parse(matchWebFolderItemImageClass.Groups["LocationX"].Value, CultureInfo.InvariantCulture);
                    webFolderItemImage.LocationY = -Int32.Parse(matchWebFolderItemImageClass.Groups["LocationY"].Value, CultureInfo.InvariantCulture);
                }
                else if (nodeImage.Attributes["style"] != null)
                {
                    NameValueCollection styleWebFolderItemImage = HtmlDocumentHelper.ParseStyleValue(nodeImage.Attributes["style"].Value);
                    if (styleWebFolderItemImage["width"] != null)
                        webFolderItemImage.Width = (int)System.Web.UI.WebControls.Unit.Parse(
                            styleWebFolderItemImage["width"], CultureInfo.InvariantCulture).Value;
                    if (styleWebFolderItemImage["height"] != null)
                        webFolderItemImage.Height = (int)System.Web.UI.WebControls.Unit.Parse(
                            styleWebFolderItemImage["height"], CultureInfo.InvariantCulture).Value;
                    if (styleWebFolderItemImage["left"] != null)
                        webFolderItemImage.LocationX = -(int)System.Web.UI.WebControls.Unit.Parse(
                            styleWebFolderItemImage["left"], CultureInfo.InvariantCulture).Value;
                    if (styleWebFolderItemImage["top"] != null)
                        webFolderItemImage.LocationY = -(int)System.Web.UI.WebControls.Unit.Parse(
                            styleWebFolderItemImage["top"], CultureInfo.InvariantCulture).Value;
                }

                if (nodeImage.Attributes["width"] != null)
                    webFolderItemImage.Width = (int)System.Web.UI.WebControls.Unit.Parse(
                        nodeImage.Attributes["width"].Value, CultureInfo.InvariantCulture).Value;
                if (nodeImage.Attributes["height"] != null)
                    webFolderItemImage.Height = (int)System.Web.UI.WebControls.Unit.Parse(
                        nodeImage.Attributes["height"].Value, CultureInfo.InvariantCulture).Value;
            }

            return webFolderItemImage;
        }

        #endregion

        #region Form Related Parsing Methods

        /// <summary>
        /// Parses an HTML form tags which can participate in a postback.
        /// </summary>
        /// <param name="html">The HTML to parse.</param>
        /// <param name="formName">The name of the form to parse.</param>
        /// <returns>The parameters found in the specified form.</returns>
        private static NameValueCollection ParseFormPostBackParameters(string html, string formName)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            return ParseFormPostBackParameters(document, formName);
        }

        /// <summary>
        /// Parses an HTML form tags which can participate in a postback.
        /// </summary>
        /// <param name="document">The HTML document to parse.</param>
        /// <param name="formName">The name of the form to parse.</param>
        /// <returns>The parameters found in the specified form.</returns>
        private static NameValueCollection ParseFormPostBackParameters(HtmlDocument document, string formName)
        {
            HtmlNode nodeForm = document.DocumentNode.SelectSingleNode(
                String.Format(CultureInfo.InvariantCulture, "//form[@name='{0}']", formName));
            HtmlNodeCollection nodeInputs = nodeForm
                .SelectNodes(".//input[@type='text' or @type='hidden' or @type='file'] | .//textarea | .//select");

            NameValueCollection parameters = new NameValueCollection();
            foreach (HtmlNode nodeInput in nodeInputs)
                if (nodeInput.Attributes["name"] != null)
                    parameters.Add(
                        nodeInput.Attributes["name"].Value,
                        nodeInput.Attributes["value"] != null ? nodeInput.Attributes["value"].Value : null);

            return parameters;
        }

        #endregion

        #endregion

    }
}
