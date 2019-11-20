using Microsoft.Live;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace InfoLocker.FileSystem
{
    public class OneDriveFileSystem
    {
        private static OneDriveFileSystem instance;
        private static object lockObj = new object();

        private LiveConnectClient liveClient;

        public static OneDriveFileSystem Instance
        {
            get
            {
                lock(lockObj)
                {
                    if (instance == null)
                    {
                        instance = new OneDriveFileSystem();
                    }

                    return instance;
                }
            }
        }

        public bool IsInitialized
        {
            get { return this.liveClient != null; }
        }

        public LiveConnectClient LiveClient
        {
            get { return this.liveClient; }
        }

        public async Task<bool> Initialize()
        {
            try
            {
                LiveAuthClient authClient = new LiveAuthClient();
                LiveLoginResult login = await authClient.LoginAsync(new string[] { "wl.signin", "wl.basic", "wl.skydrive", "wl.skydrive_update" });

                if (login.Status == LiveConnectSessionStatus.Connected)
                {
                    lock (lockObj)
                    {
                        this.liveClient = new LiveConnectClient(login.Session);
                    }

                    return true;
                }

                return false;
            }
            catch(Exception)
            {
                return false;
            }
        }
    }
}
