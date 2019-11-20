using Microsoft.Live;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace InfoLocker.FileSystem
{
    public class OneDriveFolder : CommonFolder
    {
        public static string RootFolder = @"ONEDRIVE";

        private string id;

        internal OneDriveFolder(string id, string name, OneDriveFolder parent)
            : base(name, parent)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            this.id = id;
        }

        public static async void Open(string path, bool create, FolderOpenOrCreateCompleted callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            try
            {
                OneDriveFolder folder = await Open(path, create);
                callback(folder, (folder != null), null);
            }
            catch(Exception exp)
            {
                callback(null, false, exp);
            }
        }

        public static async Task<OneDriveFolder> Open(string path, bool create)
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

            if (!OneDriveFileSystem.Instance.IsInitialized)
            {
                bool init = await OneDriveFileSystem.Instance.Initialize();
                if (!init)
                {
                    throw new Exception("Could not connect to OneDrive");
                }
            }

            if ((path == (@"\" + OneDriveFolder.RootFolder)) ||
                (path == @"\"))
            {
                OneDriveFolder newFolder = new OneDriveFolder("me/skydrive", OneDriveFolder.RootFolder, null);
                return newFolder;
            }
            else
            {
                string folderName = Path.GetFileName(path);
                string directory = Path.GetDirectoryName(path);

                OneDriveFolder parent = await OneDriveFolder.Open(directory, create);
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
                        return child as OneDriveFolder;
                    }
                }

                if (create)
                {
                    Dictionary<string, object> folderDetails = new Dictionary<string, object>();
                    folderDetails.Add("name", folderName);
                    LiveOperationResult createFolder = await OneDriveFileSystem.Instance.LiveClient.PostAsync(parent.ID, folderDetails);
                    if (createFolder.Result == null)
                    {
                        throw new Exception(string.Format("Could not create new folder - {0}", path));
                    }

                    dynamic folder = createFolder.Result;
                    string name = folder.name;
                    string id = folder.id;

                    OneDriveFolder newFolder = new OneDriveFolder(id, name, parent);
                    return newFolder;
                }
                else
                {
                    return null;
                }
            }
        }

        public string ID
        {
            get { return this.id; }
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

            LiveOperationResult deleteFolder = await OneDriveFileSystem.Instance.LiveClient.DeleteAsync(this.ID);
            if (deleteFolder.Result == null)
            {
                throw new Exception(string.Format("Could not this folder - {0}", this.FullPath));
            }

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

            LiveOperationResult getFolder = await OneDriveFileSystem.Instance.LiveClient.GetAsync(this.ID + "/files");
            if (getFolder.Result == null)
            {
                throw new Exception(string.Format("Could not enumerate files in this folder - {0}", this.FullPath));
            }

            dynamic result = getFolder.Result;
            foreach (dynamic child in result.data)
            {
                string name = child.name;
                string id = child.id;

                if (getFolders && 
                    ((child.type == "folder") || (child.type == "album")))
                {
                    OneDriveFolder childFolder = new OneDriveFolder(id, name, this);
                    children.Add(childFolder);
                }
                else if (getFiles && 
                    ((child.type == "file") || (child.type == "photo") || (child.type == "video") || (child.type == "audio")))
                {
                    OneDriveFile childFolder = new OneDriveFile(id, name, this);
                    children.Add(childFolder);
                }
                else
                {
                    throw new Exception(string.Format("Unknown child type - {0}", child.type));
                }
            }

            return children;
        }
    }
}
