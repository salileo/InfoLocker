using System;
using System.Collections.Generic;
using System.Text;

namespace InfoLocker.Parts
{
    public class Cabinet : Node
    {
        public delegate void PropertyUpdatedEventHandler(Cabinet sender, string oldValue, string newValue);

        #region private members
        private string label;
        private string password;
        #endregion

        #region events
        public event PropertyUpdatedEventHandler LabelChanged;
        public event PropertyUpdatedEventHandler PasswordChanged;
        #endregion 

        /// <summary>
        /// Constructor for a new cabinet
        /// </summary>
        /// <param name="label">name of the cabinet</param>
        public Cabinet(string label)
            : base(true)
        {
            if (string.IsNullOrEmpty(label))
            {
                throw new ArgumentNullException("label");
            }

            this.label = label;
        }

        /// <summary>
        /// Constructor for a cabinet that is being re-constructed
        /// </summary>
        /// <param name="id">id of the cabinet</param>
        /// <param name="created">time of creation</param>
        /// <param name="modified">time of last modification</param>
        /// <param name="label">name of the cabinet</param>
        /// <param name="password">password of the cabinet</param>
        public Cabinet(string id, DateTime created, DateTime modified, string label, string password)
            : base(id, created, modified, true)
        {
            if (string.IsNullOrEmpty(label))
            {
                throw new ArgumentNullException("label");
            }

            this.label = label;
            this.password = password;
        }

        /// <summary>
        /// Get or set the label for the cabinet
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
        /// Get or set the password for the cabinet
        /// </summary>
        public string Password
        {
            get { return this.password; }
            set
            {
                if (this.password != value)
                {
                    string oldValue = this.password;
                    this.password = value;

                    if (this.PasswordChanged != null)
                    {
                        this.PasswordChanged(this, oldValue, value);
                    }

                    this.IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Adds a folder to the cabinet
        /// </summary>
        /// <param name="newChild">the new folder</param>
        /// <param name="markDirty">whether to mark the current node dirty</param>
        public override void AddChild(Node newChild, bool markDirty = true)
        {
            if (newChild is Folder)
            {
                base.AddChild(newChild, markDirty);
            }
            else
            {
                throw new ArgumentException(string.Format("Cannot add node of type {0} to a cabinet", newChild.GetType().ToString()));
            }
        }

        /// <summary>
        /// Adds a folder to the cabinet at the given position
        /// </summary>
        /// <param name="newChild">the new folder</param>
        /// <param name="index">the position</param>
        /// <param name="markDirty">whether to mark the current node dirty</param>
        public override void AddChild(Node newChild, int index, bool markDirty = true)
        {
            if (newChild is Folder)
            {
                base.AddChild(newChild, index, markDirty);
            }
            else
            {
                throw new ArgumentException(string.Format("Cannot add node of type {0} to a cabinet", newChild.GetType().ToString()));
            }
        }

        /// <summary>
        /// Compares 2 cabinets
        /// </summary>
        /// <param name="other">cabinet to compare</param>
        /// <returns>true if equal, false otherwise</returns>
        public override bool IsEqual(Node other)
        {
            Cabinet otherCabinet = other as Cabinet;
            if (otherCabinet == null)
            {
                return false;
            }

            if (this.Label == otherCabinet.Label &&
                this.Password == otherCabinet.Password)
            {
                return base.IsEqual(otherCabinet);
            }

            return false;
        }

        /// <summary>
        /// Creates a clone of this cabinet
        /// </summary>
        /// <returns>the new cabinet</returns>
        public override Node Clone()
        {
            Cabinet newCabinet = new Cabinet(this.ID, this.CreationTime, this.LastModifiedTime, this.Label, this.Password);

            if (this.HasChildren)
            {
                foreach (Node child in this.Children)
                {
                    Node newNode = child.Clone();
                    newCabinet.AddChild(newNode, false);
                }
            }

            return newCabinet;
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
