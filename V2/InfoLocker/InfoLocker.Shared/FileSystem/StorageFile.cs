using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace InfoLocker.FileSystem
{
    public class StorageFile : CommonFile
    {
        private Windows.Storage.StorageFile internalFile;

        internal StorageFile(Windows.Storage.StorageFile internalFile, string name, StorageFolder parent)
            : base(name, parent)
        {
            if (internalFile == null)
            {
                throw new ArgumentNullException("internalFile");
            }

            this.internalFile = internalFile;
        }

        public static async void Open(string path, bool useRoaming, FileOpenOrCreateCompleted callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            try
            {
                StorageFile file = await Open(path, useRoaming);
                callback(file, (file != null), null);
            }
            catch (Exception exp)
            {
                callback(null, false, exp);
            }
        }

        public static async Task<StorageFile> Open(string path, bool useRoaming)
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

            string filename = Path.GetFileName(path);
            string directory = Path.GetDirectoryName(path);

            StorageFolder parent = await StorageFolder.Open(directory, false, useRoaming);
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
                    return child as StorageFile;
                }
            }

            return null;
        }

        public static async void Create(string path, string initialContent, bool useRoaming, FileOpenOrCreateCompleted callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            try
            {
                StorageFile file = await Create(path, initialContent, useRoaming);
                callback(file, (file != null), null);
            }
            catch (Exception exp)
            {
                callback(null, false, exp);
            }
        }

        public static async Task<StorageFile> Create(string path, string initialContent, bool useRoaming)
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

            try
            {
                StorageFile file = await StorageFile.Open(path, useRoaming);
                if (file != null)
                {
                    await file.Delete();
                }

            }
            catch (Exception)
            {
                // nothing to do here
            }

            string filename = Path.GetFileName(path);
            string directory = Path.GetDirectoryName(path);

            StorageFolder parent = await StorageFolder.Open(directory, true, useRoaming);
            if (parent == null)
            {
                throw new Exception(string.Format("Could not find/create the parent folder - {0}", path));
            }

            Windows.Storage.StorageFile child = await parent.InternalFolder.CreateFileAsync(filename);
            if (child == null)
            {
                throw new Exception(string.Format("Could not create new file - {0}", path));
            }

            await Windows.Storage.FileIO.WriteTextAsync(child, initialContent);

            StorageFile newFile = new StorageFile(child, filename, parent);
            return newFile;
        }

        public Windows.Storage.StorageFile InternalFile
        {
            get { return this.internalFile; }
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
            await this.InternalFile.DeleteAsync();
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
            string data = await Windows.Storage.FileIO.ReadTextAsync(this.InternalFile);
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
            await Windows.Storage.FileIO.WriteTextAsync(this.InternalFile, data);
        }

        public override void Close()
        {
            // nothing to do here
        }
    }
}
