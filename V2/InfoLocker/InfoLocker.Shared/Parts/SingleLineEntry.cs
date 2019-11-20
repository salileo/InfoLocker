using System;
using System.Collections.Generic;
using System.Text;

namespace InfoLocker.Parts
{
    public class SingleLineEntry : Node
    {
        public delegate void PropertyUpdatedEventHandler(SingleLineEntry sender, string oldValue, string newValue);

        #region private members
        private string label;
        private string content;
        #endregion

        #region events
        public event PropertyUpdatedEventHandler LabelChanged;
        public event PropertyUpdatedEventHandler ContentChanged;
        #endregion 

        /// <summary>
        /// Constructor for a new single line entry
        /// </summary>
        /// <param name="label">name of the entry</param>
        public SingleLineEntry(string label)
            : base(false)
        {
            if (string.IsNullOrEmpty(label))
            {
                throw new ArgumentNullException("label");
            }

            this.label = label;
        }

        /// <summary>
        /// Constructor for an entry that is being re-constructed
        /// </summary>
        /// <param name="id">id of the entry</param>
        /// <param name="created">time of creation</param>
        /// <param name="modified">time of last modification</param>
        /// <param name="label">name of the entry</param>
        /// <param name="content">content of the entry</param>
        public SingleLineEntry(string id, DateTime created, DateTime modified, string label, string content)
            : base(id, created, modified, false)
        {
            if (string.IsNullOrEmpty(label))
            {
                throw new ArgumentNullException("label");
            }

            this.label = label;
            this.content = content;
        }

        /// <summary>
        /// Get or set the label for the entry
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
        /// Get or set the content for the entry
        /// </summary>
        public string Content
        {
            get { return this.content; }
            set
            {
                if (this.content != value)
                {
                    if (value != null && value.Contains("\n"))
                    {
                        throw new ArgumentException(string.Format("SingleLineEntry cannot have newline in the content - {0}", value));
                    }

                    string oldValue = this.content;
                    this.content = value;

                    if (this.ContentChanged != null)
                    {
                        this.ContentChanged(this, oldValue, value);
                    }

                    this.IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Compares 2 entries
        /// </summary>
        /// <param name="other">entry to compare</param>
        /// <returns>true if equal, false otherwise</returns>
        public override bool IsEqual(Node other)
        {
            SingleLineEntry otherEntry = other as SingleLineEntry;
            if (otherEntry == null)
            {
                return false;
            }

            if (this.Label == otherEntry.Label &&
                this.Content == otherEntry.Content)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Creates a clone of this entry
        /// </summary>
        /// <returns>the new entry</returns>
        public override Node Clone()
        {
            SingleLineEntry newEntry = new SingleLineEntry(this.ID, this.CreationTime, this.LastModifiedTime, this.Label, this.Content);
            return newEntry;
        }

        /// <summary>
        /// Return the string representation of the object
        /// </summary>
        /// <returns>the string representation</returns>
        public override string ToString()
        {
            string nodeStr = base.ToString();
            return string.Format(
                "Label={0}, {1}, Content={2}",
                this.Label,
                nodeStr,
                this.Content);
        }
    }
}
