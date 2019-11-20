using System;

namespace HgCo.WindowsLive.SkyDrive
{
    /// <summary>
    /// Provides webfolderitemimage specific data.
    /// </summary>
    [Serializable]
    public class WebFolderItemImageInfo
    {
        /// <summary>
        /// Gets or sets the web address.
        /// </summary>
        /// <value>The web address.</value>
        public string WebAddress { get; set; }

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        /// <value>The width.</value>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>The height.</value>
        public int? Height { get; set; }

        /// <summary>
        /// Gets or sets the horizontal location inside a striped image.
        /// </summary>
        /// <value>The horizontal location.</value>
        public int? LocationX { get; set; }

        /// <summary>
        /// Gets or sets the vertical location inside a striped image.
        /// </summary>
        /// <value>The vertical location.</value>
        public int? LocationY { get; set; }
    }
}
