using System;
using System.Collections.Generic;
using System.Text;

namespace InfoLocker.FileSystem
{
    public class CommonEntry
    {
        private string name;
        private bool isFile;
        private CommonFolder parent;

        protected CommonEntry(string name, bool isFile, CommonFolder parent)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (parent == null)
            {
                if (isFile)
                {
                    throw new ArgumentException("Cannot have a file with no parent");
                }
            }

            this.name = name;
            this.isFile = isFile;
            this.parent = parent;
        }

        public string Name
        {
            get { return this.name; }
        }

        public bool IsFile
        {
            get { return this.isFile; }
        }

        public CommonFolder Parent
        {
            get { return this.parent; }
        }

        public string FullPath
        {
            get
            {
                StringBuilder bldr = new StringBuilder();
                if (this.Parent != null)
                {
                    bldr.Append(this.Parent.FullPath);
                }
                else
                {
                    bldr.Append("\\");
                }

                bldr.Append(this.Name);
                return bldr.ToString();
            }
        }
    }
}
