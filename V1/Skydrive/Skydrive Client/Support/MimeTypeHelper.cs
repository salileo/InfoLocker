using System;
using System.Collections.Generic;
using System.IO;

namespace HgCo.WindowsLive.SkyDrive.Support
{
    /// <summary>
    /// Provides methods for mapping FileName into the appropriate MIME Content-Type.
    /// </summary>
    internal static class MimeTypeHelper
    {
        #region Fields
        
        /// <summary>
        /// Represents the default (fall-back) Content-Type.
        /// </summary>
        public const string DefaultContentType = "application/octet-stream";
        
        /// <summary>
        /// The dictionary containing the MIME Types.
        /// </summary>
        private static Dictionary<string, string> dicMimeType;
        
        #endregion

        #region Properties
        
        /// <summary>
        /// Gets the dictionary containing the MIME Types.
        /// </summary>
        /// <value>The MIME Type dictionary (Dictionary[FileExtension:string, Content-Type:string]).</value>
        public static Dictionary<string, string> MimeTypeDictionary 
        {
            get 
            {
                if (dicMimeType == null)
                    InitMimeTypeDictionary();
                return dicMimeType; 
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the <see cref="MimeTypeHelper"/> class.
        /// </summary>
        private static void InitMimeTypeDictionary()
        {
            dicMimeType = new Dictionary<string, string>(256);
            dicMimeType.Add(".ai", "application/postscript");
            dicMimeType.Add(".aif", "audio/x-aiff");
            dicMimeType.Add(".aifc", "audio/x-aiff");
            dicMimeType.Add(".aiff", "audio/x-aiff");
            dicMimeType.Add(".arj", "application/x-arj-compressed");
            dicMimeType.Add(".asc", "text/plain");
            //dicMimeType.Add(".asc txt", "text/plain");
            dicMimeType.Add(".asf", "video/x-ms-asf");
            dicMimeType.Add(".asx", "video/x-ms-asx");
            dicMimeType.Add(".au", "audio/basic");
            //dicMimeType.Add(".au", "audio/ulaw");
            dicMimeType.Add(".avi", "video/x-msvideo");
            dicMimeType.Add(".bat", "application/x-msdos-program");
            dicMimeType.Add(".bcpio", "application/x-bcpio");
            dicMimeType.Add(".bin", "application/octet-stream");
            dicMimeType.Add(".c", "text/plain");
            dicMimeType.Add(".cc", "text/plain");
            dicMimeType.Add(".ccad", "application/clariscad");
            dicMimeType.Add(".cdf", "application/x-netcdf");
            dicMimeType.Add(".class", "application/octet-stream");
            dicMimeType.Add(".cod", "application/vnd.rim.cod");
            dicMimeType.Add(".com", "application/x-msdos-program");
            dicMimeType.Add(".cpio", "application/x-cpio");
            dicMimeType.Add(".cpt", "application/mac-compactpro");
            dicMimeType.Add(".csh", "application/x-csh");
            dicMimeType.Add(".css", "text/css");
            dicMimeType.Add(".dcr", "application/x-director");
            dicMimeType.Add(".deb", "application/x-debian-package");
            dicMimeType.Add(".dir", "application/x-director");
            dicMimeType.Add(".dl", "video/dl");
            dicMimeType.Add(".dms", "application/octet-stream");
            dicMimeType.Add(".doc", "application/msword");
            dicMimeType.Add(".drw", "application/drafting");
            dicMimeType.Add(".dvi", "application/x-dvi");
            dicMimeType.Add(".dwg", "application/acad");
            dicMimeType.Add(".dxf", "application/dxf");
            dicMimeType.Add(".dxr", "application/x-director");
            dicMimeType.Add(".eps", "application/postscript");
            dicMimeType.Add(".etx", "text/x-setext");
            dicMimeType.Add(".exe", "application/octet-stream");
            //dicMimeType.Add(".exe", "application/x-msdos-program");
            dicMimeType.Add(".ez", "application/andrew-inset");
            dicMimeType.Add(".f", "text/plain");
            dicMimeType.Add(".f90", "text/plain");
            dicMimeType.Add(".fli", "video/fli");
            //dicMimeType.Add(".fli", "video/x-fli");
            dicMimeType.Add(".flv", "video/flv");
            dicMimeType.Add(".gif", "image/gif");
            dicMimeType.Add(".gl", "video/gl");
            dicMimeType.Add(".gtar", "application/x-gtar");
            //dicMimeType.Add(".gz", "application/x-gunzip");
            dicMimeType.Add(".gz", "application/x-gzip");
            dicMimeType.Add(".h", "text/plain");
            dicMimeType.Add(".hdf", "application/x-hdf");
            dicMimeType.Add(".hh", "text/plain");
            dicMimeType.Add(".hqx", "application/mac-binhex40");
            dicMimeType.Add(".htm", "text/html");
            dicMimeType.Add(".html", "text/html");
            //dicMimeType.Add(".html htm", "text/html");
            dicMimeType.Add(".ice", "x-conference/x-cooltalk");
            dicMimeType.Add(".ief", "image/ief");
            dicMimeType.Add(".iges", "model/iges");
            dicMimeType.Add(".igs", "model/iges");
            dicMimeType.Add(".ips", "application/x-ipscript");
            dicMimeType.Add(".ipx", "application/x-ipix");
            dicMimeType.Add(".jad", "text/vnd.sun.j2me.app-descriptor");
            dicMimeType.Add(".jar", "application/java-archive");
            dicMimeType.Add(".jpe", "image/jpeg");
            dicMimeType.Add(".jpeg", "image/jpeg");
            dicMimeType.Add(".jpg", "image/jpeg");
            dicMimeType.Add(".js", "application/x-javascript");
            dicMimeType.Add(".kar", "audio/midi");
            dicMimeType.Add(".latex", "application/x-latex");
            dicMimeType.Add(".lha", "application/octet-stream");
            dicMimeType.Add(".lsp", "application/x-lisp");
            dicMimeType.Add(".lzh", "application/octet-stream");
            dicMimeType.Add(".m", "text/plain");
            dicMimeType.Add(".m3u", "audio/x-mpegurl");
            dicMimeType.Add(".man", "application/x-troff-man");
            dicMimeType.Add(".me", "application/x-troff-me");
            dicMimeType.Add(".mesh", "model/mesh");
            dicMimeType.Add(".mid", "audio/midi");
            dicMimeType.Add(".midi", "audio/midi");
            dicMimeType.Add(".mif", "application/vnd.mif");
            //dicMimeType.Add(".mif", "application/x-mif");
            dicMimeType.Add(".mime", "www/mime");
            dicMimeType.Add(".mov", "video/quicktime");
            dicMimeType.Add(".movie", "video/x-sgi-movie");
            //dicMimeType.Add(".mp2", "audio/mpeg");
            dicMimeType.Add(".mp2", "video/mpeg");
            dicMimeType.Add(".mp3", "audio/mpeg");
            dicMimeType.Add(".mp4", "video/mp4");
            dicMimeType.Add(".mpe", "video/mpeg");
            dicMimeType.Add(".mpeg", "video/mpeg");
            dicMimeType.Add(".mpg", "video/mpeg");
            dicMimeType.Add(".mpga", "audio/mpeg");
            dicMimeType.Add(".ms", "application/x-troff-ms");
            dicMimeType.Add(".msh", "model/mesh");
            dicMimeType.Add(".nc", "application/x-netcdf");
            dicMimeType.Add(".oda", "application/oda");
            dicMimeType.Add(".ogg", "application/ogg");
            dicMimeType.Add(".ogm", "application/ogg");
            dicMimeType.Add(".pbm", "image/x-portable-bitmap");
            dicMimeType.Add(".pdb", "chemical/x-pdb");
            dicMimeType.Add(".pdf", "application/pdf");
            dicMimeType.Add(".pgm", "image/x-portable-graymap");
            dicMimeType.Add(".pgn", "application/x-chess-pgn");
            dicMimeType.Add(".pgp", "application/pgp");
            dicMimeType.Add(".pl", "application/x-perl");
            dicMimeType.Add(".pm", "application/x-perl");
            dicMimeType.Add(".png", "image/png");
            dicMimeType.Add(".pnm", "image/x-portable-anymap");
            dicMimeType.Add(".pot", "application/vnd.ms-powerpoint");
            dicMimeType.Add(".ppm", "image/x-portable-pixmap");
            dicMimeType.Add(".pps", "application/vnd.ms-powerpoint");
            dicMimeType.Add(".ppt", "application/vnd.ms-powerpoint");
            dicMimeType.Add(".ppz", "application/vnd.ms-powerpoint");
            dicMimeType.Add(".pre", "application/x-freelance");
            dicMimeType.Add(".prt", "application/pro_eng");
            dicMimeType.Add(".ps", "application/postscript");
            dicMimeType.Add(".qt", "video/quicktime");
            dicMimeType.Add(".ra", "audio/x-realaudio");
            dicMimeType.Add(".ram", "audio/x-pn-realaudio");
            dicMimeType.Add(".rar", "application/x-rar-compressed");
            //dicMimeType.Add(".ras", "image/cmu-raster");
            dicMimeType.Add(".ras", "image/x-cmu-raster");
            dicMimeType.Add(".rgb", "image/x-rgb");
            dicMimeType.Add(".rm", "audio/x-pn-realaudio");
            dicMimeType.Add(".roff", "application/x-troff");
            dicMimeType.Add(".rpm", "audio/x-pn-realaudio-plugin");
            dicMimeType.Add(".rtf", "application/rtf");
            //dicMimeType.Add(".rtf", "text/rtf");
            dicMimeType.Add(".rtx", "text/richtext");
            dicMimeType.Add(".scm", "application/x-lotusscreencam");
            dicMimeType.Add(".set", "application/set");
            dicMimeType.Add(".sgm", "text/sgml");
            dicMimeType.Add(".sgml", "text/sgml");
            dicMimeType.Add(".sh", "application/x-sh");
            dicMimeType.Add(".shar", "application/x-shar");
            dicMimeType.Add(".silo", "model/mesh");
            dicMimeType.Add(".sit", "application/x-stuffit");
            dicMimeType.Add(".skd", "application/x-koan");
            dicMimeType.Add(".skm", "application/x-koan");
            dicMimeType.Add(".skp", "application/x-koan");
            dicMimeType.Add(".skt", "application/x-koan");
            dicMimeType.Add(".smi", "application/smil");
            dicMimeType.Add(".smil", "application/smil");
            dicMimeType.Add(".snd", "audio/basic");
            dicMimeType.Add(".sol", "application/solids");
            dicMimeType.Add(".spl", "application/x-futuresplash");
            dicMimeType.Add(".src", "application/x-wais-source");
            dicMimeType.Add(".step", "application/STEP");
            dicMimeType.Add(".stl", "application/SLA");
            dicMimeType.Add(".stp", "application/STEP");
            dicMimeType.Add(".sv4cpio", "application/x-sv4cpio");
            dicMimeType.Add(".sv4crc", "application/x-sv4crc");
            dicMimeType.Add(".swf", "application/x-shockwave-flash");
            dicMimeType.Add(".t", "application/x-troff");
            dicMimeType.Add(".tar", "application/x-tar");
            dicMimeType.Add(".tar.gz", "application/x-tar-gz");
            dicMimeType.Add(".tcl", "application/x-tcl");
            dicMimeType.Add(".tex", "application/x-tex");
            dicMimeType.Add(".texi", "application/x-texinfo");
            dicMimeType.Add(".texinfo", "application/x-texinfo");
            dicMimeType.Add(".tgz", "application/x-tar-gz");
            dicMimeType.Add(".tif", "image/tiff");
            //dicMimeType.Add(".tif tiff", "image/tiff");
            dicMimeType.Add(".tiff", "image/tiff");
            dicMimeType.Add(".tr", "application/x-troff");
            dicMimeType.Add(".tsi", "audio/TSP-audio");
            dicMimeType.Add(".tsp", "application/dsptype");
            dicMimeType.Add(".tsv", "text/tab-separated-values");
            dicMimeType.Add(".txt", "text/plain");
            dicMimeType.Add(".unv", "application/i-deas");
            dicMimeType.Add(".ustar", "application/x-ustar");
            dicMimeType.Add(".vcd", "application/x-cdlink");
            dicMimeType.Add(".vda", "application/vda");
            dicMimeType.Add(".viv", "video/vnd.vivo");
            dicMimeType.Add(".vivo", "video/vnd.vivo");
            dicMimeType.Add(".vrm", "x-world/x-vrml");
            //dicMimeType.Add(".vrml", "model/vrml");
            dicMimeType.Add(".vrml", "x-world/x-vrml");
            dicMimeType.Add(".wav", "audio/x-wav");
            dicMimeType.Add(".wax", "audio/x-ms-wax");
            dicMimeType.Add(".wma", "audio/x-ms-wma");
            dicMimeType.Add(".wmv", "video/x-ms-wmv");
            dicMimeType.Add(".wmx", "video/x-ms-wmx");
            dicMimeType.Add(".wrl", "model/vrml");
            dicMimeType.Add(".wvx", "video/x-ms-wvx");
            dicMimeType.Add(".xbm", "image/x-xbitmap");
            dicMimeType.Add(".xlc", "application/vnd.ms-excel");
            dicMimeType.Add(".xll", "application/vnd.ms-excel");
            dicMimeType.Add(".xlm", "application/vnd.ms-excel");
            //dicMimeType.Add(".xls", "application/excel");
            dicMimeType.Add(".xls", "application/vnd.ms-excel");
            dicMimeType.Add(".xlw", "application/vnd.ms-excel");
            dicMimeType.Add(".xml", "text/xml");
            dicMimeType.Add(".xpm", "image/x-xpixmap");
            dicMimeType.Add(".xwd", "image/x-xwindowdump");
            dicMimeType.Add(".xyz", "chemical/x-pdb");
            //dicMimeType.Add(".zip", "application/x-zip-compressed");
            dicMimeType.Add(".zip", "application/zip");
        }

        /// <summary>
        /// Gets the type of the content.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public static string GetContentType(string fileName)
        {
            if (!String.IsNullOrEmpty(fileName))
            {
                string fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
                if (MimeTypeDictionary.ContainsKey(fileExtension))
                {
                    string contentType = MimeTypeDictionary[fileExtension];
                    return contentType;
                }
                else return DefaultContentType;
            }
            return DefaultContentType;
        }

        #endregion
    }
}
