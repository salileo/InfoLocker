using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using HgCo.WindowsLive.SkyDrive.Support;

namespace HgCo.WindowsLive.SkyDrive
{
    /// <summary>
    /// Provides webfolderitem content specific data. This is an <c>abstract</c> class.
    /// </summary>
    [Serializable]
    [XmlInclude(typeof(WebFavoriteInfo))]
    [XmlInclude(typeof(WebFileInfo))]
    [XmlInclude(typeof(WebFolderInfo))]
    public abstract class WebFolderItemInfo : ICloneable
    {
        #region Fields
        /// <summary>
        /// The delimiter character used in path URL.
        /// </summary>
        public const char PathUrlSegmentDelimiter = '/';

        /// <summary>
        /// The regular expression to parse value of Size property.
        /// </summary>
        private static readonly Regex RegexSize = new Regex("^(?i:\\s*(?<Quantity>\\d+(\\.\\d+)?)\\s+(?<Unit>(byte|bytes|kb|mb)))$");

        /// <summary>
        /// The variable used to store value of Size property.
        /// </summary>
        private string size;
        
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the ItemType.
        /// </summary>
        /// <value>The ItemType.</value>
        public WebFolderItemType ItemType { get; set; }

        /// <summary>
        /// Gets or sets the name of the item's creator.
        /// </summary>
        /// <value>The name of the item's creator.</value>
        public string CreatorName { get; set; }

        /// <summary>
        /// Gets or sets the URL to the creator's profile.
        /// </summary>
        /// <value>The URL to the creator's profile.</value>
        public string CreatorUrl { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the ShareType.
        /// </summary>
        /// <value>The ShareType.</value>
        public WebFolderItemShareType ShareType { get; set; }

        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        /// <value>The size.</value>
        public string Size 
        {
            get { return size; }
            set 
            {
                size = value;
                if (!String.IsNullOrEmpty(size) && RegexHelper.IsMatch(RegexSize, size))
                {
                    Match matchSize = RegexHelper.Match(RegexSize, size);
                    decimal quantity = Decimal.Parse(matchSize.Groups["Quantity"].Value, CultureInfo.InvariantCulture);
                    string unit = matchSize.Groups["Unit"].Value.ToUpperInvariant();
                    switch (unit)
                    {
                        case "KB":
                            SizeMean = (int)Math.Round(quantity * 1024);
                            SizeMin = (int)Math.Round((quantity - 0.049M) * 1024);
                            SizeMax = (int)Math.Round((quantity + 0.05M) * 1024);
                            break;
                        case "MB":
                            SizeMean = (int)Math.Round(quantity * 1024 * 1024);
                            SizeMin = (int)Math.Round((quantity - 0.049M) * 1024 * 1024);
                            SizeMax = (int)Math.Round((quantity + 0.05M) * 1024 * 1024);
                            break;
                        default:
                            SizeMean = SizeMin = SizeMax = (int)Math.Round(quantity);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the mean size in bytes.
        /// </summary>
        /// <value>The mean size in bytes.</value>
        [XmlIgnore]
        public int? SizeMean { get; private set; }
        
        /// <summary>
        /// Gets or sets the min size in bytes.
        /// </summary>
        /// <value>The min size in bytes.</value>
        [XmlIgnore]
        public int? SizeMin { get; private set; }

        /// <summary>
        /// Gets or sets the max size in bytes.
        /// </summary>
        /// <value>The max size in bytes.</value>
        [XmlIgnore]
        public int? SizeMax { get; private set; }

        /// <summary>
        /// Gets or sets the date when item was added.
        /// </summary>
        /// <value>The date when item was added.</value>
        public DateTime? DateAdded { get; set; }

        /// <summary>
        /// Gets or sets the date when item was modified.
        /// </summary>
        /// <value>The date when item was modified.</value>
        public DateTime? DateModified { get; set; }

        /// <summary>
        /// Gets or sets the path URL.
        /// </summary>
        /// <value>The path URL.</value>
        public string PathUrl { get; set; }

        /// <summary>
        /// Gets or sets the URL to view the item.
        /// </summary>
        /// <value>The view URL.</value>
        public string ViewUrl { get; set; }

        /// <summary>
        /// Gets or sets the URL to download the item.
        /// </summary>
        /// <value>The download URL.</value>
        /// <remarks></remarks>
        public string DownloadUrl { get; set; }

        /// <summary>
        /// Gets or sets the webfolderitemicon.
        /// </summary>
        /// <value>The webfolderitemicon.</value>
        public WebFolderItemIconInfo WebIcon { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            if (String.IsNullOrEmpty(Name))
                return ShareType != WebFolderItemShareType.None ?
                    String.Format(CultureInfo.InvariantCulture, "{0} ({1})", PathUrl, ShareType) : PathUrl;
            else
                return ShareType != WebFolderItemShareType.None ?
                    String.Format(CultureInfo.InvariantCulture, "{0} ({1})", Name, ShareType) : Name;
        }

        /// <summary>
        /// Gets an array containing the path segments that make up the specified path URL.
        /// </summary>
        /// <param name="pathUrl">The path URL.</param>
        /// <returns>The segments of the path URL.</returns>
        public static string[] GetPathUrlSegments(string pathUrl)
        {
            string[] pathUrlSegments = null;
            if (!String.IsNullOrEmpty(pathUrl))
                pathUrlSegments = pathUrl.StartsWith(PathUrlSegmentDelimiter.ToString(), StringComparison.OrdinalIgnoreCase) ? 
                    pathUrl.Substring(1).Split(PathUrlSegmentDelimiter) :
                    pathUrl.Split(PathUrlSegmentDelimiter);
            return pathUrlSegments;
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public abstract object Clone();

        /// <summary>
        /// Creates a new object of T that is a copy of the current instance.
        /// </summary>
        /// <typeparam name="T">The type of the new object, it has to be derived from <see cref="WebFolderItemInfo" />.</typeparam>
        /// <returns>A new object of T that is a copy of this instance.</returns>
        protected virtual T Clone<T>() where T : WebFolderItemInfo, new()
        {
            T webFolderItemNew = new T
            {
                CreatorName = CreatorName,
                CreatorUrl = CreatorUrl,
                DateAdded = DateAdded,
                DateModified = DateModified,
                Description = Description,
                Name = Name,
                PathUrl = PathUrl,
                ShareType = ShareType,
                Size = Size,
                ViewUrl = ViewUrl,
                WebIcon = WebIcon
            };

            return webFolderItemNew;
        }

        #endregion

    }
}
