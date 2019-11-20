using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using HgCo.WindowsLive.SkyDrive.Support;

namespace HgCo.WindowsLive.SkyDrive
{
    /// <summary>
    /// Provides SkyDrive storage secific data.
    /// </summary>
    [Serializable]
    public class WebDriveInfo
    {
        #region Fields
        
        /// <summary>
        /// The regular expression to parse value of FreeDiskSpace and UsedDiskSpace property.
        /// </summary>
        private static readonly Regex RegexDiskSpace = new Regex("^(?i:\\s*(?<Quantity>\\d+(\\.\\d+)?)\\s+(?<Unit>(byte|bytes|kb|mb|gb)))$");

        /// <summary>
        /// The variable used to store value of UsedDiskSpace property.
        /// </summary>
        private string usedDiskSpace;

        /// <summary>
        /// The variable used to store value of FreeDiskSpace property.
        /// </summary>
        private string freeDiskSpace;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Windows Live Identifier (CID).
        /// </summary>
        /// <value>The CID.</value>
        public string Cid { get; set; }

        /// <summary>
        /// Gets or sets the used disk space.
        /// </summary>
        /// <value>The used disk space.</value>
        public string UsedDiskSpace 
        {
            get { return usedDiskSpace; }
            set
            {
                usedDiskSpace = value;

                if (!String.IsNullOrEmpty(usedDiskSpace) && RegexHelper.IsMatch(RegexDiskSpace, usedDiskSpace))
                {
                    Match matchDiskSpace = RegexHelper.Match(RegexDiskSpace, usedDiskSpace);
                    decimal quantity = Decimal.Parse(matchDiskSpace.Groups["Quantity"].Value, CultureInfo.InvariantCulture);
                    string unit = matchDiskSpace.Groups["Unit"].Value.ToUpperInvariant();

                    switch (unit)
                    {
                        case "KB":
                            UsedDiskSpaceMean = (long)Math.Round(quantity * 1024);
                            UsedDiskSpaceMin = (long)Math.Round((quantity - 0.049M) * 1024);
                            UsedDiskSpaceMax = (long)Math.Round((quantity + 0.05M) * 1024);
                            break;
                        case "MB":
                            UsedDiskSpaceMean = (long)Math.Round(quantity * 1024 * 1024);
                            UsedDiskSpaceMin = (long)Math.Round((quantity - 0.049M) * 1024 * 1024);
                            UsedDiskSpaceMax = (long)Math.Round((quantity + 0.05M) * 1024 * 1024);
                            break;
                        case "GB":
                            UsedDiskSpaceMean = (long)Math.Round(quantity * 1024 * 1024 * 1024);
                            UsedDiskSpaceMin = (long)Math.Round((quantity - 0.049M) * 1024 * 1024 * 1024);
                            UsedDiskSpaceMax = (long)Math.Round((quantity + 0.05M) * 1024 * 1024 * 1024);
                            break;
                        default:
                            UsedDiskSpaceMean = UsedDiskSpaceMin = UsedDiskSpaceMax = (long)Math.Round(quantity);
                            break;
                    }
                }
                else usedDiskSpace = null;
            }
        }

        /// <summary>
        /// Gets or sets the mean used disk space in bytes.
        /// </summary>
        /// <value>The mean used disk space in bytes.</value>
        [XmlIgnore]
        public long? UsedDiskSpaceMean { get; private set; }

        /// <summary>
        /// Gets or sets the min used disk space in bytes.
        /// </summary>
        /// <value>The min used disk space in bytes.</value>
        [XmlIgnore]
        public long? UsedDiskSpaceMin { get; private set; }

        /// <summary>
        /// Gets or sets the max used disk space in bytes.
        /// </summary>
        /// <value>The max used disk space in bytes.</value>
        [XmlIgnore]
        public long? UsedDiskSpaceMax { get; private set; }

        /// <summary>
        /// Gets or sets the free disk space.
        /// </summary>
        /// <value>The free disk space.</value>
        public string FreeDiskSpace
        {
            get { return freeDiskSpace; }
            set
            {
                freeDiskSpace = value;

                if (!String.IsNullOrEmpty(freeDiskSpace) && RegexHelper.IsMatch(RegexDiskSpace, freeDiskSpace))
                {
                    Match matchDiskSpace = RegexHelper.Match(RegexDiskSpace, freeDiskSpace);
                    decimal quantity = Decimal.Parse(matchDiskSpace.Groups["Quantity"].Value, CultureInfo.InvariantCulture);
                    string unit = matchDiskSpace.Groups["Unit"].Value.ToUpperInvariant();

                    switch (unit)
                    {
                        case "KB":
                            FreeDiskSpaceMean = (long)Math.Round(quantity * 1024);
                            FreeDiskSpaceMin = (long)Math.Round((quantity - 0.049M) * 1024);
                            FreeDiskSpaceMax = (long)Math.Round((quantity + 0.05M) * 1024);
                            break;
                        case "MB":
                            FreeDiskSpaceMean = (long)Math.Round(quantity * 1024 * 1024);
                            FreeDiskSpaceMin = (long)Math.Round((quantity - 0.049M) * 1024 * 1024);
                            FreeDiskSpaceMax = (long)Math.Round((quantity + 0.05M) * 1024 * 1024);
                            break;
                        case "GB":
                            FreeDiskSpaceMean = (long)Math.Round(quantity * 1024 * 1024 * 1024);
                            FreeDiskSpaceMin = (long)Math.Round((quantity - 0.049M) * 1024 * 1024 * 1024);
                            FreeDiskSpaceMax = (long)Math.Round((quantity + 0.05M) * 1024 * 1024 * 1024);
                            break;
                        default:
                            FreeDiskSpaceMean = FreeDiskSpaceMin = FreeDiskSpaceMax = (long)Math.Round(quantity);
                            break;
                    }
                }
                else freeDiskSpace = null;
            }
        }

        /// <summary>
        /// Gets or sets the mean free disk space in bytes.
        /// </summary>
        /// <value>The mean free disk space in bytes.</value>
        [XmlIgnore]
        public long? FreeDiskSpaceMean { get; private set; }

        /// <summary>
        /// Gets or sets the min free disk space in bytes.
        /// </summary>
        /// <value>The min free disk space in bytes.</value>
        [XmlIgnore]
        public long? FreeDiskSpaceMin { get; private set; }

        /// <summary>
        /// Gets or sets the max free disk space in bytes.
        /// </summary>
        /// <value>The max free disk space in bytes.</value>
        [XmlIgnore]
        public long? FreeDiskSpaceMax { get; private set; }

        #endregion
    }
}
