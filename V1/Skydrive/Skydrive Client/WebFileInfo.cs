using System;

namespace HgCo.WindowsLive.SkyDrive
{
    /// <summary>
    /// Provides webfile content specific data.
    /// </summary>
    [Serializable]
    public class WebFileInfo : WebFolderItemInfo
    {
        #region Properties

        /// <summary>
        /// Gets or sets the extension.
        /// </summary>
        /// <value>The extension.</value>
        public string Extension { get; set; }

        /// <summary>
        /// Gets the full name (= Name + Extension).
        /// </summary>
        /// <value>The full name.</value>
        public string FullName { get { return String.Concat(Name, Extension); } }

        /// <summary>
        /// Gets or sets ContentType.
        /// </summary>
        /// <value>The ContentType.</value>
        public string ContentType { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WebFileInfo"/> class.
        /// </summary>
        public WebFileInfo()
        {
            ItemType = WebFolderItemType.File;
        }
        
        #endregion

        #region Methods

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            WebFileInfo webFileNew = Clone<WebFileInfo>();
            webFileNew.ContentType = ContentType;
            return webFileNew;
        }

        /// <summary>
        /// Creates a new object of T that is a copy of the current instance.
        /// </summary>
        /// <typeparam name="T">The type of the new object, it has to be derived from <see cref="WebFolderItemInfo"/>.</typeparam>
        /// <returns>A new object of T that is a copy of this instance.</returns>
        protected override T Clone<T>()
        {
            T webFolderItemNew = base.Clone<T>();
            WebFileInfo webFileNew = webFolderItemNew as WebFileInfo;
            if (webFileNew != null)
            {
                webFileNew.ContentType = ContentType;
            }
            return webFolderItemNew;
        }

        /// <summary>
        /// Creates a webfile instance.
        /// </summary>
        /// <param name="fileName">The local name of the file (with or without local path information).</param>
        /// <param name="webFolderParent">The parent webfolder.</param>
        /// <returns>The webfile represents that file.</returns>
        public static WebFileInfo CreateWebFileInstance(string fileName, WebFolderInfo webFolderParent)
        {
            var fiFile = new System.IO.FileInfo(fileName);
            var webFile = new WebFileInfo
            {
                Name = System.IO.Path.GetFileNameWithoutExtension(fiFile.Name),
                ShareType = webFolderParent.ShareType,
                PathUrl = String.Concat(webFolderParent.PathUrl, PathUrlSegmentDelimiter, fiFile.Name),

                Extension = fiFile.Extension,
            };
            return webFile;
        }
        #endregion
    }
}
