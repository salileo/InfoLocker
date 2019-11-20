using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HgCo.WindowsLive.SkyDrive
{
    /// <summary>
    /// Provides webfolder content specific data.
    /// </summary>
    [Serializable]
    public class WebFolderInfo : WebFolderItemInfo
    {
        #region Properties
        
        /// <summary>
        /// Gets or sets ContentType.
        /// </summary>
        /// <value>The ContentType.</value>
        public WebFolderContentType ContentType { get; set; }

        /// <summary>
        /// Gets a value indicating whether this webfolder is a root webfolder.
        /// </summary>
        /// <value><c>true</c> if this webfolder is root; otherwise, <c>false</c>.</value>
        public bool IsRoot 
        { 
            get 
            {
                bool isRoot = false;
                if (!String.IsNullOrEmpty(PathUrl))
                    isRoot = PathUrl.LastIndexOf(PathUrlSegmentDelimiter) == 0;
                return isRoot;
            } 
        }

        /// <summary>
        /// Gets or sets the sub webfolderitems.
        /// </summary>
        /// <value>The sub webfolderitems.</value>
        public Collection<WebFolderItemInfo> SubItems { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WebFolderInfo"/> class.
        /// </summary>
        public WebFolderInfo()
        {
            ItemType = WebFolderItemType.Folder;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the webfolders from the list of SubItems.
        /// </summary>
        /// <returns>The list of webfolders.</returns>
        public WebFolderInfo[] GetSubFolders()
        {
            List<WebFolderInfo> lSubfolder = new List<WebFolderInfo>();
            if (SubItems != null)
                foreach (WebFolderItemInfo webFolderItem in SubItems)
                    if (webFolderItem != null && webFolderItem.ItemType == WebFolderItemType.Folder)
                        lSubfolder.Add(webFolderItem as WebFolderInfo);
            return lSubfolder.ToArray();
        }

        /// <summary>
        /// Gets the webfiles from the list of SubItems.
        /// </summary>
        /// <returns>The list of webfiles.</returns>
        public WebFileInfo[] GetFiles()
        {
            List<WebFileInfo> lFile = new List<WebFileInfo>();
            if (SubItems != null)
                foreach (WebFolderItemInfo webFolderItem in SubItems)
                    if (webFolderItem != null && webFolderItem.ItemType == WebFolderItemType.File)
                        lFile.Add(webFolderItem as WebFileInfo);
            return lFile.ToArray();
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            WebFolderInfo webFolderNew = Clone<WebFolderInfo>();
            webFolderNew.ContentType = ContentType;
            if (SubItems != null)
            {
                webFolderNew.SubItems = new Collection<WebFolderItemInfo>();
                foreach (WebFolderItemInfo subItem in SubItems)
                    webFolderNew.SubItems.Add(subItem as WebFolderItemInfo);
            }
            return webFolderNew;
        }

        /// <summary>
        /// Creates a new object of T that is a copy of the current instance.
        /// </summary>
        /// <typeparam name="T">The type of the new object, it has to be derived from <see cref="WebFolderItemInfo"/>.</typeparam>
        /// <returns>A new object of T that is a copy of this instance.</returns>
        protected override T Clone<T>()
        {
            T webFolderItemNew = base.Clone<T>();
            WebFolderInfo webFolderNew = webFolderItemNew as WebFolderInfo;
            if (webFolderNew != null)
            {
                webFolderNew.ContentType = ContentType;
                if (SubItems != null)
                {
                    webFolderNew.SubItems = new Collection<WebFolderItemInfo>();
                    foreach (WebFolderItemInfo subItem in SubItems)
                        webFolderNew.SubItems.Add(subItem as WebFolderItemInfo);
                }
            }
            return webFolderItemNew;
        }

        /// <summary>
        /// Creates a root webfolder instance.
        /// </summary>
        /// <param name="folderName">The local name of the folder (with or without local path information).</param>
        /// <returns>The root webfolder represents that folder.</returns>
        public static WebFolderInfo CreateRootWebFolderInstance(string folderName)
        {
            var webFolderRoot = CreateRootWebFolderInstance(folderName, WebFolderItemShareType.Private, WebFolderContentType.Documents);
            return webFolderRoot;
        }

        /// <summary>
        /// Creates a root webfolder instance.
        /// </summary>
        /// <param name="folderName">The local name of the folder (with or without local path information).</param>
        /// <param name="shareType">The share type of the root webfolder.</param>
        /// <returns>The root webfolder represents that folder.</returns>
        public static WebFolderInfo CreateRootWebFolderInstance(string folderName, WebFolderItemShareType shareType)
        {
            var webFolderRoot = CreateRootWebFolderInstance(folderName, shareType, WebFolderContentType.Documents);
            return webFolderRoot;
        }

        /// <summary>
        /// Creates a root webfolder instance.
        /// </summary>
        /// <param name="folderName">The local name of the folder (with or without local path information).</param>
        /// <param name="shareType">The share type of the root webfolder.</param>
        /// <param name="contentType">The content type of the root webfolder.</param>
        /// <returns>The root webfolder represents that folder.</returns>
        public static WebFolderInfo CreateRootWebFolderInstance(string folderName, WebFolderItemShareType shareType, WebFolderContentType contentType)
        {
            var diFolder = new System.IO.DirectoryInfo(folderName);
            var webFolderRoot = new WebFolderInfo
            {
                Name = diFolder.Name,
                ShareType = shareType,
                PathUrl = String.Concat(PathUrlSegmentDelimiter, diFolder.Name),
                ContentType = contentType
            };
            return webFolderRoot;
        }

        /// <summary>
        /// Creates a sub webfolder instance.
        /// </summary>
        /// <param name="folderName">The local name of the folder (with or without local path information).</param>
        /// <param name="webFolderParent">The parent webfolder.</param>
        /// <returns>The sub webfolder represents that folder.</returns>
        public static WebFolderInfo CreateSubWebFolderInstance(string folderName, WebFolderInfo webFolderParent)
        {
            var diFolder = new System.IO.DirectoryInfo(folderName);
            var webFolderSub = new WebFolderInfo
            {
                Name = diFolder.Name,
                ShareType = webFolderParent.ShareType,
                PathUrl = String.Concat(webFolderParent.PathUrl, PathUrlSegmentDelimiter, diFolder.Name),
                ContentType = webFolderParent.ContentType
            };
            return webFolderSub;
        }

        #endregion

    }
}
