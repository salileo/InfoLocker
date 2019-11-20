using System;

namespace HgCo.WindowsLive.SkyDrive
{
    /// <summary>
    /// Provides favorite webfile content specific data.
    /// </summary>
    [Serializable]
    public class WebFavoriteInfo : WebFileInfo
    {
        #region Constants
        /// <summary>
        /// The content type of a webfavorite.
        /// </summary>
        public const string WebFavoriteContentType = "Web Address"; 

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the web address.
        /// </summary>
        /// <value>The web address.</value>
        public string WebAddress { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WebFavoriteInfo"/> class.
        /// </summary>
        public WebFavoriteInfo()
            : base()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            var webFavoriteNew = Clone<WebFavoriteInfo>();
            webFavoriteNew.WebAddress = WebAddress;
            return webFavoriteNew;
        }

        /// <summary>
        /// Creates a new object of T that is a copy of the current instance.
        /// </summary>
        /// <typeparam name="T">The type of the new object, it has to be derived from <see cref="WebFolderItemInfo"/>.</typeparam>
        /// <returns>A new object of T that is a copy of this instance.</returns>
        protected override T Clone<T>()
        {
            T webFolderItemNew = base.Clone<T>();
            WebFavoriteInfo webFavoriteNew = webFolderItemNew as WebFavoriteInfo;
            if (webFavoriteNew != null)
            {
                webFavoriteNew.WebAddress = WebAddress;
            }
            return webFolderItemNew;
        }

        #endregion
    }
}
