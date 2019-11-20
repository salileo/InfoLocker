using System;

namespace HgCo.WindowsLive.SkyDrive
{
    /// <summary>
    /// Specifies the share type of a webfolderitem.
    /// </summary>
    public enum WebFolderItemShareType
    {
        /// <summary>
        /// The specified webfolderitem's share type cannot be determined.
        /// </summary>
        None,
        /// <summary>
        /// The specified webfolderitem is shared with public.
        /// </summary>
        Public,
        /// <summary>
        /// The specified webfolderitem is shared with my network.
        /// </summary>
        MyNetwork,
        /// <summary>
        /// The specified webfolderitem is shared with people I selected.
        /// </summary>
        PeopleSelected,
        /// <summary>
        /// The specified webfolderitem is not shared, it's private.
        /// </summary>
        Private
    }
}
