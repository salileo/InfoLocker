using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace HgCo.WindowsLive.SkyDrive.Support
{
    /// <summary>
    /// Provides methods for parsing WebResponse's Headers.
    /// </summary>
    internal static class WebResponseHelper
    {
        #region Fields
        
        /// <summary>
        /// The regular expression to parse Set-Cookie's part Domain.
        /// </summary>
        private static readonly Regex RegexCookiePartDomain = new Regex(@"^(?i:\s?(?<Name>domain)=?(\s|(?<Value>[^\s].*)))$");
        
        /// <summary>
        /// The regular expression to parse Set-Cookie's part Path.
        /// </summary>
        private static readonly Regex RegexCookiePartPath = new Regex(@"^(?i:\s?(?<Name>path)=?(\s|(?<Value>[^\s].*)))$");
        
        /// <summary>
        /// The regular expression to parse Set-Cookie's part Expires.
        /// </summary>
        private static readonly Regex RegexCookiePartExpires = new Regex(@"^(?i:\s?(?<Name>expires)=?(\s|(?<Value>[^\s].*)))$");

        /// <summary>
        /// The regular expression to parse Set-Cookie's part HttpOnly.
        /// </summary>
        private static readonly Regex RegexCookiePartHttpOnly = new Regex(@"^(?i:\s?(?<Name>HTTPOnly)=?(\s|(?<Value>[^\s].*)))$");

        /// <summary>
        /// The regular expression to parse Set-Cookie's part Secure.
        /// </summary>
        private static readonly Regex RegexCookiePartSecure = new Regex(@"^(?i:\s?(?<Name>secure)=?(\s|(?<Value>[^\s].*)))$");

        /// <summary>
        /// The regular expression to parse Set-Cookie's part Version.
        /// </summary>
        private static readonly Regex RegexCookiePartVersion = new Regex(@"^(?i:\s?(?<Name>version)=?(\s|(?<Value>[^\s].*)))$");

        /// <summary>
        /// The regular expression to parse Set-Cookie's part Name and Value.
        /// </summary>
        private static readonly Regex RegexCookiePartNameValue = new Regex(@"^((?<Name>[^=]+)=)?(\s|(\s*(?<Value>.*)))$");

        /// <summary>
        /// The regular expression to determine whether a splitted response cookie is broken.
        /// </summary>
        private static readonly Regex RegexCookieIsBroken = new Regex("(?i:expires\\s*=\\s*(Mon)|(Tue)|(Wed)|(Thu)|(Fri)|(Sat)|(Sun)\\s*)$");

        #endregion

        #region Methods

        /// <summary>
        /// Parses the cookies from the given webresponse's header.
        /// </summary>
        /// <param name="webResponse">The web response to parse.</param>
        /// <returns>The list of cookies.</returns>
        public static Cookie[] ParseCookies(WebResponse webResponse)
        {
            List<Cookie> lCookie = new List<Cookie>();
            if (webResponse != null &&
                !String.IsNullOrEmpty(webResponse.Headers["Set-Cookie"]))
            {
                string[] responseCookies = webResponse.Headers["Set-Cookie"].Split(',');
                for (int idx = 0; idx < responseCookies.Length; idx++)
                {
                    string responseCookie = responseCookies[idx];
                    if (RegexHelper.IsMatch(RegexCookieIsBroken, responseCookie))
                    {
                        responseCookie += responseCookies[idx + 1];
                        idx++;
                    }
                    Cookie cookie = ParseCookie(responseCookie);
                    if (String.IsNullOrEmpty(cookie.Domain))
                        cookie.Domain = webResponse.ResponseUri.Authority;
                    lCookie.Add(cookie);
                }
            }
            return lCookie.ToArray();
        }

        /// <summary>
        /// Parses a cookie from Set-Cookie string.
        /// </summary>
        /// <param name="setCookie">The Set-Cookie string.</param>
        /// <returns>The parsed cookie.</returns>
        public static Cookie ParseCookie(string setCookie)
        {
            Cookie cookie = new Cookie() 
            { 
                Path = "/", 
                Expired = false,
                Expires = DateTime.MaxValue
            };
            string[] parts = setCookie.Split(';');
            string partNameValue = String.Empty;

            for (int idxPart = parts.Length - 1; idxPart >= 0; idxPart--)
            {
                string part = parts[idxPart];
                if (!String.IsNullOrEmpty(part))
                {
                    if (RegexHelper.IsMatch(RegexCookiePartDomain, part))
                    {
                        string domain = RegexHelper.Match(RegexCookiePartDomain, part).Groups["Value"].Value;
                        cookie.Domain = domain;
                    }
                    else if (RegexHelper.IsMatch(RegexCookiePartPath, part))
                    {
                        string path = RegexHelper.Match(RegexCookiePartPath, part).Groups["Value"].Value;
                        cookie.Path = path;
                    }
                    else if (RegexHelper.IsMatch(RegexCookiePartExpires, part))
                        try
                        {
                            string expires = RegexHelper.Match(RegexCookiePartExpires, part).Groups["Value"].Value;
                            if (!String.IsNullOrEmpty(expires))
                                cookie.Expires = DateTime.Parse(expires, CultureInfo.InvariantCulture);
                        }
                        catch (FormatException) { }
                    else if (RegexHelper.IsMatch(RegexCookiePartHttpOnly, part))
                    {
                        string httpOnly = RegexHelper.Match(RegexCookiePartHttpOnly, part).Groups["Value"].Value;
                        if (!String.IsNullOrEmpty(httpOnly))
                            cookie.HttpOnly = Boolean.Parse(httpOnly);
                    }
                    else if (RegexHelper.IsMatch(RegexCookiePartSecure, part))
                    {
                        string secure = RegexHelper.Match(RegexCookiePartSecure, part).Groups["Value"].Value;
                        if (!String.IsNullOrEmpty(secure))
                            cookie.Secure = Boolean.Parse(secure);
                    }
                    else if (RegexHelper.IsMatch(RegexCookiePartVersion, part))
                    {
                        string version = RegexHelper.Match(RegexCookiePartVersion, part).Groups["Value"].Value;
                        if (!String.IsNullOrEmpty(version))
                            cookie.Version = Int32.Parse(version, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        partNameValue = String.Concat(part, partNameValue);
                    }
                }
            }
            if (RegexHelper.IsMatch(RegexCookiePartNameValue, partNameValue))
            {
                Match matchCookieNameValue = RegexHelper.Match(RegexCookiePartNameValue, partNameValue);
                string name = matchCookieNameValue.Groups["Name"].Value;
                string value = matchCookieNameValue.Groups["Value"].Value;
                if (!String.IsNullOrEmpty(name))
                    cookie.Name = name;
                if (!String.IsNullOrEmpty(value))
                    cookie.Value = value;
            }

            return cookie;
        }

        #endregion
    }
}
