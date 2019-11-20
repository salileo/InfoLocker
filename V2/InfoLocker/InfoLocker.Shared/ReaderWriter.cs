using InfoLocker.Parts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace InfoLocker
{
    public class ReaderWriter
    {
        private const string CabinetNodeName = "Cabinet";
        private const string FolderNodeName = "Folder";
        private const string CardNodeName = "Card";
        private const string MultiLineEntryNodeName = "MultiLineEntry";
        private const string SingleLineEntryNodeName = "SingleLineEntry";

        private const string IdAttribute = "Id";
        private const string CreatedAttribute = "Created";
        private const string ModifiedAttribute = "Modified";
        private const string LabelAttribute = "Label";
        private const string PasswordAttribute = "Password";

        #region Reader

        /// <summary>
        /// Parses a string to create a cabinet object
        /// </summary>
        /// <param name="str">the string to parse</param>
        /// <returns>the newly created cabinet object</returns>
        /// <exception cref="Exception">This method can throw an exception</exception>
        public static Cabinet ReadFromString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            {
                return ReadFromStream(stream);
            }
        }

        /// <summary>
        /// Parses a stream to create a cabinet object
        /// </summary>
        /// <param name="stream">the stream to parse</param>
        /// <returns>the newly created cabinet object</returns>
        /// <exception cref="Exception">This method can throw an exception</exception>
        public static Cabinet ReadFromStream(Stream stream)
        {
            if (stream == null)
            {
                return null;
            }

            XmlReaderSettings settings = new XmlReaderSettings();
            XmlReader reader = XmlReader.Create(stream, settings);
            reader.MoveToContent();

            if (reader.NodeType == XmlNodeType.Element)
            {
                if (reader.Name == ReaderWriter.CabinetNodeName)
                {
                    return ReadCabinet(reader);
                }
                else
                {
                    throw new Exception(string.Format("Unexpected element '{0}' while parsing", reader.Name));
                }
            }
            else
            {
                throw new Exception(string.Format("Unexpected node type '{0}' while parsing", reader.NodeType.ToString()));
            }
        }

        /// <summary>
        /// Parses XML to create a cabinet
        /// </summary>
        /// <param name="parentReader">the XML reader</param>
        /// <returns>the newly created cabinet object</returns>
        private static Cabinet ReadCabinet(XmlReader parentReader)
        {
            using (XmlReader reader = parentReader.ReadSubtree())
            {
                if (!reader.Read())
                {
                    throw new Exception("Error parsing cabinet subtree");
                }

                string id = reader.GetAttribute(ReaderWriter.IdAttribute);
                if (string.IsNullOrEmpty(id))
                {
                    throw new Exception("Id missing for cabinet");
                }

                string created = reader.GetAttribute(ReaderWriter.CreatedAttribute);
                if (string.IsNullOrEmpty(created))
                {
                    throw new Exception("Created time missing for cabinet");
                }

                DateTime createdTime;
                if (!DateTime.TryParse(created, out createdTime))
                {
                    throw new Exception("Error parsing created time for cabinet");
                }

                string modified = reader.GetAttribute(ReaderWriter.ModifiedAttribute);
                if (string.IsNullOrEmpty(modified))
                {
                    throw new Exception("Modified time missing for cabinet");
                }

                DateTime modifiedTime;
                if (!DateTime.TryParse(modified, out modifiedTime))
                {
                    throw new Exception("Error parsing modified time for cabinet");
                }

                string label = reader.GetAttribute(ReaderWriter.LabelAttribute);
                if (string.IsNullOrEmpty(label))
                {
                    throw new Exception("Label missing for cabinet");
                }

                string password = reader.GetAttribute(ReaderWriter.PasswordAttribute);

                Cabinet newCabinet = new Cabinet(id, createdTime, modifiedTime, label, password);

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == ReaderWriter.FolderNodeName)
                        {
                            try
                            {
                                Folder childFolder = ReadFolder(reader);
                                newCabinet.AddChild(childFolder, false);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(string.Format("Error while parsing cabinet '{0}'", label), ex);
                            }
                        }
                        else
                        {
                            throw new Exception(string.Format("Unexpected element '{0}' while parsing cabinet", reader.Name));
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.Whitespace)
                    {
                        continue;
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        break;
                    }
                    else
                    {
                        throw new Exception(string.Format("Unexpected node type '{0}' while parsing cabinet", reader.NodeType.ToString()));
                    }
                }

                return newCabinet;
            }
        }

        /// <summary>
        /// Parses XML to create a folder
        /// </summary>
        /// <param name="parentReader">the XML reader</param>
        /// <returns>the newly created folder object</returns>
        private static Folder ReadFolder(XmlReader parentReader)
        {
            using (XmlReader reader = parentReader.ReadSubtree())
            {
                if (!reader.Read())
                {
                    throw new Exception("Error parsing folder subtree");
                }

                string id = reader.GetAttribute(ReaderWriter.IdAttribute);
                if (string.IsNullOrEmpty(id))
                {
                    throw new Exception("Id missing for folder");
                }

                string created = reader.GetAttribute(ReaderWriter.CreatedAttribute);
                if (string.IsNullOrEmpty(created))
                {
                    throw new Exception("Created time missing for folder");
                }

                DateTime createdTime;
                if (!DateTime.TryParse(created, out createdTime))
                {
                    throw new Exception("Error parsing created time for folder");
                }

                string modified = reader.GetAttribute(ReaderWriter.ModifiedAttribute);
                if (string.IsNullOrEmpty(modified))
                {
                    throw new Exception("Modified time missing for folder");
                }

                DateTime modifiedTime;
                if (!DateTime.TryParse(modified, out modifiedTime))
                {
                    throw new Exception("Error parsing modified time for folder");
                }

                string label = reader.GetAttribute(ReaderWriter.LabelAttribute);
                if (string.IsNullOrEmpty(label))
                {
                    throw new Exception("Label missing for folder");
                }

                Folder newFolder = new Folder(id, createdTime, modifiedTime, label);

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == ReaderWriter.FolderNodeName)
                        {
                            try
                            {
                                Folder childFolder = ReadFolder(reader);
                                newFolder.AddChild(childFolder, false);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(string.Format("Error while parsing folder '{0}'", label), ex);
                            }
                        }
                        else if (reader.Name == ReaderWriter.CardNodeName)
                        {
                            try
                            {
                                Card childCard = ReadCard(reader);
                                newFolder.AddChild(childCard, false);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(string.Format("Error while parsing folder '{0}'", label), ex);
                            }
                        }
                        else
                        {
                            throw new Exception(string.Format("Unexpected element '{0}' while parsing folder", reader.Name));
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.Whitespace)
                    {
                        continue;
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        break;
                    }
                    else
                    {
                        throw new Exception(string.Format("Unexpected node type '{0}' while parsing folder", reader.NodeType.ToString()));
                    }
                }

                return newFolder;
            }
        }

        /// <summary>
        /// Parses XML to create a card
        /// </summary>
        /// <param name="parentReader">the XML reader</param>
        /// <returns>the newly created card object</returns>
        private static Card ReadCard(XmlReader parentReader)
        {
            using (XmlReader reader = parentReader.ReadSubtree())
            {
                if (!reader.Read())
                {
                    throw new Exception("Error parsing card subtree");
                }

                string id = reader.GetAttribute(ReaderWriter.IdAttribute);
                if (string.IsNullOrEmpty(id))
                {
                    throw new Exception("Id missing for card");
                }

                string created = reader.GetAttribute(ReaderWriter.CreatedAttribute);
                if (string.IsNullOrEmpty(created))
                {
                    throw new Exception("Created time missing for card");
                }

                DateTime createdTime;
                if (!DateTime.TryParse(created, out createdTime))
                {
                    throw new Exception("Error parsing created time for card");
                }

                string modified = reader.GetAttribute(ReaderWriter.ModifiedAttribute);
                if (string.IsNullOrEmpty(modified))
                {
                    throw new Exception("Modified time missing for card");
                }

                DateTime modifiedTime;
                if (!DateTime.TryParse(modified, out modifiedTime))
                {
                    throw new Exception("Error parsing modified time for card");
                }

                string label = reader.GetAttribute(ReaderWriter.LabelAttribute);
                if (string.IsNullOrEmpty(label))
                {
                    throw new Exception("Label missing for card");
                }

                Card newCard = new Card(id, createdTime, modifiedTime, label);

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == ReaderWriter.MultiLineEntryNodeName)
                        {
                            try
                            {
                                MultiLineEntry childEntry = ReadMultiLineEntry(reader);
                                newCard.AddChild(childEntry, false);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(string.Format("Error while parsing card '{0}'", label), ex);
                            }
                        }
                        else if (reader.Name == ReaderWriter.SingleLineEntryNodeName)
                        {
                            try
                            {
                                SingleLineEntry childEntry = ReadSingleLineEntry(reader);
                                newCard.AddChild(childEntry, false);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(string.Format("Error while parsing card '{0}'", label), ex);
                            }
                        }
                        else
                        {
                            throw new Exception(string.Format("Unexpected element '{0}' while parsing card", reader.Name));
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.Whitespace)
                    {
                        continue;
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        break;
                    }
                    else
                    {
                        throw new Exception(string.Format("Unexpected node type '{0}' while parsing card", reader.NodeType.ToString()));
                    }
                }

                return newCard;
            }
        }

        /// <summary>
        /// Parses XML to create a multiLine entry
        /// </summary>
        /// <param name="parentReader">the XML reader</param>
        /// <returns>the newly created multiLine entry object</returns>
        private static MultiLineEntry ReadMultiLineEntry(XmlReader parentReader)
        {
            using (XmlReader reader = parentReader.ReadSubtree())
            {
                if (!reader.Read())
                {
                    throw new Exception("Error parsing multiLine entry subtree");
                }

                string id = reader.GetAttribute(ReaderWriter.IdAttribute);
                if (string.IsNullOrEmpty(id))
                {
                    throw new Exception("Id missing for multiLine entry");
                }

                string created = reader.GetAttribute(ReaderWriter.CreatedAttribute);
                if (string.IsNullOrEmpty(created))
                {
                    throw new Exception("Created time missing for multiLine entry");
                }

                DateTime createdTime;
                if (!DateTime.TryParse(created, out createdTime))
                {
                    throw new Exception("Error parsing created time for multiLine entry");
                }

                string modified = reader.GetAttribute(ReaderWriter.ModifiedAttribute);
                if (string.IsNullOrEmpty(modified))
                {
                    throw new Exception("Modified time missing for multiLine entry");
                }

                DateTime modifiedTime;
                if (!DateTime.TryParse(modified, out modifiedTime))
                {
                    throw new Exception("Error parsing modified time for multiLine entry");
                }

                string label = reader.GetAttribute(ReaderWriter.LabelAttribute);
                if (string.IsNullOrEmpty(label))
                {
                    throw new Exception("Label missing for multiLine entry");
                }

                string content = null;

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Text)
                    {
                        content = reader.Value;
                    }
                    else if (reader.NodeType == XmlNodeType.Whitespace)
                    {
                        continue;
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        break;
                    }
                    else
                    {
                        throw new Exception(string.Format("Unexpected node type '{0}' while parsing multiLine entry", reader.NodeType.ToString()));
                    }
                }

                MultiLineEntry newEntry = new MultiLineEntry(id, createdTime, modifiedTime, label, content);
                return newEntry;
            }
        }

        /// <summary>
        /// Parses XML to create a singleLine entry
        /// </summary>
        /// <param name="parentReader">the XML reader</param>
        /// <returns>the newly created singleLine entry object</returns>
        private static SingleLineEntry ReadSingleLineEntry(XmlReader parentReader)
        {
            using (XmlReader reader = parentReader.ReadSubtree())
            {
                if (!reader.Read())
                {
                    throw new Exception("Error parsing singleLine entry subtree");
                }

                string id = reader.GetAttribute(ReaderWriter.IdAttribute);
                if (string.IsNullOrEmpty(id))
                {
                    throw new Exception("Id missing for singleLine entry");
                }

                string created = reader.GetAttribute(ReaderWriter.CreatedAttribute);
                if (string.IsNullOrEmpty(created))
                {
                    throw new Exception("Created time missing for singleLine entry");
                }

                DateTime createdTime;
                if (!DateTime.TryParse(created, out createdTime))
                {
                    throw new Exception("Error parsing created time for singleLine entry");
                }

                string modified = reader.GetAttribute(ReaderWriter.ModifiedAttribute);
                if (string.IsNullOrEmpty(modified))
                {
                    throw new Exception("Modified time missing for singleLine entry");
                }

                DateTime modifiedTime;
                if (!DateTime.TryParse(modified, out modifiedTime))
                {
                    throw new Exception("Error parsing modified time for singleLine entry");
                }

                string label = reader.GetAttribute(ReaderWriter.LabelAttribute);
                if (string.IsNullOrEmpty(label))
                {
                    throw new Exception("Label missing for singleLine entry");
                }

                string content = null;

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Text)
                    {
                        content = reader.Value;
                    }
                    else if (reader.NodeType == XmlNodeType.Whitespace)
                    {
                        continue;
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        break;
                    }
                    else
                    {
                        throw new Exception(string.Format("Unexpected node type '{0}' while parsing singleLine entry", reader.NodeType.ToString()));
                    }
                }

                SingleLineEntry newEntry = new SingleLineEntry(id, createdTime, modifiedTime, label, content);
                return newEntry;
            }
        }
        #endregion

        #region Writer
        /// <summary>
        /// Writes a cabinet object to a string
        /// </summary>
        /// <param name="root">the cabinet object</param>
        /// <param name="str">the output string</param>
        public static void WriteToString(Cabinet root, out string str)
        {
            str = null;

            using (MemoryStream stream = new MemoryStream())
            {
                WriteToStream(root, stream);
                stream.Seek(0, SeekOrigin.Begin);

                using (StreamReader reader = new StreamReader(stream))
                {
                    str = reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Writes a cabinet object to a stream
        /// </summary>
        /// <param name="root">the cabinet object</param>
        /// <param name="stream">the output stream</param>
        public static void WriteToStream(Cabinet root, Stream stream)
        {
            if (root == null)
            {
                throw new ArgumentNullException("root");
            }

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;
            settings.Indent = true;

            XmlWriter writer = XmlWriter.Create(stream, settings);
            writer.WriteStartDocument();

            WriteCabinet(writer, root);

            writer.WriteEndDocument();
            writer.Flush();

        }

        /// <summary>
        /// Writes a cabinet object to an XML writer
        /// </summary>
        /// <param name="writer">the XML writer</param>
        /// <param name="cabinet">the cabinet object</param>
        private static void WriteCabinet(XmlWriter writer, Cabinet cabinet)
        {
            if (cabinet == null)
            {
                throw new ArgumentNullException("cabinet");
            }

            writer.WriteStartElement(ReaderWriter.CabinetNodeName);
            
            writer.WriteAttributeString(ReaderWriter.IdAttribute, cabinet.ID);
            writer.WriteAttributeString(ReaderWriter.CreatedAttribute, cabinet.CreationTime.ToString());
            writer.WriteAttributeString(ReaderWriter.ModifiedAttribute, cabinet.LastModifiedTime.ToString());
            writer.WriteAttributeString(ReaderWriter.LabelAttribute, cabinet.Label);

            if (!string.IsNullOrEmpty(cabinet.Password))
            {
                writer.WriteAttributeString(ReaderWriter.PasswordAttribute, cabinet.Password);
            }

            if (cabinet.HasChildren)
            {
                foreach (Node child in cabinet.Children)
                {
                    try
                    {
                        WriteFolder(writer, child as Folder);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("Error while writing cabinet '{0}'", cabinet.Label), ex);
                    }
                }
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes a folder object to an XML writer
        /// </summary>
        /// <param name="writer">the XML writer</param>
        /// <param name="folder">the folder object</param>
        private static void WriteFolder(XmlWriter writer, Folder folder)
        {
            if (folder == null)
            {
                throw new ArgumentNullException("folder");
            }

            writer.WriteStartElement(ReaderWriter.FolderNodeName);

            writer.WriteAttributeString(ReaderWriter.IdAttribute, folder.ID);
            writer.WriteAttributeString(ReaderWriter.CreatedAttribute, folder.CreationTime.ToString());
            writer.WriteAttributeString(ReaderWriter.ModifiedAttribute, folder.LastModifiedTime.ToString());
            writer.WriteAttributeString(ReaderWriter.LabelAttribute, folder.Label);

            if (folder.HasChildren)
            {
                foreach (Node child in folder.Children)
                {
                    try
                    {
                        if (child is Folder)
                        {
                            WriteFolder(writer, child as Folder);
                        }
                        else
                        {
                            WriteCard(writer, child as Card);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("Error while writing folder '{0}'", folder.Label), ex);
                    }
                }
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes a card object to an XML writer
        /// </summary>
        /// <param name="writer">the XML writer</param>
        /// <param name="card">the card object</param>
        private static void WriteCard(XmlWriter writer, Card card)
        {
            if (card == null)
            {
                throw new ArgumentNullException("card");
            }

            writer.WriteStartElement(ReaderWriter.CardNodeName);

            writer.WriteAttributeString(ReaderWriter.IdAttribute, card.ID);
            writer.WriteAttributeString(ReaderWriter.CreatedAttribute, card.CreationTime.ToString());
            writer.WriteAttributeString(ReaderWriter.ModifiedAttribute, card.LastModifiedTime.ToString());
            writer.WriteAttributeString(ReaderWriter.LabelAttribute, card.Label);

            if (card.HasChildren)
            {
                foreach (Node child in card.Children)
                {
                    try
                    {
                        if (child is MultiLineEntry)
                        {
                            WriteMultiLineEntry(writer, child as MultiLineEntry);
                        }
                        else
                        {
                            WriteSingleLineEntry(writer, child as SingleLineEntry);
                        }
                    }
                    catch(Exception ex)
                    {
                        throw new Exception(string.Format("Error while writing card '{0}'", card.Label), ex);
                    }
                }
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes a multiLine entry object to an XML writer
        /// </summary>
        /// <param name="writer">the XML writer</param>
        /// <param name="entry">the entry object</param>
        private static void WriteMultiLineEntry(XmlWriter writer, MultiLineEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }

            writer.WriteStartElement(ReaderWriter.MultiLineEntryNodeName);

            writer.WriteAttributeString(ReaderWriter.IdAttribute, entry.ID);
            writer.WriteAttributeString(ReaderWriter.CreatedAttribute, entry.CreationTime.ToString());
            writer.WriteAttributeString(ReaderWriter.ModifiedAttribute, entry.LastModifiedTime.ToString());
            writer.WriteAttributeString(ReaderWriter.LabelAttribute, entry.Label);

            if (!string.IsNullOrEmpty(entry.Content))
            {
                writer.WriteString(entry.Content);
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes a singleLine entry object to an XML writer
        /// </summary>
        /// <param name="writer">the XML writer</param>
        /// <param name="entry">the entry object</param>
        private static void WriteSingleLineEntry(XmlWriter writer, SingleLineEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }

            writer.WriteStartElement(ReaderWriter.SingleLineEntryNodeName);

            writer.WriteAttributeString(ReaderWriter.IdAttribute, entry.ID);
            writer.WriteAttributeString(ReaderWriter.CreatedAttribute, entry.CreationTime.ToString());
            writer.WriteAttributeString(ReaderWriter.ModifiedAttribute, entry.LastModifiedTime.ToString());
            writer.WriteAttributeString(ReaderWriter.LabelAttribute, entry.Label);

            if (!string.IsNullOrEmpty(entry.Content))
            {
                writer.WriteString(entry.Content);
            }

            writer.WriteEndElement();
        }
        #endregion
    }
}
