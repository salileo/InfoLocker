using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Windows;
using System.Security;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.ComponentModel;

namespace InfoLocker
{
    public class StorageFile : INotifyPropertyChanged
    {
        #region PublicProperties
        public string DefaultStorageName { get { return m_defaultStorageName; } }
        public string FileName { get { return m_fileName; } }
        
        public StorageAttributes FileInfo
        {
            get { return m_fileInfo; }
            private set
            {
                m_fileInfo = value;
                NotifyPropertyChanged("FileInfo");
            }
        }
        
        public bool IsLocked
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

        public bool IsDirty
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
        private string m_defaultStorageName;
        private string m_fileName;
        private string m_password;

        private StorageAttributes m_fileInfo;
        private Node_Folder m_actualRootNode;
        private Node_Folder m_dummyRootNode;
        
        private bool m_isLocked;
        private bool m_isDirty;
        private bool m_isInitialized;
        #endregion

        #region Constructors
        public StorageFile()
        {
            this.PropertyChanged += new PropertyChangedEventHandler(OnSelfPropertyChanged);
            Clear();
        }

        public StorageFile(string filename)
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

            m_password = string.Empty;

            FileInfo = null;
            IsDirty = false;

            Lock();
        }

        private void Initialize(string filename, bool checkFileExistence)
        {
            if (m_isInitialized)
                throw (new Exception("Storage already initialized"));

            if (checkFileExistence)
            {
                if (!File.Exists(filename))
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

        private bool CheckIntegrity(string filename, string password)
        {
            bool success = true;

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
        public void Create(string filename, string password)
        {
            if (m_isInitialized)
                throw (new Exception("Storage already initialized"));

            if (File.Exists(filename))
                File.Delete(filename);

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

        private Node_Folder ParseDocument(XmlReader reader, string password)
        {
            Node_Folder newRoot = null;

            if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "Store"))
            {
                string storedPassword = reader.GetAttribute("Password");
                if (!string.IsNullOrEmpty(storedPassword) && (storedPassword != password))
                    throw (new Exception("Incorrect password"));

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == "Folder")
                        {
                            newRoot = new Node_Folder();
                            newRoot.DeSerialize(reader);
                            break;
                        }
                        else
                        {
                            throw (new Exception("Unknown child '" + reader.Name + "' encountered"));
                        }
                    }
                }

                if (newRoot == null)
                    throw (new Exception("Storage is empty"));
            }
            else if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "Folder"))
            {
                newRoot = new Node_Folder();
                newRoot.DeSerialize(reader);
            }
            else
            {
                throw (new Exception("Storage is empty"));
            }

            return newRoot;
        }

        public void Open(string password)
        {
            Node_Folder newRoot = null;
            bool saveOnOpen = false;

            if (m_actualRootNode != null)
                return;

            if (password.Length == 0)
            {
                FileStream unencrypted_stream = null;
                XmlTextReader reader = null;

                try
                {
                    try
                    {
                        unencrypted_stream = new FileStream(FileName, FileMode.Open, FileAccess.Read);
                        reader = new XmlTextReader(unencrypted_stream);
                        reader.WhitespaceHandling = WhitespaceHandling.None;
                        reader.MoveToContent();
                    }
                    catch (Exception exp)
                    {
                        throw (new Exception("Incorrect password", exp));
                    }

                    if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "Folder"))
                    {
                        newRoot = new Node_Folder();
                        string storedPassword = newRoot.DeSerialize(reader);
                        if (!string.IsNullOrEmpty(storedPassword) && (storedPassword != password))
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
                FileStream encrypted_stream = new FileStream(FileName, FileMode.Open, FileAccess.Read);
                MemoryStream decrypted_stream = null;
                XmlTextReader reader = null;

                try
                {
                    //first try AES
                    try
                    {
                        encrypted_stream.Seek(0, SeekOrigin.Begin);
                        decrypted_stream = Encryptor.Decrypt(Encryptor.Encryption.AES, encrypted_stream, password);
                        reader = new XmlTextReader(decrypted_stream);
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
                            reader = new XmlTextReader(decrypted_stream);
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
                        string storedPassword = newRoot.DeSerialize(reader);
                        if (!string.IsNullOrEmpty(storedPassword) && (storedPassword != password))
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

        public void Close(bool saveWhileClosing)
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

        public void SaveAs(string filename, string password, bool checkSync)
        {
            if (checkSync && !IsInSync())
                throw (new Exception("Storage is out of sync"));

            if (m_actualRootNode == null)
                throw (new Exception("Storage is not initialized"));

            string tempFileName = Path.GetTempFileName();

            if (password.Length == 0)
            {
                FileStream unencrypted_stream = null;
                XmlTextWriter writer = null;

                try
                {
                    unencrypted_stream = new FileStream(tempFileName, FileMode.Create, FileAccess.Write);
                    writer = new XmlTextWriter(unencrypted_stream, System.Text.Encoding.UTF8);
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
                FileStream encrypted_stream = null;
                XmlTextWriter writer = null;

                try
                {
                    encrypted_stream = new FileStream(tempFileName, FileMode.Create, FileAccess.Write);
                    writer = new XmlTextWriter(decrypted_stream, System.Text.Encoding.UTF8);
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

            File.Copy(tempFileName, filename, true);

            try
            {
                File.Delete(tempFileName);
            }
            catch (Exception)
            {
                //no need to show any error as it is just a temp file.
            }
        }

        public bool IsInSync()
        {
            try
            {
                bool inSync = true;
                
                if(FileInfo != null)
                    inSync = FileInfo.IsEqual(new StorageAttributes(FileName));

                return inSync;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Sync()
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

        public void UnLock(string password)
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

        public bool TryUnLock(string password)
        {
            bool success = true;

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
        protected void NotifyPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        #endregion
    }
}
