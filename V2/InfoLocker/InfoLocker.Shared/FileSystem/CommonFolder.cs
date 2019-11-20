using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace InfoLocker.FileSystem
{
    public class CommonFolder : CommonEntry
    {
        public delegate void FolderOpenOrCreateCompleted(CommonFolder folder, bool success, Exception exp);
        public delegate void FolderDeleteCompleted(CommonFolder folder, bool success, Exception exp);
        public delegate void ChildrenEnumerated(CommonFolder folder, List<CommonEntry> children, Exception exp);

        protected CommonFolder(string name, CommonFolder parent)
            : base(name, false, parent)
        {
        }
    
        public virtual Task Delete(FolderDeleteCompleted callback)
        {
            throw new NotImplementedException("Delete");
        }

        public virtual Task<bool> Delete()
        {
            throw new NotImplementedException("Delete");
        }

        public virtual Task GetChildren(bool getFolders, bool getFiles, ChildrenEnumerated callback)
        {
            throw new NotImplementedException("GetChildren");
        }

        public virtual Task<List<CommonEntry>> GetChildren(bool getFolders, bool getFiles)
        {
            throw new NotImplementedException("GetChildren");
        }
    }
}
