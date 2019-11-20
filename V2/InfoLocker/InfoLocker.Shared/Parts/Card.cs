using System;
using System.Collections.Generic;
using System.Text;

namespace InfoLocker.Parts
{
    public class Card : Node
    {
        public delegate void PropertyUpdatedEventHandler(Card sender, string oldValue, string newValue);

        #region private members
        private string label;
        #endregion

        #region events
        public event PropertyUpdatedEventHandler LabelChanged;
        #endregion 

        /// <summary>
        /// Constructor for a new card
        /// </summary>
        /// <param name="label">name of the card</param>
        public Card(string label)
            : base(true)
        {
            if (string.IsNullOrEmpty(label))
            {
                throw new ArgumentNullException("label");
            }

            this.label = label;
        }

        /// <summary>
        /// Constructor for a card that is being re-constructed
        /// </summary>
        /// <param name="id">id of the card</param>
        /// <param name="created">time of creation</param>
        /// <param name="modified">time of last modification</param>
        /// <param name="label">name of the card</param>
        public Card(string id, DateTime created, DateTime modified, string label)
            : base(id, created, modified, true)
        {
            if (string.IsNullOrEmpty(label))
            {
                throw new ArgumentNullException("label");
            }

            this.label = label;
        }

        /// <summary>
        /// Get or set the label for the card
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
        /// Adds an entry to the card
        /// </summary>
        /// <param name="newChild">the new child</param>
        /// <param name="markDirty">whether to mark the current node dirty</param>
        public override void AddChild(Node newChild, bool markDirty = true)
        {
            if (newChild is SingleLineEntry ||
                newChild is MultiLineEntry)
            {
                base.AddChild(newChild, markDirty);
            }
            else
            {
                throw new ArgumentException(string.Format("Cannot add node of type {0} to a card", newChild.GetType().ToString()));
            }
        }

        /// <summary>
        /// Adds an entry to the card at the given position
        /// </summary>
        /// <param name="newChild">the new child</param>
        /// <param name="index">the position</param>
        /// <param name="markDirty">whether to mark the current node dirty</param>
        public override void AddChild(Node newChild, int index, bool markDirty = true)
        {
            if (newChild is SingleLineEntry ||
                newChild is MultiLineEntry)
            {
                base.AddChild(newChild, index, markDirty);
            }
            else
            {
                throw new ArgumentException(string.Format("Cannot add node of type {0} to a card", newChild.GetType().ToString()));
            }
        }

        /// <summary>
        /// Compares 2 cards
        /// </summary>
        /// <param name="other">card to compare</param>
        /// <returns>true if equal, false otherwise</returns>
        public override bool IsEqual(Node other)
        {
            Card otherCard = other as Card;
            if (otherCard == null)
            {
                return false;
            }

            if (this.Label == otherCard.Label)
            {
                return base.IsEqual(otherCard);
            }

            return false;
        }

        /// <summary>
        /// Creates a clone of this card
        /// </summary>
        /// <returns>the new entry</returns>
        public override Node Clone()
        {
            Card newCard = new Card(this.ID, this.CreationTime, this.LastModifiedTime, this.Label);

            if (this.HasChildren)
            {
                foreach (Node child in this.Children)
                {
                    Node newNode = child.Clone();
                    newCard.AddChild(newNode, false);
                }
            }

            return newCard;
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
