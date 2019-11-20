package com.salilsoftware.InfoLocker.Data;

import java.util.Collections;
import java.util.Comparator;
import java.util.LinkedList;

import org.w3c.dom.NamedNodeMap;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;
import org.xmlpull.v1.XmlSerializer;

import com.salilsoftware.InfoLocker.Utilities.NotifyConsumer;
import com.salilsoftware.InfoLocker.Utilities.NotifyProvider;
import com.salilsoftware.InfoLocker.Utilities.StringUtils;

public class Node_Folder extends Node_Common implements NotifyConsumer
{
    public LinkedList<Node_Common> SubNodes()
    {
    	LinkedList<Node_Common> nodes = new LinkedList<Node_Common>();
        
        for (Node_Common node : SubFolders())
            nodes.add(node);

        for (Node_Common node : SubNotes())
            nodes.add(node);

        return nodes;
    }

    public LinkedList<Node_Folder> SubFolders() { return m_subFolders; }
    public LinkedList<Node_Note> SubNotes() { return m_subNotes; }

    private LinkedList<Node_Folder> m_subFolders;
    private LinkedList<Node_Note> m_subNotes;
    
    public Node_Folder()
    {
    	super(Node_Common.Type.Folder);
    	m_subFolders = new LinkedList<Node_Folder>();
    	m_subNotes = new LinkedList<Node_Note>();

    	this.Listners.add(this);
    }

    public Boolean AddNode(Node_Common node)
    {
        if (node == null)
            return false;

        if (SubNodes().contains(node))
        {
            //node already a child of this folder
            node.Parent(this);
            return true;
        }

        if (node.Parent() == this)
        {
            //node is not in this folder's list but for some reason the parent property points to this node.
            //reset the parent property
            node.Parent(null);
        }

        Boolean success = true;
        if (node.NodeType() == Type.Folder)
        {
            SubFolders().add((Node_Folder)node);
            NotifyPropertyChanged("SubFolders");
        }
        else if (node.NodeType() == Type.Note)
        {
            SubNotes().add((Node_Note)node);
            NotifyPropertyChanged("SubNotes");
        }
        else
        {
            success = false;
        }

        if (success)
        {
            if (node.Parent() != null)
                node.Parent().RemoveNode(node);

            node.Parent(this);
            IsDirty(true);
        }

        return success;
    }

    public Boolean RemoveNode(Node_Common node)
    {
        if (node == null)
            return false;

        Boolean success = true;
        if (node.NodeType() == Type.Folder)
        {
            success = SubFolders().remove((Node_Folder)node);
            NotifyPropertyChanged("SubFolders");
        }
        else if (node.NodeType() == Type.Note)
        {
            success = SubNotes().remove((Node_Note)node);
            NotifyPropertyChanged("SubNotes");
        }
        else
        {
            success = false;
        }

        if (success)
        {
            node.Parent(null);
            IsDirty(true);
        }

        return success;
    }

    public Boolean SortNodes(int level)
    {
        try
        {
        	Comparator<Node_Common> nodeCompare = new Comparator<Node_Common>()
        	{
				public int compare(Node_Common object1, Node_Common object2)
				{
					return Node_Common.DiffName(object1, object2);
				}
			};

			Collections.sort(m_subFolders, nodeCompare);
            NotifyPropertyChanged("SubFolders");

			Collections.sort(m_subNotes, nodeCompare);
            NotifyPropertyChanged("SubNotes");

            this.IsDirty(true);
        }
        catch (Exception exp)
        {
            return false;
        }

        if (level == 0)
            return true;

        Boolean success = true;
        for (Node_Folder folder : SubFolders())
        {
            Boolean ret = folder.SortNodes(Math.max(level - 1, -1));
            success = success && ret;
        }

        return success;
    }

    public void Serialize(XmlSerializer writer, String password) throws Exception
    {
    	writer.startTag(null, "Folder");
    	writer.attribute(null, "Name", Name());

    	//used only by the root node
        if(!StringUtils.IsNullOrEmpty(password))
        	writer.attribute(null, "Password", password);

        for (Node_Folder folder : SubFolders())
        {
            try
            {
                folder.Serialize(writer, null);
            }
            catch (Exception exp)
            {
                throw (new Exception("Could not write folder '" + folder.Name() + "'", exp));
            }
        }

        for (Node_Note note : SubNotes())
        {
            try
            {
                note.Serialize(writer);
            }
            catch (Exception exp)
            {
                throw (new Exception("Could not write note '" + note.Name() + "'", exp));
            }
        }
        
        writer.endTag(null, "Folder");
    }

    public String DeSerialize(Node reader) throws Exception
    {
    	NamedNodeMap attr = reader.getAttributes();

    	Node nameNode = attr.getNamedItem("Name");
    	if (nameNode == null)
    		throw (new Exception("Error reading name of note"));

    	String name = nameNode.getNodeValue();
    	if (name == null)
		    throw (new Exception("Error reading name of note"));

	    Name(name);
    	
        //used only by the root node
	    Node passwordNode = attr.getNamedItem("Password"); 
        String password = null;
        if (passwordNode != null)
        	password = passwordNode.getNodeValue();

        NodeList children = reader.getChildNodes();
        int index = 0;
        while (index < children.getLength())
        {
        	Node child = children.item(index);
        	if (StringUtils.Equals(child.getNodeName(), "Folder"))
            {
                Node_Folder newfolder = new Node_Folder();

                try
                {
                    newfolder.DeSerialize(child);
                }
                catch (Exception exp)
                {
                    throw (new Exception("Could not read a folder of '" + Name() + "'", exp));
                }

                AddNode(newfolder);
            }
            else if (StringUtils.Equals(child.getNodeName(), "Note"))
            {
                Node_Note newnote = new Node_Note();

                try
                {
                    newnote.DeSerialize(child);
                }
                catch (Exception exp)
                {
                    throw (new Exception("Could not read a note of '" + Name() + "'", exp));
                }

                AddNode(newnote);
            }
        	
        	index++;
        }

        IsDirty(false);
        return password;
    }

    public Boolean IsEqual(Node_Folder other)
    {
        if (!StringUtils.Equals(this.Name(), other.Name()) ||
            (this.SubFolders().size() != other.SubFolders().size()) ||
            (this.SubNotes().size() != other.SubNotes().size()))
        {
            return false;
        }

        int index = 0;
        while (index < this.SubFolders().size())
        {
            if (!this.SubFolders().get(index).IsEqual(other.SubFolders().get(index)))
                return false;

            index++;
        }

        index = 0;
        while (index < this.SubNotes().size())
        {
            if (!this.SubNotes().get(index).IsEqual(other.SubNotes().get(index)))
                return false;

            index++;
        }

        return true;
    }

	public void HandlePropertyChange(NotifyProvider source, String propName)
	{
		if (StringUtils.Equals(propName, "SubFolders") ||
			StringUtils.Equals(propName, "SubNotes"))
		{
			NotifyPropertyChanged("SubNodes");
		}
	}
}
