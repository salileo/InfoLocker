using Microsoft.Live;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace InfoLocker.FileSystem
{
    public class OneDriveFile : CommonFile
    {
        private string id;

        internal OneDriveFile(string id, string name, OneDriveFolder parent)
            : base(name, parent)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            this.id = id;
        }

        public static async void Open(string path, FileOpenOrCreateCompleted callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            try
            {
                OneDriveFile file = await Open(path);
                callback(file, (file != null), null);
            }
            catch (Exception exp)
            {
                callback(null, false, exp);
            }
        }

        public static async Task<OneDriveFile> Open(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            string newPath;
            while(true)
            {
                newPath = path.Replace(@"\\", @"\");
                if (newPath == path)
                {
                    break;
                }

                path = newPath;
            }

            if (path[0] != '\\' || path.EndsWith("\\"))
            {
                throw new ArgumentException(string.Format("File path is incorrect - {0}", path));
            }

            if (!OneDriveFileSystem.Instance.IsInitialized)
            {
                bool init = await OneDriveFileSystem.Instance.Initialize();
                if (!init)
                {
                    throw new Exception("Could not connect to OneDrive");
                }
            }

            string filename = Path.GetFileName(path);
            string directory = Path.GetDirectoryName(path);

            OneDriveFolder parent = await OneDriveFolder.Open(directory, false);
            if (parent == null)
            {
                throw new Exception(string.Format("Could not find/create the parent folder - {0}", path));
            }

            List<CommonEntry> children = await parent.GetChildren(false, true);
            if (children == null)
            {
                throw new Exception(string.Format("Could not enumerate files in the parent folder - {0}", path));
            }

            foreach (CommonEntry child in children)
            {
                if (child.Name.Equals(filename, StringComparison.CurrentCultureIgnoreCase))
                {
                    return child as OneDriveFile;
                }
            }

            return null;
        }

        public static async void Create(string path, string initialContent, FileOpenOrCreateCompleted callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            try
            {
                OneDriveFile file = await Create(path, initialContent);
                callback(file, (file != null), null);
            }
            catch (Exception exp)
            {
                callback(null, false, exp);
            }
        }

        public static async Task<OneDriveFile> Create(string path, string initialContent)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            if (string.IsNullOrEmpty(initialContent))
            {
                throw new ArgumentNullException("initialContent");
            }

            string newPath;
            while (true)
            {
                newPath = path.Replace(@"\\", @"\");
                if (newPath == path)
                {
                    break;
                }

                path = newPath;
            }

            if (path[0] != '\\' || path.EndsWith("\\"))
            {
                throw new ArgumentException(string.Format("File path is incorrect - {0}", path));
            }

            if (!OneDriveFileSystem.Instance.IsInitialized)
            {
                bool init = await OneDriveFileSystem.Instance.Initialize();
                if (!init)
                {
                    throw new Exception("Could not connect to OneDrive");
                }
            }

            string filename = Path.GetFileName(path);
            string directory = Path.GetDirectoryName(path);

            OneDriveFolder parent = await OneDriveFolder.Open(directory, true);
            if (parent == null)
            {
                throw new Exception(string.Format("Could not find/create the parent folder - {0}", path));
            }

            byte[] randomData = Encoding.UTF8.GetBytes(initialContent);
            MemoryStream stream = new MemoryStream(randomData);
            LiveOperationResult createFile = await OneDriveFileSystem.Instance.LiveClient.BackgroundUploadAsync(parent.ID, filename, stream.AsInputStream(), OverwriteOption.Overwrite);
            if (createFile.Result == null)
            {
                throw new Exception(string.Format("Could not create new file - {0}", path));
            }

            dynamic file = createFile.Result;
            string name = file.name;
            string id = file.id;

            OneDriveFile newFile = new OneDriveFile(id, name, parent);
            return newFile;
        }

        public string ID
        {
            get { return this.id; }
        }

        public override async Task Delete(FileDeleteCompleted callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            try
            {
                bool success = await this.Delete();
                callback(this, success, null);
            }
            catch (Exception exp)
            {
                callback(this, false, exp);
            }
        }

        public override async Task<bool> Delete()
        {
            LiveOperationResult deleteFolder = await OneDriveFileSystem.Instance.LiveClient.DeleteAsync(this.ID);
            if (deleteFolder.Result == null)
            {
                throw new Exception(string.Format("Could not this file - {0}", this.FullPath));
            }

            return true;
        }

        public override async Task Read(FileReadCompleted callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            try
            {
                string data = await this.Read();
                callback(this, data, null);
            }
            catch (Exception exp)
            {
                callback(this, null, exp);
            }
        }

        public override async Task<string> Read()
        {
            LiveDownloadOperationResult download = await OneDriveFileSystem.Instance.LiveClient.BackgroundDownloadAsync(this.ID + "/content");
            if (download.Stream == null)
            {
                throw new Exception(string.Format("Could not download file content - {0}", this.Name));
            }

            Stream stream = download.Stream.AsStreamForRead(0);
            StreamReader reader = new StreamReader(stream);
            string data = await reader.ReadToEndAsync();
            return data;
        }

        public override async Task Write(string data, FileWriteCompleted callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            try
            {
                await this.Write(data);
                callback(this, true, null);
            }
            catch (Exception exp)
            {
                callback(this, false, exp);
            }
        }

        public override async Task Write(string data)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            MemoryStream stream = new MemoryStream(dataBytes);

            LiveOperationResult createFile = await OneDriveFileSystem.Instance.LiveClient.BackgroundUploadAsync((this.Parent as OneDriveFolder).ID, this.Name, stream.AsInputStream(), OverwriteOption.Overwrite);
            if (createFile.Result == null)
            {
                throw new Exception(string.Format("Could not write file - {0}", this.FullPath));
            }

            dynamic file = createFile.Result;
            this.id = file.id;
        }

        public override void Close()
        {
            // nothing to do here
        }
    }
}
