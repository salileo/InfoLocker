using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace InfoLocker
{
    public class FileManager : IFileManager
    {
        private string m_filename;
        private StorageAttributes m_attr;
        public FileManager(string filename)
        {
            m_filename = filename;
            m_attr = new StorageAttributes(filename);
        }

        public string FileName
        {
            get { return m_filename; }
        }

        public IStorageAttributes FileInfo
        {
            get { return m_attr; }
        }

        public bool Exists()
        {
            return File.Exists(m_filename);
        }

        public string FileNameWithoutExtension
        {
            get { return Path.GetFileNameWithoutExtension(FileName); }
        }

        public MemoryStream Read()
        {
            byte[] content = File.ReadAllBytes(m_filename);
            MemoryStream stream = new MemoryStream(content, false);
            return stream;
        }

        private string tempFileName;
        public void Write(MemoryStream input)
        {
            FileStream stream = new FileStream(tempFileName, FileMode.Create, FileAccess.Write);
            input.WriteTo(stream);
            stream.Close();
        }

        public string LockWrite()
        {
            tempFileName = Path.GetTempFileName();
            return tempFileName;
        }

        public void UnlockWrite()
        {
            File.Copy(tempFileName, m_filename, true);

            try
            {
                File.Delete(tempFileName);
            }
            catch (Exception)
            {
                //no need to show any error as it is just a temp file.
            }
        }

        public void Delete()
        {
            File.Delete(m_filename);
        }
    }
}
