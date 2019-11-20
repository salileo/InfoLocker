using System;
using System.Collections.Generic;
using System.Text;

namespace InfoLocker.Parts
{
    public class Folder : Node
    {
        public delegate void PropertyUpdatedEventHandler(Folder sender, string oldValue, string newValue);

        #region private members
        private string label;
        #endregion

        #region events
        public event PropertyUpdatedEventHandler LabelChanged;
        #endregion 

        /// <summary>
        /// Constructor for a new folder
        /// </summary>
        /// <param name="label">name of the folder</param>
        public Folder(string label)
            : base(true)
        {
            if (string.IsNullOrEmpty(label))
            {
                throw new ArgumentNullException("label");
            }

            this.label = label;
        }

        /// <summary>
        /// Constructor for a folder that is being re-constructed
        /// </summary>
        /// <param name="id">id of the folder</param>
        /// <param name="created">time of creation</param>
        /// <param name="modified">time of last modification</param>
        /// <param name="label">name of the folder</param>
        public Folder(string id, DateTime created, DateTime modified, string label)
            : base(id, created, modified, true)
        {
            if (string.IsNullOrEmpty(label))
            {
                throw new ArgumentNullException("label");
            }

            this.label = label;
        }

        /// <summary>
        /// Get or set the label for the folder
        /// </summary>
        public string Label
        {
            get { return this.label; }
            set
            {
                if (this.label != value)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        throw new ArgumentNullException("value");
                    }

                    if (value.Contains("\n"))
                    {
                        throw new ArgumentException(string.Format("Label cannot have newline - {0}", value));
                    }

                    string oldValue = this.label;
                    this.label = value;

                    if (this.LabelChanged != null)
                    {
                        this.LabelChanged(this, oldValue, value);
                    }

                    this.IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Adds a card or sub-folder to the folder
        /// </summary>
        /// <param name="newChild">the new child</param>
        /// <param name="markDirty">whether to mark the current node dirty</param>
        public override void AddChild(Node newChild, bool markDirty = true)
        {
            if (newChild is Card ||
                newChild is Folder)
            {
                base.AddChild(newChild, markDirty);
            }
            else
            {
                throw new ArgumentException(string.Format("Cannot add node of type {0} to a folder", newChild.GetType().ToString()));
            }
        }

        /// <summary>
        /// Adds a card or sub-folder to the folder at the given position
        /// </summary>
        /// <param name="newChild">the new child</param>
        /// <param name="index">the position</param>
        /// <param name="markDirty">whether to mark the current node dirty</param>
        public override void AddChild(Node newChild, int index, bool markDirty = true)
        {
            if (newChild is Card ||
                newChild is Folder)
            {
                base.AddChild(newChild, index, markDirty);
            }
            else
            {
                throw new ArgumentException(string.Format("Cannot add node of type {0} to a folder", newChild.GetType().ToString()));
            }
        }

        /// <summary>
        /// Compares 2 folders
        /// </summary>
        /// <param name="other">folder to compare</param>
        /// <returns>true if equal, false otherwise</returns>
        public override bool IsEqual(Node other)
        {
            Folder otherFolder = other as Folder;
            if (otherFolder == null)
            {
                return false;
            }

            if (this.Label == otherFolder.Label)
            {
                return base.IsEqual(otherFolder);
            }

            return false;
        }

        /// <summary>
        /// Creates a clone of this folder
        /// </summary>
        /// <returns>the new folder</returns>
        public override Node Clone()
        {
            Folder newFolder = new Folder(this.ID, this.CreationTime, this.LastModifiedTime, this.Label);

            if (this.HasChildren)
            {
                foreach (Node child in this.Children)
                {
                    Node newNode = child.Clone();
                    newFolder.AddChild(newNode, false);
                }
            }

            return newFolder;
        }

        /// <summary>
        /// Return the string representation of the object
        /// </summary>
        /// <returns>the string representation</returns>
        public override string ToString()
        {
            string nodeStr = base.ToString();
            return string.Format(
                "Label={0}, {1}",
                this.Label,
                nodeStr);
        }
    }
}
