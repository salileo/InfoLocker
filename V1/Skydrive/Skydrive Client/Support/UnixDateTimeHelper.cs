using System;

namespace HgCo.WindowsLive.SkyDrive.Support
{
    /// <summary>
    /// Provides methods for converting UNIX time to/from .Net datetime.
    /// </summary>
    internal static class UnixDateTimeHelper
    {
        #region Fields
        
        /// <summary>
        /// The starting datetime (origin) of the UNIX time representation.
        /// </summary>
        public static readonly DateTime UnixDateTimeOrigin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        
        #endregion

        #region Methods

        /// <summary>
        /// Converts the UNIX time to its .Net datetime equivalent.
        /// </summary>
        /// <param name="value">The UNIX time.</param>
        /// <returns>The .Net datetime.</returns>
        public static DateTime ConvertToDateTime(int value)
        {
            DateTime date = UnixDateTimeOrigin;
            if (value > 0)
                date = UnixDateTimeOrigin.AddSeconds(value);
            return date;
        }

        /// <summary>
        /// Converts UNIX time string to its .Net datetime equivalent.
        /// </summary>
        /// <param name="value">The UNIX time string.</param>
        /// <returns>The .Net datetime.</returns>
        /// <remarks>If UNIX time string cannot be converted to .Net datetime, null is returned.</remarks>
        public static DateTime? ConvertToDateTime(string value)
        {
            DateTime date = UnixDateTimeOrigin;
            int time = 0;
            bool success = Int32.TryParse(value, out time);
            if (success && time > 0)
                date = UnixDateTimeOrigin.AddSeconds(time);
            return date;
        }

        /// <summary>
        /// Converts .Net datetime to its UNIX time equivalent.
        /// </summary>
        /// <param name="date">The .Net datetime.</param>
        /// <returns>The UNIX time.</returns>
        /// <remarks>If .Net datetime cannot be converted to UNIX time, zero is returned.</remarks>
        public static int Parse(DateTime date)
        {
            int time = 0;
            TimeSpan diff = date - UnixDateTimeOrigin;
            if (diff.TotalSeconds > 0 && diff.TotalSeconds <= Int32.MaxValue)
                time = (int)Math.Floor(diff.TotalSeconds);
            return time;
        }

        /// <summary>
        /// Converts .Net datetime to its UNIX time equivalent.
        /// </summary>
        /// <param name="date">The .Net datetime.</param>
        /// <returns>The UNIX time.</returns>
        /// <remarks>If .Net datetime cannot be converted to UNIX time, zero is returned.</remarks>
        public static int Parse(DateTime? date)
        {
            int time = 0;
            if (date.HasValue)
            {
                TimeSpan diff = date.Value - UnixDateTimeOrigin;
                if (diff.TotalSeconds > 0 && diff.TotalSeconds <= Int32.MaxValue)
                    time = (int)Math.Floor(diff.TotalSeconds);
            }
            return time;
        }

        #endregion
    }
}
