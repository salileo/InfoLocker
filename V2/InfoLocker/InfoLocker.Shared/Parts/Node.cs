using System;
using System.Collections.Generic;
using System.Text;

namespace InfoLocker.Parts
{
    public abstract class Node
    {
        public delegate void DirtyStateUpdatedEventHandler(Node sender, bool isDirty);
        public delegate void ParentUpdatedEventHandler(Node sender, Node oldParent, Node newParent);
        public delegate void ChildrenUpdatedEventHandler(Node sender, Node child);

        #region private members
        private string id;
        private DateTime created;
        private DateTime modified;

        private bool isDirty;
        private Node parent;
        private List<Node> children;
        private bool canHaveChildren;
        #endregion

        #region events
        public event DirtyStateUpdatedEventHandler DirtyStateChanged;
        public event ParentUpdatedEventHandler ParentChanged;
        public event ChildrenUpdatedEventHandler ChildAdded;
        public event ChildrenUpdatedEventHandler ChildRemoved;
        #endregion

        /// <summary>
        /// Constructor for a new node
        /// </summary>
        public Node(bool canHaveChildren)
        {
            this.id = Node.CreateNewID();
            this.created = DateTime.UtcNow;
            this.modified = DateTime.UtcNow;

            this.canHaveChildren = canHaveChildren;

            // for new nodes the dirty should be set by default
            this.isDirty = true;
        }

        /// <summary>
        /// Constructor for a node that is being re-constructed
        /// </summary>
        public Node(string id, DateTime create, DateTime modified, bool canHaveChildren)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            if (create > DateTime.UtcNow)
            {
                throw new ArgumentException(string.Format("Create time ({0}) cannot be greater than current time ({1})", create.ToString(), DateTime.UtcNow.ToString()));
            }

            if (create > modified)
            {
                throw new ArgumentException(string.Format("Create time ({0}) cannot be greater than the modified time ({1})", create.ToString(), DateTime.UtcNow.ToString()));
            }

            this.id = id;
            this.created = create;
            this.modified = modified;

            this.canHaveChildren = canHaveChildren;
        }

        /// <summary>
        /// Gets the ID of the current node
        /// </summary>
        public string ID
        {
            get { return this.id; }
        }

        /// <summary>
        /// Gets the creation time of the current node
        /// </summary>
        public DateTime CreationTime
        {
            get { return this.created; }
        }

        /// <summary>
        /// Gets or sets the last modified time of the current node
        /// </summary>
        public DateTime LastModifiedTime
        {
            get { return this.modified; }
        }

        /// <summary>
        /// Gets whether the current node is dirty
        /// </summary>
        public bool IsDirty
        {
            get { return this.isDirty; }
            protected set
            {
                if (this.isDirty != value)
                {
                    this.isDirty = value;

                    if (this.DirtyStateChanged != null)
                    {
                        this.DirtyStateChanged(this, value);
                    }
                }

                if (this.isDirty)
                {
                    this.modified = DateTime.UtcNow;
                }

                if (this.isDirty)
                {
                    // if this node is set as dirty, then all ancestors should be marked dirty
                    if (this.Parent != null)
                    {
                        this.Parent.IsDirty = value;
                    }
                }
                else
                {
                    // if this node is set as not dirty, then all children should also be marked as not dirty
                    if (this.HasChildren)
                    {
                        foreach (Node child in this.children)
                        {
                            child.IsDirty = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the parent of the current node
        /// </summary>
        public Node Parent
        {
            get { return this.parent; }
        }

        /// <summary>
        /// Gets whether there are any children of this node or not
        /// </summary>
        public bool HasChildren
        {
            get { return (this.children != null) ? (this.children.Count > 0) : false; }
        }

        /// <summary>
        /// Gets the list of children of this node
        /// </summary>
        public List<Node> Children
        {
            get { return this.children; }
        }

        /// <summary>
        /// Adds a node to the end of the children list of the current node
        /// </summary>
        /// <param name="newChild">the new child</param>
        /// <param name="markDirty">whether to mark the current node dirty</param>
        public virtual void AddChild(Node newChild, bool markDirty = true)
        {
            if (newChild == null)
            {
                throw new ArgumentNullException("newChild");
            }

            if (newChild.Parent != null)
            {
                throw new ArgumentException("Child is already attached to a parent");
            }

            Node tmpNode = this;
            while (tmpNode != null)
            {
                if (tmpNode == newChild)
                {
                    throw new ArgumentException("Cannot add an ancestor as a child");
                }

                tmpNode = tmpNode.Parent;
            }

            if (!this.canHaveChildren)
            {
                throw new InvalidOperationException("Current node cannot have children");
            }

            if (this.children == null)
            {
                this.children = new List<Node>();
            }

            if (this.children.Contains(newChild))
            {
                throw new InvalidOperationException("Child is already in the list");
            }

            this.children.Add(newChild);
            newChild.SetParent(this, markDirty);

            if (this.ChildAdded != null)
            {
                this.ChildAdded(this, newChild);
            }

            if (markDirty)
            {
                this.IsDirty = true;
            }
        }

        /// <summary>
        /// Adds a node at the given index within the children list of the current node
        /// </summary>
        /// <param name="newChild">the new child</param>
        /// <param name="index">the position</param>
        /// <param name="markDirty">whether to mark the current node dirty</param>
        public virtual void AddChild(Node newChild, int index, bool markDirty = true)
        {
            if (newChild == null)
            {
                throw new ArgumentNullException("newChild");
            }

            if (newChild.Parent != null)
            {
                throw new ArgumentException("Child is already attached to a parent");
            }

            Node tmpNode = this;
            while (tmpNode != null)
            {
                if (tmpNode == newChild)
                {
                    throw new ArgumentException("Cannot add an ancestor as a child");
                }

                tmpNode = tmpNode.Parent;
            }

            if (!this.canHaveChildren)
            {
                throw new InvalidOperationException("Current node cannot have children");
            }

            if (this.children == null)
            {
                this.children = new List<Node>();
            }

            if (this.children.Contains(newChild))
            {
                throw new InvalidOperationException("Child is already in the list");
            }

            this.children.Insert(index, newChild);
            newChild.SetParent(this, markDirty);

            if (this.ChildAdded != null)
            {
                this.ChildAdded(this, newChild);
            }

            if (markDirty)
            {
                this.IsDirty = true;
            }
        }

        /// <summary>
        /// Removes a node from the children list of the current node
        /// </summary>
        /// <param name="child">the child to remove</param>
        /// <param name="markDirty">whether to mark the current node dirty</param>
        /// <returns>true if the child was removed successfully, false otherwise</returns>
        public virtual bool RemoveChild(Node child, bool markDirty = true)
        {
            if (this.children != null)
            {
                if (this.children.Remove(child))
                {
                    child.SetParent(null, markDirty);

                    if (this.ChildRemoved != null)
                    {
                        this.ChildRemoved(this, child);
                    }

                    if (markDirty)
                    {
                        this.IsDirty = true;
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes a node at a given index within the children list of the current node
        /// </summary>
        /// <param name="index">index of the child to remove</param>
        /// <param name="markDirty">whether to mark the current node dirty</param>
        /// <returns>true if the child was removed successfully, false otherwise</returns>
        public virtual bool RemoveChild(int index, bool markDirty = true)
        {
            if (this.children != null &&
                this.children.Count > index)
            {
                Node child = this.children[index];
                return this.RemoveChild(child, markDirty);
            }

            return false;
        }

        /// <summary>
        /// Removes all the children of the current node
        /// </summary>
        /// <param name="markDirty">whether to mark the current node dirty</param>
        public virtual void RemoveAllChildren(bool markDirty = true)
        {
            while(this.HasChildren)
            {
                Node child = this.children[0];
                this.RemoveChild(child, markDirty);
            }
        }

        /// <summary>
        /// Compares 2 nodes
        /// </summary>
        /// <param name="other">node to compare</param>
        /// <returns>true if equal, false otherwise</returns>
        public virtual bool IsEqual(Node other)
        {
            if (other == null)
            {
                return false;
            }

            if (this.HasChildren && other.HasChildren)
            {
                bool childrenEqual = true;

                int index = 0;
                while (index < this.Children.Count)
                {
                    Node thisChild = this.Children[index];
                    Node otherChild = other.Children[index];

                    if (!thisChild.IsEqual(otherChild))
                    {
                        childrenEqual = false;
                        break;
                    }

                    index++;
                }

                return childrenEqual;
            }
            else if (!this.HasChildren && !other.HasChildren)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Creates a clone of this node
        /// </summary>
        /// <returns>the new node</returns>
        public abstract Node Clone();

        /// <summary>
        /// Return the string representation of the object
        /// </summary>
        /// <returns>the string representation</returns>
        public override string ToString()
        {
            return string.Format(
                "ID={0}, Dirty={1}, Children={2}",
                this.ID,
                this.IsDirty,
                this.HasChildren ? this.Children.Count : 0);
        }

        /// <summary>
        /// Sets the parent of this node
        /// </summary>
        /// <param name="newParent">the new parent</param>
        /// <param name="markDirty">whether to mark the node dirty or not</param>
        private void SetParent(Node newParent, bool markDirty)
        {
            if (this.parent != newParent)
            {
                Node oldParent = this.parent;
                this.parent = newParent;

                if (this.ParentChanged != null)
                {
                    this.ParentChanged(this, oldParent, newParent);
                }

                if (markDirty)
                {
                    this.IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Creates a new ID for a node
        /// </summary>
        /// <returns></returns>
        private static string CreateNewID()
        {
            string guid = Guid.NewGuid().ToString("N");
            return guid;
        }
    }
}
