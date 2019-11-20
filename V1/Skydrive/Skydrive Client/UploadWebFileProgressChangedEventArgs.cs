using System;

namespace HgCo.WindowsLive.SkyDrive
{
    /// <summary>
    /// Provides data for the <see cref="E:UploadValuesProgressChanged"/> event of a <see cref="SkyDriveWebClient"/>.
    /// </summary>
    public class UploadWebFileProgressChangedEventArgs : EventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the number of bytes sent.
        /// </summary>
        /// <value>The number of bytes sent.</value>
        public long BytesSent { get; protected set; }

        /// <summary>
        /// Gets the total number of bytes that will be sent.
        /// </summary>
        /// <value>The total number of bytes that will be sent.</value>
        public long TotalBytesToSent { get; protected set; }

        /// <summary>
        /// Gets the uploading task progress percentage.
        /// </summary>
        /// <value>The uploading task progress percentage.</value>
        public int ProgressPercentage 
        {
            get
            {
                if (TotalBytesToSent > 0)
                    return (int)((BytesSent / (decimal)TotalBytesToSent) * 100);
                else return 0;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadWebFileProgressChangedEventArgs"/> class.
        /// </summary>
        /// <param name="bytesSent">The number of bytes sent.</param>
        /// <param name="totalBytesToSent">The total number of bytes that will be sent.</param>
        internal UploadWebFileProgressChangedEventArgs(long bytesSent, long totalBytesToSent)
        {
            BytesSent = bytesSent;
            TotalBytesToSent = totalBytesToSent;
        }

        #endregion
    }
}
