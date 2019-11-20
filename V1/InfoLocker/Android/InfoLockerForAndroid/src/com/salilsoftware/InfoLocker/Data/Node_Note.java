package com.salilsoftware.InfoLocker.Data;

import org.w3c.dom.NamedNodeMap;
import org.w3c.dom.Node;
import org.xmlpull.v1.XmlSerializer;

import com.salilsoftware.InfoLocker.Utilities.StringUtils;

public class Node_Note extends Node_Common
{
    public String Content() { return m_content; }
    public void Content(String value)
    {
        if (!StringUtils.Equals(m_content, value))
        {
            m_content = value;
            NotifyPropertyChanged("Content");
            IsDirty(true);
        }
    }

    private String m_content;

    public Node_Note()
    {
    	super(Node_Common.Type.Note);
        m_content = null;
    }

    public void Serialize(XmlSerializer writer) throws Exception
    {
    	writer.startTag(null, "Note");
    	writer.attribute(null, "Name", Name());
    	writer.attribute(null, "Content", Content());
    	writer.endTag(null, "Note");
    }

    public void DeSerialize(Node reader) throws Exception
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
	    Node contentNode = attr.getNamedItem("Content"); 
        if (contentNode != null)
        {
        	String content = contentNode.getNodeValue();
            if (content != null)
                Content(content);
        }
    	
        IsDirty(false);
    }

    public Boolean IsEqual(Node_Note other)
    {
        if (!StringUtils.Equals(this.Name(), other.Name()) ||
            !StringUtils.Equals(this.Content(), other.Content()))
        {
            return false;
        }

        return true;
    }
}
