using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Text.RegularExpressions;

namespace HgCo.WindowsLive.SkyDrive.Support
{
    /// <summary>
    /// Provides methods for parsing HTML documents.
    /// </summary>
    internal static class HtmlDocumentHelper
    {
        #region Fields

        /// <summary>
        /// The regular expression to parse any HTML tag.
        /// </summary>
        private static readonly Regex RegexTag = new Regex("<\\s*(?<Name>\\w+)(\\s+(?<Attributes>(\\s*[\\w\\-]+(\\s*=\\s*(\"[^\"]*\")|('[^']*'))?)*))?\\s*/?\\s*>");
        
        /// <summary>
        /// The regular expression to parse an HTML META tag.
        /// </summary>
        private static readonly Regex RegexTagMeta = new Regex("(?i:<\\s*(?<Name>meta)(\\s+(?<Attributes>(\\s*[\\w\\-]+(\\s*=\\s*(\"[^\"]*\")|('[^']*'))?)*))?\\s*/?\\s*>)");

        /// <summary>
        /// The regular expression to parse attribute Refresh of HTML META tag.
        /// </summary>
        private static readonly Regex RegexTagMetaRefreshUrl = new Regex("URL=(?<URL>[^\"]+)");

        /// <summary>
        /// The regular expression to parse attributes of an HTML tag.
        /// </summary>
        private static readonly Regex RegexTagAttribute = new Regex("(?<Name>[\\w\\-]+)(\\s*=\\s*(\"(?<Value>[^\"]*)\")|('(?<Value>[^']*')))?");

        /// <summary>
        /// The regular expression to parse value of style attribute of an HTML tag.
        /// </summary>
        private static readonly Regex RegexTagStyleValue = new Regex("\\s*(?<Name>[\\w\\-]+)\\s*:\\s*(?<Value>[^;]+);?");

        /// <summary>
        /// The regular expression to parse an escaped Unicode character.
        /// </summary>
        private static readonly Regex RegexUnicodeChar = new Regex(@"&#(?<Value>\d+);");

        /// <summary>
        /// The regular expression to parse an escaped Javascript character.
        /// </summary>
        private static readonly Regex RegexJavascriptChar = new Regex(@"\\(?<Value>(x[0-9A-Fa-f]{2})|(\d{2}))");

        #endregion

        #region Methods

        /// <summary>
        /// Gets a tag by name.
        /// </summary>
        /// <param name="htmlDocument">The HTML document to parse.</param>
        /// <param name="tagName">The name of the tag.</param>
        /// <returns>The tag.</returns>
        public static string GetTagByName(string htmlDocument, string tagName)
        {
            if (!String.IsNullOrEmpty(tagName))
            {
                MatchCollection matchTags = RegexHelper.Matches(RegexTag, htmlDocument);
                foreach (Match matchTag in matchTags)
                {
                    string attributes = matchTag.Groups["Attributes"].Value;
                    string name = GetTagAttributeValueByName(attributes, "name");
                    if (tagName.Equals(name, StringComparison.OrdinalIgnoreCase))
                        return matchTag.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the meta tag's refresh URI.
        /// </summary>
        /// <param name="htmlDocument">The HTML document.</param>
        /// <returns>The refresh URI.</returns>
        public static Uri GetMetaTagRefreshUri(string htmlDocument)
        {
            MatchCollection matchMetaTags = RegexHelper.Matches(RegexTagMeta, htmlDocument);
            foreach (Match matchMetaTag in matchMetaTags)
            {
                NameValueCollection attributes = GetTagAttributes(matchMetaTag.Value);
                if (!String.IsNullOrEmpty(attributes["http-equiv"]) &&
                    attributes["http-equiv"].Equals("refresh", StringComparison.OrdinalIgnoreCase))
                {
                    string url = RegexHelper.Match(RegexTagMetaRefreshUrl, attributes["content"]).Groups["URL"].Value;
                    return UriHelper.GetUri(url);
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the value of a tag's attribute.
        /// </summary>
        /// <param name="tagAttributes">The tag attributes.</param>
        /// <param name="tagAttributeName">The name of the tag attribute to get.</param>
        /// <returns>The attribute's value.</returns>
        public static string GetTagAttributeValueByName(string tagAttributes, string tagAttributeName)
        {
            if (!String.IsNullOrEmpty(tagAttributeName))
            {
                MatchCollection matchTagAttributes = RegexHelper.Matches(RegexTagAttribute, tagAttributes);
                foreach (Match matchTagAttribute in matchTagAttributes)
                {
                    string name = matchTagAttribute.Groups["Name"].Value;
                    if (tagAttributeName.Equals(name, StringComparison.OrdinalIgnoreCase))
                        return matchTagAttribute.Groups["Value"].Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a tag's attributes.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <returns>The collection of attributes.</returns>
        public static NameValueCollection GetTagAttributes(string tag)
        {
            NameValueCollection tagAttributes = new NameValueCollection();
            if (!String.IsNullOrEmpty(tag))
            {
                Match matchTag = RegexHelper.Match(RegexTag, tag);
                MatchCollection matchTagAttributes = RegexHelper.Matches(RegexTagAttribute, matchTag.Groups["Attributes"].Value);
                foreach (Match matchTagAttribute in matchTagAttributes)
                {
                    string name = matchTagAttribute.Groups["Name"].Value.ToLowerInvariant();
                    string value = matchTagAttribute.Groups["Value"].Value;
                    tagAttributes.Add(name, value);
                }
            }

            return tagAttributes;
        }

        /// <summary>
        /// Parses a tag's style attributes.
        /// </summary>
        /// <param name="styleValue">The value of style attribute to parse.</param>
        /// <returns>The collection of style key/value pairs.</returns>
        public static NameValueCollection ParseStyleValue(string styleValue)
        {
            NameValueCollection styleValues = new NameValueCollection();
            if (!String.IsNullOrEmpty(styleValue))
            {
                MatchCollection matchStyleValues = RegexHelper.Matches(RegexTagStyleValue, styleValue);
                foreach (Match matchStyleValue in matchStyleValues)
                {
                    string name = matchStyleValue.Groups["Name"].Value.ToLowerInvariant();
                    string value = matchStyleValue.Groups["Value"].Value;
                    styleValues.Add(name, value);
                }
            }

            return styleValues;
        }

        /// <summary>
        /// Decodes a unicode string by replacing the escaped strings to the appropriate unicode chars.
        /// </summary>
        /// <param name="text">The string to decode.</param>
        /// <returns>The decoded string.</returns>
        public static string DecodeUnicodeString(string text)
        {
            string valueParsed = RegexHelper.Replace(RegexUnicodeChar, text, delegate(Match match)
            {
                string unicodeValue = match.Groups[1].Value;
                char c = (char)Int32.Parse(unicodeValue, CultureInfo.InvariantCulture);
                return c.ToString();
            });
            return valueParsed;
        }

        /// <summary>
        /// Decodes a javascript string by replacing the escaped strings (\x00) 
        /// to the appropriate ASCII char.
        /// </summary>
        /// <param name="text">The string to decode.</param>
        /// <returns>The decoded string.</returns>
        public static string DecodeJavascriptString(string text)
        {
            string valueParsed = RegexHelper.Replace(RegexJavascriptChar, text, delegate(Match match)
            {
                string jsValue = match.Groups[1].Value;
                if (jsValue.StartsWith("x", StringComparison.OrdinalIgnoreCase))
                {
                    char c = (char)Int32.Parse(jsValue.Substring(1), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                    return c.ToString();
                }
                else
                {
                    char c = (char)Int32.Parse(jsValue, CultureInfo.InvariantCulture);
                    return c.ToString();
                }
            });
            return valueParsed;
        }

        #endregion
    }
}
