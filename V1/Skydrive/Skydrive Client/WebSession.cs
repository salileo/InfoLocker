using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using HgCo.WindowsLive.SkyDrive.Support;

namespace HgCo.WindowsLive.SkyDrive
{
    /// <summary>
    /// Provides methods for handling web session information.
    /// </summary>
    public class WebSession : ICloneable
    {
        #region Fields

        /// <summary>
        /// The regular expression to parse Windows Live Identifier (CID).
        /// </summary>
        private static readonly Regex RegexCid = new Regex("(?i:(?<CID>[a-z0-9]{16}))");

        /// <summary>
        /// The list of cookies.
        /// </summary>
        private readonly List<Cookie> cookieList;

        /// <summary>
        /// The lock object used for thread safety.
        /// </summary>
        private readonly object lockObject;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Windows Live Identifier (CID).
        /// </summary>
        /// <value>The CID.</value>
        public string Cid { get; set; }
        
        #endregion

        #region Contructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSession"/> class.
        /// </summary>
        public WebSession()
        {
            cookieList = new List<Cookie>();
            lockObject = new object();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Applies session information on the given web request.
        /// </summary>
        /// <param name="webRequest">The web request.</param>
        public void Apply(WebRequest webRequest)
        {
            lock (lockObject)
            {
                if (webRequest != null && cookieList.Count > 0)
                {
                    List<string> lCookie = new List<string>();
                    foreach (Cookie myCookie in cookieList)
                        if (webRequest.RequestUri.Authority.EndsWith(myCookie.Domain, StringComparison.OrdinalIgnoreCase) &&
                            myCookie.Expires >= DateTime.Now)
                        {
                            if (!String.IsNullOrEmpty(myCookie.Name))
                                lCookie.Add(String.Format(CultureInfo.InvariantCulture, "{0}={1}", myCookie.Name, myCookie.Value));
                            else lCookie.Add(myCookie.Value);
                        }

                    string requestCookie = String.Join("; ", lCookie.ToArray());
                    webRequest.Headers[HttpRequestHeader.Cookie] = requestCookie;
                }
            }
        }

        /// <summary>
        /// Reads session information out of the specified web response.
        /// </summary>
        /// <param name="webResponse">The web response.</param>
        public void Read(WebResponse webResponse)
        {
            lock (lockObject) 
            { 
                Cookie[] cookies = WebResponseHelper.ParseCookies(webResponse); 
                foreach (Cookie cookie in cookies) 
                { 
                    AddCookie(cookie); 
                    if (String.IsNullOrEmpty(Cid) && 
                        cookie.Name.Equals("drua", StringComparison.OrdinalIgnoreCase) && 
                        RegexHelper.IsMatch(RegexCid, cookie.Value ?? String.Empty)) 
                    { 
                        Cid = String.Format(
                            CultureInfo.InvariantCulture, 
                            "cid-{0}", 
                            RegexHelper.Match(RegexCid, cookie.Value).Value); 
                    } 
                } 
            }
        }

        /// <summary>
        /// Adds a cookie to the session.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="domain">The domain.</param>
        public void AddCookie(string name, string value, string domain)
        {
            lock (lockObject)
            {
                Cookie cookie = null;
                if (!String.IsNullOrEmpty(name))
                    cookie = new Cookie
                    {
                        Name = name,
                        Value = value,
                        Domain = domain,
                        Expired = false,
                        Expires = DateTime.MaxValue
                    };
                else
                    cookie = new Cookie
                    {
                        Value = value,
                        Domain = domain,
                        Expired = false,
                        Expires = DateTime.MaxValue
                    };
                AddCookie(cookie);
            }
        }

        /// <summary>
        /// Adds a cookie to the session.
        /// </summary>
        /// <param name="cookie">The cookie to add.</param>
        public void AddCookie(Cookie cookie)
        {
            lock (lockObject)
            {
                if (cookie != null)
                {
                    Cookie cookieFounded = null;
                    foreach (Cookie myCookie in cookieList)
                        if (myCookie.Name == cookie.Name &&
                            myCookie.Domain == cookie.Domain && myCookie.Path == cookie.Path)
                        {
                            cookieFounded = myCookie;
                            break;
                        }

                    if (cookieFounded != null)
                        cookieList.Remove(cookieFounded);
                    cookieList.Add(cookie);
                }
            }
        }

        /// <summary>
        /// Resets the session.
        /// </summary>
        public void Reset()
        {
            Cid = null;
            cookieList.Clear();
        }

        /// <summary>
        /// Creates a new WebSession object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new WebSession object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            WebSession session = new WebSession();
            session.Cid = Cid;
            session.cookieList.AddRange(cookieList);
            return session;
        }

        #endregion
    }
}
