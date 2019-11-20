using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace InfoLocker.FileSystem
{
    public class StorageFolder : CommonFolder
    {
        public static string LocalRootFolder = @"LOCALSTORAGE";
        public static string RoamingRootFolder = @"ROAMINGSTORAGE";

        Windows.Storage.StorageFolder internalFolder;

        internal StorageFolder(Windows.Storage.StorageFolder internalFolder, string name, StorageFolder parent)
            : base(name, parent)
        {
            if (internalFolder == null)
            {
                throw new ArgumentNullException("internalFolder");
            }

            this.internalFolder = internalFolder;
        }

        public static async void Open(string path, bool create, bool useRoaming, FolderOpenOrCreateCompleted callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            try
            {
                StorageFolder folder = await Open(path, create, useRoaming);
                callback(folder, (folder != null), null);
            }
            catch(Exception exp)
            {
                callback(null, false, exp);
            }
        }

        public static async Task<StorageFolder> Open(string path, bool create, bool useRoaming)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
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

            if (path[0] != '\\')
            {
                throw new ArgumentException(string.Format("File path is incorrect - {0}", path));
            }

            if ((path.Length > 1) && path.EndsWith("\\"))
            {
                path = path.Substring(0, path.Length - 1);
            }

            if ((path == @"\") ||
                (path == (@"\" + StorageFolder.LocalRootFolder)) ||
                (path == (@"\" + StorageFolder.RoamingRootFolder)))
            {
                if (path != @"\")
                {
                    if ((useRoaming && path == (@"\" + StorageFolder.LocalRootFolder)) ||
                        (!useRoaming && path == (@"\" + StorageFolder.RoamingRootFolder)))
                    {
                        throw new Exception("Root path does not match parameter type");
                    }
                }

                StorageFolder newFolder = null;
                if (useRoaming)
                {
                    newFolder = new StorageFolder(Windows.Storage.ApplicationData.Current.RoamingFolder, StorageFolder.RoamingRootFolder, null);
                }
                else
                {
                    newFolder = new StorageFolder(Windows.Storage.ApplicationData.Current.LocalFolder, StorageFolder.LocalRootFolder, null);
                }

                return newFolder;
            }
            else
            {
                string folderName = Path.GetFileName(path);
                string directory = Path.GetDirectoryName(path);

                StorageFolder parent = await StorageFolder.Open(directory, create, useRoaming);
                if (parent == null)
                {
                    throw new Exception(string.Format("Could not find/create the parent folder - {0}", path));
                }

                List<CommonEntry> children = await parent.GetChildren(true, false);
                if (children == null)
                {
                    throw new Exception(string.Format("Could not enumerate folders in the parent folder - {0}", path));
                }

                foreach (CommonEntry child in children)
                {
                    if (child.Name.Equals(folderName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return child as StorageFolder;
                    }
                }

                if (create)
                {
                    Windows.Storage.StorageFolder child = await parent.InternalFolder.CreateFolderAsync(folderName);
                    if (child == null)
                    {
                        throw new Exception(string.Format("Could not create new folder - {0}", path));
                    }

                    StorageFolder newFolder = new StorageFolder(child, folderName, parent);
                    return newFolder;
                }
                else
                {
                    return null;
                }
            }
        }

        public Windows.Storage.StorageFolder InternalFolder
        {
            get { return this.internalFolder; }
        }

        public override async Task Delete(FolderDeleteCompleted callback)
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
            if (this.Parent == null)
            {
                throw new Exception("Cannot delete root folder");
            }

            await this.InternalFolder.DeleteAsync();
            return true;
        }

        public override async Task GetChildren(bool getFolders, bool getFiles, ChildrenEnumerated callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            try
            {
                List<CommonEntry> children = await GetChildren(getFolders, getFiles);
                callback(this, children, null);
            }
            catch (Exception exp)
            {
                callback(this, null, exp);
            }
        }

        public override async Task<List<CommonEntry>> GetChildren(bool getFolders, bool getFiles)
        {
            List<CommonEntry> children = new List<CommonEntry>();

            if (getFolders)
            {
                IReadOnlyList<Windows.Storage.StorageFolder> subFolders = await this.InternalFolder.GetFoldersAsync();
                if (subFolders == null)
                {
                    throw new Exception(string.Format("Could not enumerate sub-folders in this folder - {0}", this.FullPath));
                }

                foreach (Windows.Storage.StorageFolder subFolder in subFolders)
                {
                    StorageFolder childFolder = new StorageFolder(subFolder, subFolder.Name, this);
                    children.Add(childFolder);
                }
            }

            if (getFiles)
            {
                IReadOnlyList<Windows.Storage.StorageFile> subFiles = await this.InternalFolder.GetFilesAsync();
                if (subFiles == null)
                {
                    throw new Exception(string.Format("Could not enumerate files in this folder - {0}", this.FullPath));
                }

                foreach (Windows.Storage.StorageFile subFile in subFiles)
                {
                    StorageFile childFolder = new StorageFile(subFile, subFile.Name, this);
                    children.Add(childFolder);
                }
            }

            return children;
        }
    }
}
