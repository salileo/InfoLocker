using System;

namespace HgCo.WindowsLive.SkyDrive
{
    /// <summary>
    /// Specifies the view type (how sub webfolderitems are listed) of a webfolder.
    /// </summary>
    public enum WebFolderViewType
    {
        /// <summary>
        /// The specified webfolder's view type cannot be determined.
        /// </summary>
        None,
        /// <summary>
        /// The specified webfolder is viewed in details.
        /// </summary>
        Details,
        /// <summary>
        /// The specified webfolder is viewed as icons.
        /// </summary>
        Icons,
        /// <summary>
        /// The specified webfolder is viewed as thumbnails.
        /// </summary>
        Thumbnails
    }
}
