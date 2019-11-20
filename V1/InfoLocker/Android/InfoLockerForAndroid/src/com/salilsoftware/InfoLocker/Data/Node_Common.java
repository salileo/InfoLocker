package com.salilsoftware.InfoLocker.Data;

import com.salilsoftware.InfoLocker.Utilities.NotifyProvider;
import com.salilsoftware.InfoLocker.Utilities.StringUtils;
import com.salilsoftware.InfoLocker.Utilities.Utils;

public class Node_Common extends NotifyProvider
{
    public enum Type { Folder, Note, Unknown };

    public Type NodeType() { return m_nodeType; }
    public Object Icon() { return m_icon; }

    public String Name() { return m_name; }
    public void Name(String value)
    {
        if (!StringUtils.IsNullOrEmpty(value) && (!StringUtils.Equals(m_name, value)))
        {
            m_name = value;
            NotifyPropertyChanged("Name");
            IsDirty(true);
        }
    }
    
    public Node_Folder Parent() { return m_parent; }
    public void Parent(Node_Folder value)
    {
        if (m_parent != value)
        {
            m_parent = value;
            NotifyPropertyChanged("Parent");

            if (m_parent != null)
                Store(m_parent.Store());

            IsDirty(true);
        }
    }

    public StorageFile Store() { return m_store; }
    public void Store(StorageFile value)
    {
        if (m_store != value)
        {
            m_store = value;
            NotifyPropertyChanged("Store");
            IsDirty(true);

            if (NodeType() == Type.Folder)
            {
                for (Node_Folder folder : ((Node_Folder)this).SubFolders())
                    folder.Store(m_store);
                for (Node_Note note : ((Node_Folder)this).SubNotes())
                    note.Store(m_store);
            }
        }
    }

    public Boolean IsDirty() { return m_dirty; }
    public void IsDirty(Boolean value)
    {
        if (m_dirty != value)
        {
            m_dirty = value;
            NotifyPropertyChanged("IsDirty");

            //if this node is dirty, then its parent is also dirty
            //if this node is not dirty then its children are also not dirty

            if (m_dirty)
            {
                if (Parent() != null)
                    Parent().IsDirty(true);
            }
            else
            {
                if (NodeType() == Type.Folder)
                {
                    for (Node_Folder folder : ((Node_Folder)this).SubFolders())
                        folder.IsDirty(false);
                    for (Node_Note note : ((Node_Folder)this).SubNotes())
                        note.IsDirty(false);
                }
            }
        }
    }

    private Type m_nodeType;
    private Object m_icon;
    private String m_name;
    private Node_Folder m_parent;
    private StorageFile m_store;
    private Boolean m_dirty;

    public Node_Common(Type type)
    {
        m_nodeType = type;

        if (type == Type.Folder)
            m_icon = Utils.GetFolderIcon();
        else if (type == Type.Note)
            m_icon = Utils.GetNoteIcon();

        m_name = null;
        m_parent = null;
        m_store = null;
        m_dirty = false;
    }

    public static int DiffName(Node_Common a, Node_Common b)
    {
        return a.Name().compareTo(b.Name());
    }
}
