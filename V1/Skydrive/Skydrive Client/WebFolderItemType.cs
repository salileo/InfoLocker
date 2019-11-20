using System;

namespace HgCo.WindowsLive.SkyDrive
{
    /// <summary>
    /// Specifies the item type of a webfolderitem.
    /// </summary>
    public enum WebFolderItemType
    {
        /// <summary>
        /// The specified webfolderitem's item type cannot be determined.
        /// </summary>
        None,
        /// <summary>
        /// The specified webfolderitem is a webfolder.
        /// </summary>
        Folder,
        /// <summary>
        /// The specified webfolderitem is a webfile.
        /// </summary>
        File
    }
}
