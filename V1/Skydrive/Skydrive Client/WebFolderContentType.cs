using System;

namespace HgCo.WindowsLive.SkyDrive
{
    /// <summary>
    /// Specifies the content type of a webfolder.
    /// </summary>
    public enum WebFolderContentType
    {
        /// <summary>
        /// The specified webfolder's content type cannot be determined.
        /// </summary>
        None,
        /// <summary>
        /// The specified webfolder contains documents.
        /// </summary>
        Documents,
        /// <summary>
        /// The specified webfolder contains favorites.
        /// </summary>
        Favorites,
        /// <summary>
        /// The specified webfolder contains photos.
        /// </summary>
        Photos
    }
}