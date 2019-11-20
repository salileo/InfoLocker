using System;

namespace HgCo.WindowsLive.SkyDrive
{
    /// <summary>
    /// Provides webfolderitemicon specific data. An icon is built from several webfolderitemimages.
    /// </summary>
    [Serializable]
    public class WebFolderItemIconInfo
    {
        /// <summary>
        /// Gets or sets the webfolderitemimage, which represents ContentType.
        /// </summary>
        /// <value>The ContentType webfolderitemimage.</value>
        public WebFolderItemImageInfo ContentTypeWebImage { get; set; }

        /// <summary>
        /// Gets or sets the webfolderitemimage, which represents the content itself.
        /// </summary>
        /// <value>The content webfolderitemimage.</value>
        public WebFolderItemImageInfo ContentWebImage { get; set; }

        /// <summary>
        /// Gets or sets the webfolderitemimage, which represents ShareType.
        /// </summary>
        /// <value>The ShareType webfolderitemimage.</value>
        public WebFolderItemImageInfo ShareTypeWebImage { get; set; }

        /// <summary>
        /// Gets or sets the horizontal offset of the content webfolderitemimage inside the icon.
        /// </summary>
        /// <value>The horiozontal offset of the content webfolderitemimage.</value>
        public int? ContentWebImageOffsetX { get; set; }

        /// <summary>
        /// Gets or sets the vertical offset of the content webfolderitemimage inside the icon.
        /// </summary>
        /// <value>The vertical offset of the content webfolderitemimage.</value>
        public int? ContentWebImageOffsetY { get; set; }

        /// <summary>
        /// Gets or sets the horizontal offset of the ShareType webfolderitemimage inside the icon.
        /// </summary>
        /// <value>The horiozontal offset of the ShareType webfolderitemimage.</value>
        public int? ShareTypeWebImageOffsetX { get; set; }

        /// <summary>
        /// Gets or sets the vertical offset of the ShareType webfolderitemimage inside the icon.
        /// </summary>
        /// <value>The vertical offset of the ShareType webfolderitemimage.</value>
        public int? ShareTypeWebImageOffsetY { get; set; }
    }
}
