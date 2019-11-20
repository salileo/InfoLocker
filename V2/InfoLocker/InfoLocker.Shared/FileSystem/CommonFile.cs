using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace InfoLocker.FileSystem
{
    public class CommonFile : CommonEntry
    {
        public delegate void FileOpenOrCreateCompleted(CommonFile file, bool success, Exception exp);
        public delegate void FileDeleteCompleted(CommonFile file, bool success, Exception exp);
        public delegate void FileReadCompleted(CommonFile file, string data, Exception exp);
        public delegate void FileWriteCompleted(CommonFile file, bool success, Exception exp);

        protected CommonFile(string name, CommonFolder parent)
            : base(name, true, parent)
        {
        }

        public virtual Task Delete(FileDeleteCompleted callback)
        {
            throw new NotImplementedException("Delete");
        }

        public virtual Task<bool> Delete()
        {
            throw new NotImplementedException("Delete");
        }

        public virtual Task Read(FileReadCompleted callback)
        {
            throw new NotImplementedException("Read");
        }

        public virtual Task<string> Read()
        {
            throw new NotImplementedException("Read");
        }

        public virtual Task Write(string data, FileWriteCompleted callback)
        {
            throw new NotImplementedException("Write");
        }

        public virtual Task Write(string data)
        {
            throw new NotImplementedException("Write");
        }

        public virtual void Close()
        {
            throw new NotImplementedException("Close");
        }
    }
}
