using System;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Xml;

namespace InfoLockerForWP7
{
    public class StorageFile : INotifyPropertyChanged
    {
        #region SupportTypes
        public class StorageAttributes
        {
            //public FileAttributes FileAttr { get; private set; }
            public DateTime CreationTime { get; private set; }
            public DateTime LastWriteTime { get; private set; }
            public long FileSize { get; private set; }

            public StorageAttributes(String filename)
            {
                //FileInfo info = new FileInfo(filename);
                //FileAttr = info.Attributes;
                //CreationTime = info.CreationTime;
                //LastWriteTime = info.LastWriteTime;
                //FileSize = info.Length;
            }

            public Boolean IsEqual(StorageAttributes info)
            {
                return (//(FileAttr == info.FileAttr) &&
                        (CreationTime == info.CreationTime) &&
                        (LastWriteTime == info.LastWriteTime) &&
                        (FileSize == info.FileSize));
            }
        }
        #endregion

        #region PublicProperties
        public String DefaultStorageName { get { return m_defaultStorageName; } }
        public String FileName { get { return m_fileName; } }
        
        public StorageAttributes FileInfo
        {
            get { return m_fileInfo; }
            private set
            {
                m_fileInfo = value;
                NotifyPropertyChanged("FileInfo");
            }
        }
        
        public Boolean IsLocked
        {
            get { return m_isLocked; }
            private set
            {
                if (m_isLocked != value)
                {
                    m_isLocked = value;
                    NotifyPropertyChanged("IsLocked");
                }
            }
        }

        public Boolean IsDirty
        {
            get { return m_isDirty; }
            private set
            {
                if (m_isDirty != value)
                {
                    m_isDirty = value;
                    NotifyPropertyChanged("IsDirty");
                }

                if(m_dummyRootNode != null)
                    m_dummyRootNode.IsDirty = m_isDirty;

                if(m_actualRootNode != null)
                    m_actualRootNode.IsDirty = m_isDirty;
            }
        }
        
        public Node_Folder RootNode
        { 
            get
            {
                if (IsLocked)
                    return m_dummyRootNode;
                else
                    return m_actualRootNode;
            }
        }
        #endregion

        #region PrivateMembers
        private String m_defaultStorageName;
        private String m_fileName;
        private String m_password;

        private StorageAttributes m_fileInfo;
        private Node_Folder m_actualRootNode;
        private Node_Folder m_dummyRootNode;
        
        private Boolean m_isLocked;
        private Boolean m_isDirty;
        private Boolean m_isInitialized;
        #endregion

        #region Constructors
        public StorageFile()
        {
            this.PropertyChanged += new PropertyChangedEventHandler(OnSelfPropertyChanged);
            Clear();
        }

        public StorageFile(String filename)
        {
            this.PropertyChanged += new PropertyChangedEventHandler(OnSelfPropertyChanged);
            Clear();

            Initialize(filename, true);
            Lock();
        }
        #endregion

        #region PrivateMethods
        private void Clear()
        {
            if (m_actualRootNode != null)
                m_actualRootNode.PropertyChanged -= OnRootNodePropertyChanged;

            m_actualRootNode = null;
            NotifyPropertyChanged("RootNode");

            m_password = String.Empty;

            FileInfo = null;
            IsDirty = false;

            Lock();
        }

        private void Initialize(String filename, Boolean checkFileExistence)
        {
            if (m_isInitialized)
                throw (new Exception("Storage already initialized"));

            if (checkFileExistence)
            {
                if (!IsolatedStorageFile.GetUserStoreForApplication().FileExists(filename))
                    throw (new Exception("File not found"));
            }

            m_defaultStorageName = Path.GetFileNameWithoutExtension(filename);
            NotifyPropertyChanged("DefaultStorageName");

            m_fileName = filename;
            NotifyPropertyChanged("FileName");

            m_dummyRootNode = new Node_Folder();
            m_dummyRootNode.Name = DefaultStorageName;
            m_dummyRootNode.IsDirty = false;
            NotifyPropertyChanged("RootNode");

            m_isInitialized = true;
        }

        private Boolean CheckIntegrity(String filename, String password)
        {
            Boolean success = true;

            try
            {
                StorageFile tempStorage = new StorageFile(filename);
                tempStorage.UnLock(password);

                if (!m_actualRootNode.IsEqual(tempStorage.RootNode))
                    throw (new Exception());
            }
            catch (Exception)
            {
                success = false;
            }

            return success;
        }

        void OnSelfPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsLocked")
                NotifyPropertyChanged("RootNode");
        }

        void OnRootNodePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsDirty")
            {
                if (m_actualRootNode.IsDirty)
                    IsDirty = true;
            }
        }
        #endregion

        #region FileManagement
        public void Create(String filename, String password)
        {
            if (m_isInitialized)
                throw (new Exception("Storage already initialized"));

            IsolatedStorageFile files = IsolatedStorageFile.GetUserStoreForApplication();
            if (files.FileExists(filename))
                files.DeleteFile(filename);

            Initialize(filename, false);

            m_actualRootNode = new Node_Folder();
            m_actualRootNode.Name = DefaultStorageName;
            m_actualRootNode.Store = this;
            m_actualRootNode.PropertyChanged += new PropertyChangedEventHandler(OnRootNodePropertyChanged);
            NotifyPropertyChanged("RootNode");

            m_password = password;
            IsDirty = true;

            //save the temporary file
            Save();

            //lock the saved file
            Lock();
        }

        public void Open(String password)
        {
            Node_Folder newRoot = null;
            Boolean saveOnOpen = false;

            if (m_actualRootNode != null)
                return;

            IsolatedStorageFile files = IsolatedStorageFile.GetUserStoreForApplication();
            if (password.Length == 0)
            {
                IsolatedStorageFileStream unencrypted_stream = null;
                XmlReader reader = null;

                try
                {
                    try
                    {
                        unencrypted_stream = files.OpenFile(FileName, FileMode.Open, FileAccess.Read);
                        reader = XmlReader.Create(unencrypted_stream);
                        reader.MoveToContent();
                    }
                    catch (Exception exp)
                    {
                        throw (new Exception("Incorrect password", exp));
                    }

                    if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "Folder"))
                    {
                        newRoot = new Node_Folder();
                        String storedPassword = newRoot.DeSerialize(reader);
                        if (!String.IsNullOrEmpty(storedPassword) && (storedPassword != password))
                            throw (new Exception("Incorrect password"));
                    }
                    else
                    {
                        throw (new Exception("Storage is empty"));
                    }
                }
                catch (Exception exp)
                {
                    throw exp;
                }
                finally
                {
                    if (unencrypted_stream != null)
                    {
                        unencrypted_stream.Close();
                        unencrypted_stream = null;
                    }
                }
            }
            else if (password.Length == 8)
            {
                IsolatedStorageFileStream encrypted_stream = files.OpenFile(FileName, FileMode.Open, FileAccess.Read);
                MemoryStream decrypted_stream = null;
                XmlReader reader = null;

                try
                {
                    //first try AES
                    try
                    {
                        encrypted_stream.Seek(0, SeekOrigin.Begin);
                        decrypted_stream = Encryptor.Decrypt(Encryptor.Encryption.AES, encrypted_stream, password);
                        reader = XmlReader.Create(decrypted_stream);
                        reader.MoveToContent();
                    }
                    catch (Exception)
                    {
                        if (decrypted_stream != null)
                        {
                            decrypted_stream.Close();
                            decrypted_stream = null;
                        }
                    }

                    //second try DES
                    if (reader == null)
                    {
                        try
                        {
                            encrypted_stream.Seek(0, SeekOrigin.Begin);
                            decrypted_stream = Encryptor.Decrypt(Encryptor.Encryption.DES, encrypted_stream, password);
                            reader = XmlReader.Create(decrypted_stream);
                            reader.MoveToContent();
                            saveOnOpen = true;
                        }
                        catch (Exception exp)
                        {
                            throw (new Exception("Incorrect password", exp));
                        }
                    }

                    if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "Folder"))
                    {
                        newRoot = new Node_Folder();
                        String storedPassword = newRoot.DeSerialize(reader);
                        if (!String.IsNullOrEmpty(storedPassword) && (storedPassword != password))
                            throw (new Exception("Incorrect password"));
                    }
                    else
                    {
                        throw (new Exception("Storage is empty"));
                    }
                }
                catch (Exception exp)
                {
                    throw exp;
                }
                finally
                {
                    if (encrypted_stream != null)
                    {
                        encrypted_stream.Close();
                        encrypted_stream = null;
                    }

                    if (decrypted_stream != null)
                    {
                        decrypted_stream.Close();
                        decrypted_stream = null;
                    }
                }
            }
            else
            {
                throw (new Exception("Incorrect password"));
            }

            FileInfo = new StorageAttributes(FileName);

            m_actualRootNode = newRoot;
            m_actualRootNode.Store = this;
            m_actualRootNode.PropertyChanged += new PropertyChangedEventHandler(OnRootNodePropertyChanged);
            NotifyPropertyChanged("RootNode");

            m_password = password;
            IsDirty = false;

            if (saveOnOpen)
            {
                IsDirty = true;
                Save();
            }
        }

        public void Close(Boolean saveWhileClosing)
        {
            if (m_actualRootNode == null)
                return;

            if (saveWhileClosing && IsDirty)
            {
                try
                {
                    Save();
                }
                catch (Exception exp)
                {
                    throw (new Exception("Save failed during close", exp));
                }
            }

            Clear();
        }

        public void Save()
        {
            if (!IsDirty)
                return;

            SaveAs(FileName, m_password, false);
            IsDirty = false;

            FileInfo = new StorageAttributes(FileName);
        }

        public void SaveAs(String filename, String password, Boolean checkSync)
        {
            if (checkSync && !IsInSync())
                throw (new Exception("Storage is out of sync"));

            if (m_actualRootNode == null)
                throw (new Exception("Storage is not initialized"));

            IsolatedStorageFile files = IsolatedStorageFile.GetUserStoreForApplication();
            String tempFileName = "_tmp1.stg";

            if (files.FileExists(tempFileName))
                files.DeleteFile(tempFileName);

            if (password.Length == 0)
            {
                IsolatedStorageFileStream unencrypted_stream = null;
                XmlWriter writer = null;

                try
                {
                    unencrypted_stream = files.OpenFile(tempFileName, FileMode.Create, FileAccess.Write);
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Encoding = System.Text.Encoding.UTF8;
                    writer = XmlWriter.Create(unencrypted_stream, settings);
                    writer.WriteStartDocument();
                    m_actualRootNode.Serialize(writer, password);
                    writer.WriteEndDocument();
                    writer.Flush();
                }
                catch (Exception exp)
                {
                    throw exp;
                }
                finally
                {
                    if (unencrypted_stream != null)
                    {
                        unencrypted_stream.Close();
                        unencrypted_stream = null;
                    }
                }
            }
            else if (password.Length == 8)
            {
                MemoryStream decrypted_stream = new MemoryStream();
                IsolatedStorageFileStream encrypted_stream = null;
                XmlWriter writer = null;

                try
                {
                    encrypted_stream = files.OpenFile(tempFileName, FileMode.Create, FileAccess.Write);
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Encoding = System.Text.Encoding.UTF8;
                    writer = XmlWriter.Create(decrypted_stream, settings);
                    writer.WriteStartDocument();
                    m_actualRootNode.Serialize(writer, password);
                    writer.WriteEndDocument();
                    writer.Flush();

                    Encryptor.Encrypt(Encryptor.Encryption.AES, decrypted_stream, encrypted_stream, password);
                }
                catch (Exception exp)
                {
                    throw exp;
                }
                finally
                {
                    if (decrypted_stream != null)
                    {
                        decrypted_stream.Close();
                        decrypted_stream = null;
                    }

                    if (encrypted_stream != null)
                    {
                        encrypted_stream.Close();
                        encrypted_stream = null;
                    }
                }
            }
            else
            {
                throw (new Exception("Password length incorrect"));
            }

            if (!CheckIntegrity(tempFileName, password))
                throw (new Exception("Integrity check failed"));

            {
                IsolatedStorageFileStream input = files.OpenFile(tempFileName, FileMode.Open, FileAccess.Read);
                IsolatedStorageFileStream output = files.OpenFile(filename, FileMode.Create, FileAccess.Write);

                int length = 1024;
                int bytes_read = 0;
                Byte[] bytes = new Byte[length];
                while ((bytes_read = input.Read(bytes, 0, length)) > 0)
                    output.Write(bytes, 0, bytes_read);

                input.Close();
                output.Close();
            }

            try
            {
                files.DeleteFile(tempFileName);
            }
            catch (Exception)
            {
                //no need to show any error as it is just a temp file.
            }
        }

        public Boolean IsInSync()
        {
            try
            {
                Boolean inSync = true;
                
                if(FileInfo != null)
                    inSync = FileInfo.IsEqual(new StorageAttributes(FileName));

                return inSync;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Boolean Sync()
        {
            if (FileInfo == null)
                return false;

            if (!IsInSync())
            {
                if (!IsDirty)
                {
                    Close(false);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region LockManagement
        public void Lock()
        {
            IsLocked = true;
        }

        public void UnLock(String password)
        {
            if (m_actualRootNode == null)
            {
                Open(password);
            }
            else
            {
                if ((password.Length != 0) && (password.Length != 8))
                    throw (new Exception("Incorrect password"));
                else if (m_password != password)
                    throw (new Exception("Incorrect password"));
            }

            IsLocked = false;
        }

        public Boolean TryUnLock(String password)
        {
            Boolean success = true;

            try
            {
                if (m_actualRootNode == null)
                {
                    Open(password);
                }
                else
                {
                    if ((password.Length != 0) && (password.Length != 8))
                        throw (new Exception("Incorrect password"));
                    else if (m_password != password)
                        throw (new Exception("Incorrect password."));
                }
            }
            catch (Exception)
            {
                success = false;
            }

            return success;
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        #endregion
    }
}
