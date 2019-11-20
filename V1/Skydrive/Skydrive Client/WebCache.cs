using System;
using System.Collections.Generic;
using System.Threading;

namespace HgCo.WindowsLive.SkyDrive
{
    /// <summary>
    /// Provides methods for implementing a simple cache for <see cref="SkyDriveWebClient"/>.
    /// </summary>
    internal class WebCache : IDisposable
    {
        #region Private Classes

        /// <summary>
        /// Represents a cache item.
        /// </summary>
        private class WebCacheItem
        {
            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            /// <value>The value.</value>
            public object Value { get; set; }

            /// <summary>
            /// Gets or sets the time when cache item was last acccessed.
            /// </summary>
            /// <value>The last access time.</value>
            public DateTime LastAccessTime { get; set; }
        }

        #endregion

        #region Fields
        
        /// <summary>
        /// The dictionary used to store the cache items.
        /// </summary>
        private readonly Dictionary<string, WebCacheItem> dicCache;

        /// <summary>
        /// The timer used to remove old items from cache.
        /// </summary>
        private readonly Timer myTimer;

        /// <summary>
        /// The lock object used for thread safety.
        /// </summary>
        private readonly object lockObject;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the number of items contained in the cache.
        /// </summary>
        /// <value>The number of items contained in the cache.</value>
        public int Count 
        { 
            get 
            {
                lock (lockObject)
                {
                    return dicCache.Count;
                }
            }
        }
        
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="WebCache"/> class.
        /// </summary>
        public WebCache()
        {
            dicCache = new Dictionary<string, WebCacheItem>();
            myTimer = new Timer(myTimer_Elapsed, null, 10 * 1000, 10 * 1000);
            lockObject = new object();
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Adds the specified item to the cache.
        /// </summary>
        /// <param name="key">The cache key used to reference the item.</param>
        /// <param name="value">The item to be added to the cache.</param>
        public void Add(string key, object value)
        {
            lock (lockObject)
            {
                dicCache.Add(
                    key,
                    new WebCacheItem
                    {
                        Value = value,
                        LastAccessTime = DateTime.Now
                    });
            }
        }

        /// <summary>
        /// Removes the specified item from the cache.
        /// </summary>
        /// <param name="key">The key of the cache item to remove.</param>
        public void Remove(string key)
        {
            lock (lockObject)   
            {
                dicCache.Remove(key);
            }
        }

        /// <summary>
        /// Gets or sets the cache item at the specified key.
        /// </summary>
        /// <value>The cache key used to reference the item.</value>
        /// <returns>The specified cache item.</returns>
        public object this[string key]
        {
            get 
            { 
                object value = null;
                if (dicCache.ContainsKey(key))
                {
                    WebCacheItem item = dicCache[key];
                    value = item.Value;
                    item.LastAccessTime = DateTime.Now;
                }
                return value; 
            }
            set 
            {
                dicCache[key] = new WebCacheItem
                {
                    Value = value,
                    LastAccessTime = DateTime.Now
                };
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with 
        /// freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (myTimer != null)
                {
                    myTimer.Dispose();
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Called when myTimer period elapsed.
        /// </summary>
        /// <param name="state">The state.</param>
        private void myTimer_Elapsed(object state)
        {
            lock (lockObject)
            {
                DateTime dtNow = DateTime.Now;
                string[] keys = new string[dicCache.Keys.Count];
                dicCache.Keys.CopyTo(keys, 0);
                for (int idxKey = 0; idxKey < keys.Length; idxKey++)
                {
                    WebCacheItem item = dicCache[keys[idxKey]];
                    if ((dtNow - item.LastAccessTime).TotalMinutes > 2)
                        Remove(keys[idxKey]);
                }
            }
        }

        #endregion

    }
}
