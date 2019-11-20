package com.salilsoftware.InfoLocker.Data;

import java.io.BufferedOutputStream;
import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.StringWriter;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;

import org.w3c.dom.Document;
import org.xmlpull.v1.XmlPullParserFactory;
import org.xmlpull.v1.XmlSerializer;

import com.salilsoftware.InfoLocker.Utilities.NotifyConsumer;
import com.salilsoftware.InfoLocker.Utilities.NotifyProvider;
import com.salilsoftware.InfoLocker.Utilities.StringUtils;

public class StorageFile extends NotifyProvider implements NotifyConsumer
{
    public class StorageAttributes
    {
        public long LastWriteTime() { return m_lastWriteTime; }
        public long FileSize() { return m_fileSize; }

        private long m_lastWriteTime;
        private long m_fileSize;
        
        public StorageAttributes(String filename)
        {
        	File file = new File(filename);
        	m_lastWriteTime = file.lastModified();
        	m_fileSize = file.length();
        }

        public Boolean IsEqual(StorageAttributes info)
        {
            return ((LastWriteTime() == info.LastWriteTime()) &&
                    (FileSize() == info.FileSize()));
        }
    }

    public String DefaultStorageName() { return m_defaultStorageName; }
    public String FileName() { return m_fileName; }
    
    public StorageAttributes FileInfo() { return m_fileInfo; }
    private void FileInfo(StorageAttributes value)
    {
        m_fileInfo = value;
        NotifyPropertyChanged("FileInfo");
    }
    
    public Boolean IsLocked() { return m_isLocked; }
    private void IsLocked(Boolean value)
    {
        if (m_isLocked != value)
        {
            m_isLocked = value;
            NotifyPropertyChanged("IsLocked");
        }
    }

    public Boolean IsDirty() { return m_isDirty; }
    private void IsDirty(Boolean value)
    {
        if (m_isDirty != value)
        {
            m_isDirty = value;
            NotifyPropertyChanged("IsDirty");
        }

        if(m_dummyRootNode != null)
            m_dummyRootNode.IsDirty(m_isDirty);

        if(m_actualRootNode != null)
            m_actualRootNode.IsDirty(m_isDirty);
    }
    
    public Node_Folder RootNode()
    { 
        if (IsLocked())
            return m_dummyRootNode;
        else
            return m_actualRootNode;
    }

    private String m_defaultStorageName;
    private String m_fileName;
    private String m_password;

    private StorageAttributes m_fileInfo;
    private Node_Folder m_actualRootNode;
    private Node_Folder m_dummyRootNode;
    
    private Boolean m_isLocked;
    private Boolean m_isDirty;
    private Boolean m_isInitialized;

    public StorageFile()
    {
    	m_isInitialized = false;
    	this.Listners.add(this);
        Clear();
    }

    public StorageFile(String filename) throws Exception
    {
    	m_isInitialized = false;
    	this.Listners.add(this);
        Clear();

        Initialize(filename, true);
        Lock();
    }

    private void Clear()
    {
        if (m_actualRootNode != null)
            m_actualRootNode.Listners.add(this);

        m_actualRootNode = null;
        NotifyPropertyChanged("RootNode");

        m_password = null;

        FileInfo(null);
        IsDirty(false);

        Lock();
    }

    private void Initialize(String filename, Boolean checkFileExistence) throws Exception
    {
        if (m_isInitialized)
            throw (new Exception("Storage already initialized"));

    	File file = new File(filename);
        if (checkFileExistence)
        {
        	if (!file.exists())
                throw (new Exception("File not found"));
        }

        m_defaultStorageName = file.getName().replaceAll(".stg", "");
        NotifyPropertyChanged("DefaultStorageName");

        m_fileName = filename;
        NotifyPropertyChanged("FileName");

        m_dummyRootNode = new Node_Folder();
        m_dummyRootNode.Name(DefaultStorageName());
        m_dummyRootNode.IsDirty(false);
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

            if (!m_actualRootNode.IsEqual(tempStorage.RootNode()))
                throw (new Exception());
        }
        catch (Exception exp)
        {
            success = false;
        }

        return success;
    }

    public void Create(String filename, String password) throws Exception
    {
        if (m_isInitialized)
            throw (new Exception("Storage already initialized"));

    	File file = new File(filename);
    	if (file.exists())
    		file.delete();

        Initialize(filename, false);

        m_actualRootNode = new Node_Folder();
        m_actualRootNode.Name(DefaultStorageName());
        m_actualRootNode.Store(this);
        m_actualRootNode.Listners.add(this);
        NotifyPropertyChanged("RootNode");

        m_password = password;
        IsDirty(true);

        //save the temporary file
        Save();

        //lock the saved file
        Lock();
    }

    public void Open(String password) throws Exception
    {
        Node_Folder newRoot = null;
        Boolean saveOnOpen = false;

        if (m_actualRootNode != null)
            return;

        if (StringUtils.IsNullOrEmpty(password))
        {
            try
            {
            	DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            	DocumentBuilder builder = factory.newDocumentBuilder();
            	Document doc = null;
            	
            	try
            	{
            		doc = builder.parse(new File(FileName()));
            	}
            	catch (Exception exp)
            	{
            		throw (new Exception("Incorrect password"));
            	}
            	
            	if (StringUtils.Equals(doc.getDocumentElement().getNodeName(), "Folder"))
            	{
            		newRoot = new Node_Folder();
                    String storedPassword = newRoot.DeSerialize(doc.getDocumentElement());
                    if (!StringUtils.IsNullOrEmpty(storedPassword) && (!StringUtils.Equals(storedPassword, password)))
                        throw (new Exception("Incorrect password"));
            	}
            }
            catch (Exception exp)
            {
            	throw exp;
            }
        }
        else if (password.length() == 8)
        {
            FileInputStream encrypted_stream = null;
            ByteArrayOutputStream decrypted_outputstream = null;
            ByteArrayInputStream decrypted_inputstream = null;

            try
            {
            	encrypted_stream = new FileInputStream(FileName());
            	DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            	DocumentBuilder builder = factory.newDocumentBuilder();
            	Document doc = null;

            	try
                {
                	decrypted_outputstream = Encryptor.Decrypt(encrypted_stream, password);
                    decrypted_inputstream = new ByteArrayInputStream(decrypted_outputstream.toByteArray());
            		doc = builder.parse(decrypted_inputstream);
                }
                catch (Exception exp)
                {
            		throw (new Exception("Incorrect password"));
                }
            	
            	if (StringUtils.Equals(doc.getDocumentElement().getNodeName(), "Folder"))
            	{
            		newRoot = new Node_Folder();
                    String storedPassword = newRoot.DeSerialize(doc.getDocumentElement());
                    if (!StringUtils.IsNullOrEmpty(storedPassword) && (!StringUtils.Equals(storedPassword, password)))
                        throw (new Exception("Incorrect password"));
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
                    encrypted_stream.close();
                    encrypted_stream = null;
                }

                if (decrypted_inputstream != null)
                {
                	decrypted_inputstream.close();
                	decrypted_inputstream = null;
                }

                if (decrypted_outputstream != null)
                {
                	decrypted_outputstream.close();
                	decrypted_outputstream = null;
                }
            }
        }
        else
        {
            throw (new Exception("Incorrect password"));
        }

        FileInfo(new StorageAttributes(FileName()));

        m_actualRootNode = newRoot;
        m_actualRootNode.Store(this);
        m_actualRootNode.Listners.add(this);
        NotifyPropertyChanged("RootNode");

        m_password = password;
        IsDirty(false);

        if (saveOnOpen)
        {
            IsDirty(true);
            Save();
        }
    }

    public void Close(Boolean saveWhileClosing) throws Exception
    {
        if (m_actualRootNode == null)
            return;

        if (saveWhileClosing && IsDirty())
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

    public void Save() throws Exception
    {
        if (!IsDirty())
            return;

        SaveAs(FileName(), m_password, false);
        IsDirty(false);

        FileInfo(new StorageAttributes(FileName()));
    }

    public void SaveAs(String filename, String password, Boolean checkSync) throws Exception
    {
        if (checkSync && !IsInSync())
            throw (new Exception("Storage is out of sync"));

        if (m_actualRootNode == null)
            throw (new Exception("Storage is not initialized"));

        File tmpFile = File.createTempFile("tmp", "stg");
        String tempFileName = tmpFile.getPath();

        String xmlData = null;
        
        try
        {
            XmlPullParserFactory factory = XmlPullParserFactory.newInstance();
            XmlSerializer serializer = factory.newSerializer();
            StringWriter writer = new StringWriter();
            serializer.setOutput(writer);
            serializer.startDocument("utf-8", false);
        	
            m_actualRootNode.Serialize(serializer, password);
            serializer.endDocument();
            xmlData = writer.toString();
        }
        catch (Exception exp)
        {
            throw exp;
        }

        if (StringUtils.IsNullOrEmpty(password))
        {
        	BufferedOutputStream unencrypted_stream = null; 

            try
            {
            	unencrypted_stream = new BufferedOutputStream(new FileOutputStream(tempFileName));
            	unencrypted_stream.write(xmlData.getBytes());
            }
            catch (Exception exp)
            {
                throw exp;
            }
            finally
            {
                if (unencrypted_stream != null)
                {
                    unencrypted_stream.flush();
                    unencrypted_stream.close();
                    unencrypted_stream = null;
                }
            }
        }
        else if (password.length() == 8)
        {
            ByteArrayInputStream decrypted_stream = null;
            BufferedOutputStream encrypted_stream = null;

            try
            {
                encrypted_stream = new BufferedOutputStream(new FileOutputStream(tempFileName));
                decrypted_stream = new ByteArrayInputStream(xmlData.getBytes());
                Encryptor.Encrypt(decrypted_stream, encrypted_stream, password);
            }
            catch (Exception exp)
            {
                throw exp;
            }
            finally
            {
                if (decrypted_stream != null)
                {
                    decrypted_stream.close();
                    decrypted_stream = null;
                }

                if (encrypted_stream != null)
                {
                    encrypted_stream.close();
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

        File mainFile = new File(filename);
        tmpFile.renameTo(mainFile);
    }

    public Boolean IsInSync()
    {
        try
        {
            Boolean inSync = true;
            
            if(FileInfo() != null)
                inSync = FileInfo().IsEqual(new StorageAttributes(FileName()));

            return inSync;
        }
        catch (Exception exp)
        {
            return false;
        }
    }

    public Boolean Sync() throws Exception
    {
        if (FileInfo() == null)
            return false;

        if (!IsInSync())
        {
            if (!IsDirty())
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

    public void Lock()
    {
        IsLocked(true);
    }

    public void UnLock(String password) throws Exception
    {
        if (m_actualRootNode == null)
        {
            Open(password);
        }
        else
        {
            if ((password.length() != 0) && (password.length() != 8))
                throw (new Exception("Incorrect password"));
            else if (!StringUtils.Equals(m_password, password))
                throw (new Exception("Incorrect password"));
        }

        IsLocked(false);
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
                if ((password.length() != 0) && (password.length() != 8))
                    throw (new Exception("Incorrect password"));
                else if (!StringUtils.Equals(m_password, password))
                    throw (new Exception("Incorrect password."));
            }
        }
        catch (Exception exp)
        {
            success = false;
        }

        return success;
    }
	public void HandlePropertyChange(NotifyProvider source, String propName)
	{
		if(source instanceof Node_Common)
		{
	        if (StringUtils.Equals(propName, "IsDirty"))
	        {
	            if (m_actualRootNode.IsDirty())
	                IsDirty(true);
	        }
		}
		else if(source instanceof StorageFile)
		{
			if(StringUtils.Equals(propName, "IsLocked"))
				NotifyPropertyChanged("RootNode");
		}
	}
}
