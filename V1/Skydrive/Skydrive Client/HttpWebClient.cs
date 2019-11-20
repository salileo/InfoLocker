using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using HgCo.WindowsLive.SkyDrive.Support;

namespace HgCo.WindowsLive.SkyDrive
{
    /// <summary>
    /// Provides methods for sending data to and receiving data from a resource identified by a URI via HTTP, 
    /// while maintaining session (cookies) information, too.
    /// </summary>
    internal class HttpWebClient
    {
        #region Events

        /// <summary>
        /// Occurs when upload values operation successfully transfers some or all of the data.
        /// </summary>
        /// <remarks>
        /// This event is raised each time upload values make progress.
        /// This event is raised when uploads are started using any of the following methods:
        /// - UploadValuesUrlEncoded(Uri, NameValueCollection)
        /// - UploadValuesUrlEncoded(Uri, NameValueCollection, bool)
        /// - UploadValuesMultipartEncoded(Uri, Dictionary[string, object])
        /// - UploadValuesMultipartEncoded(Uri, Dictionary[string, object], bool)
        /// </remarks>
        public event EventHandler<UploadValuesProgressChangedEventArgs> UploadValuesProgressChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the session.
        /// </summary>
        /// <value>The session.</value>
        public WebSession Session
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the value of User-agent HTTP header.
        /// </summary>
        /// <value>The User-agent HTTP header.</value>
        public string UserAgent { get; set; }

        /// <summary>
        /// Gets or sets the time-out value in milliseconds for HTTP requests.
        /// </summary>
        /// <value>The number of milliseconds to wait before a request times out. The default is 100,000 milliseconds (100 seconds).</value>
        public int Timeout { get; set; }

        /// <summary>
        /// Gets or sets proxy information for <see cref="HttpWebClient"/>.
        /// </summary>
        /// <value>The <see cref="IWebProxy"/> object to use to proxy the <see cref="HttpWebClient"/></value>
        public IWebProxy Proxy { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpWebClient"/> class.
        /// </summary>
        public HttpWebClient() : this(null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpWebClient"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        public HttpWebClient(WebSession session)
        {
            Session = session != null ? session : new WebSession();
            Timeout = 100000;
            
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.ServerCertificateValidationCallback +=
                new RemoteCertificateValidationCallback(ValidateServerCertificateCallback);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the HTTP web request.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>The HTTP web request.</returns>
        public HttpWebRequest GetHttpWebRequest(Uri address)
        {
            HttpWebRequest webreq = (HttpWebRequest)WebRequest.Create(address);
            webreq.UserAgent = UserAgent;
            webreq.AllowAutoRedirect = false;
            webreq.AutomaticDecompression = DecompressionMethods.GZip;
            webreq.Timeout = Timeout;
            if (Proxy != null)
                webreq.Proxy = Proxy;
            Session.Apply(webreq);
            return webreq;
        }

        /// <summary>
        /// Gets the HTTP web response.
        /// </summary>
        /// <param name="webRequest">The web request.</param>
        /// <param name="allowAutoRedirect">if set to <c>true</c>, response redirection is handled automatically.</param>
        /// <returns>The HTTP web response</returns>
        public HttpWebResponse GetHttpWebResponse(HttpWebRequest webRequest, bool allowAutoRedirect)
        {
            HttpWebResponse webresp = (HttpWebResponse)webRequest.GetResponse();
            Session.Read(webresp);
            while (allowAutoRedirect && webresp.StatusCode == HttpStatusCode.Found)
            {
                Uri uriLocation = UriHelper.GetUri(webresp.Headers[HttpResponseHeader.Location]);
                HttpWebRequest webreq = GetHttpWebRequest(uriLocation);
                webresp = GetHttpWebResponse(webreq, allowAutoRedirect);
            }
            return webresp;
        }

        /// <summary>
        /// Reads the content of a web response.
        /// </summary>
        /// <param name="webResponse">The web response to read.</param>
        /// <returns>The web response's content.</returns>
        public static byte[] ReadData(WebResponse webResponse)
        {
            List<byte> lResponseByte = new List<byte>();
            using (Stream sr = webResponse.GetResponseStream())
            {
                int count = 0;
                byte[] buffer = new byte[64 * 1024];
                while ((count = sr.Read(buffer, 0, buffer.Length)) > 0)
                {
                    byte[] data = new byte[count];
                    Array.Copy(buffer, data, count);
                    lResponseByte.AddRange(data);
                }
            }
            return lResponseByte.ToArray();
        }

        /// <summary>
        /// Reads the content of a web response as a string.
        /// </summary>
        /// <param name="webResponse">The web response to read.</param>
        /// <returns>The web response's content in string.</returns>
        public static string ReadString(WebResponse webResponse)
        {
            string responseString = null;
            using (StreamReader sr = new StreamReader(webResponse.GetResponseStream()))
                responseString = sr.ReadToEnd();
            return responseString;
        }

        /// <summary>
        /// Downloads the requested resource as a string.
        /// </summary>
        /// <param name="address">The address of the resource to download.</param>
        /// <returns>The requested resource.</returns>
        public string DownloadString(Uri address)
        {
            string tmp = string.Empty;
            return DownloadString(address, true, string.Empty, ref tmp);
        }

        /// <summary>
        /// Downloads the requested resource as a string.
        /// </summary>
        /// <param name="address">The address of the resource to download.</param>
        /// <param name="finalURL">Reference to the final URL loaded</param>
        /// <returns>The requested resource.</returns>
        public string DownloadString(Uri address, ref string finalURL)
        {
            return DownloadString(address, true, string.Empty, ref finalURL);
        }

        /// <summary>
        /// Downloads the requested resource as a string.
        /// </summary>
        /// <param name="address">The address of the resource to download.</param>
        /// <param name="referer">The referer URL</param>
        /// <param name="finalURL">Reference to the final URL loaded</param>
        /// <returns>The requested resource.</returns>
        public string DownloadString(Uri address, string referer, ref string finalURL)
        {
            return DownloadString(address, true, referer, ref finalURL);
        }
        
        /// <summary>
        /// Downloads the requested resource as a string.
        /// </summary>
        /// <param name="address">The address of the resource to download.</param>
        /// <param name="allowAutoRedirect">if set to <c>true</c>, response redirection is handled automatically.</param>
        /// <param name="referer">The referer URL</param>
        /// <param name="finalURL">Reference to the final URL loaded</param>
        /// <returns>The requested resource.</returns>
        public string DownloadString(Uri address, bool allowAutoRedirect, string referer, ref string finalURL)
        {
            HttpWebRequest webreq = GetHttpWebRequest(address);
            if (!string.IsNullOrEmpty(referer))
                webreq.Referer = referer;
            HttpWebResponse webresp = GetHttpWebResponse(webreq, allowAutoRedirect);

            finalURL = webresp.ResponseUri.OriginalString;
            string responseString = ReadString(webresp);
            return responseString;
        }

        /// <summary>
        /// Downloads the requested resource as a byte array.
        /// </summary>
        /// <param name="address">The address of the resource to download.</param>
        /// <returns>The requested resource.</returns>
        public byte[] DownloadData(Uri address)
        {
            return DownloadData(address, true);
        }

        /// <summary>
        /// Downloads the requested resource as a byte array.
        /// </summary>
        /// <param name="address">The address of the resource to download.</param>
        /// <param name="allowAutoRedirect">if set to <c>true</c>, response redirection is handled automatically.</param>
        /// <returns>The requested resource.</returns>
        public byte[] DownloadData(Uri address, bool allowAutoRedirect)
        {
            HttpWebRequest webreq = GetHttpWebRequest(address);
            HttpWebResponse webresp = GetHttpWebResponse(webreq, allowAutoRedirect);

            byte[] responseBytes = ReadData(webresp);
            return responseBytes;
        }

        /// <summary>
        /// Downloads the requested resource.
        /// </summary>
        /// <param name="address">The address of the resource to download.</param>
        /// <returns>The requested resource.</returns>
        public Stream DownloadStream(Uri address)
        {
            return DownloadStream(address, true);
        }

        /// <summary>
        /// Downloads the requested resource.
        /// </summary>
        /// <param name="address">The address of the resource to download.</param>
        /// <param name="allowAutoRedirect">if set to <c>true</c>, response redirection is handled automatically.</param>
        /// <returns>The requested resource.</returns>
        public Stream DownloadStream(Uri address, bool allowAutoRedirect)
        {
            HttpWebRequest webreq = GetHttpWebRequest(address);
            HttpWebResponse webresp = GetHttpWebResponse(webreq, allowAutoRedirect);

            Stream responseStream = webresp.GetResponseStream();
            return responseStream;
        }

        /// <summary>
        /// Uploads a name/value collection in URL encoded format to a resource with the specified URI.
        /// </summary>
        /// <param name="address">The address of the resource to upload to.</param>
        /// <param name="parameters">The parameters to upload to the resourece.</param>
        /// <returns>The response from the resource.</returns>
        public string UploadValuesUrlEncoded(Uri address, NameValueCollection parameters)
        {
            return UploadValuesUrlEncoded(address, parameters, true, string.Empty);
        }

        /// <summary>
        /// Uploads a name/value collection in URL encoded format to a resource with the specified URI.
        /// </summary>
        /// <param name="address">The address of the resource to upload to.</param>
        /// <param name="parameters">The parameters to upload to the resourece.</param>
        /// <param name="allowAutoRedirect">if set to <c>true</c>, response redirection is handled automatically.</param>
        /// <returns>The response from the resource.</returns>
        public string UploadValuesUrlEncoded(Uri address, NameValueCollection parameters, bool allowAutoRedirect)
        {
            return UploadValuesUrlEncoded(address, parameters, allowAutoRedirect, string.Empty);
        }

        /// <summary>
        /// Uploads a name/value collection in URL encoded format to a resource with the specified URI.
        /// </summary>
        /// <param name="address">The address of the resource to upload to.</param>
        /// <param name="parameters">The parameters to upload to the resourece.</param>
        /// <param name="referer">The referer URL</param>
        /// <returns>The response from the resource.</returns>
        public string UploadValuesUrlEncoded(Uri address, NameValueCollection parameters, string referer)
        {
            return UploadValuesUrlEncoded(address, parameters, true, referer);
        }

        /// <summary>
        /// Uploads a name/value collection in URL encoded format to a resource with the specified URI.
        /// </summary>
        /// <param name="address">The address of the resource to upload to.</param>
        /// <param name="parameters">The parameters to upload to the resourece.</param>
        /// <param name="allowAutoRedirect">if set to <c>true</c>, response redirection is handled automatically.</param>
        /// <param name="referer">The referer URL</param>
        /// <returns>The response from the resource.</returns>
        public string UploadValuesUrlEncoded(Uri address, NameValueCollection parameters, bool allowAutoRedirect, string referer)
        {
            HttpWebRequest webreq = GetHttpWebRequest(address);
            if (!string.IsNullOrEmpty(referer))
                webreq.Referer = referer;

            webreq.Method = WebRequestMethods.Http.Post;
            webreq.ContentType = "application/x-www-form-urlencoded";

            StringBuilder sbContent = new StringBuilder();
            for (int idxParameter = 0; idxParameter < parameters.Count; idxParameter++)
                if (idxParameter == 0)
                    sbContent.AppendFormat("{0}={1}",
                        HttpUtility.HtmlEncode(parameters.GetKey(idxParameter)),
                        HttpUtility.HtmlEncode(parameters[idxParameter]));
                else sbContent.AppendFormat("&{0}={1}",
                        HttpUtility.HtmlEncode(parameters.GetKey(idxParameter)),
                        HttpUtility.HtmlEncode(parameters[idxParameter]));
            byte[] contentBytes = Encoding.UTF8.GetBytes(sbContent.ToString());
            webreq.ContentLength = contentBytes.Length;

            using (Stream sw = webreq.GetRequestStream())
                sw.Write(contentBytes, 0, contentBytes.Length);

            HttpWebResponse webresp = GetHttpWebResponse(webreq, allowAutoRedirect);
            string responseString = ReadString(webresp);
            return responseString;
        }

        /// <summary>
        /// Uploads a name/value collection in Multipart encoded format to a resource with the specified URI.
        /// </summary>
        /// <param name="address">The address of the resource to upload to.</param>
        /// <param name="parameters">The parameters to upload to the resourece.</param>
        /// <returns>The response from the resource.</returns>
        public string UploadValuesMultipartEncoded(Uri address, Dictionary<string, object> parameters)
        {
            return UploadValuesMultipartEncoded(address, parameters);
        }

        /// <summary>
        /// Uploads a name/value collection in Multipart encoded format to a resource with the specified URI.
        /// </summary>
        /// <param name="address">The address of the resource to upload to.</param>
        /// <param name="parameters">The parameters to upload to the resourece.</param>
        /// <param name="allowAutoRedirect">if set to <c>true</c>, response redirection is handled automatically.</param>
        /// <returns>The response from the resource.</returns>
        public string UploadValuesMultipartEncoded(Uri address, Dictionary<string, object> parameters, bool allowAutoRedirect)
        {
            string mpBoundaryString = String.Format(
                CultureInfo.InvariantCulture,
                "---------------------------{0:x}",
                DateTime.Now.Ticks);
            byte[] mpNewLineBytes = Encoding.ASCII.GetBytes(Environment.NewLine);
            byte[] mpLastBoundaryBytes = Encoding.ASCII.GetBytes(String.Format(
                CultureInfo.InvariantCulture, 
                "--{0}--{1}", mpBoundaryString, 
                Environment.NewLine));
            Dictionary<string, byte[]> dicMultiPartFormHeaderBytes = new Dictionary<string, byte[]>(parameters.Count);
            Dictionary<string, byte[]> dicMultiPartFormBodyBytes = new Dictionary<string, byte[]>(parameters.Count);

            long contentLength = mpLastBoundaryBytes.Length;
            foreach (string parameterKey in parameters.Keys)
            {
                object parameterValue = parameters[parameterKey];
                FileInfo fiParameter = parameterValue as FileInfo;
                if (fiParameter != null)
                {
                    byte[] mpFormHeaderBytes = GenerateMultiPartFormFieldHeaderBytes(parameterKey, fiParameter.Name, mpBoundaryString);
                    dicMultiPartFormHeaderBytes[parameterKey] = mpFormHeaderBytes;
                    contentLength += mpFormHeaderBytes != null ? mpFormHeaderBytes.Length : 0;
                    contentLength += fiParameter.Length + mpNewLineBytes.Length;
                }
                else
                {
                    byte[] mpFormHeaderBytes = GenerateMultiPartFormFieldHeaderBytes(parameterKey, mpBoundaryString);
                    dicMultiPartFormHeaderBytes[parameterKey] = mpFormHeaderBytes;

                    byte[] mpFormBodyBytes = GenerateMultiPartFormFieldContentBytes(parameterValue);
                    dicMultiPartFormBodyBytes[parameterKey] = mpFormBodyBytes;

                    contentLength += mpFormHeaderBytes != null ? mpFormHeaderBytes.Length : 0;
                    contentLength += mpFormBodyBytes != null ? mpFormBodyBytes.Length : 0;
                }
            }

            HttpWebRequest webreq = GetHttpWebRequest(address);
            webreq.Method = WebRequestMethods.Http.Post;
            webreq.ContentType = String.Format(
                CultureInfo.InvariantCulture,
                "multipart/form-data; boundary=\"{0}\"",
                mpBoundaryString);
            webreq.ContentLength = contentLength;

            using (Stream sw = webreq.GetRequestStream())
            {
                long contentLengthSent = 0;

                foreach (string parameterKey in parameters.Keys)
                {
                    object parameterValue = parameters[parameterKey];
                    FileInfo fiParameter = parameterValue as FileInfo;
                    if (fiParameter != null)
                    {
                        byte[] mpFormHeaderBytes = dicMultiPartFormHeaderBytes[parameterKey];
                        sw.Write(mpFormHeaderBytes, 0, mpFormHeaderBytes.Length);
                        contentLengthSent += mpFormHeaderBytes.Length;

                        using (FileStream fs = fiParameter.OpenRead())
                        {
                            int count = 0;
                            byte[] buffer = new byte[64 * 1024];
                            while ((count = fs.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                sw.Write(buffer, 0, count);
                                contentLengthSent += count;

                                OnUploadValuesProgressChanged(new UploadValuesProgressChangedEventArgs(
                                    contentLengthSent,
                                    contentLength));
                            }
                            sw.Write(mpNewLineBytes, 0, mpNewLineBytes.Length);
                            contentLengthSent += mpNewLineBytes.Length;
                        }
                    }
                    else
                    {
                        byte[] mpFormHeaderBytes = dicMultiPartFormHeaderBytes[parameterKey];
                        byte[] mpFormBodyBytes = dicMultiPartFormBodyBytes[parameterKey];

                        sw.Write(mpFormHeaderBytes, 0, mpFormHeaderBytes.Length);
                        sw.Write(mpFormBodyBytes, 0, mpFormBodyBytes.Length);
                        contentLengthSent += mpFormHeaderBytes.Length + mpFormBodyBytes.Length;
                    }

                    OnUploadValuesProgressChanged(new UploadValuesProgressChangedEventArgs(
                        contentLengthSent,
                        contentLength));
                }

                sw.Write(mpLastBoundaryBytes, 0, mpLastBoundaryBytes.Length);
                contentLengthSent += mpLastBoundaryBytes.Length;

                OnUploadValuesProgressChanged(new UploadValuesProgressChangedEventArgs(
                    contentLengthSent,
                    contentLength));
            }

            HttpWebResponse webresp = GetHttpWebResponse(webreq, allowAutoRedirect);
            string responseString = ReadString(webresp);
            return responseString;
        }

        #endregion

        #region Protected Methods
        
        /// <summary>
        /// Raises the <see cref="E:UploadValuesProgressChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="UploadValuesProgressChangedEventArgs"/> instance containing the event data.</param>
        protected void OnUploadValuesProgressChanged(UploadValuesProgressChangedEventArgs e)
        {
            if (UploadValuesProgressChanged != null)
                UploadValuesProgressChanged(this, e);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Verifies the remote Secure Sockets Layer (SSL) certificate used for authentication.
        /// </summary>
        /// <param name="sender">An object that contains state information for this validation.</param>
        /// <param name="cert">The certificate used to authenticate the remote party.</param>
        /// <param name="chain">The chain of certificate authorities associated with the remote certificate.</param>
        /// <param name="error">One or more errors associated with the remote certificate.</param>
        /// <returns>A boolean value that determines whether the specified certificate is accepted for authentication.</returns>
        private static bool ValidateServerCertificateCallback(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
        {
            return true;
        }

        /// <summary>
        /// Generates the header bytes of a form field for a multi part request.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="multipartBoundary">The boundary of the multi part request.</param>
        /// <returns>The list of bytes representing the form field's header.</returns>
        private static byte[] GenerateMultiPartFormFieldHeaderBytes(string fieldName, string multipartBoundary)
        {
            StringBuilder sb = new StringBuilder();
            if (!String.IsNullOrEmpty(fieldName))
            {
                sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "--{0}", multipartBoundary));
                sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "Content-Disposition: form-data; name=\"{0}\"", fieldName));
                sb.AppendLine();
            }
            string mpFormHeaderString = sb.ToString();
            byte[] mpFormHeaderBytes = Encoding.UTF8.GetBytes(mpFormHeaderString);
            return mpFormHeaderBytes;
        }

        /// <summary>
        /// Generates the header bytes of a form file field for a multi part request.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="multipartBoundary">The boundary of the multi part request.</param>
        /// <returns>The list of bytes representing the form file field's header.</returns>
        private static byte[] GenerateMultiPartFormFieldHeaderBytes(string fieldName, string fileName, string multipartBoundary)
        {
            StringBuilder sb = new StringBuilder();
            if (!String.IsNullOrEmpty(fieldName))
            {
                sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "--{0}", multipartBoundary));
                sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"", fieldName, fileName));
                sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "Content-Type: {0}", MimeTypeHelper.GetContentType(fileName)));
                sb.AppendLine();
            }
            string mpFormHeaderString = sb.ToString();
            byte[] mpFormHeaderBytes = Encoding.UTF8.GetBytes(mpFormHeaderString);
            return mpFormHeaderBytes;
        }

        /// <summary>
        /// Generates the content bytes of a form field for a multi part request.
        /// </summary>
        /// <param name="fieldValue">The value of the field.</param>
        /// <returns>The list of bytes representing the form field's content.</returns>
        private static byte[] GenerateMultiPartFormFieldContentBytes(object fieldValue)
        {
            string mpFormBodyString = String.Concat(fieldValue, Environment.NewLine);
            byte[] mpFormBodyBytes = Encoding.UTF8.GetBytes(mpFormBodyString);
            return mpFormBodyBytes;
        }

        #endregion
    }
}
