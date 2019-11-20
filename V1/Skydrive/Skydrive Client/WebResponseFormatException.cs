using System;
using System.Runtime.Serialization;

namespace HgCo.WindowsLive.SkyDrive
{
    /// <summary>
    /// The exception is thrown when the format of web response does not meet the expectations.
    /// </summary>
    [Serializable]
    public class WebResponseFormatException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebResponseFormatException"/> class.
        /// </summary>
        public WebResponseFormatException() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebResponseFormatException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public WebResponseFormatException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebResponseFormatException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public WebResponseFormatException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebResponseFormatException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="info"/> parameter is null.
        /// </exception>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">
        /// The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0).
        /// </exception>
        protected WebResponseFormatException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
